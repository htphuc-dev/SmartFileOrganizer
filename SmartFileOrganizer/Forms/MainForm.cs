using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer
{
    public partial class MainForm : Form
    {
        private readonly FileScanner scanner;
        private readonly FileOrganizer organizer;

        private List<FileItem> scannedFiles = new();

        public MainForm()
        {
            InitializeComponent();

            scanner = new FileScanner();
            organizer = new FileOrganizer();

            SetupInitialState();
        }

        private void SetupInitialState()
        {
            btnOrganize.Enabled = false;

            lblEmptyState.Visible = true;
            picEmptyState.Visible = true;

            lblStatus.Text = "●  Status: Ready";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using FolderBrowserDialog dialog = new FolderBrowserDialog();

            dialog.Description = "Select a folder to organize";
            dialog.UseDescriptionForTitle = true;

            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text) &&
                Directory.Exists(txtFolderPath.Text))
            {
                dialog.SelectedPath = txtFolderPath.Text;
            }

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtFolderPath.Text = dialog.SelectedPath;

                scannedFiles.Clear();
                dgvFiles.Rows.Clear();

                lblEmptyState.Visible = true;
                picEmptyState.Visible = true;

                btnOrganize.Enabled = false;

                lblStatus.Text = "●  Folder selected";
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            string folderPath = txtFolderPath.Text.Trim();

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                MessageBox.Show(
                    "Please select a folder first.",
                    "Folder Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(
                    "The selected folder does not exist.",
                    "Invalid Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            try
            {
                lblStatus.Text = "●  Scanning files...";
                Application.DoEvents();

                scannedFiles = scanner.Scan(folderPath);

                dgvFiles.Rows.Clear();

                foreach (FileItem file in scannedFiles)
                {
                    string destinationFolder =
                        organizer.GetDestinationFolder(
                            folderPath,
                            file);

                    dgvFiles.Rows.Add(
                        file.Name,
                        file.Category.ToString(),
                        destinationFolder);
                }

                bool hasFiles = scannedFiles.Count > 0;

                lblEmptyState.Visible = !hasFiles;
                picEmptyState.Visible = !hasFiles;

                btnOrganize.Enabled = hasFiles;

                lblStatus.Text =
                    $"●  Scan complete — {scannedFiles.Count} file(s)";
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show(
                    "Access to this folder was denied.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text = "●  Access denied";
            }
            catch (IOException ex)
            {
                MessageBox.Show(
                    $"An error occurred while scanning:\n\n{ex.Message}",
                    "Scan Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text = "●  Scan failed";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Unexpected error:\n\n{ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                lblStatus.Text = "●  Error";
            }
        }
        private void btnOrganize_Click(object sender, EventArgs e)
        {
            if (scannedFiles.Count == 0)
            {
                MessageBox.Show(
                    "There are no files to organize.",
                    "Nothing to Organize",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            string folderPath = txtFolderPath.Text.Trim();

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show(
                    "The selected folder no longer exists.",
                    "Invalid Folder",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            DialogResult result = MessageBox.Show(
                $"Organize {scannedFiles.Count} file(s)?\n\n" +
                "Files will be moved into category folders.",
                "Confirm Organization",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
            {
                return;
            }

            try
            {
                int successCount = 0;
                int failedCount = 0;

                btnOrganize.Enabled = false;
                btnScan.Enabled = false;
                btnBrowse.Enabled = false;

                lblStatus.Text = "●  Organizing files...";
                Application.DoEvents();

                foreach (FileItem file in scannedFiles)
                {
                    string destinationFolder =
                        organizer.GetDestinationFolder(
                            folderPath,
                            file);

                    bool success =
                        organizer.MoveFile(
                            file.FullPath,
                            destinationFolder);

                    if (success)
                    {
                        successCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                dgvFiles.Rows.Clear();

                scannedFiles.Clear();

                lblEmptyState.Visible = true;
                picEmptyState.Visible = true;

                btnBrowse.Enabled = true;
                btnScan.Enabled = true;
                btnOrganize.Enabled = false;

                lblStatus.Text =
                    $"●  Organized — {successCount} moved, {failedCount} failed";

                MessageBox.Show(
                    $"Organization completed.\n\n" +
                    $"Successfully moved: {successCount}\n" +
                    $"Failed: {failedCount}",
                    "Organization Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                btnBrowse.Enabled = true;
                btnScan.Enabled = true;
                btnOrganize.Enabled = scannedFiles.Count > 0;

                lblStatus.Text = "●  Organization failed";

                MessageBox.Show(
                    $"An error occurred:\n\n{ex.Message}",
                    "Organization Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupInitialState();
        }
    }
}