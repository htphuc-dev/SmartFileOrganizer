using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;
using System.Diagnostics;

namespace SmartFileOrganizer.Services
{
    public class FileScanner
    {
        public List<FileItem> Scan(string folderPath, CancellationToken cancellationToken = default,
            IProgress<int>? progress = null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<FileItem> files = new();
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            Stopwatch progressTimer = Stopwatch.StartNew();

            // Let enumeration and metadata errors reach the caller. A partial scan
            // must never be presented as a successful (possibly empty) scan.
            // Enumerate once, lazily: no extra pass just to count files.
            foreach (FileInfo fileInfo in directoryInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly))
            {
                cancellationToken.ThrowIfCancellationRequested();
                fileInfo.Refresh();
                files.Add(new FileItem
                {
                    Name = fileInfo.Name,
                    FullPath = fileInfo.FullName,
                    Extension = fileInfo.Extension,
                    Size = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTime,
                    ModifiedDate = fileInfo.LastWriteTime,
                    Category = FileExtensionHelper.GetCategory(fileInfo.Extension)
                });

                if (files.Count == 1 || progressTimer.ElapsedMilliseconds >= 100)
                {
                    progress?.Report(files.Count);
                    progressTimer.Restart();
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            progress?.Report(files.Count);
            // Cancellation from the final report must not publish a partial scan.
            cancellationToken.ThrowIfCancellationRequested();
            return files;
        }
    }
}
