using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Helpers
{
    public static class FileSelectionHelper
    {
        public static FileItem[] GetSelectedFiles(IEnumerable<FileItem> files)
        {
            return files.Where(file => file.IsSelected).ToArray();
        }

        public static (int TotalCount, int SelectedCount, long SelectedSize) GetSummary(IEnumerable<FileItem> files)
        {
            int totalCount = 0;
            int selectedCount = 0;
            long selectedSize = 0;
            foreach (FileItem file in files)
            {
                totalCount++;
                if (file.IsSelected)
                {
                    selectedCount++;
                    selectedSize = checked(selectedSize + file.Size);
                }
            }

            return (totalCount, selectedCount, selectedSize);
        }

        public static void SetAll(IEnumerable<FileItem> files, bool selected)
        {
            foreach (FileItem file in files)
            {
                file.IsSelected = selected;
            }
        }
    }
}
