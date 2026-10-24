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

                // Do not overwrite, including when another process takes this name
                // after the availability check. Report that conflict for this file.
                File.Move(sourcePath, destinationPath, overwrite: false);

                return new FileMoveResult
                {
                    SourcePath = sourcePath,
                    DestinationPath = destinationPath,
                    Success = true
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
