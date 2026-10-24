using SmartFileOrganizer.Models;

namespace SmartFileOrganizer.Helpers
{
    public static class FileExtensionHelper
    {
        public static FileCategory GetCategory(string extension, ClassificationSettings? settings = null)
        {
            ClassificationSettings snapshot = settings ?? ClassificationSettings.Defaults;
            if (!string.IsNullOrEmpty(extension))
            {
                foreach (FileCategory category in snapshot.Categories)
                {
                    if (category.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                    {
                        return category;
                    }
                }
            }

            return snapshot.Categories.Single(category => string.Equals(category.Id,
                FileCategory.FallbackId, StringComparison.OrdinalIgnoreCase));
        }
    }
}
