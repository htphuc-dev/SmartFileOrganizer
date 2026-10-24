using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer.Tests;

internal static class Program
{
    private static readonly FileScanner Scanner = new();
    private static readonly FileOrganizer Organizer = new();

    private static int Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        (string Name, Action<string> Run)[] tests =
        [
            ("Empty folder is a successful empty scan", EmptyFolder),
            ("Missing folder raises an error", MissingFolder),
            ("A file cannot be scanned as a folder", ScanFileAsFolder),
            ("Scan reads metadata, Unicode names and uppercase extensions, without recursion", ScanMetadata),
            ("Existing classification rules are preserved", ClassificationRules),
            ("Move preserves contents and returns full source and destination paths", SuccessfulMove),
            ("File name collisions retain all contents and increment suffixes", FileCollisions),
            ("Directory name collisions increment suffixes", DirectoryCollisions),
            ("Extensionless file collisions are safe", ExtensionlessFile),
            ("File deleted after scanning returns a useful failure", DeletedAfterScan),
            ("Blocked destination preserves source and supports retry", BlockedDestinationAndRetry),
            ("An expected failure does not prevent the next file from moving", ContinueAfterFailure),
            ("Directories cannot be moved as files", RejectDirectorySource),
            ("Invalid source path returns a failure", InvalidSourcePath),
            ("Invalid destination path preserves source", InvalidDestinationPath)
        ];

        if (OperatingSystem.IsWindows())
        {
            tests = [.. tests,
                ("Windows: access-denied scan propagates the error", AccessDeniedScan),
                ("Windows: exclusively locked source is preserved and can be retried", LockedSourceAndRetry)];
        }

        int failures = 0;
        int skipped = 0;
        foreach (var test in tests)
        {
            try
            {
                InTemporaryDirectory(test.Run);
                Console.WriteLine($"PASS {test.Name}");
            }
            catch (SkipCheckException ex)
            {
                skipped++;
                Console.WriteLine($"SKIP {test.Name}: {ex.Message}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"FAIL {test.Name}\n{ex}");
            }
        }

        Console.WriteLine($"{tests.Length - failures - skipped} passed, {failures} failed, {skipped} skipped ({tests.Length} checks).");
        return failures == 0 ? 0 : 1;
    }

    private static void EmptyFolder(string root)
    {
        Check(Scanner.Scan(root).Count == 0, "An empty folder should return an empty list.");
    }

    private static void MissingFolder(string root)
    {
        Expect<DirectoryNotFoundException>(() => Scanner.Scan(Path.Combine(root, "missing")));
    }

    private static void ScanFileAsFolder(string root)
    {
        string path = WriteFile(root, "not-a-folder", "data");
        Expect<IOException>(() => Scanner.Scan(path));
    }

