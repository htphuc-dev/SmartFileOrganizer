using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            FileScanner scanner = new FileScanner();

            List<FileItem> files = scanner.Scan(@"D:\Admin");

            System.Diagnostics.Debug.WriteLine($"Số file tìm thấy: {files.Count}");

            foreach (FileItem file in files)
            {
                System.Diagnostics.Debug.WriteLine(
                        $"Name: {file.Name} | " +
                        $"Extension: {file.Extension} | " +
                        $"Size: {file.Size} bytes | " +
                        $"Created: {file.CreatedDate} | " +
                        $"Modified: {file.ModifiedDate}" +
                        $"Category: {file.Category}"
                );
            }
        }
    }
}
