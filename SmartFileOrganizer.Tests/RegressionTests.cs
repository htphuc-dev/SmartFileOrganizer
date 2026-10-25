using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    private static void ScanFailureIsNotPartialSuccess(string root)
    {
        string empty = WriteFile(root, "tệp rỗng.TXT", "");
        WriteFile(root, "no extension", "no extension");
        List<FileItem>? published = null;
        int read = 0;
        Expect<IOException>(() => published = Scanner.Scan(root, progress: new InlineProgress<int>(count =>
        {
            read = count;
            throw new IOException("Simulated read/report failure after the first file.");
        })));
        Check(read == 1 && published == null && File.ReadAllText(empty) == "",
            "A failed scan returned a partial success or changed its source.");
        Check(Scanner.Scan(root).Single(file => file.FullPath == empty).Size == 0,
            "A zero-byte Unicode file was not scanned correctly.");
    }

    private static FileItem[] LoadTestPreview(MainForm form, string root)
    {
        UiField<TextBox>(form, "txtFolderPath").Text = root;
        FileItem[] files = Scanner.Scan(root).ToArray();
        foreach (FileItem file in files) file.PreviewDestinationPath = Organizer.GetPreviewDestinationPath(root, file);
        List<FileItem> pending = UiField<List<FileItem>>(form, "scannedFiles");
        pending.Clear();
        pending.AddRange(files);
        typeof(MainForm).GetField("scannedFolderPath", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, root);
        UiCall(form, "RenderPendingFiles");
        return files;
    }

    private static void GridSortingAndSelection(string root)
    {
        WriteFile(root, "z small.txt", "a");
        WriteFile(root, "b large.txt", new string('b', 2048));
        WriteFile(root, "a medium.txt", new string('c', 128));
        using MainForm form = CreateUndoTestForm(root);
        FileItem[] files = LoadTestPreview(form, root);
        DataGridView grid = UiField<DataGridView>(form, "dgvFiles");
        DataGridViewColumn size = UiField<DataGridViewColumn>(form, "colSize");
        grid.Sort(size, ListSortDirection.Ascending);
        Check(grid.Rows.Cast<DataGridViewRow>().Select(row => row.Cells[size.Index].Value)
            .SequenceEqual(new object[] { 1L, 128L, 2048L }), "The actual grid sorted formatted sizes instead of raw bytes.");
        UiCall(form, "SetAllSelected", false);
        int selectedColumn = UiField<DataGridViewColumn>(form, "colSelected").Index;
        grid.Rows[1].Cells[selectedColumn].Value = true;
        FileItem selected = (FileItem)grid.Rows[1].Tag!;
        Check(selected.IsSelected && files.Count(file => file.IsSelected) == 1 &&
            UiField<Label>(form, "lblSelectionSummary").Text.Contains("Selected size: 128 B"),
            "A checkbox after sorting updated another file or incorrect totals.");
        grid.Sort(UiField<DataGridViewColumn>(form, "colFileName"), ListSortDirection.Descending);
        Check(grid.Rows.Cast<DataGridViewRow>().All(row => row.Cells[selectedColumn].Value is bool value && value == ((FileItem)row.Tag!).IsSelected),
            "Re-sorting detached checkboxes from their file identities.");
        FileItem[] chosen = SmartFileOrganizer.Helpers.FileSelectionHelper.GetSelectedFiles(files);
        FileOrganizationResult moved = Organizer.OrganizeFiles(root, chosen);
        Check(moved.SuccessCount == 1 && File.ReadAllText(moved.FileResults[0].DestinationPath) == new string('c', 128) &&
            File.ReadAllText(Path.Combine(root, "z small.txt")) == "a" &&
            File.ReadAllText(Path.Combine(root, "b large.txt")) == new string('b', 2048), "Sorting/selection moved the wrong file.");
        UiCall(form, "RecordOrganizationResult", moved);
        UiField<TextBox>(form, "txtFolderPath").Text = Directory.CreateDirectory(Path.Combine(root, "different folder")).FullName;
        Check(grid.Rows.Count == 0 && UiField<List<FileItem>>(form, "scannedFiles").Count == 0 &&
            !UiField<Button>(form, "btnOrganize").Enabled && UiField<Button>(form, "btnUndo").Enabled,
            "Changing paths did not invalidate the old scan independently of Undo.");
    }

    private static T SettingsField<T>(SettingsForm form, string name) =>
        (T)typeof(SettingsForm).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;

    private static void SettingsClick(SettingsForm form, string name) =>
        typeof(SettingsForm).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [form, EventArgs.Empty]);

    private static void SettingsDraftDiscard(string root)
    {
        string path = Path.Combine(root, "configuration", "settings.json");
        SettingsStore store = new(path);
        store.Save(CodeSettings());
        byte[] before = File.ReadAllBytes(path);
        ClassificationSettings active = store.Current;
        using (SettingsForm form = new(store))
        {
            DataGridView grid = SettingsField<DataGridView>(form, "dgvCategories");
            grid.Rows[0].Cells[0].Value = "Uncommitted name";
            SettingsClick(form, "btnAddGroup_Click");
            grid.EndEdit();
            Check(ReferenceEquals(active, store.Current) && File.ReadAllBytes(path).SequenceEqual(before),
                "Editing a draft changed active or saved settings before Save.");
            Check(SettingsField<Button>(form, "btnCancelSettings").DialogResult == DialogResult.Cancel,
                "The Settings Cancel action does not cancel the dialog.");
            form.DialogResult = DialogResult.Cancel;
        }
        Check(ReferenceEquals(active, store.Current) && File.ReadAllBytes(path).SequenceEqual(before),
            "Discarding/closing a draft published unsaved rules.");
    }

    private static void SettingsDraftSave(string root)
    {
        SettingsStore store = new(Path.Combine(root, "configuration", "settings.json"));
        using SettingsForm form = new(store);
        DataGridView grid = SettingsField<DataGridView>(form, "dgvCategories");
        DataGridViewRow fallback = grid.Rows.Cast<DataGridViewRow>().Single(row => (string)row.Tag! == FileCategory.FallbackId);
        grid.CurrentCell = fallback.Cells[0];
        Check(!SettingsField<Button>(form, "btnDeleteGroup").Enabled && fallback.Cells[2].ReadOnly,
            "The fallback group can be deleted or assigned extensions.");
        int count = grid.Rows.Count;
        SettingsClick(form, "btnDeleteGroup_Click");
        Check(grid.Rows.Count == count, "The protected fallback was removed.");
        SettingsClick(form, "btnAddGroup_Click");
        grid.EndEdit();
        DataGridViewRow added = grid.CurrentRow!;
        added.Cells[0].Value = "Mã nguồn";
        added.Cells[1].Value = "Sources";
        added.Cells[2].Value = "CS; .cpp\n.PY";
        grid.Rows.Cast<DataGridViewRow>().Single(row => (string)row.Tag! == "documents").Cells[1].Value = "Papers";
        // Represents an accepted delete; the native confirmation itself is a manual check.
        grid.Rows.Remove(grid.Rows.Cast<DataGridViewRow>().Single(row => (string)row.Tag! == "music"));
        Check(!File.Exists(store.SettingsPath) && store.Current.Categories.All(category => category.Name != "Mã nguồn"),
            "Draft changes escaped before saving.");
        SettingsClick(form, "btnSaveSettings_Click");
        Check(form.DialogResult == DialogResult.OK && form.RulesChanged && store.Current.Categories.Count == 7 &&
            store.Current.Categories.All(category => category.Id != "music") &&
            store.Current.Categories.Single(category => category.Name == "Mã nguồn").Extensions.SequenceEqual(new[] { ".cs", ".cpp", ".py" }) &&
            store.Current.Categories.Single(category => category.Id == "documents").FolderName == "Papers",
            "Save did not publish the added, edited and deleted draft groups.");
        SettingsStore reloaded = new(store.SettingsPath);
        reloaded.Load();
        Check(reloaded.LoadWarning == null && SettingsValidator.AreEquivalent(store.Current, reloaded.Current),
            "UI-saved settings did not reload correctly.");
    }

    private static void SettingsDraftValidation(string root)
    {
        SettingsStore store = new(Path.Combine(root, "settings.json"));
        ClassificationSettings active = store.Current;
        using SettingsForm form = new(store);
        DataGridView grid = SettingsField<DataGridView>(form, "dgvCategories");
        SettingsClick(form, "btnAddGroup_Click");
        grid.EndEdit();
        grid.CurrentRow!.Cells[2].Value = "PDF";
        SettingsClick(form, "btnSaveSettings_Click");
        Check(form.DialogResult == DialogResult.None && !File.Exists(store.SettingsPath) && ReferenceEquals(store.Current, active) &&
            SettingsField<Label>(form, "lblError").Text.Contains(".pdf") && grid.CurrentCell!.ErrorText.Length > 0,
            "A conflicting draft was published or its validation error was hidden.");
    }

    private static void OperationStateGuards(string root)
    {
        string original = WriteFile(root, "keep.txt", "unchanged");
        using MainForm form = CreateUndoTestForm(root);
        LoadTestPreview(form, root);
        CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        try
        {
            foreach (string method in new[] { "ScanAsync", "OrganizeAsync", "UndoAsync" })
                Check(((Task)UiCall(form, method)!).IsCompletedSuccessfully, "A second operation did not return immediately while busy.");
            Check(ReferenceEquals(UiField<CancellationTokenSource>(form, "operationCts"), source) &&
                File.ReadAllText(original) == "unchanged" && Scanner.Scan(root).Count == 1,
                "A guarded operation replaced the worker or changed files.");
            Check((bool)UiCall(form, "CanReportProgress", source)!, "Current progress is unexpectedly blocked.");
            UiCall(form, "RequestCancellation");
            Check(!(bool)UiCall(form, "CanReportProgress", source)! && !UiField<Button>(form, "btnCancel").Enabled,
                "Cancelling still accepts progress or another cancellation.");
        }
        finally { UiCall(form, "EndOperation", source); }
        CancellationTokenSource next = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        try
        {
            Check(!(bool)UiCall(form, "CanReportProgress", source)! && (bool)UiCall(form, "CanReportProgress", next)!,
                "Old callbacks are accepted during a newer operation.");
        }
        finally { UiCall(form, "EndOperation", next); }
    }

    private sealed class QueuedUiContext : SynchronizationContext
    {
        private readonly BlockingCollection<(SendOrPostCallback Callback, object? State)> queue = new();
        public override void Post(SendOrPostCallback callback, object? state) => queue.Add((callback, state));
        public void PumpUntil(Func<bool> finished)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            while (!finished())
            {
                if (timer.Elapsed > TimeSpan.FromSeconds(10)) throw new TimeoutException("The UI completion did not finish.");
                if (queue.TryTake(out var item, 50)) item.Callback(item.State);
            }
        }
    }

    private static void CloseWhileProcessing(string root)
    {
        using MainForm form = CreateUndoTestForm(root);
        SynchronizationContext? previous = SynchronizationContext.Current;
        QueuedUiContext context = new();
        SynchronizationContext.SetSynchronizationContext(context);
        CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
        try
        {
            FormClosingEventArgs closing = new(CloseReason.UserClosing, false);
            UiCall(form, "MainForm_FormClosing", form, closing);
            Check(closing.Cancel && source.IsCancellationRequested && !form.IsDisposed &&
                !(bool)UiCall(form, "CanReportProgress", source)!, "Close did not wait for the cancelled worker.");
            UiCall(form, "EndOperation", source);
            context.PumpUntil(() => form.IsDisposed);
        }
        finally
        {
            if (UiField<CancellationTokenSource?>(form, "operationCts") != null) UiCall(form, "EndOperation", source);
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    private static void LayoutTree(Control control)
    {
        control.PerformLayout();
        foreach (Control child in control.Controls) LayoutTree(child);
        control.PerformLayout();
    }

    private static void MainLayoutLongStatus(string root)
    {
        using MainForm form = CreateUndoTestForm(root);
        _ = form.Handle;
        LayoutTree(form);
        UiCall(form, "SetMinimumLayoutSize");
        Size minimum = form.MinimumSize;
        Rectangle area = Screen.FromControl(form).WorkingArea;
        Size available = new(Math.Min(1366, area.Width) - 24, Math.Min(768, area.Height) - 24);
        form.MinimumSize = new Size(Math.Min(minimum.Width, available.Width), Math.Min(minimum.Height, available.Height));
        object error = Enum.Parse(typeof(MainForm).GetNestedType("Feedback", BindingFlags.NonPublic)!, "Error");
        string message = string.Join(" ", Enumerable.Repeat("A long file operation error with details.", 50));
        UiField<TextBox>(form, "txtFolderPath").Text = Path.Combine(root, new string('x', 220));
        foreach (Size size in new[] { form.Size, form.MinimumSize, available })
        {
            form.Size = size;
            UiCall(form, "SetStatus", message, error);
            CancellationTokenSource source = (CancellationTokenSource)UiCall(form, "BeginOperation")!;
            try
            {
                LayoutTree(form);
                foreach (string name in new[] { "btnSelectAll", "btnClearSelection", "btnCancel", "progressFiles", "layoutFolderPath", "btnBrowse" })
                {
                    for (Control child = UiField<Control>(form, name); child.Parent is Control parent; child = parent)
                    {
                        Check(parent.ClientRectangle.Contains(child.Bounds), $"{name} is clipped by {parent.Name} at {size} with long status text.");
                        if (parent == form) break;
                    }
                }
                Control path = UiField<Control>(form, "layoutFolderPath");
                Control browse = UiField<Control>(form, "btnBrowse");
                Check(path.Top == browse.Top && path.Height == browse.Height, "Folder input and Browse are misaligned.");
                Check(UiField<ToolTip>(form, "tips").GetToolTip(UiField<Label>(form, "lblStatus")) == message,
                    "The full truncated status is not available as a tooltip.");
            }
            finally { UiCall(form, "EndOperation", source); }
        }
    }

    private static void UndoLockedAndRetry(string root)
    {
        string original = WriteFile(root, "locked.txt", "preserve locked Undo contents");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root)));
        FileMoveResult[] snapshot = history.GetSnapshot();
        using (FileStream locked = new(snapshot[0].DestinationPath, FileMode.Open, FileAccess.Read, FileShare.None))
        {
            FileOrganizationResult failed = UndoService.UndoFiles(snapshot);
            history.ApplyUndoResults(snapshot, failed);
            Check(failed.FailedCount == 1 && history.PendingCount == 1 && !File.Exists(original),
                "A locked Undo file lost its retry record.");
        }
        history.ApplyUndoResults(snapshot, UndoService.UndoFiles(snapshot));
        Check(history.PendingCount == 0 && File.ReadAllText(original) == "preserve locked Undo contents",
            "Undo retry after unlocking failed or changed contents.");
    }
}
