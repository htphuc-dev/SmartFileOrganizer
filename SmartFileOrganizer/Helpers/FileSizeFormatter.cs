namespace SmartFileOrganizer.Helpers
{
    public static class FileSizeFormatter
    {
        public static string Format(long bytes)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(bytes);
            string[] units = ["B", "KB", "MB", "GB"];
            double size = bytes;
            int unit = 0;
            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return unit == 0 ? $"{bytes} B" : $"{size:0.#} {units[unit]}";
        }
    }
}
