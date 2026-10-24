using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer
{
    public partial class MainForm : Form
    {
        private readonly FileScanner scanner;
        private readonly FileOrganizer organizer;

        private List<FileItem> scannedFiles = new();
        private readonly Dictionary<FileItem, string> fileErrors = new();
        private string? scannedFolderPath;
        private bool isProcessing;

        public MainForm()
        {
            InitializeComponent();

            scanner = new FileScanner();
            organizer = new FileOrganizer();

            SetupInitialState();
        }

        private void SetupInitialState()
        {
            ClearScanResults();
            lblStatus.Text = "●  Status: Ready";
        }

        private static string NormalizeFolderPath(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Please select a folder first.");
            }

            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(folderPath.Trim()));
        }

        private bool CurrentPathMatchesScan()
        {
            if (scannedFolderPath == null)
            {
                return false;
            }

            try
            {
                return string.Equals(NormalizeFolderPath(txtFolderPath.Text),
                    scannedFolderPath, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex) when (ex is ArgumentException or IOException
                or NotSupportedException or System.Security.SecurityException)
            {
                return false;
            }
        }

        private void SetProcessing(bool processing)
        {
            isProcessing = processing;
            txtFolderPath.Enabled = !processing;
            btnBrowse.Enabled = !processing;
            btnScan.Enabled = !processing;
            btnOrganize.Enabled = !processing && scannedFiles.Count > 0 && CurrentPathMatchesScan();
        }

        private void UpdateEmptyState(string message)
        {
            bool isEmpty = scannedFiles.Count == 0;
            lblEmptyState.Text = message;
            lblEmptyState.Visible = isEmpty;
            picEmptyState.Visible = isEmpty;
        }

        private void ClearScanResults()
        {
            scannedFolderPath = null;
            scannedFiles.Clear();
            fileErrors.Clear();
            dgvFiles.Rows.Clear();
            btnOrganize.Enabled = false;
            UpdateEmptyState("No files to display\r\nSelect a folder and click \"Scan Files\" to get started.");
        }

        private void txtFolderPath_TextChanged(object sender, EventArgs e)
        {
            ClearScanResults();
            lblStatus.Text = "●  Folder path changed — scan again";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (isProcessing)
            {
                return;
            }

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
                // Selecting the same folder again must invalidate the scan too.
                ClearScanResults();
                lblStatus.Text = "●  Folder selected — scan to continue";
            }
        }

        private void RenderPendingFiles()
        {
            dgvFiles.Rows.Clear();

            foreach (FileItem file in scannedFiles)
            {
                string destinationFolder = organizer.GetDestinationFolder(scannedFolderPath!, file);
                int index = dgvFiles.Rows.Add(file.Name, file.Category.ToString(), destinationFolder, "Ready");
                DataGridViewRow row = dgvFiles.Rows[index];
                row.Tag = file;

                if (fileErrors.TryGetValue(file, out string? error))
                {
                    SetRowError(row, error);
                }
            }
        }

        private DataGridViewRow? FindFileRow(FileItem file)
        {
            // Keep the correct association even if the user sorts the grid.
            return dgvFiles.Rows.Cast<DataGridViewRow>().FirstOrDefault(row => ReferenceEquals(row.Tag, file));
        }

        private void SetRowError(DataGridViewRow row, string error)
        {
            DataGridViewCell cell = row.Cells[colResult.Index];
            cell.Value = $"Failed: {error}";
            cell.ToolTipText = error;
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (isProcessing)
            {
                return;
            }

            ClearScanResults();

            try
            {
                SetProcessing(true);
                lblStatus.Text = "●  Scanning files...";

                string folderPath = NormalizeFolderPath(txtFolderPath.Text);
                List<FileItem> files = scanner.Scan(folderPath);

                // Commit a scan only after all files and metadata were read.
                scannedFiles = files;
                scannedFolderPath = folderPath;
                RenderPendingFiles();
                UpdateEmptyState("This folder contains no files.\r\nSubfolders are not scanned.");
                lblStatus.Text = $"●  Scan complete — {scannedFiles.Count} file(s)";
            }
            catch (Exception ex)
            {
                ClearScanResults();
                UpdateEmptyState("Scan failed.\r\nResolve the reported error and scan again.");
                lblStatus.Text = "●  Scan failed";

                MessageBox.Show(this, $"Unable to scan the selected folder:\n\n{ex.Message}",
                    "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetProcessing(false);
            }
        }

        private void btnOrganize_Click(object sender, EventArgs e)
        {
            if (isProcessing)
            {
                return;
            }

            if (!CurrentPathMatchesScan())
            {
                ClearScanResults();
                lblStatus.Text = "●  Scan required — folder path does not match";
                MessageBox.Show(this, "The folder path does not match a successful scan. Please scan again.",
                    "Scan Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (scannedFiles.Count == 0)
            {
                MessageBox.Show(this, "There are no files to organize.", "Nothing to Organize",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirmation = MessageBox.Show(this,
                $"Organize {scannedFiles.Count} file(s)?\n\nFiles will be moved into category folders.",
                "Confirm Organization", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            int totalCount = scannedFiles.Count;
            int successCount = 0;
            int failedCount = 0;
            FileItem? currentFile = null;
            bool currentFailureCounted = false;

            try
            {
                SetProcessing(true);
                lblStatus.Text = "●  Organizing files...";

                foreach (FileItem file in scannedFiles.ToArray())
                {
                    currentFile = file;
                    currentFailureCounted = false;
                    string destinationFolder = organizer.GetDestinationFolder(scannedFolderPath!, file);
                    FileMoveResult result = organizer.MoveFile(file.FullPath, destinationFolder);

                    if (result.Success)
                    {
                        // Remove from the retry list before any further UI work.
                        scannedFiles.Remove(file);
                        fileErrors.Remove(file);
                        successCount++;

                        DataGridViewRow? row = FindFileRow(file);
                        if (row != null)
                        {
                            dgvFiles.Rows.Remove(row);
                        }
                    }
                    else
                    {
                        failedCount++;
                        currentFailureCounted = true;
                        fileErrors[file] = result.ErrorMessage;
                        DataGridViewRow? row = FindFileRow(file);
                        if (row != null)
                        {
                            SetRowError(row, result.ErrorMessage);
                        }
                    }

                    currentFile = null;
                }

                lblStatus.Text = $"●  Organized — {successCount} moved, {failedCount} failed";
                MessageBox.Show(this,
                    $"Organization completed.\n\nSuccessfully moved: {successCount}\nFailed: {failedCount}" +
                    (failedCount > 0 ? "\n\nFailed files remain in the list. Resolve the errors and click Organize to retry." : ""),
                    "Organization Complete", MessageBoxButtons.OK,
                    failedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (currentFile != null && scannedFiles.Contains(currentFile))
                {
                    fileErrors[currentFile] = $"Unexpected error: {ex.Message}";
                    if (!currentFailureCounted)
                    {
                        failedCount++;
                    }
                }

                RenderPendingFiles();
                int notAttemptedCount = totalCount - successCount - failedCount;
                lblStatus.Text = $"●  Stopped — {successCount} moved, {failedCount} failed, {notAttemptedCount} not attempted";
                MessageBox.Show(this,
                    $"Organization stopped:\n\n{ex.Message}\n\nSuccessfully moved: {successCount}\n" +
                    $"Failed: {failedCount}\nNot attempted: {notAttemptedCount}\n\nRemaining files are kept for retry.",
                    "Organization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateEmptyState("No pending files.\r\nAll scanned files have been organized.");
                SetProcessing(false);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupInitialState();
        }
    }
}
