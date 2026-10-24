namespace SmartFileOrganizer.Models
{
    // FileResults is an immutable snapshot of the results since the last report.
    // Its first input index is ProcessedCount - FileResults.Count.
    public sealed record FileOrganizationProgress(
        int ProcessedCount,
        int TotalCount,
        int SuccessCount,
        int FailedCount,
        IReadOnlyList<FileMoveResult> FileResults);
}
