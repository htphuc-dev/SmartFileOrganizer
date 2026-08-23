using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;
using System.IO; // add at top of file

namespace SmartFileOrganizer
{
    using SmartFileOrganizer.Services;
    using SmartFileOrganizer.Models;
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            FileScanner scanner = new FileScanner();

            List<FileItem> files = scanner.Scan(@"D:\Admin");

            FileOrganizer organizer = new FileOrganizer();

            foreach (FileItem file in files)
            {
                string destinationFolder =
                    organizer.GetDestinationFolder(@"D:\Sorted", file);
                organizer.EnsureDestinationFolder(destinationFolder);

                System.Diagnostics.Debug.WriteLine(
                    $"{file.Name} -> {destinationFolder}"
                );
            }
        }
    }
}
