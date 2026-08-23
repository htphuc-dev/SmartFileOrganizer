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

    }
}
