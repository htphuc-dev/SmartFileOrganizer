using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Helpers
{
    public static class FileExtensionHelper
    {
        public static FileCategory GetCategory(string extension)
        {
            extension = extension.ToLower();

            if (extension == ".jpg" ||
                extension == ".jpeg" ||
                extension == ".png" ||
                extension == ".gif")
            {
                return FileCategory.Images;
            }

            if (extension == ".pdf" ||
                extension == ".doc" ||
                extension == ".docx" ||
                extension == ".txt")
            {
                return FileCategory.Documents;
            }

            if (extension == ".mp4" ||
                extension == ".avi" ||
                extension == ".mkv")
            {
                return FileCategory.Videos;
            }

            if (extension == ".mp3" ||
                extension == ".wav")
            {
                return FileCategory.Music;
            }

            if (extension == ".zip" ||
                extension == ".rar" ||
                extension == ".7z")
            {
                return FileCategory.Archives;
            }

            if (extension == ".exe")
            {
                return FileCategory.Applications;
            }

            return FileCategory.Others;
        }
    }
}