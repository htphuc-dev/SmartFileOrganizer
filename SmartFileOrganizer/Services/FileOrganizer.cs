using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public class FileOrganizer
    {
        public FileOrganizationResult OrganizeFiles(string sourceFolder, IReadOnlyList<FileItem> files,
            CancellationToken cancellationToken = default, IProgress<FileOrganizationProgress>? progress = null)
        {
            List<FileMoveResult> results = new();
            List<FileMoveResult> unreportedResults = new();
            System.Diagnostics.Stopwatch progressTimer = System.Diagnostics.Stopwatch.StartNew();
            int successCount = 0;
            bool cancelled = false;
            Exception? error = null;

            void ReportProgress()
            {
                progress?.Report(new FileOrganizationProgress(results.Count, files.Count, successCount,
                    results.Count - successCount, unreportedResults.ToArray()));
                unreportedResults.Clear();
                progressTimer.Restart();
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (FileItem file in files)
                {
                    // Finish the current File.Move and journal its result before
                    // observing cancellation at the next file boundary.
                    cancellationToken.ThrowIfCancellationRequested();
                    FileMoveResult result = MoveFile(file.FullPath, GetDestinationFolder(sourceFolder, file));
                    results.Add(result);
                    unreportedResults.Add(result);
                    if (result.Success)
                    {
                        successCount++;
                    }

                    if (results.Count == 1 || progressTimer.ElapsedMilliseconds >= 100)
                    {
                        ReportProgress();
                    }
                }

                if (unreportedResults.Count > 0)
                {
                    ReportProgress();
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                cancelled = true;
            }
            catch (Exception ex)
            {
                // Preserve completed moves even if a later operation/report fails.
                error = ex;
            }

            return new FileOrganizationResult
            {
                FileResults = results.AsReadOnly(),
                TotalCount = files.Count,
                SuccessCount = successCount,
                Cancelled = cancelled,
                Error = error
            };
        }

        public string GetDestinationFolder(string sourceFolder, FileItem file)
        {
            SettingsValidator.ValidateFolderName(file.Category.FolderName);
            return Path.Combine(sourceFolder, file.Category.FolderName);
        }

        public string GetPreviewDestinationPath(string sourceFolder, FileItem file)
        {
            // Read-only preview; MoveFile resolves availability again when moving.
            return GetAvailableFilePath(GetDestinationFolder(sourceFolder, file), file.FullPath);
        }

        public void EnsureDestinationFolder(string destinationFolder)
        {
            Directory.CreateDirectory(destinationFolder);
        }
        public string GetAvailableFilePath(string destinationFolder, string sourceFilePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string extension = Path.GetExtension(sourceFilePath);

            string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
            int count = 1;

            while (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                string newFileName = $"{fileNameWithoutExtension} ({count}){extension}";
                destinationPath = Path.Combine(destinationFolder, newFileName);
                count++;
            }

            return destinationPath;
        }

        public FileMoveResult MoveFile(string sourceFilePath, string destinationFolder)
        {
            string sourcePath = sourceFilePath;
            string destinationPath = string.Empty;

            try
            {
                sourcePath = Path.GetFullPath(sourceFilePath);
                string targetFolder = Path.GetFullPath(destinationFolder);
                destinationPath = Path.Combine(targetFolder, Path.GetFileName(sourcePath));

                // Unlike File.Exists, GetAttributes preserves access and I/O errors.
                if ((File.GetAttributes(sourcePath) & FileAttributes.Directory) != 0)
                {
                    throw new IOException("The source path is a directory, not a file.");
                }

                EnsureDestinationFolder(targetFolder);

                destinationPath = GetAvailableFilePath(targetFolder, sourcePath);

                FileInfo sourceInfo = new(sourcePath);
                long fileLength = sourceInfo.Length;
                DateTime lastWriteTimeUtc = sourceInfo.LastWriteTimeUtc;

                // Do not overwrite, including when another process takes this name
                // after the availability check. Report that conflict for this file.
                File.Move(sourcePath, destinationPath, overwrite: false);

                return new FileMoveResult
                {
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Success = true,
                    FileLength = fileLength,
                    LastWriteTimeUtc = lastWriteTimeUtc
                };
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                or ArgumentException or NotSupportedException or System.Security.SecurityException)
            {
                string reason = ex switch
                {
                    FileNotFoundException => "The source file no longer exists.",
                    DirectoryNotFoundException => "A source or destination directory was not found.",
                    UnauthorizedAccessException or System.Security.SecurityException => "Access denied.",
                    ArgumentException or NotSupportedException => "Invalid source or destination path.",
                    _ => "Unable to move the file (I/O error)."
                };

                return new FileMoveResult
                {
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Success = false,
                    ErrorMessage = $"{reason} {ex.Message}"
                };
            }
        }

    }
}