    private static void ScanMetadata(string root)
    {
        string path = WriteFile(root, "Báo cáo tiếng Việt.PDF", "Nội dung tiếng Việt\n");
        DateTime created = new(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        DateTime modified = new(2025, 2, 3, 4, 5, 6, DateTimeKind.Utc);
        File.SetCreationTimeUtc(path, created);
        File.SetLastWriteTimeUtc(path, modified);
        string nested = Directory.CreateDirectory(Path.Combine(root, "nested")).FullName;
        WriteFile(nested, "hidden.txt", "Do not scan subfolders.");

        List<FileItem> files = Scanner.Scan(Path.Combine(root, "."));
        Check(files.Count == 1, "Only the immediate file should be scanned.");
        FileItem file = files[0];
        Check(file.Name == "Báo cáo tiếng Việt.PDF", "Unicode filename changed.");
        Check(file.FullPath == path, "The source path should be absolute and normalized.");
        Check(file.Extension == ".PDF", "The original extension should be retained.");
        Check(file.Category == FileCategory.Documents, "Uppercase PDF should be classified as Documents.");
        Check(file.Size == File.ReadAllBytes(path).LongLength, "File length is incorrect.");
        Check(file.CreatedDate == File.GetCreationTime(path), "Creation time is incorrect.");
        Check(file.ModifiedDate == File.GetLastWriteTime(path), "Modification time is incorrect.");
    }

    private static void ClassificationRules(string root)
    {
        (string Extension, FileCategory Category)[] expected =
        [
            (".jpg", FileCategory.Images), (".jpeg", FileCategory.Images),
            (".png", FileCategory.Images), (".gif", FileCategory.Images),
            (".pdf", FileCategory.Documents), (".doc", FileCategory.Documents),
            (".docx", FileCategory.Documents), (".txt", FileCategory.Documents),
            (".mp4", FileCategory.Videos), (".avi", FileCategory.Videos), (".mkv", FileCategory.Videos),
            (".mp3", FileCategory.Music), (".wav", FileCategory.Music),
            (".zip", FileCategory.Archives), (".rar", FileCategory.Archives), (".7z", FileCategory.Archives),
            (".exe", FileCategory.Applications), ("", FileCategory.Others), (".unknown", FileCategory.Others)
        ];

        foreach (var item in expected)
        {
            Check(FileExtensionHelper.GetCategory(item.Extension) == item.Category,
                $"Classification changed for {item.Extension}.");
            Check(FileExtensionHelper.GetCategory(item.Extension.ToUpperInvariant()) == item.Category,
                $"Uppercase classification changed for {item.Extension}.");
            Check(Organizer.GetDestinationFolder(root, new FileItem { Category = item.Category }) ==
                Path.Combine(root, item.Category.ToString()), "Destination category changed.");
        }
    }

    private static void SuccessfulMove(string root)
    {
        string source = Path.Combine(root, "ảnh.PNG");
        byte[] contents = [0, 1, 127, 128, 255, 10, 13];
        File.WriteAllBytes(source, contents);
        FileItem file = Scanner.Scan(root).Single();
        string folder = Organizer.GetDestinationFolder(root, file);

        FileMoveResult result = Organizer.MoveFile(source, folder);

        Check(result.Success, result.ErrorMessage);
        Check(result.SourcePath == source, "Incorrect source in move result.");
        Check(result.DestinationPath == Path.Combine(root, "Images", "ảnh.PNG"), "Incorrect destination.");
        Check(result.ErrorMessage.Length == 0, "Successful move should have no error.");
        Check(!File.Exists(source), "Successfully moved source should no longer exist.");
        Check(File.ReadAllBytes(result.DestinationPath).SequenceEqual(contents), "Contents changed during move.");
    }

    private static void FileCollisions(string root)
    {
        string source = WriteFile(root, "report.txt", "new report");
        string folder = Directory.CreateDirectory(Path.Combine(root, "Documents")).FullName;
        string original = WriteFile(folder, "report.txt", "original report");
        string first = WriteFile(folder, "report (1).txt", "first report");

        FileMoveResult result = Organizer.MoveFile(source, folder);

        Check(result.Success, result.ErrorMessage);
        Check(result.DestinationPath == Path.Combine(folder, "report (2).txt"), "Expected suffix (2).");
        Check(File.ReadAllText(original) == "original report", "Existing destination was overwritten.");
        Check(File.ReadAllText(first) == "first report", "Existing suffixed file was overwritten.");
        Check(File.ReadAllText(result.DestinationPath) == "new report", "Moved contents changed.");
    }

    private static void DirectoryCollisions(string root)
    {
        string source = WriteFile(root, "report.txt", "new report");
        string folder = Directory.CreateDirectory(Path.Combine(root, "Documents")).FullName;
        string first = Directory.CreateDirectory(Path.Combine(folder, "report.txt")).FullName;
        Directory.CreateDirectory(Path.Combine(folder, "report (1).txt"));
        string sentinel = WriteFile(first, "keep.txt", "keep this directory");

        FileMoveResult result = Organizer.MoveFile(source, folder);

        Check(result.Success, result.ErrorMessage);
        Check(result.DestinationPath == Path.Combine(folder, "report (2).txt"), "Directory collisions were not skipped.");
        Check(File.ReadAllText(sentinel) == "keep this directory", "Colliding directory contents changed.");
        Check(File.ReadAllText(result.DestinationPath) == "new report", "Moved contents changed.");
    }

    private static void ExtensionlessFile(string root)
    {
        string source = WriteFile(root, "README", "no extension");
        FileItem file = Scanner.Scan(root).Single();
        Check(file.Extension == "" && file.Category == FileCategory.Others, "Extensionless classification changed.");
        string folder = Directory.CreateDirectory(Organizer.GetDestinationFolder(root, file)).FullName;
        string existing = WriteFile(folder, "README", "original");
        Directory.CreateDirectory(Path.Combine(folder, "README (1)"));

        FileMoveResult result = Organizer.MoveFile(source, folder);

        Check(result.Success, result.ErrorMessage);
        Check(result.DestinationPath == Path.Combine(folder, "README (2)"), "Incorrect extensionless suffix.");
        Check(File.ReadAllText(existing) == "original", "Existing extensionless file changed.");
        Check(File.ReadAllText(result.DestinationPath) == "no extension", "Moved contents changed.");
    }

    private static void DeletedAfterScan(string root)
    {
        string source = WriteFile(root, "deleted.txt", "data");
        FileItem file = Scanner.Scan(root).Single();
        File.Delete(source);

        FileMoveResult result = Organizer.MoveFile(file.FullPath, Organizer.GetDestinationFolder(root, file));

        CheckFailure(result);
        Check(result.SourcePath == source, "Failure should identify the source.");
        Check(result.ErrorMessage.Contains("no longer exists"), "Missing-file reason is not clear.");
        Check(!File.Exists(result.DestinationPath), "Deleted file was incorrectly created at the destination.");
    }

    private static void BlockedDestinationAndRetry(string root)
    {
        string source = WriteFile(root, "retry.txt", "Nội dung phải được giữ nguyên");
        byte[] original = File.ReadAllBytes(source);
        FileItem file = Scanner.Scan(root).Single();
        string folder = Organizer.GetDestinationFolder(root, file);
        File.WriteAllText(folder, "blocking file");

        FileMoveResult failed = Organizer.MoveFile(file.FullPath, folder);

        CheckFailure(failed);
        Check(File.ReadAllBytes(source).SequenceEqual(original), "Failed move modified or deleted the source.");
        Check(File.ReadAllText(folder) == "blocking file", "Blocking file was modified.");

        File.Delete(folder);
        FileMoveResult retried = Organizer.MoveFile(file.FullPath, folder);
        Check(retried.Success, retried.ErrorMessage);
        Check(File.ReadAllBytes(retried.DestinationPath).SequenceEqual(original), "Retry changed contents.");
        Check(!File.Exists(source), "Retry should remove the successfully moved source.");
    }

    private static void ContinueAfterFailure(string root)
    {
        string deleted = WriteFile(root, "deleted.txt", "deleted");
        string source = WriteFile(root, "keep.txt", "keep");
        FileItem[] scanned = Scanner.Scan(root).OrderBy(file => file.Name).ToArray();
        File.Delete(deleted);

        FileMoveResult[] results = scanned.Select(file =>
            Organizer.MoveFile(file.FullPath, Organizer.GetDestinationFolder(root, file))).ToArray();

        Check(results.Length == 2 && !results[0].Success && results[1].Success,
            "A missing source should not prevent the following source from moving.");
        Check(File.ReadAllText(results[1].DestinationPath) == "keep" && !File.Exists(source),
            "The following file was not moved safely.");
    }

    private static void RejectDirectorySource(string root)
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "source-directory")).FullName;
        string sentinel = WriteFile(source, "keep.txt", "keep");

        FileMoveResult result = Organizer.MoveFile(source, Path.Combine(root, "destination"));

        CheckFailure(result);
        Check(result.ErrorMessage.Contains("directory"), "Directory-source error should identify the cause.");
        Check(Directory.Exists(source) && File.ReadAllText(sentinel) == "keep", "Directory source was changed.");
    }

    private static void InvalidSourcePath(string root)
    {
        CheckFailure(Organizer.MoveFile("invalid\0.txt", Path.Combine(root, "Documents")));
    }

    private static void InvalidDestinationPath(string root)
    {
        string source = WriteFile(root, "source.txt", "unchanged");
        CheckFailure(Organizer.MoveFile(source, "invalid\0folder"));
        Check(File.ReadAllText(source) == "unchanged", "Invalid destination changed the source.");
    }

    private static void AccessDeniedScan(string root)
    {
        string folder = Directory.CreateDirectory(Path.Combine(root, "denied")).FullName;
        WriteFile(folder, "private.txt", "data");
        DirectoryInfo directory = new(folder);
        DirectorySecurity original = directory.GetAccessControl();
        DirectorySecurity denied = directory.GetAccessControl();
        using WindowsIdentity identity = WindowsIdentity.GetCurrent();
        denied.AddAccessRule(new FileSystemAccessRule(identity.User!, FileSystemRights.ListDirectory,
            AccessControlType.Deny));

        bool permissionsChanged = false;
        try
        {
            try
            {
                directory.SetAccessControl(denied);
                permissionsChanged = true;
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException
                or System.Security.SecurityException or PlatformNotSupportedException)
            {
                throw new SkipCheckException($"Cannot configure temporary folder ACL: {ex.Message}");
            }

            Expect<UnauthorizedAccessException>(() => Scanner.Scan(folder));
        }
        finally
        {
            if (permissionsChanged)
            {
                directory.SetAccessControl(original);
            }
        }

        Check(Scanner.Scan(folder).Count == 1, "Restoring access should allow a successful scan.");
    }

    private static void LockedSourceAndRetry(string root)
    {
        string source = WriteFile(root, "locked.txt", "locked contents");
        FileItem file = Scanner.Scan(root).Single();
        string folder = Organizer.GetDestinationFolder(root, file);
        byte[] original = File.ReadAllBytes(source);

        using (FileStream locked = new(source, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            FileMoveResult failed = Organizer.MoveFile(file.FullPath, folder);
            CheckFailure(failed);
            Check(locked.Length == original.Length, "Failed move changed the locked source length.");
            byte[] actual = new byte[original.Length];
            locked.ReadExactly(actual);
            Check(actual.SequenceEqual(original), "Failed move changed the locked source contents.");
            Check(!File.Exists(failed.DestinationPath), "A failed move created a destination file.");
        }

        Check(File.ReadAllBytes(source).SequenceEqual(original), "Source was not preserved after unlocking.");
        FileMoveResult retried = Organizer.MoveFile(file.FullPath, folder);
        Check(retried.Success, retried.ErrorMessage);
        Check(File.ReadAllBytes(retried.DestinationPath).SequenceEqual(original), "Retry changed locked file contents.");
    }

    private static string WriteFile(string folder, string name, string contents)
    {
        string path = Path.Combine(folder, name);
        File.WriteAllText(path, contents);
        return path;
    }

    private static void CheckFailure(FileMoveResult result)
    {
        Check(!result.Success, "The operation should have failed.");
        Check(!string.IsNullOrWhiteSpace(result.ErrorMessage), "Failure should include a useful error.");
    }

    private static void Expect<T>(Action action) where T : Exception
    {
        try
        {
            action();
        }
        catch (T)
        {
            return;
        }

        throw new InvalidOperationException($"Expected {typeof(T).Name}, but no exception was raised.");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static void InTemporaryDirectory(Action<string> test)
    {
        string testBase = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "SmartFileOrganizer.Stage1Tests"));
        string root = Path.GetFullPath(Path.Combine(testBase, Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(root);

        try
        {
            test(root);
        }
        finally
        {
            // Only delete this run's generated directory under the named temp root.
            if (!root.StartsWith(testBase + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Refusing to clean up outside the test directory.");
            }

            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class SkipCheckException(string message) : Exception(message);
}
