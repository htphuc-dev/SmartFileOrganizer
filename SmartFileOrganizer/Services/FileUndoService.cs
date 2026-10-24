using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public sealed class FileUndoService
    {
        public FileOrganizationResult UndoFiles(IReadOnlyList<FileMoveResult> files,
            CancellationToken cancellationToken = default, IProgress<FileOrganizationProgress>? progress = null)
        {
            List<FileMoveResult> results = new();
            List<FileMoveResult> unreportedResults = new();
            System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();
            int successCount = 0;
            bool cancelled = false;
            Exception? error = null;

            void ReportProgress()
            {
                progress?.Report(new FileOrganizationProgress(results.Count, files.Count, successCount,
                    results.Count - successCount, unreportedResults.ToArray()));
                unreportedResults.Clear();
                timer.Restart();
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                foreach (FileMoveResult file in files)
                {
                    // Journal a completed restore before observing cancellation.
                    cancellationToken.ThrowIfCancellationRequested();
                    FileMoveResult restored = RestoreFile(file);
                    results.Add(restored);
                    unreportedResults.Add(restored);
                    if (restored.Success) successCount++;
                    if (results.Count == 1 || timer.ElapsedMilliseconds >= 100) ReportProgress();
                }
                if (unreportedResults.Count > 0) ReportProgress();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
            }
            catch (Exception ex)
            {
                // A failed report must not lose the restore that already happened.
                error = ex;
            }

            return new FileOrganizationResult
            {
                FileResults = results.AsReadOnly(), TotalCount = files.Count,
                SuccessCount = successCount, Cancelled = cancelled, Error = error
            };
        }

        private static FileMoveResult RestoreFile(FileMoveResult file)
        {
            string source = file.DestinationPath;
            string destination = file.SourcePath;
            try
            {
                if (!file.Success || !Path.IsPathFullyQualified(source) || !Path.IsPathFullyQualified(destination) ||
                    file.FileLength == null || file.LastWriteTimeUtc == null)
                    throw new ArgumentException("The move record does not contain a verified file and absolute paths.");

                source = Path.GetFullPath(source);
                destination = Path.GetFullPath(destination);
                if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("The recorded source and destination are the same.");

                FileAttributes attributes = File.GetAttributes(source);
                if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint)) != 0)
                    throw new IOException("The organized path is no longer a regular file.");

                FileInfo current = new(source);
                if (current.Length != file.FileLength || current.LastWriteTimeUtc != file.LastWriteTimeUtc)
                    throw new IOException("The organized file has changed since it was moved.");

                if (File.Exists(destination) || Directory.Exists(destination))
                    throw new IOException("The original path is occupied. Nothing was overwritten.");

                // Restore the exact recorded name; never generate a suffix or
                // reconstruct a path from the current classification rules.
                File.Move(source, destination, overwrite: false);
                return new FileMoveResult
                {
                    SourcePath = source, DestinationPath = destination, Success = true,
                    FileLength = file.FileLength, LastWriteTimeUtc = file.LastWriteTimeUtc
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException
                or NotSupportedException or System.Security.SecurityException)
            {
                return new FileMoveResult
                {
                    SourcePath = source, DestinationPath = destination, Success = false,
                    ErrorMessage = $"Unable to restore '{destination}': {ex.Message}"
                };
            }
        }
    }
}
