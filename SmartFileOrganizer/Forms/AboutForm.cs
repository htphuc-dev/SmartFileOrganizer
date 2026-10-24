using System.Reflection;
using SmartFileOrganizer.Helpers;

namespace SmartFileOrganizer
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            Icon = WindowPresentation.CreateApplicationIcon();
            Assembly assembly = typeof(AboutForm).Assembly;
            string version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                .Split('+')[0] ?? assembly.GetName().Version?.ToString() ?? "Unknown";
            lblVersion.Text = $"Version {version}";
        }

        private void AboutForm_Shown(object sender, EventArgs e)
        {
            WindowPresentation.FitToWorkingArea(this);
            btnClose.Focus();
        }
    }
}
