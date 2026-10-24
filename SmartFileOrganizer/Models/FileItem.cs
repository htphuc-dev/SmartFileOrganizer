namespace SmartFileOrganizer.Models
{
    public class FileItem
    {
        public bool IsSelected { get; set; } = true;

        public string PreviewDestinationPath { get; set; } = string.Empty;

        public string Name { get; set; } =string.Empty;

        public string FullPath { get; set; } =string.Empty;

        public string Extension { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime ModifiedDate { get; set; }

        public FileCategory Category { get; set; }
    } 
}
