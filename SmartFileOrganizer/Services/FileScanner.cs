using SmartFileOrganizer.Helpers;
using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public class FileScanner
    {
        public List<FileItem> Scan(string folderPath)
        {
            List<FileItem> files = new List<FileItem>();

            if (!Directory.Exists(folderPath))
            {
                return files;
            }

            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

                FileInfo[] fileInfos = directoryInfo.GetFiles();

                foreach (FileInfo fileInfo in fileInfos)
                {
                    FileItem item = new FileItem();

                    item.Name = fileInfo.Name;
                    item.FullPath = fileInfo.FullName;
                    item.Extension = fileInfo.Extension;
                    item.Size = fileInfo.Length;
                    item.CreatedDate = fileInfo.CreationTime;
                    item.ModifiedDate = fileInfo.LastWriteTime;
                    item.Category =
                        FileExtensionHelper.GetCategory(fileInfo.Extension);

                    files.Add(item);
                }
            }
            catch (UnauthorizedAccessException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Không có quyền truy cập vào thư mục."
                );
            }
            catch (IOException)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Có lỗi khi đọc thư mục."
                );
            }

            return files;
        }
    }
}