namespace SmartFileOrganizer.Models
{
    public sealed class FileOrganizationResult
    {
        // Authoritative journal, in input order. Available even after cancellation
        // or an unexpected exception, independently of progress delivery.
        public IReadOnlyList<FileMoveResult> FileResults { get; init; } = Array.Empty<FileMoveResult>();

        public int TotalCount { get; init; }
        public int ProcessedCount => FileResults.Count;
        public int SuccessCount { get; init; }
        public int FailedCount => ProcessedCount - SuccessCount;
        public bool Cancelled { get; init; }
        public Exception? Error { get; init; }
    }
}
