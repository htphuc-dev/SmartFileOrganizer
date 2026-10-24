using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    // Session state owned by the UI thread, independent of the scan/selection.
    public sealed class FileUndoHistory
    {
        private readonly List<FileMoveResult> pendingFiles = new();

        public int PendingCount => pendingFiles.Count;

        public FileMoveResult[] GetSnapshot() => pendingFiles.ToArray();

        public void RecordOrganization(FileOrganizationResult result)
        {
            FileMoveResult[] moved = result.FileResults.Where(file => file.Success).ToArray();
            if (moved.Length == 0) return;

            pendingFiles.Clear();
            pendingFiles.AddRange(moved);
        }

        public void ApplyUndoResults(IReadOnlyList<FileMoveResult> snapshot, FileOrganizationResult result)
        {
            if (result.ProcessedCount > snapshot.Count)
                throw new ArgumentException("Undo results exceed the submitted transaction.", nameof(result));

            for (int index = 0; index < result.ProcessedCount; index++)
            {
                FileMoveResult restored = result.FileResults[index];
                FileMoveResult original = snapshot[index];
                if (restored.Success &&
                    string.Equals(restored.SourcePath, original.DestinationPath, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(restored.DestinationPath, original.SourcePath, StringComparison.OrdinalIgnoreCase))
                {
                    // Reference identity prevents an old completion from removing
                    // entries belonging to a newer transaction.
                    pendingFiles.Remove(original);
                }
            }
        }
    }
}
