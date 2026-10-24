namespace SmartFileOrganizer.Models
{
    public sealed class FileCategory : IEquatable<FileCategory>
    {
        public const string FallbackId = "others";

        [System.Text.Json.Serialization.JsonConstructor]
        public FileCategory(string id, string name, string folderName, IReadOnlyList<string> extensions)
        {
            Id = id;
            Name = name;
            FolderName = folderName;
            ArgumentNullException.ThrowIfNull(extensions);
            Extensions = Array.AsReadOnly(extensions.ToArray());
        }

        public string Id { get; }
        public string Name { get; }
        public string FolderName { get; }
        public IReadOnlyList<string> Extensions { get; }

        // Built-in values preserve the existing API; custom groups use their own IDs.
        public static FileCategory Images { get; } = new("images", "Images", "Images", [".jpg", ".jpeg", ".png", ".gif"]);
        public static FileCategory Documents { get; } = new("documents", "Documents", "Documents", [".pdf", ".doc", ".docx", ".txt"]);
        public static FileCategory Videos { get; } = new("videos", "Videos", "Videos", [".mp4", ".avi", ".mkv"]);
        public static FileCategory Music { get; } = new("music", "Music", "Music", [".mp3", ".wav"]);
        public static FileCategory Archives { get; } = new("archives", "Archives", "Archives", [".zip", ".rar", ".7z"]);
        public static FileCategory Applications { get; } = new("applications", "Applications", "Applications", [".exe"]);
        public static FileCategory Others { get; } = new(FallbackId, "Others", "Others", []);

        public bool Equals(FileCategory? other) => other != null &&
            string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object? obj) => obj is FileCategory category && Equals(category);
        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
        public static bool operator ==(FileCategory? left, FileCategory? right) => Equals(left, right);
        public static bool operator !=(FileCategory? left, FileCategory? right) => !Equals(left, right);
        public override string ToString() => Name;
    }
}
