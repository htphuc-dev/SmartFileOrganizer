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
        private readonly HashSet<FileItem> notProcessedFiles = new();
        private readonly Dictionary<FileItem, DataGridViewRow> fileRows = new();
        private string? scannedFolderPath;
        private bool isProcessing;
        private bool closeRequested;
        private CancellationTokenSource? operationCts;
        private TaskCompletionSource? operationCompletion;

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
            bool enabled = !processing && !closeRequested;
            txtFolderPath.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            btnScan.Enabled = enabled;
            btnOrganize.Enabled = enabled && scannedFiles.Count > 0 && CurrentPathMatchesScan();
            btnCancel.Enabled = processing && !closeRequested && operationCts?.IsCancellationRequested == false;
        }

        private CancellationTokenSource BeginOperation()
        {
            CancellationTokenSource source = new();
            operationCts = source;
            operationCompletion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            SetProcessing(true);
            return source;
        }

        private void EndOperation(CancellationTokenSource source)
        {
            try
            {
                operationCts = null;
                source.Dispose();
                SetProcessing(false);
            }
            finally
            {
                TaskCompletionSource? completion = operationCompletion;
                operationCompletion = null;
                completion?.TrySetResult();
            }
        }

        private bool CanReportProgress(CancellationTokenSource source)
        {
            return ReferenceEquals(operationCts, source) && !source.IsCancellationRequested
                && !closeRequested && !IsDisposed && !Disposing;
        }

        private void RequestCancellation()
        {
            if (operationCts == null || operationCts.IsCancellationRequested)
            {
                return;
            }

            lblStatus.Text = "●  Cancelling...";
            btnCancel.Enabled = false;
            operationCts.Cancel();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            RequestCancellation();
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isProcessing)
            {
                return;
            }

            e.Cancel = true;
            if (closeRequested)
            {
                return;
            }

            closeRequested = true;
            Task completion = operationCompletion!.Task;
            RequestCancellation();
            await completion;

            if (!IsDisposed && !Disposing)
            {
                Close();
            }
        }

        private void UpdateEmptyState(string message)
        {
            bool isEmpty = scannedFiles.Count == 0;
            lblEmptyState.Text = message;
            lblEmptyState.Visible = isEmpty;
            picEmptyState.Visible = isEmpty;
        }

        private void ResetProgress()
        {
            progressFiles.Style = ProgressBarStyle.Blocks;
            progressFiles.Value = 0;
            progressFiles.Maximum = 1;
        }

        private void ClearScanResults()
        {
            scannedFolderPath = null;
            scannedFiles.Clear();
            fileErrors.Clear();
            notProcessedFiles.Clear();
            fileRows.Clear();
            dgvFiles.Rows.Clear();
            btnOrganize.Enabled = false;
            ResetProgress();
            UpdateEmptyState("No files to display\r\nSelect a folder and click \"Scan Files\" to get started.");
        }

        private void txtFolderPath_TextChanged(object sender, EventArgs e)
        {
            ClearScanResults();
            lblStatus.Text = "●  Folder path changed — scan again";
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (isProcessing || closeRequested)
            {
                return;
            }

            using FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "Select a folder to organize";
            dialog.UseDescriptionForTitle = true;

            if (!string.IsNullOrWhiteSpace(txtFolderPath.Text) && Directory.Exists(txtFolderPath.Text))
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
            // Build the grid once after a complete scan, never after each move.
            DataGridViewRow[] rows = new DataGridViewRow[scannedFiles.Count];
            fileRows.Clear();
            for (int index = 0; index < scannedFiles.Count; index++)
            {
                FileItem file = scannedFiles[index];
                DataGridViewRow row = (DataGridViewRow)dgvFiles.RowTemplate.Clone();
                row.CreateCells(dgvFiles, file.Name, file.Category.ToString(),
                    organizer.GetDestinationFolder(scannedFolderPath!, file), "Ready");
                row.Tag = file;
                SetRowResult(row, file);
                rows[index] = row;
                fileRows.Add(file, row);
            }

            dgvFiles.Rows.Clear();
            if (rows.Length > 0)
            {
                dgvFiles.Rows.AddRange(rows);
            }
        }

        private void SetRowResult(DataGridViewRow row, FileItem file)
        {
            DataGridViewCell cell = row.Cells[colResult.Index];
            fileErrors.TryGetValue(file, out string? error);
            if (notProcessedFiles.Contains(file))
            {
                cell.Value = "Not processed";
                cell.ToolTipText = error == null ? "Not processed in this run."
                    : $"Not processed in this run.\nPrevious failure: {error}";
            }
            else
            {
                cell.Value = error == null ? "Ready" : $"Failed: {error}";
                cell.ToolTipText = error ?? string.Empty;
            }
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            await ScanAsync();
        }

        private async Task ScanAsync()
        {
            if (isProcessing || closeRequested)
            {
                return;
            }

            string requestedPath = txtFolderPath.Text;
            ClearScanResults();
            CancellationTokenSource source = BeginOperation();
            bool acceptProgress = true;

            try
            {
                string folderPath = NormalizeFolderPath(requestedPath);
                progressFiles.Style = ProgressBarStyle.Marquee;
                lblStatus.Text = "●  Scanning files...";
                IProgress<int> progress = new Progress<int>(count =>
                {
                    if (acceptProgress && CanReportProgress(source))
                    {
                        lblStatus.Text = $"●  Scanning — {count} file(s) read";
                    }
                });

                List<FileItem> files;
                try
                {
                    files = await Task.Run(() => scanner.Scan(folderPath, source.Token, progress));
                }
                finally
                {
                    // Queued callbacks must not overwrite the final state.
                    acceptProgress = false;
                }

                source.Token.ThrowIfCancellationRequested();
                scannedFiles = files;
                scannedFolderPath = folderPath;
                RenderPendingFiles();
                progressFiles.Style = ProgressBarStyle.Blocks;
                progressFiles.Maximum = Math.Max(1, files.Count);
                progressFiles.Value = files.Count;
                UpdateEmptyState("This folder contains no files.\r\nSubfolders are not scanned.");
                lblStatus.Text = $"●  Scan complete — {files.Count} file(s)";
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested)
            {
                ClearScanResults();
                UpdateEmptyState("Scan cancelled.\r\nScan again to load a complete list.");
                lblStatus.Text = "●  Scan cancelled — scan again";
            }
            catch (Exception ex)
            {
                ClearScanResults();
                UpdateEmptyState("Scan failed.\r\nResolve the reported error and scan again.");
                lblStatus.Text = "●  Scan failed";
                if (!closeRequested)
                {
                    MessageBox.Show(this, $"Unable to scan the selected folder:\n\n{ex.Message}",
                        "Scan Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                acceptProgress = false;
                EndOperation(source);
            }
        }

        private void ApplyOrganizationResults(IReadOnlyList<FileItem> files,
            IReadOnlyList<FileMoveResult> results, int firstIndex, HashSet<FileItem> applied)
        {
            dgvFiles.SuspendLayout();
            try
            {
                for (int index = 0; index < results.Count; index++)
                {
                    FileItem file = files[firstIndex + index];
                    if (applied.Contains(file))
                    {
                        continue;
                    }

                    FileMoveResult result = results[index];
                    notProcessedFiles.Remove(file);
                    if (result.Success)
                    {
                        // Update the retry list before doing any further UI work.
                        scannedFiles.Remove(file);
                        fileErrors.Remove(file);
                        applied.Add(file);
                        if (fileRows.Remove(file, out DataGridViewRow? row))
                        {
                            dgvFiles.Rows.Remove(row);
                        }
                    }
                    else
                    {
                        fileErrors[file] = result.ErrorMessage;
                        applied.Add(file);
                        if (fileRows.TryGetValue(file, out DataGridViewRow? row))
                        {
                            SetRowResult(row, file);
                        }
                    }
                }
            }
            finally
            {
                dgvFiles.ResumeLayout();
            }
        }

        private void MarkUnprocessed(IReadOnlyList<FileItem> files, int processedCount)
        {
            for (int index = processedCount; index < files.Count; index++)
            {
                FileItem file = files[index];
                notProcessedFiles.Add(file);
                if (fileRows.TryGetValue(file, out DataGridViewRow? row))
                {
                    SetRowResult(row, file);
                }
            }
        }

        private async void btnOrganize_Click(object sender, EventArgs e)
        {
            await OrganizeAsync();
        }

        private async Task OrganizeAsync()
        {
            if (isProcessing || closeRequested)
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
                return;
            }

            // Capture all worker inputs on the UI thread, before starting any work.
            string folderPath = scannedFolderPath!;
            FileItem[] files = scannedFiles.ToArray();
            DialogResult confirmation = MessageBox.Show(this,
                $"Organize {files.Length} file(s)?\n\nFiles will be moved into category folders.",
                "Confirm Organization", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirmation != DialogResult.Yes)
            {
                return;
            }

            CancellationTokenSource source = BeginOperation();
            HashSet<FileItem> applied = new();
            FileOrganizationResult? result = null;
            bool acceptProgress = true;

            try
            {
                ResetProgress();
                progressFiles.Maximum = files.Length;
                lblStatus.Text = $"●  Organizing — 0/{files.Length}, 0 moved, 0 failed";
                IProgress<FileOrganizationProgress> progress = new Progress<FileOrganizationProgress>(update =>
                {
                    if (!acceptProgress || !CanReportProgress(source))
                    {
                        return;
                    }

                    ApplyOrganizationResults(files, update.FileResults,
                        update.ProcessedCount - update.FileResults.Count, applied);
                    progressFiles.Value = update.ProcessedCount;
                    lblStatus.Text = $"●  Organizing — {update.ProcessedCount}/{update.TotalCount}, " +
                        $"{update.SuccessCount} moved, {update.FailedCount} failed";
                });

                try
                {
                    // Do not cancel Task.Run itself: the batch must return its
                    // journal, including a move that completed after Cancel.
                    result = await Task.Run(() => organizer.OrganizeFiles(folderPath, files, source.Token, progress));
                }
                finally
                {
                    acceptProgress = false;
                }

                // Reconcile from the authoritative results, even when reports were
                // delayed, skipped during cancellation, or never delivered.
                ApplyOrganizationResults(files, result.FileResults, 0, applied);
                MarkUnprocessed(files, result.ProcessedCount);
                progressFiles.Value = result.ProcessedCount;
                string outcome = result.Error != null ? "Stopped" : result.Cancelled ? "Cancelled" : "Organized";
                lblStatus.Text = $"●  {outcome} — {result.ProcessedCount}/{result.TotalCount}, " +
                    $"{result.SuccessCount} moved, {result.FailedCount} failed";

                if (!closeRequested && !result.Cancelled)
                {
                    string details = $"Successfully moved: {result.SuccessCount}\nFailed: {result.FailedCount}\n" +
                        $"Not processed: {result.TotalCount - result.ProcessedCount}";
                    MessageBox.Show(this,
                        (result.Error == null ? "Organization completed." : $"Organization stopped:\n{result.Error.Message}") +
                        $"\n\n{details}" + (scannedFiles.Count > 0 ? "\n\nRemaining files are kept for retry." : ""),
                        result.Error == null ? "Organization Complete" : "Organization Error",
                        MessageBoxButtons.OK, result.Error != null ? MessageBoxIcon.Error
                            : result.FailedCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (result != null)
                {
                    ApplyOrganizationResults(files, result.FileResults, 0, applied);
                }

                MarkUnprocessed(files, result?.ProcessedCount ?? 0);
                int processedCount = result?.ProcessedCount ?? 0;
                int successCount = result?.SuccessCount ?? 0;
                int failedCount = result?.FailedCount ?? 0;
                progressFiles.Value = processedCount;
                lblStatus.Text = $"●  Stopped — {processedCount}/{files.Length}, " +
                    $"{successCount} moved, {failedCount} failed";
                if (!closeRequested)
                {
                    MessageBox.Show(this, $"Organization stopped:\n\n{ex.Message}\n\n" +
                        $"Successfully moved: {successCount}\nFailed: {failedCount}\n" +
                        $"Not processed: {files.Length - processedCount}\n\nRemaining files are kept for retry.",
                        "Organization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                acceptProgress = false;
                try
                {
                    UpdateEmptyState("No pending files.\r\nAll scanned files have been organized.");
                }
                finally
                {
                    EndOperation(source);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetupInitialState();
        }
    }
}
