using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Services
{
    public class FileOrganizer
    {
        public string GetDestinationFolder(string sourceFolder, FileItem file)
        {
            string categoryName = file.Category.ToString();

            string destinationFolder =
                Path.Combine(sourceFolder, categoryName);

            return destinationFolder;
        }

        public void EnsureDestinationFolder(string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
        }
        public string GetAvailableFilePath(string destinationFolder, string sourceFilePath)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            string extension = Path.GetExtension(sourceFilePath);

            string destinationPath = Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath));
            int count = 1;

            while (File.Exists(destinationPath))
            {
                string newFileName = $"{fileNameWithoutExtension} ({count}){extension}";
                destinationPath = Path.Combine(destinationFolder, newFileName);
                count++;
            }

            return destinationPath;
        }

        public bool MoveFile(string sourceFilePath, string destinationFolder)
        {
            try
            {
                if (!File.Exists(sourceFilePath))
                {
                    return false;
                }

                EnsureDestinationFolder(destinationFolder);

                string availableFilePath =
                    GetAvailableFilePath(destinationFolder, sourceFilePath);

                File.Move(sourceFilePath, availableFilePath);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (IOException)
            {
                return false;
            }
        }

    }
}
