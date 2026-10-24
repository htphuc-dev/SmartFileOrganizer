namespace SmartFileOrganizer.Helpers
{
    internal static class WindowPresentation
    {
        public static Icon CreateApplicationIcon()
        {
            using Stream stream = typeof(WindowPresentation).Assembly.GetManifestResourceStream(
                "SmartFileOrganizer.ApplicationIcon.ico")!;
            using Icon icon = new(stream, new Size(32, 32));
            return (Icon)icon.Clone();
        }

        public static void FitToWorkingArea(Form form)
        {
            // WinForms handles DPI scaling. Clamp the resulting pixel bounds once;
            // do not scale controls or change the application's DPI mode here.
            Rectangle area = Screen.FromControl(form.Owner ?? form).WorkingArea;
            Size available = new(Math.Max(1, area.Width - 24), Math.Max(1, area.Height - 24));
            form.MinimumSize = new Size(Math.Min(form.MinimumSize.Width, available.Width),
                Math.Min(form.MinimumSize.Height, available.Height));
            form.Size = new Size(Math.Min(form.Width, available.Width), Math.Min(form.Height, available.Height));
            form.Location = new Point(Math.Clamp(form.Left, area.Left, area.Right - form.Width),
                Math.Clamp(form.Top, area.Top, area.Bottom - form.Height));
        }
    }
}
