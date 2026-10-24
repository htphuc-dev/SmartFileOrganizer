using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    private static readonly FileUndoService UndoService = new();

    private static void UndoAllFiles(string root)
    {
        var contents = Enumerable.Range(0, 1080).ToDictionary(index => $"file-{index:D4}.txt", index => $"contents {index}");
        foreach (var file in contents) WriteFile(root, file.Key, file.Value);
        FileOrganizationResult moved = Organizer.OrganizeFiles(root, Scanner.Scan(root));
        FileUndoHistory history = new();
        history.RecordOrganization(moved);
        Check(moved.SuccessCount == 1080 && Scanner.Scan(root).Count == 0 && history.PendingCount == 1080,
            "Organizing every file must retain an Undo transaction independent of the empty scan.");

        FileMoveResult[] snapshot = history.GetSnapshot();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot);
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.SuccessCount == 1080 && restored.FailedCount == 0 && history.PendingCount == 0,
            "A full Undo should clear the transaction.");
        foreach (var file in contents)
            Check(File.ReadAllText(Path.Combine(root, file.Key)) == file.Value, "Undo changed the file name or contents.");
    }

    private static void UndoActualPaths(string root)
    {
        string category = Directory.CreateDirectory(Path.Combine(root, "Documents")).FullName;
        WriteFile(category, "Báo cáo.TXT", "existing categorized file");
        string document = WriteFile(root, "Báo cáo.TXT", "original Unicode contents");
        string extensionless = WriteFile(root, "LICENSE", "license contents");
        FileOrganizationResult moved = Organizer.OrganizeFiles(root, Scanner.Scan(root));
        Check(moved.FileResults.Any(file => file.DestinationPath.EndsWith("Báo cáo (1).TXT")),
            "The transaction must record the collision-resolved path.");
        FileOrganizationResult restored = UndoService.UndoFiles(moved.FileResults);
        Check(restored.SuccessCount == 2 && File.ReadAllText(document) == "original Unicode contents" &&
            File.ReadAllText(extensionless) == "license contents", "Undo did not restore the recorded paths and contents.");
        Check(File.ReadAllText(Path.Combine(category, "Báo cáo.TXT")) == "existing categorized file",
            "Undo touched a pre-existing category file.");
    }

    private static void UndoHistoryReplacement(string root)
    {
        string first = WriteFile(root, "first.txt", "first transaction");
        FileUndoHistory history = new();
        FileOrganizationResult original = Organizer.OrganizeFiles(root, Scanner.Scan(root));
        history.RecordOrganization(original);
        FileMoveResult record = history.GetSnapshot().Single();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Array.Empty<FileItem>()));
        history.RecordOrganization(Organizer.OrganizeFiles(root,
            [new FileItem { FullPath = Path.Combine(root, "missing.txt"), Category = FileCategory.Documents }]));
        using CancellationTokenSource source = new();
        source.Cancel();
        history.RecordOrganization(Organizer.OrganizeFiles(root,
            [new FileItem { FullPath = first }], source.Token));
        Check(history.PendingCount == 1 && ReferenceEquals(history.GetSnapshot()[0], record),
            "Empty, wholly failed and pre-cancelled runs must preserve the previous transaction.");

        string latest = WriteFile(root, "latest.txt", "new transaction");
        FileOrganizationResult next = Organizer.OrganizeFiles(root, Scanner.Scan(root));
        history.RecordOrganization(next);
        history.ApplyUndoResults(original.FileResults, new FileOrganizationResult());
        Check(history.PendingCount == 1 && history.GetSnapshot()[0].SourcePath == latest,
            "New successes must replace, rather than append to, old history.");
        FileOrganizationResult restored = UndoService.UndoFiles(history.GetSnapshot());
        Check(restored.SuccessCount == 1 && File.ReadAllText(latest) == "new transaction" && !File.Exists(first),
            "Undo should restore only the most recent successful transaction.");
    }

    private static void UndoCancelledOrganization(string root)
    {
        WriteFile(root, "a.txt", "a");
        WriteFile(root, "b.txt", "b");
        using CancellationTokenSource source = new();
        FileOrganizationResult moved = Organizer.OrganizeFiles(root, Scanner.Scan(root), source.Token,
            new InlineProgress<FileOrganizationProgress>(_ => source.Cancel()));
        FileUndoHistory history = new();
        history.RecordOrganization(moved);
        Check(moved.Cancelled && moved.SuccessCount == 1 && history.PendingCount == 1,
            "Cancellation lost the completed move.");
        Check(UndoService.UndoFiles(history.GetSnapshot()).SuccessCount == 1 && Scanner.Scan(root).Count == 2,
            "Undo could not restore a partially cancelled organization.");
    }

    private static void UndoOrganizationError(string root)
    {
        WriteFile(root, "a.txt", "a");
        WriteFile(root, "b.txt", "b");
        FileOrganizationResult moved = Organizer.OrganizeFiles(root, Scanner.Scan(root),
            progress: new InlineProgress<FileOrganizationProgress>(_ => throw new InvalidOperationException("report failed")));
        FileUndoHistory history = new();
        history.RecordOrganization(moved);
        Check(moved.Error != null && history.PendingCount == 1 && UndoService.UndoFiles(history.GetSnapshot()).SuccessCount == 1,
            "An unexpected batch/report failure lost its successful move.");
    }

    private static void UndoPartialFailure(string root)
    {
        string conflict = WriteFile(root, "a.txt", "original a");
        string other = WriteFile(root, "b.txt", "original b");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root).OrderBy(file => file.Name).ToArray()));
        File.WriteAllText(conflict, "new unrelated file");
        FileMoveResult[] snapshot = history.GetSnapshot();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot);
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.SuccessCount == 1 && restored.FailedCount == 1 && history.PendingCount == 1 &&
            File.ReadAllText(conflict) == "new unrelated file" && File.ReadAllText(other) == "original b",
            "A conflict must not overwrite either file or prevent the other restore.");
        Check(File.ReadAllText(history.GetSnapshot()[0].DestinationPath) == "original a", "The pending original file was lost.");
        File.Delete(conflict);
        snapshot = history.GetSnapshot();
        history.ApplyUndoResults(snapshot, UndoService.UndoFiles(snapshot));
        Check(history.PendingCount == 0 && File.ReadAllText(conflict) == "original a" &&
            File.ReadAllText(other) == "original b", "Retry did not restore only the remaining file.");
    }

    private static void UndoMissingAndDirectory(string root)
    {
        string directoryConflict = WriteFile(root, "a.txt", "a");
        WriteFile(root, "b.txt", "b");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root).OrderBy(file => file.Name).ToArray()));
        Directory.CreateDirectory(directoryConflict);
        FileMoveResult[] snapshot = history.GetSnapshot();
        File.Delete(snapshot[1].DestinationPath);
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot);
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.FailedCount == 2 && history.PendingCount == 2 && Directory.Exists(directoryConflict) &&
            File.Exists(snapshot[0].DestinationPath) && !File.Exists(snapshot[1].SourcePath),
            "Missing files and occupied original directories must remain pending, without fabricated restores.");
    }

    private static void UndoChangedFile(string root)
    {
        string original = WriteFile(root, "changed.txt", "original");
        FileMoveResult moved = Organizer.OrganizeFiles(root, Scanner.Scan(root)).FileResults.Single();
        File.WriteAllText(moved.DestinationPath, "replacement with a different length");
        FileOrganizationResult result = UndoService.UndoFiles([moved]);
        Check(result.FailedCount == 1 && !File.Exists(original) &&
            File.ReadAllText(moved.DestinationPath) == "replacement with a different length",
            "A changed categorized file must not be restored.");
        File.WriteAllText(moved.DestinationPath, "original");
        File.SetLastWriteTimeUtc(moved.DestinationPath, moved.LastWriteTimeUtc!.Value.AddMinutes(1));
        Check(UndoService.UndoFiles([moved]).FailedCount == 1 && !File.Exists(original),
            "A same-size file with a changed timestamp must not be restored.");
    }

    private static void UndoInvalidRecord(string root)
    {
        string source = WriteFile(root, "untouched.txt", "untouched");
        string destination = Path.Combine(root, "restore.txt");
        FileInfo info = new(source);
        FileMoveResult[] invalid =
        [
            new() { SourcePath = destination, DestinationPath = source },
            new() { SourcePath = destination, DestinationPath = source, Success = true },
            new() { SourcePath = "relative.txt", DestinationPath = source, Success = true,
                FileLength = info.Length, LastWriteTimeUtc = info.LastWriteTimeUtc },
            new() { SourcePath = source, DestinationPath = source, Success = true,
                FileLength = info.Length, LastWriteTimeUtc = info.LastWriteTimeUtc },
            new() { SourcePath = destination, DestinationPath = root, Success = true,
                FileLength = 0, LastWriteTimeUtc = info.LastWriteTimeUtc }
        ];
        Check(UndoService.UndoFiles(invalid).FailedCount == invalid.Length && !File.Exists(destination) &&
            File.ReadAllText(source) == "untouched", "Invalid records moved an unrelated file.");
    }

    private static void UndoPreCancelled(string root)
    {
        WriteFile(root, "a.txt", "a");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root)));
        FileMoveResult[] snapshot = history.GetSnapshot();
        using CancellationTokenSource source = new();
        source.Cancel();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot, source.Token);
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.Cancelled && restored.ProcessedCount == 0 && history.PendingCount == 1 &&
            File.Exists(snapshot[0].DestinationPath) && !File.Exists(snapshot[0].SourcePath),
            "A pre-cancelled Undo changed files or discarded history.");
    }

    private static void UndoCancellationAndRetry(string root)
    {
        foreach (string name in new[] { "a.txt", "b.txt", "c.txt" }) WriteFile(root, name, name);
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root)));
        FileMoveResult[] snapshot = history.GetSnapshot();
        using CancellationTokenSource source = new();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot, source.Token,
            new InlineProgress<FileOrganizationProgress>(_ => source.Cancel()));
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.Cancelled && restored.SuccessCount == 1 && history.PendingCount == 2,
            "A cancelled Undo did not retain precisely its unrestored files.");
        File.WriteAllText(snapshot[0].SourcePath, "already restored; do not retry");
        snapshot = history.GetSnapshot();
        FileOrganizationResult retried = UndoService.UndoFiles(snapshot);
        history.ApplyUndoResults(snapshot, retried);
        Check(retried.SuccessCount == 2 && history.PendingCount == 0 && Scanner.Scan(root).Count == 3 &&
            Directory.GetFiles(root).Any(path => File.ReadAllText(path) == "already restored; do not retry"),
            "Resuming Undo retried a completed restore or lost a pending file.");
    }

    private static void UndoProgressFailure(string root)
    {
        WriteFile(root, "a.txt", "a");
        WriteFile(root, "b.txt", "b");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root)));
        FileMoveResult[] snapshot = history.GetSnapshot();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot,
            progress: new InlineProgress<FileOrganizationProgress>(_ => throw new InvalidOperationException("report failed")));
        history.ApplyUndoResults(snapshot, restored);
        Check(restored.Error != null && restored.SuccessCount == 1 && history.PendingCount == 1 &&
            File.Exists(snapshot[0].SourcePath), "A report failure lost the completed restore journal.");
        snapshot = history.GetSnapshot();
        history.ApplyUndoResults(snapshot, UndoService.UndoFiles(snapshot));
        Check(history.PendingCount == 0 && Scanner.Scan(root).Count == 2, "Retry after a report failure did not finish Undo.");
    }

    private static void UndoLateCancellation(string root)
    {
        WriteFile(root, "last.txt", "last");
        FileUndoHistory history = new();
        history.RecordOrganization(Organizer.OrganizeFiles(root, Scanner.Scan(root)));
        FileMoveResult[] snapshot = history.GetSnapshot();
        using CancellationTokenSource source = new();
        FileOrganizationResult restored = UndoService.UndoFiles(snapshot, source.Token,
            new InlineProgress<FileOrganizationProgress>(_ => source.Cancel()));
        history.ApplyUndoResults(snapshot, restored);
        Check(!restored.Cancelled && restored.SuccessCount == 1 && history.PendingCount == 0 &&
            File.ReadAllText(snapshot[0].SourcePath) == "last", "Late cancellation discarded a completed Undo.");
    }
}
