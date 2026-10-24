using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    private static void PreCancelledScan(string root)
    {
        using CancellationTokenSource source = new();
        source.Cancel();
        int reports = 0;
        Expect<OperationCanceledException>(() => Scanner.Scan(Path.Combine(root, "missing"), source.Token,
            new InlineProgress<int>(_ => reports++)));
        Check(reports == 0, "A pre-cancelled scan should not report any files.");
    }

    private static void CancelDuringScan(string root)
    {
        for (int index = 0; index < 3; index++)
        {
            WriteFile(root, $"file-{index}.txt", $"contents-{index}");
        }

        using CancellationTokenSource source = new();
        List<FileItem>? published = null;
        int readCount = 0;
        IProgress<int> progress = new InlineProgress<int>(count =>
        {
            readCount = count;
            source.Cancel();
        });

        Expect<OperationCanceledException>(() => published = Scanner.Scan(root, source.Token, progress));
        Check(readCount == 1, "Cancellation should be deterministic at the first metadata report.");
        Check(published == null, "A cancelled scan must not return partial results.");
        Check(Scanner.Scan(root).Count == 3, "A fresh scan should still read all files.");
        for (int index = 0; index < 3; index++)
        {
            Check(File.ReadAllText(Path.Combine(root, $"file-{index}.txt")) == $"contents-{index}",
                "Scanning modified source contents.");
        }
    }

    private static void PreCancelledOrganization(string root)
    {
        string path = WriteFile(root, "keep.txt", "unchanged");
        FileItem[] files = Scanner.Scan(root).ToArray();
        using CancellationTokenSource source = new();
        source.Cancel();

        FileOrganizationResult result = Organizer.OrganizeFiles(root, files, source.Token);

        Check(result.Cancelled && result.Error == null, "Pre-cancellation should be reported as cancellation.");
        Check(result.TotalCount == 1 && result.ProcessedCount == 0 && result.SuccessCount == 0 && result.FailedCount == 0,
            "A pre-cancelled batch must have no processed results.");
        Check(File.ReadAllText(path) == "unchanged", "Pre-cancellation modified the source.");
        Check(!Directory.Exists(Path.Combine(root, "Documents")), "A pre-cancelled batch should not create destinations.");
    }

    private static void CancelOrganizationAndResume(string root)
    {
        string[] names = ["a.txt", "b.txt", "c.txt"];
        foreach (string name in names)
        {
            WriteFile(root, name, $"new contents: {name}");
        }

        string destination = Directory.CreateDirectory(Path.Combine(root, "Documents")).FullName;
        string existing = WriteFile(destination, "a.txt", "old destination");
        string collision = Directory.CreateDirectory(Path.Combine(destination, "a (1).txt")).FullName;
        string sentinel = WriteFile(collision, "keep", "old directory contents");
        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        using CancellationTokenSource source = new();
        IProgress<FileOrganizationProgress> progress = new InlineProgress<FileOrganizationProgress>(update =>
        {
            Check(update.ProcessedCount == 1 && update.SuccessCount == 1 && update.FailedCount == 0,
                "First move progress counts are incorrect.");
            source.Cancel();
        });

        FileOrganizationResult cancelled = Organizer.OrganizeFiles(root, files, source.Token, progress);

        Check(cancelled.Cancelled && cancelled.Error == null, "The batch should stop at the cancellation boundary.");
        Check(cancelled.TotalCount == 3 && cancelled.ProcessedCount == 1 && cancelled.SuccessCount == 1,
            "Completed move must remain recorded after cancellation.");
        Check(cancelled.ProcessedCount < cancelled.TotalCount, "An interrupted batch must not count as 100% processed.");
        string moved = cancelled.FileResults.Single().DestinationPath;
        Check(moved == Path.Combine(destination, "a (2).txt"), "Collisions with both file and directory must be skipped.");
        Check(!File.Exists(files[0].FullPath) && File.ReadAllText(moved) == "new contents: a.txt",
            "The completed move was lost or changed.");
        Check(File.ReadAllText(files[1].FullPath) == "new contents: b.txt" &&
            File.ReadAllText(files[2].FullPath) == "new contents: c.txt", "Unprocessed sources changed.");

        FileItem[] remaining = RemainingFiles(files, cancelled);
        FileOrganizationResult resumed = Organizer.OrganizeFiles(root, remaining);
        Check(remaining.Length == 2 && resumed.ProcessedCount == 2 && resumed.SuccessCount == 2 && resumed.Error == null,
            "Resume should move only the two remaining files.");
        Check(!resumed.FileResults.Any(item => item.SourcePath == files[0].FullPath), "Resume retried an already moved source.");
        Check(File.ReadAllText(existing) == "old destination" && File.ReadAllText(sentinel) == "old directory contents",
            "An existing destination was changed during cancellation or resume.");
        Check(File.ReadAllText(moved) == "new contents: a.txt" && !File.Exists(Path.Combine(destination, "a (3).txt")),
            "The completed move was processed again.");
        foreach (string name in names.Skip(1))
        {
            Check(File.ReadAllText(Path.Combine(destination, name)) == $"new contents: {name}", "Resume changed contents.");
        }
    }

    private static void BatchContinuesAfterFailures(string root)
    {
        string deleted = WriteFile(root, "a.txt", "deleted after scan");
        string blocked = WriteFile(root, "b.txt", "preserve blocked source");
        WriteFile(root, "c.PNG", "image contents");
        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        File.Delete(deleted);
        string blocker = WriteFile(root, "Documents", "keep blocking file");
        List<FileOrganizationProgress> reports = new();

        FileOrganizationResult result = Organizer.OrganizeFiles(root, files,
            progress: new InlineProgress<FileOrganizationProgress>(reports.Add));

        Check(!result.Cancelled && result.Error == null && result.ProcessedCount == 3 &&
            result.SuccessCount == 1 && result.FailedCount == 2, "Batch should count failures and continue to the image.");
        Check(!result.FileResults[0].Success && !result.FileResults[1].Success && result.FileResults[2].Success,
            "Batch results should retain input order and individual outcomes.");
        Check(result.FileResults.Take(2).All(item => !string.IsNullOrWhiteSpace(item.ErrorMessage)),
            "Batch failures need individual causes.");
        Check(File.ReadAllText(blocked) == "preserve blocked source" && File.ReadAllText(blocker) == "keep blocking file",
            "Failed moves changed source or existing destination contents.");
        Check(File.ReadAllText(result.FileResults[2].DestinationPath) == "image contents", "The following file was not preserved.");
        Check(reports.Sum(report => report.FileResults.Count) == 3 && reports[^1].ProcessedCount == 3 &&
            reports[^1].SuccessCount == 1 && reports[^1].FailedCount == 2, "Progress must count failures as processed.");
        File.Delete(blocker);
        FileOrganizationResult retried = Organizer.OrganizeFiles(root, RemainingFiles(files, result));
        Check(retried.ProcessedCount == 2 && retried.SuccessCount == 1 && retried.FailedCount == 1,
            "Retry should include the failed sources and exclude the moved image.");
        Check(File.ReadAllText(Path.Combine(root, "Documents", "b.txt")) == "preserve blocked source",
            "Retry after resolving a failure changed contents.");
    }

    private static void ProgressFailurePreservesResults(string root)
    {
        WriteFile(root, "a.txt", "first file");
        WriteFile(root, "b.txt", "second file");
        FileItem[] files = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        IProgress<FileOrganizationProgress> brokenProgress = new InlineProgress<FileOrganizationProgress>(_ =>
            throw new InvalidOperationException("Simulated progress delivery failure."));

        FileOrganizationResult result = Organizer.OrganizeFiles(root, files, progress: brokenProgress);

        Check(result.Error is InvalidOperationException && !result.Cancelled, "Unexpected errors should be reported separately.");
        Check(result.ProcessedCount == 1 && result.SuccessCount == 1 && result.FileResults[0].Success,
            "The completed result must survive an exception during progress delivery.");
        Check(File.ReadAllText(result.FileResults[0].DestinationPath) == "first file", "The completed move was lost.");
        Check(File.ReadAllText(files[1].FullPath) == "second file", "The unprocessed source changed.");

        FileOrganizationResult resumed = Organizer.OrganizeFiles(root, RemainingFiles(files, result));
        Check(resumed.ProcessedCount == 1 && resumed.SuccessCount == 1 && resumed.Error == null,
            "Resume after an unexpected error must exclude completed moves.");
        Check(File.ReadAllText(resumed.FileResults[0].DestinationPath) == "second file", "Resume changed contents.");
    }

    private static void CancellationAfterFinalMove(string root)
    {
        string path = WriteFile(root, "last.txt", "completed before cancellation");
        FileItem[] files = Scanner.Scan(root).ToArray();
        using CancellationTokenSource source = new();

        FileOrganizationResult result = Organizer.OrganizeFiles(root, files, source.Token,
            new InlineProgress<FileOrganizationProgress>(_ => source.Cancel()));

        Check(source.IsCancellationRequested && !result.Cancelled && result.Error == null,
            "A batch with every file completed should report completion despite a late cancellation request.");
        Check(result.ProcessedCount == 1 && result.SuccessCount == 1 && !File.Exists(path),
            "Cancellation must not discard the just-completed move.");
        Check(File.ReadAllText(result.FileResults[0].DestinationPath) == "completed before cancellation", "Contents changed.");
    }

    private static FileItem[] RemainingFiles(IEnumerable<FileItem> files, FileOrganizationResult result)
    {
        HashSet<string> moved = result.FileResults.Where(item => item.Success)
            .Select(item => item.SourcePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return files.Where(file => !moved.Contains(file.FullPath)).ToArray();
    }

    // Inline reporting gives tests an exact cancellation/error boundary without sleeps.
    private sealed class InlineProgress<T>(Action<T> report) : IProgress<T>
    {
        public void Report(T value) => report(value);
    }
}
