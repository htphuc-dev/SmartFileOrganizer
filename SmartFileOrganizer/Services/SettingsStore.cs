using System.Text.Json;
using System.Text.Json.Serialization;
using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public sealed class SettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
        };

        // Tests supply an explicit temporary path and never touch LocalAppData.
        public SettingsStore(string? settingsPath = null)
        {
            SettingsPath = Path.GetFullPath(settingsPath ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SmartFileOrganizer", "settings.json"));
        }

        public string SettingsPath { get; }
        public ClassificationSettings Current { get; private set; } = ClassificationSettings.Defaults;
        public string? LoadWarning { get; private set; }
        public bool RequiresOverwriteConfirmation { get; private set; }

        public void Load()
        {
            Current = ClassificationSettings.Defaults;
            LoadWarning = null;
            RequiresOverwriteConfirmation = false;
            try
            {
                ClassificationSettings? settings = JsonSerializer.Deserialize<ClassificationSettings>(
                    File.ReadAllText(SettingsPath), JsonOptions);
                if (settings == null)
                {
                    throw new SettingsValidationException("The configuration is empty.");
                }

                Current = SettingsValidator.NormalizeAndValidate(settings);
            }
            catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
            {
                // No first-run file is normal. Do not create one during startup.
            }
            catch (Exception ex) when (ex is JsonException or SettingsValidationException or ArgumentException
                or IOException or UnauthorizedAccessException or NotSupportedException or System.Security.SecurityException)
            {
                RequiresOverwriteConfirmation = true;
                LoadWarning = $"Unable to load settings:\n{SettingsPath}\n\n{ex.Message}\n\n" +
                    "Using temporary defaults. The existing configuration has not been changed.";
            }
        }

        public bool Save(ClassificationSettings draft, bool replaceInvalidFile = false)
        {
            ClassificationSettings validated = SettingsValidator.NormalizeAndValidate(draft);
            if (RequiresOverwriteConfirmation && !replaceInvalidFile)
            {
                throw new InvalidOperationException("Confirm replacement of the unreadable or invalid settings file before saving.");
            }

            bool changed = !SettingsValidator.AreEquivalent(Current, validated);
            string directory = Path.GetDirectoryName(SettingsPath)!;
            Directory.CreateDirectory(directory);
            string temporaryPath = Path.Combine(directory, $".settings-{Guid.NewGuid():N}.tmp");
            try
            {
                using (FileStream stream = new(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, validated, JsonOptions);
                    stream.Flush(flushToDisk: true);
                }

                // Same-directory rename replaces the complete file, never a partial write.
                File.Move(temporaryPath, SettingsPath, overwrite: true);
            }
            finally
            {
                try
                {
                    File.Delete(temporaryPath);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    // Preserve the original save error if cleanup also fails.
                }
            }

            // Publish the immutable snapshot only after the disk commit succeeds.
            Current = validated;
            LoadWarning = null;
            RequiresOverwriteConfirmation = false;
            return changed;
        }
    }
}
