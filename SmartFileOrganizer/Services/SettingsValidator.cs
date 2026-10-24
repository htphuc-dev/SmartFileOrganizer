using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public sealed class SettingsValidationException(string message, string? categoryId = null, string? field = null)
        : Exception(message)
    {
        public string? CategoryId { get; } = categoryId;
        public string? Field { get; } = field;
    }

    public static class SettingsValidator
    {
        public static IReadOnlyList<string> ParseExtensions(string text)
        {
            return NormalizeExtensions(text.Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static IReadOnlyList<string> NormalizeExtensions(IEnumerable<string> values)
        {
            HashSet<string> unique = new(StringComparer.OrdinalIgnoreCase);
            List<string> normalized = new();
            foreach (string value in values)
            {
                string extension = value?.Trim() ?? string.Empty;
                if (!extension.StartsWith('.'))
                {
                    extension = "." + extension;
                }

                if (extension.Length < 2 || extension.Length > 255 || extension[1..].Any(character => character == '.' ||
                    char.IsWhiteSpace(character) || char.IsControl(character) || "<>:\"/\\|?*,;".Contains(character)))
                {
                    throw new SettingsValidationException($"Invalid extension '{value}'. Use a final extension such as .pdf or .gz, not .tar.gz.",
                        field: "extensions");
                }

                extension = extension.ToLowerInvariant();
                if (unique.Add(extension))
                {
                    normalized.Add(extension);
                }
            }

            return normalized.AsReadOnly();
        }

        public static void ValidateFolderName(string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName) || folderName.Length > 255 || folderName is "." or ".." ||
                folderName.Any(character => char.IsControl(character) || "<>:\"/\\|?*".Contains(character)) ||
                folderName.EndsWith('.') || char.IsWhiteSpace(folderName[^1]))
            {
                throw new SettingsValidationException("Destination must be one valid Windows subfolder name, with no path, trailing dot or space.",
                    field: "folderName");
            }

            string stem = folderName.Split('.')[0].TrimEnd().ToUpperInvariant();
            bool numberedDevice = stem.Length == 4 && (stem.StartsWith("COM") || stem.StartsWith("LPT")) &&
                "123456789¹²³".Contains(stem[3]);
            if (stem is "CON" or "PRN" or "AUX" or "NUL" or "CLOCK$" or "CONIN$" or "CONOUT$" || numberedDevice)
            {
                throw new SettingsValidationException($"'{folderName}' is a reserved Windows device name.", field: "folderName");
            }
        }

        public static ClassificationSettings NormalizeAndValidate(ClassificationSettings settings)
        {
            if (settings.Version != ClassificationSettings.CurrentVersion)
            {
                throw new SettingsValidationException($"Unsupported settings version {settings.Version}; expected {ClassificationSettings.CurrentVersion}.");
            }

            List<FileCategory> result = new();
            HashSet<string> ids = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
            HashSet<string> folders = new(StringComparer.OrdinalIgnoreCase);
            Dictionary<string, string> extensionOwners = new(StringComparer.OrdinalIgnoreCase);
            foreach (FileCategory category in settings.Categories)
            {
                if (category == null)
                {
                    throw new SettingsValidationException("A category must not be null.");
                }

                try
                {
                    if (string.IsNullOrWhiteSpace(category.Id) || category.Id.Any(character =>
                        !char.IsLetterOrDigit(character) && character != '-' && character != '_') || !ids.Add(category.Id))
                    {
                        throw new SettingsValidationException("Category IDs must be nonempty, unique, and contain only letters, digits, '-' or '_'.");
                    }

                    string name = category.Name?.Trim() ?? string.Empty;
                    if (name.Length == 0 || !names.Add(name))
                    {
                        throw new SettingsValidationException("Group name must be nonempty and unique (ignoring case).", field: "name");
                    }

                    ValidateFolderName(category.FolderName);
                    if (!folders.Add(category.FolderName))
                    {
                        throw new SettingsValidationException($"Destination folder '{category.FolderName}' is used by another group.", field: "folderName");
                    }

                    IReadOnlyList<string> extensions = NormalizeExtensions(category.Extensions);
                    if (string.Equals(category.Id, FileCategory.FallbackId, StringComparison.OrdinalIgnoreCase) && extensions.Count != 0)
                    {
                        throw new SettingsValidationException("The Others fallback group must not have its own extensions.", field: "extensions");
                    }

                    foreach (string extension in extensions)
                    {
                        if (extensionOwners.TryGetValue(extension, out string? owner))
                        {
                            throw new SettingsValidationException($"Extension '{extension}' belongs to both '{owner}' and '{name}'.", field: "extensions");
                        }

                        extensionOwners.Add(extension, name);
                    }

                    result.Add(new FileCategory(category.Id, name, category.FolderName, extensions));
                }
                catch (SettingsValidationException ex)
                {
                    throw new SettingsValidationException($"Group '{category.Name}': {ex.Message}", category.Id, ex.Field);
                }
            }

            if (!ids.Contains(FileCategory.FallbackId))
            {
                throw new SettingsValidationException("The Others fallback group is required and cannot be deleted.");
            }

            return new ClassificationSettings(settings.Version, result);
        }

        public static bool AreEquivalent(ClassificationSettings first, ClassificationSettings second)
        {
            if (first.Version != second.Version || first.Categories.Count != second.Categories.Count)
            {
                return false;
            }

            foreach (FileCategory category in first.Categories)
            {
                FileCategory? other = second.Categories.FirstOrDefault(item => item == category);
                if (other == null || category.Name != other.Name || category.FolderName != other.FolderName ||
                    !category.Extensions.ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(other.Extensions))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
