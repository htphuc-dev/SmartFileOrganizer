namespace SmartFileOrganizer.Models
{
    public sealed class FileMoveResult
    {
        public string SourcePath { get; init; } = string.Empty;

        // The actual destination on success, or the attempted path on failure.
        // Empty if the input paths could not be resolved.
        public string DestinationPath { get; init; } = string.Empty;

        public bool Success { get; init; }

        public string ErrorMessage { get; init; } = string.Empty;
    }
}
