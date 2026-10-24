using System.Globalization;
using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    private static void PartialSelection(string root)
    {
        WriteFile(root, "a.txt", "selected document");
        string untouched = WriteFile(root, "b.PNG", "unselected image");
        WriteFile(root, "c.zip", "selected archive");
        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        Check(files.All(file => file.IsSelected), "A successful scan should select every file by default.");
        files[1].IsSelected = false;
        var summary = FileSelectionHelper.GetSummary(files);
        Check(summary.TotalCount == 3 && summary.SelectedCount == 2 &&
            summary.SelectedSize == files[0].Size + files[2].Size, "Partial selection totals are incorrect.");

        FileOrganizationResult result = Organizer.OrganizeFiles(root, FileSelectionHelper.GetSelectedFiles(files));

        Check(result.TotalCount == 2 && result.SuccessCount == 2 && result.Error == null,
            "Only the chosen document and archive should be processed.");
        Check(File.ReadAllText(untouched) == "unselected image" && !Directory.Exists(Path.Combine(root, "Images")),
            "An unselected file was moved or modified.");
        Check(File.ReadAllText(Path.Combine(root, "Documents", "a.txt")) == "selected document" &&
            File.ReadAllText(Path.Combine(root, "Archives", "c.zip")) == "selected archive", "Selected contents changed.");
        FileItem[] remaining = RemainingFiles(files, result);
        Check(remaining.Length == 1 && ReferenceEquals(remaining[0], files[1]) && !remaining[0].IsSelected,
            "Unselected files and their choices must remain in the pending list.");
    }

    private static void EmptySelection(string root)
    {
        string path = WriteFile(root, "keep.txt", "unchanged");
        FileItem[] files = Scanner.Scan(root).ToArray();
        FileSelectionHelper.SetAll(files, false);
        FileItem[] selected = FileSelectionHelper.GetSelectedFiles(files);
        Check(selected.Length == 0 && FileSelectionHelper.GetSummary(files).SelectedSize == 0,
            "Clearing the selection should produce an empty work snapshot.");
        int reports = 0;

        FileOrganizationResult result = Organizer.OrganizeFiles(root, selected,
            progress: new InlineProgress<FileOrganizationProgress>(_ => reports++));

        Check(result.TotalCount == 0 && result.ProcessedCount == 0 && result.Error == null && reports == 0,
            "No selected files must mean no processed files or progress.");
        Check(File.ReadAllText(path) == "unchanged" && Directory.GetFileSystemEntries(root).Length == 1,
            "An empty selection changed the filesystem.");
    }

    private static void SelectionSummary(string root)
    {
        FileItem empty = new() { Name = "empty", Size = 0 };
        FileItem small = new() { Name = "small", Size = 513, IsSelected = false };
        FileItem large = new() { Name = "large", Size = 3L * 1024 * 1024 * 1024 + 17 };
        FileItem[] files = [empty, small, large];
        var initial = FileSelectionHelper.GetSummary(files);
        Check(initial.TotalCount == 3 && initial.SelectedCount == 2 && initial.SelectedSize == large.Size,
            "Selected totals must include empty files and retain 64-bit byte values.");
        Check(FileSelectionHelper.GetSelectedFiles(files.Reverse()).SequenceEqual(new[] { large, empty }),
            "Selection must follow file identity, not display position.");
        FileSelectionHelper.SetAll(files, true);
        var all = FileSelectionHelper.GetSummary(files);
        Check(all.SelectedCount == 3 && all.SelectedSize == large.Size + small.Size, "Select All totals are incorrect.");
        FileSelectionHelper.SetAll(files, false);
        var none = FileSelectionHelper.GetSummary(files);
        Check(none.TotalCount == 3 && none.SelectedCount == 0 && none.SelectedSize == 0, "Clear Selection totals are incorrect.");
        small.IsSelected = true;
        Check(FileSelectionHelper.GetSummary(files).SelectedCount == 1 &&
            FileSelectionHelper.GetSummary(files).SelectedSize == 513, "Individual changes should update totals.");
    }

    private static void ReadableSizes(string root)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            Check(FileSizeFormatter.Format(0) == "0 B", "Empty files must display 0 B.");
            Check(FileSizeFormatter.Format(1023) == "1023 B", "Byte size is incorrect.");
            Check(FileSizeFormatter.Format(1024) == "1 KB", "KB size is incorrect.");
            Check(FileSizeFormatter.Format(1536) == "1.5 KB", "Fractional KB size is incorrect.");
            Check(FileSizeFormatter.Format(3 * 1024 * 1024 / 2) == "1.5 MB", "MB size is incorrect.");
            FileItem file = new() { Size = 3L * 1024 * 1024 * 1024 };
            Check(FileSizeFormatter.Format(file.Size) == "3 GB" && file.Size == 3221225472,
                "Formatting must not replace the original byte value.");
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    private static void ReadOnlyPreview(string root)
    {
        string source = WriteFile(root, "Báo cáo.PDF", "preview must not move this file");
        FileItem file = Scanner.Scan(root).Single();
        string[] before = Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories);

        string preview = Organizer.GetPreviewDestinationPath(root, file);

        Check(preview == Path.Combine(root, "Documents", "Báo cáo.PDF"), "Preview should include category and filename.");
        Check(before.SequenceEqual(Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories)),
            "Preview must not create folders or files.");
        Check(File.ReadAllText(source) == "preview must not move this file", "Preview modified or moved the source.");
    }

    private static void PreviewCollisions(string root)
    {
        WriteFile(root, "report.txt", "new contents");
        WriteFile(root, "README", "extensionless source");
        FileItem[] files = Scanner.Scan(root).ToArray();
        string documents = Directory.CreateDirectory(Path.Combine(root, "Documents")).FullName;
        string existing = WriteFile(documents, "report.txt", "old contents");
        Directory.CreateDirectory(Path.Combine(documents, "report (1).txt"));
        string others = Directory.CreateDirectory(Path.Combine(root, "Others")).FullName;
        Directory.CreateDirectory(Path.Combine(others, "README"));
        string extensionless = WriteFile(others, "README (1)", "keep extensionless destination");
        string[] before = Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories).Order().ToArray();

        foreach (FileItem file in files)
        {
            string preview = Organizer.GetPreviewDestinationPath(root, file);
            string expected = file.Extension == "" ? Path.Combine(others, "README (2)")
                : Path.Combine(documents, "report (2).txt");
            Check(preview == expected, "Preview should reuse the normal suffix rules for both file and directory collisions.");
        }

        Check(before.SequenceEqual(Directory.GetFileSystemEntries(root, "*", SearchOption.AllDirectories).Order()),
            "Preview changed the directory tree.");
        Check(File.ReadAllText(existing) == "old contents" && File.ReadAllText(extensionless) == "keep extensionless destination",
            "Preview changed an existing destination.");
    }

    private static void DestinationAppearsAfterPreview(string root)
    {
        string source = WriteFile(root, "report.txt", "new source contents");
        FileItem file = Scanner.Scan(root).Single();
        string preview = Organizer.GetPreviewDestinationPath(root, file);
        string folder = Directory.CreateDirectory(Path.GetDirectoryName(preview)!).FullName;
        File.WriteAllText(preview, "created after preview");
        Directory.CreateDirectory(Path.Combine(folder, "report (1).txt"));

        FileOrganizationResult result = Organizer.OrganizeFiles(root, FileSelectionHelper.GetSelectedFiles(new[] { file }));

        Check(result.SuccessCount == 1 && result.FileResults[0].DestinationPath == Path.Combine(folder, "report (2).txt"),
            "Move must recheck names rather than use the stale preview.");
        Check(File.ReadAllText(preview) == "created after preview", "A destination created after preview was overwritten.");
        Check(!File.Exists(source) && File.ReadAllText(result.FileResults[0].DestinationPath) == "new source contents",
            "Rechecking the destination changed source contents.");
    }

    private static void CancelSelectedAndResume(string root)
    {
        foreach (string name in new[] { "a.txt", "b.txt", "c.txt", "d.txt" })
        {
            WriteFile(root, name, $"contents {name}");
        }

        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        files[3].IsSelected = false;
        FileItem[] snapshot = FileSelectionHelper.GetSelectedFiles(files);
        using CancellationTokenSource source = new();
        FileOrganizationResult cancelled = Organizer.OrganizeFiles(root, snapshot, source.Token,
            new InlineProgress<FileOrganizationProgress>(_ => source.Cancel()));

        Check(cancelled.Cancelled && cancelled.TotalCount == 3 && cancelled.SuccessCount == 1,
            "Cancellation progress should count only selected files.");
        FileItem[] remaining = RemainingFiles(files, cancelled);
        Check(remaining.SequenceEqual(files.Skip(1)) && remaining[0].IsSelected && remaining[1].IsSelected && !remaining[2].IsSelected,
            "Cancellation should retain choices of failed/unprocessed and unselected files.");
        Check(FileSelectionHelper.GetSummary(remaining).SelectedSize == files[1].Size + files[2].Size,
            "Post-cancellation selected size is incorrect.");
        remaining[0].IsSelected = false;
        remaining[2].IsSelected = true;

        FileOrganizationResult resumed = Organizer.OrganizeFiles(root, FileSelectionHelper.GetSelectedFiles(remaining));

        Check(resumed.TotalCount == 2 && resumed.SuccessCount == 2 && resumed.FileResults.All(item =>
            item.SourcePath != files[0].FullPath && item.SourcePath != files[1].FullPath),
            "Resume must use current choices and exclude the moved source and newly unchecked source.");
        Check(File.ReadAllText(files[1].FullPath) == "contents b.txt", "Newly unselected source changed.");
        Check(File.ReadAllText(Path.Combine(root, "Documents", "a.txt")) == "contents a.txt" &&
            !File.Exists(Path.Combine(root, "Documents", "a (1).txt")), "Resume processed the earlier success again.");
        FileItem[] pending = RemainingFiles(remaining, resumed);
        Check(pending.Length == 1 && !pending[0].IsSelected, "Only the unselected source should remain pending.");
    }

    private static void RetrySelectedFailures(string root)
    {
        string failed = WriteFile(root, "a.txt", "restore after failure");
        WriteFile(root, "b.txt", "first success");
        string unselected = WriteFile(root, "c.PNG", "initially unselected");
        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        files[2].IsSelected = false;
        File.Delete(failed);
        FileOrganizationResult first = Organizer.OrganizeFiles(root, FileSelectionHelper.GetSelectedFiles(files));
        Check(first.FailedCount == 1 && first.SuccessCount == 1, "Expected one failure and one success.");
        FileItem[] remaining = RemainingFiles(files, first);
        Check(remaining.Length == 2 && remaining[0].IsSelected && !remaining[1].IsSelected,
            "A failure must retain its selection while unselected files stay unselected.");

        File.WriteAllText(failed, "restored contents");
        remaining[0].IsSelected = false;
        remaining[1].IsSelected = true;
        FileOrganizationResult retry = Organizer.OrganizeFiles(root, FileSelectionHelper.GetSelectedFiles(remaining));
        Check(retry.TotalCount == 1 && retry.SuccessCount == 1 && retry.FileResults[0].SourcePath == unselected,
            "Retry should honor the current selection, including an unchecked prior failure.");
        Check(File.ReadAllText(failed) == "restored contents" &&
            File.ReadAllText(retry.FileResults[0].DestinationPath) == "initially unselected", "Retry changed contents.");
        Check(File.ReadAllText(Path.Combine(root, "Documents", "b.txt")) == "first success" &&
            !File.Exists(Path.Combine(root, "Documents", "b (1).txt")), "Retry processed a previous success again.");
    }
}
