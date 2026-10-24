namespace SmartFileOrganizer.Models
{
    public sealed class ClassificationSettings
    {
        public const int CurrentVersion = 1;

        [System.Text.Json.Serialization.JsonConstructor]
        public ClassificationSettings(int version, IReadOnlyList<FileCategory> categories)
        {
            Version = version;
            ArgumentNullException.ThrowIfNull(categories);
            Categories = Array.AsReadOnly(categories.ToArray());
        }

        public int Version { get; }
        public IReadOnlyList<FileCategory> Categories { get; }

        public static ClassificationSettings Defaults { get; } = new(CurrentVersion,
            [FileCategory.Images, FileCategory.Documents, FileCategory.Videos, FileCategory.Music,
             FileCategory.Archives, FileCategory.Applications, FileCategory.Others]);
    }
}
