using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public class FileScanner
    {
        public List<FileItem> Scan(string folderPath)
        {
            List<FileItem> files = new();
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            // Let enumeration and metadata errors reach the caller. A partial scan
            // must never be presented as a successful (possibly empty) scan.
            foreach (FileInfo fileInfo in directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly))
            {
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
            }

            return files;
        }
    }
}
