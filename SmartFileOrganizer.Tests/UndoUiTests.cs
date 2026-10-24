using System.Reflection;
using System.Windows.Forms;
using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    // In-process control-state regression checks; no window is shown, no dialog
    // is automated, and all configuration/files live in the test directory.
    private static MainForm CreateUndoTestForm(string root)
    {
        ConstructorInfo constructor = typeof(MainForm).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
            null, [typeof(SettingsStore)], null)!;
        return (MainForm)constructor.Invoke([new SettingsStore(Path.Combine(root, "settings.json"))]);
    }

    private static T UiField<T>(MainForm form, string name) =>
        (T)typeof(MainForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;

    private static object? UiCall(MainForm form, string name, params object?[] arguments) =>
        typeof(MainForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, arguments);

    private static object UiPreviewState(string name) =>
        Enum.Parse(typeof(MainForm).GetNestedType("PreviewState", BindingFlags.NonPublic)!, name);

    private static FileOrganizationResult OrganizeInTestForm(MainForm form, string root)
    {
        UiField<TextBox>(form, "txtFolderPath").Text = root;
        FileItem[] files = Scanner.Scan(root).ToArray();
        UiField<List<FileItem>>(form, "scannedFiles").AddRange(files);
        typeof(MainForm).GetField("scannedFolderPath", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, root);
        UiCall(form, "RenderPendingFiles");
        CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        try
        {
            FileOrganizationResult moved = Organizer.OrganizeFiles(root, files);
            UiCall(form, "RecordOrganizationResult", moved);
            UiCall(form, "ApplyOrganizationResults", files, moved.FileResults, 0, new HashSet<FileItem>());
            UiCall(form, "SetPreviewState", UiPreviewState("Organized"));
            Check(!UiField<Button>(form, "btnUndo").Enabled, "Recording history must not enable Undo while still busy.");
            return moved;
        }
        finally
        {
            UiCall(form, "EndOperation", source);
        }
    }

    private static FileOrganizationResult UndoInTestForm(MainForm form)
    {
        FileMoveResult[] snapshot = UiField<FileUndoHistory>(form, "undoHistory").GetSnapshot();
        CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        try
        {
            Check(!UiField<Button>(form, "btnUndo").Enabled && UiField<Button>(form, "btnCancel").Enabled,
                "Undo must be locked and Cancel available throughout an active operation.");
            FileOrganizationResult restored = UndoService.UndoFiles(snapshot, source.Token);
            UiCall(form, "ApplyUndoResults", snapshot, restored);
            Check(!UiField<Button>(form, "btnUndo").Enabled, "Undo became enabled before releasing busy.");
            return restored;
        }
        finally
        {
            UiCall(form, "EndOperation", source);
        }
    }

    private static void UndoEmptyPreviewState(string root)
    {
        for (int index = 0; index < 1080; index++) WriteFile(root, $"file-{index:D4}.txt", $"contents {index}");
        using MainForm form = CreateUndoTestForm(root);
        FileOrganizationResult moved = OrganizeInTestForm(form, root);
        Button undo = UiField<Button>(form, "btnUndo");
        Check(moved.SuccessCount == 1080 && UiField<DataGridView>(form, "dgvFiles").Rows.Count == 0 &&
            UiField<List<FileItem>>(form, "scannedFiles").Count == 0 &&
            UiField<Label>(form, "lblEmptyTitle").Text == "All files organized" && undo.Enabled,
            "1080/1080 moved and an empty All files organized preview must leave Undo enabled.");
        for (Control? parent = undo.Parent; parent != null; parent = parent.Parent)
            Check(parent.Enabled, "An Undo parent control is disabled.");

        UiField<TextBox>(form, "txtFolderPath").Text = Directory.CreateDirectory(Path.Combine(root, "other-folder")).FullName;
        UiCall(form, "ClearScanResults", UiPreviewState("RulesChanged"));
        Check(undo.Enabled && UiField<FileUndoHistory>(form, "undoHistory").PendingCount == 1080,
            "Changing the folder/rules or clearing the scan discarded Undo.");
        UiField<TextBox>(form, "txtFolderPath").Text = root;
        UiField<List<FileItem>>(form, "scannedFiles").AddRange(Scanner.Scan(root));
        UiCall(form, "RenderPendingFiles");
        Check(undo.Enabled && UiField<DataGridView>(form, "dgvFiles").Rows.Count == 0,
            "Re-scanning an empty folder disabled a valid Undo transaction.");

        CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        UiCall(form, "RequestCancellation");
        Check(!undo.Enabled && !UiField<Button>(form, "btnCancel").Enabled,
            "Waiting for cancellation must not unlock Undo or allow another Cancel.");
        UiCall(form, "EndOperation", source);
        Check(undo.Enabled, "Releasing a cancelled operation did not restore Undo availability.");
        Check(UndoInTestForm(form).SuccessCount == 1080 && !undo.Enabled,
            "Completing Undo did not disable the button after restoring every file.");
        for (int index = 0; index < 1080; index++)
            Check(File.ReadAllText(Path.Combine(root, $"file-{index:D4}.txt")) == $"contents {index}",
                "The UI transaction restored incorrect paths or contents.");
    }

    private static void UndoPartialState(string root)
    {
        string conflict = WriteFile(root, "a.txt", "original a");
        string other = WriteFile(root, "b.txt", "original b");
        using MainForm form = CreateUndoTestForm(root);
        OrganizeInTestForm(form, root);
        File.WriteAllText(conflict, "occupied");
        FileOrganizationResult partial = UndoInTestForm(form);
        Check(partial.SuccessCount == 1 && partial.FailedCount == 1 && UiField<Button>(form, "btnUndo").Enabled &&
            UiField<FileUndoHistory>(form, "undoHistory").PendingCount == 1 && File.ReadAllText(conflict) == "occupied" &&
            File.ReadAllText(other) == "original b", "Partial Undo did not preserve its remaining transaction and button state.");
        File.Delete(conflict);
        Check(UndoInTestForm(form).SuccessCount == 1 && !UiField<Button>(form, "btnUndo").Enabled &&
            File.ReadAllText(conflict) == "original a", "The final retry did not restore the remaining file and disable Undo.");
    }
}
