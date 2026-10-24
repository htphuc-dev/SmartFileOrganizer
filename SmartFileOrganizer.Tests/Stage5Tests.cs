using System.Text.Json;
using System.Text.Json.Nodes;
using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer.Tests;

internal static partial class Program
{
    private static ClassificationSettings CodeSettings(string name = "SourceCode", string folder = "Sources")
    {
        return SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
            [.. ClassificationSettings.Defaults.Categories,
             new FileCategory("source-code", name, folder, ["CS", ".cpp", ".PY", ".cs"])]));
    }

    private static void CustomClassification(string root)
    {
        WriteFile(root, "hello.CS", "source code");
        WriteFile(root, "main.cpp", "C++ code");
        WriteFile(root, "script.py", "Python code");
        WriteFile(root, "unknown.xyz", "unknown");
        WriteFile(root, "README", "no extension");
        string nested = Directory.CreateDirectory(Path.Combine(root, "nested")).FullName;
        WriteFile(nested, "hidden.cs", "not scanned");
        ClassificationSettings settings = CodeSettings();

        List<FileItem> files = Scanner.Scan(root, settings: settings);
        Check(files.Count == 5, "Custom rules must not enable recursive scanning.");
        Check(files.Count(file => file.Category.Id == "source-code") == 3, "SourceCode extensions were not applied.");
        Check(files.Where(file => file.Name is "README" or "unknown.xyz").All(file => file.Category == FileCategory.Others),
            "Unknown and extensionless files must use the fallback.");
        FileItem code = files.Single(file => file.Name == "hello.CS");
        Check(code.Category.Name == "SourceCode" && code.Category.FolderName == "Sources",
            "Display name and destination folder must be independent.");
        string preview = Organizer.GetPreviewDestinationPath(root, code);
        Check(preview == Path.Combine(root, "Sources", "hello.CS") && !Directory.Exists(Path.Combine(root, "Sources")),
            "Custom preview must be read-only and use the configured folder.");
        FileOrganizationResult result = Organizer.OrganizeFiles(root, [code]);
        Check(result.SuccessCount == 1 && result.FileResults[0].DestinationPath == preview &&
            File.ReadAllText(preview) == "source code", "Custom classification did not produce a safe move.");
    }

    private static void RuleExtensions(string root)
    {
        Check(SettingsValidator.ParseExtensions("PDF, pdf; .PDF\r\n .CS ; ;\n .cpp")
            .SequenceEqual(new[] { ".pdf", ".cs", ".cpp" }), "Extension input was not normalized and deduplicated.");
        ClassificationSettings settings = CodeSettings();
        Check(settings.Categories[^1].Extensions.SequenceEqual(new[] { ".cs", ".cpp", ".py" }),
            "JSON-style extension arrays must use the same normalization as Settings input.");
        SettingsValidationException error = RuleFailure(() => SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
            [.. settings.Categories, new FileCategory("conflict", "Overlap", "Overlap", ["PDF"])])));
        Check(error.Message.Contains(".pdf") && error.Message.Contains("Documents") && error.Message.Contains("Overlap") &&
            error.CategoryId == "conflict" && error.Field == "extensions", "Conflict must identify both groups, field and extension.");
    }

    private static void RejectInvalidRuleExtensions(string root)
    {
        foreach (string text in new[] { ".", "..", ".tar.gz", "C:\\pdf", "../pdf", "a/b", "*.txt", "?", "bad name", "a|b", "a:b", "a\0b" })
        {
            RuleFailure(() => SettingsValidator.ParseExtensions(text));
        }

        Check(FileExtensionHelper.GetCategory(Path.GetExtension("archive.tar.gz")) == FileCategory.Others,
            "A compound extension must not gain implicit special rules.");
    }

    private static void RejectInvalidRuleFolders(string root)
    {
        string[] invalid = ["", " ", ".", "..", "../escape", "..\\escape", "C:\\absolute", "/absolute", "C:relative",
            "a/b", "a\\b", "a:b", "a*", "a?", "a|b", "a<b", "a>b", "a\"b", "a\0b", "name.", "name ",
            "CON", "con.txt", "NUL", "AUX", "PRN", "COM1", "COM9.txt", "LPT1", "LPT9", "LPT³", "CONIN$", new string('x', 256)];
        foreach (string name in invalid)
        {
            RuleFailure(() => SettingsValidator.ValidateFolderName(name));
        }

        SettingsValidator.ValidateFolderName("Mã nguồn");
        SettingsValidator.ValidateFolderName(".hidden");
        SettingsValidator.ValidateFolderName("COM10");
        RuleFailure(() => Organizer.GetDestinationFolder(root, new FileItem
        {
            Category = new FileCategory("unsafe", "Unsafe", "..\\escape", [".cs"])
        }));
    }

    private static void RejectConflictingGroups(string root)
    {
        foreach (FileCategory category in new[]
        {
            new FileCategory("empty-name", " ", "EmptyName", []),
            new FileCategory("duplicate-name", "images", "UniqueFolder", []),
            new FileCategory("duplicate-folder", "Unique name", "dOCUMENTS", []),
            new FileCategory("IMAGES", "Unique name", "UniqueFolder", []),
            new FileCategory("", "Unique name", "UniqueFolder", [])
        })
        {
            RuleFailure(() => SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
                [.. ClassificationSettings.Defaults.Categories, category])));
        }
    }

    private static void ProtectFallback(string root)
    {
        RuleFailure(() => SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
            ClassificationSettings.Defaults.Categories.Where(category => category != FileCategory.Others).ToArray())));
        RuleFailure(() => SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
            [new FileCategory("others", "Others", "Others", [".foo"])])));
        ClassificationSettings renamed = SettingsValidator.NormalizeAndValidate(new ClassificationSettings(1,
            [new FileCategory("others", "Catch all", "Unmatched", [])]));
        Check(FileExtensionHelper.GetCategory(".unknown", renamed).FolderName == "Unmatched" &&
            FileExtensionHelper.GetCategory("", renamed).Id == FileCategory.FallbackId,
            "Fallback identity must survive changes to its display or folder name.");
    }

    private static void SettingsRoundTrip(string root)
    {
        string path = Path.Combine(root, "configuration", "settings.json");
        SettingsStore store = new(path);
        ClassificationSettings custom = CodeSettings();
        Check(store.Save(custom), "Adding a group must count as a rule change.");
        SettingsStore reloaded = new(path);
        reloaded.Load();
        Check(reloaded.LoadWarning == null && SettingsValidator.AreEquivalent(custom, reloaded.Current),
            "Saved rules did not round trip.");
        using JsonDocument json = JsonDocument.Parse(File.ReadAllText(path));
        Check(json.RootElement.GetProperty("version").GetInt32() == 1 &&
            json.RootElement.GetProperty("categories").GetArrayLength() == 8, "JSON must contain versioned category data.");
        ClassificationSettings reordered = new(1, custom.Categories.Reverse().Select(category =>
            new FileCategory(category.Id, category.Name, category.FolderName, category.Extensions.Reverse().ToArray())).ToArray());
        Check(!store.Save(reordered), "Saving equivalent rules must not invalidate an existing scan.");
        Check(store.Save(CodeSettings("Renamed code", "NewSources")), "Renaming a group/folder must invalidate the old scan.");
    }

    private static void MissingSettings(string root)
    {
        string path = Path.Combine(root, "missing", "settings.json");
        SettingsStore store = new(path);
        store.Load();
        Check(store.LoadWarning == null && !store.RequiresOverwriteConfirmation &&
            SettingsValidator.AreEquivalent(store.Current, ClassificationSettings.Defaults) && store.Current.Categories.Count == 7,
            "Missing settings must use the exact defaults without a warning.");
        Check(!Directory.Exists(Path.GetDirectoryName(path)), "Loading defaults must not create a configuration directory.");
    }

    private static void InvalidSettingsArePreserved(string root)
    {
        JsonSerializerOptions options = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        string valid = JsonSerializer.Serialize(CodeSettings(), options);
        JsonNode unsupported = JsonNode.Parse(valid)!;
        unsupported["version"] = 99;
        JsonNode unsafeFolder = JsonNode.Parse(valid)!;
        unsafeFolder["categories"]!.AsArray()[^1]!["folderName"] = "..\\escape";
        JsonNode conflict = JsonNode.Parse(valid)!;
        conflict["categories"]!.AsArray()[^1]!["extensions"] = new JsonArray("PDF");
        JsonNode missingFallback = JsonNode.Parse(valid)!;
        missingFallback["categories"]!.AsArray().RemoveAt(6);
        string[] invalid = ["{invalid json", "null", "{}", "{\"version\":1,\"categories\":null}",
            "{\"version\":1,\"categories\":[null]}",
            "{\"version\":1,\"categories\":[{\"id\":\"others\",\"name\":\"Others\",\"folderName\":\"Others\",\"extensions\":null}]}",
            unsupported.ToJsonString(), unsafeFolder.ToJsonString(), conflict.ToJsonString(), missingFallback.ToJsonString()];
        string path = Path.Combine(root, "settings.json");
        foreach (string text in invalid)
        {
            File.WriteAllText(path, text);
            byte[] before = File.ReadAllBytes(path);
            SettingsStore store = new(path);
            store.Load();
            Check(store.RequiresOverwriteConfirmation && store.LoadWarning?.Contains("temporary defaults") == true &&
                SettingsValidator.AreEquivalent(store.Current, ClassificationSettings.Defaults), "Invalid settings must fall back without crashing.");
            Check(File.ReadAllBytes(path).SequenceEqual(before), "Loading invalid settings overwrote the recoverable file.");
        }
    }

    private static void ConfirmInvalidSettingsReplacement(string root)
    {
        string path = WriteFile(root, "settings.json", "invalid JSON to preserve");
        SettingsStore store = new(path);
        store.Load();
        Expect<InvalidOperationException>(() => store.Save(CodeSettings()));
        Check(File.ReadAllText(path) == "invalid JSON to preserve" && store.RequiresOverwriteConfirmation,
            "Save without confirmation must keep the broken configuration.");
        Check(store.Save(CodeSettings(), replaceInvalidFile: true) && !store.RequiresOverwriteConfirmation && store.LoadWarning == null,
            "Explicit Save should replace invalid settings with a complete validated configuration.");
        SettingsStore reloaded = new(path);
        reloaded.Load();
        Check(reloaded.LoadWarning == null && SettingsValidator.AreEquivalent(reloaded.Current, CodeSettings()), "Replacement did not persist.");
    }

    private static void FailedSettingsSave(string root)
    {
        string blocker = WriteFile(root, "blocked", "do not replace");
        SettingsStore store = new(Path.Combine(blocker, "settings.json"));
        ClassificationSettings before = store.Current;
        ExpectSaveFailure(() => store.Save(CodeSettings()));
        Check(ReferenceEquals(before, store.Current) && File.ReadAllText(blocker) == "do not replace",
            "A failed write must preserve active rules and existing filesystem contents.");
        RuleFailure(() => store.Save(new ClassificationSettings(2, ClassificationSettings.Defaults.Categories)));
        Check(ReferenceEquals(before, store.Current), "Validation failure must not publish a draft.");
    }

    private static void AtomicSettingsReplacement(string root)
    {
        string path = Path.Combine(root, "settings.json");
        SettingsStore store = new(path);
        store.Save(CodeSettings());
        byte[] before = File.ReadAllBytes(path);
        ClassificationSettings active = store.Current;
        using (FileStream locked = new(path, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            ExpectSaveFailure(() => store.Save(CodeSettings("Changed name", "ChangedFolder")));
            Check(ReferenceEquals(active, store.Current) && File.ReadAllBytes(path).SequenceEqual(before),
                "Failed replacement truncated the previous file or published unsaved rules.");
            Check(!Directory.EnumerateFiles(root, "*.tmp").Any(), "Failed replacement left a temporary file.");
        }
        Check(store.Save(CodeSettings("Changed name", "ChangedFolder")), "Save must succeed after releasing the lock.");
    }

    private static void SettingsSnapshot(string root)
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "source")).FullName;
        WriteFile(source, "a.cs", "one");
        WriteFile(source, "b.py", "two");
        SettingsStore store = new(Path.Combine(root, "settings.json"));
        store.Save(CodeSettings());
        ClassificationSettings snapshot = store.Current;
        List<FileCategory> draft = snapshot.Categories.ToList();
        draft.Clear();
        Check(snapshot.Categories.Count == 8, "Editing a draft must not mutate active categories.");
        List<FileItem> files = Scanner.Scan(source, progress: new InlineProgress<int>(count =>
        {
            if (count == 1)
            {
                store.Save(CodeSettings("New name", "NewFolder"));
            }
        }), settings: snapshot);
        Check(files.All(file => file.Category.Name == "SourceCode" && file.Category.FolderName == "Sources") &&
            store.Current.Categories[^1].Name == "New name", "A scan observed configuration edits instead of one snapshot.");
    }

    private static void HistoricalMovePaths(string root)
    {
        string source = Directory.CreateDirectory(Path.Combine(root, "source")).FullName;
        string original = WriteFile(source, "history.cs", "keep recorded paths");
        SettingsStore store = new(Path.Combine(root, "settings.json"));
        store.Save(CodeSettings());
        FileOrganizationResult moved = Organizer.OrganizeFiles(source, Scanner.Scan(source, settings: store.Current));
        FileMoveResult history = moved.FileResults.Single();
        string destination = Path.Combine(source, "Sources", "history.cs");
        store.Save(CodeSettings("Renamed", "RenamedFolder"));
        store.Save(ClassificationSettings.Defaults);
        Check(history.SourcePath == original && history.DestinationPath == destination && history.Success &&
            File.ReadAllText(destination) == "keep recorded paths", "Editing/deleting a rule must not reinterpret completed move records.");
    }

    private static SettingsValidationException RuleFailure(Action action)
    {
        try { action(); }
        catch (SettingsValidationException ex) { return ex; }
        throw new InvalidOperationException("Expected a settings validation error.");
    }

    private static void ExpectSaveFailure(Action action)
    {
        try { action(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { return; }
        throw new InvalidOperationException("Expected an I/O or permissions error while saving settings.");
    }
}
