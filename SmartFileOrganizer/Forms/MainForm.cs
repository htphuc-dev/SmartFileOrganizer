using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;
using SmartFileOrganizer.Helpers;

namespace SmartFileOrganizer
{
    public partial class MainForm : Form
    {
        private readonly FileScanner scanner;
        private readonly FileOrganizer organizer;
        private readonly SettingsStore settingsStore;
        private readonly FileUndoService undoService = new();
        private readonly FileUndoHistory undoHistory = new();

        private List<FileItem> scannedFiles = new();
        private readonly Dictionary<FileItem, string> fileErrors = new();
        private readonly HashSet<FileItem> notProcessedFiles = new();
        private readonly Dictionary<FileItem, DataGridViewRow> fileRows = new();
        private string? scannedFolderPath;
        private bool isProcessing;
        private bool closeRequested;
        private bool updatingSelection;
        private CancellationTokenSource? operationCts;
        private TaskCompletionSource? operationCompletion;

        private enum Feedback { Info, Busy, Success, Warning, Error }
        private enum PreviewState { ChooseFolder, ScanRequired, Scanning, EmptyFolder, ScanCancelled, ScanFailed, Organized, RulesChanged }

        public MainForm() : this(new SettingsStore())
        {
        }

        internal MainForm(SettingsStore settingsStore)
        {
            InitializeComponent();
            Icon = WindowPresentation.CreateApplicationIcon();
            scanner = new FileScanner();
            organizer = new FileOrganizer();
            this.settingsStore = settingsStore;
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                settingsStore.Load();
            }
            SetupInitialState();
        }

        private void SetupInitialState()
        {
            ClearScanResults();
            SetStatus("Ready. Choose a folder to begin.");
        }

        private void SetStatus(string text, Feedback feedback = Feedback.Info)
        {
            Color color = feedback switch
            {
                Feedback.Busy => Color.FromArgb(30, 115, 220),
                Feedback.Success => Color.FromArgb(21, 128, 83),
                Feedback.Warning => Color.FromArgb(180, 83, 9),
                Feedback.Error => Color.FromArgb(185, 28, 28),
                _ => Color.FromArgb(100, 116, 139)
            };
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
            picStatus.IconColor = color;
            picStatus.IconChar = feedback switch
            {
                Feedback.Busy => FontAwesome.Sharp.IconChar.Clock,
                Feedback.Success => FontAwesome.Sharp.IconChar.CheckCircle,
                Feedback.Warning => FontAwesome.Sharp.IconChar.ExclamationTriangle,
                Feedback.Error => FontAwesome.Sharp.IconChar.ExclamationCircle,
                _ => FontAwesome.Sharp.IconChar.InfoCircle
            };
            tips.SetToolTip(lblStatus, text);
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
            UpdateSelectionSummary();
        }

        private void UpdateControlState()
        {
            bool enabled = !isProcessing && !closeRequested;
            txtFolderPath.Enabled = enabled;
            btnBrowse.Enabled = enabled;
            btnScan.Enabled = enabled;
            btnSettings.Enabled = enabled;
            btnAbout.Enabled = enabled;
            btnUndo.Enabled = enabled && undoHistory.PendingCount > 0;
            btnUndo.IconColor = btnUndo.Enabled ? Color.FromArgb(36, 50, 71) : SystemColors.GrayText;
            string undoTip = undoHistory.PendingCount == 0 ? "No files available to undo in this session."
                : $"{undoHistory.PendingCount:N0} file(s) available to undo. " +
                    (enabled ? "Restore the most recent organization without overwriting existing files."
                        : "Wait for the current operation to finish.");
            tips.SetToolTip(btnUndo, undoTip);
            tips.SetToolTip(pnlUndo, undoTip);
            colSelected.ReadOnly = !enabled;
            bool canSelect = enabled && scannedFiles.Count > 0 && CurrentPathMatchesScan();
            btnSelectAll.Enabled = canSelect;
            btnClearSelection.Enabled = canSelect;
            btnOrganize.Enabled = canSelect && scannedFiles.Any(file => file.IsSelected);
            btnCancel.Enabled = isProcessing && !closeRequested && operationCts?.IsCancellationRequested == false;
        }

        private void UpdateSelectionSummary()
        {
            var summary = FileSelectionHelper.GetSummary(scannedFiles);
            lblSelectionSummary.Text = $"Files: {summary.TotalCount:N0} | Selected: {summary.SelectedCount:N0} | " +
                $"Selected size: {FileSizeFormatter.Format(summary.SelectedSize)}";
            tips.SetToolTip(lblSelectionSummary, lblSelectionSummary.Text + $" ({summary.SelectedSize:N0} bytes selected)");
            UpdateControlState();
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

            SetStatus("Cancelling... Waiting for the current file operation to finish.", Feedback.Warning);
            operationCts.Cancel();
            UpdateControlState();
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

        private void SetPreviewState(PreviewState state)
        {
            bool isEmpty = scannedFiles.Count == 0;
            pnlEmptyState.Visible = isEmpty;
            if (!isEmpty) return;

            (string title, string message, FontAwesome.Sharp.IconChar icon) = state switch
            {
                PreviewState.ChooseFolder => ("Choose a folder", "Browse for a folder to start organizing your files.", FontAwesome.Sharp.IconChar.FolderOpen),
                PreviewState.ScanRequired => ("Ready to scan", "Click Scan Files to preview files in the selected folder.", FontAwesome.Sharp.IconChar.Search),
                PreviewState.Scanning => ("Scanning your files", "Reading files and checking expected destinations. You can cancel at any time.", FontAwesome.Sharp.IconChar.Search),
                PreviewState.EmptyFolder => ("This folder is empty", "No files were found directly in this folder. Subfolders are not scanned.", FontAwesome.Sharp.IconChar.FolderOpen),
                PreviewState.ScanCancelled => ("Scan cancelled", "Click Scan Files again to load a complete preview.", FontAwesome.Sharp.IconChar.Ban),
                PreviewState.ScanFailed => ("Scan failed", "Resolve the reported error, then scan this folder again.", FontAwesome.Sharp.IconChar.ExclamationTriangle),
                PreviewState.RulesChanged => ("Classification rules updated", "Scan again to refresh groups and expected destinations.", FontAwesome.Sharp.IconChar.Gear),
                _ => ("All files organized", "Every scanned file has been moved. Scan another folder to continue.", FontAwesome.Sharp.IconChar.CheckCircle)
            };
            lblEmptyTitle.Text = title;
            lblEmptyState.Text = message;
            picEmptyState.IconChar = icon;
            picEmptyState.IconColor = state == PreviewState.Organized ? Color.FromArgb(21, 128, 83)
                : state is PreviewState.ScanFailed or PreviewState.ScanCancelled ? Color.FromArgb(180, 83, 9)
                : Color.FromArgb(30, 115, 220);
            pnlEmptyState.BringToFront();
        }

        private void ResetProgress()
        {
            progressFiles.Style = ProgressBarStyle.Blocks;
            progressFiles.Value = 0;
            progressFiles.Maximum = 1;
        }

        private void ClearScanResults(PreviewState? state = null)
        {
            scannedFolderPath = null;
            scannedFiles.Clear();
            fileErrors.Clear();
            notProcessedFiles.Clear();
            fileRows.Clear();
            dgvFiles.Rows.Clear();
            ResetProgress();
            SetPreviewState(state ?? (string.IsNullOrWhiteSpace(txtFolderPath.Text) ? PreviewState.ChooseFolder : PreviewState.ScanRequired));
            UpdateSelectionSummary();
        }

        private void txtFolderPath_TextChanged(object sender, EventArgs e)
        {
            ClearScanResults();
            SetStatus(string.IsNullOrWhiteSpace(txtFolderPath.Text) ? "Choose a folder to begin." : "Folder path changed. Scan to refresh the preview.");
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
                SetStatus("Folder selected. Click Scan Files to continue.");
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            if (isProcessing || closeRequested)
            {
                return;
            }

            using SettingsForm dialog = new(settingsStore);
            if (dialog.ShowDialog(this) == DialogResult.OK && dialog.RulesChanged)
            {
                ClearScanResults(PreviewState.RulesChanged);
                SetStatus("Classification rules changed. Scan again before organizing.", Feedback.Warning);
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (isProcessing || closeRequested) return;
            using AboutForm dialog = new();
            dialog.ShowDialog(this);
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            SetMinimumLayoutSize();
            WindowPresentation.FitToWorkingArea(this);
            txtFolderPath.Focus();
        }

        private void SetMinimumLayoutSize()
        {
            // Measure after WinForms has scaled and laid out the controls. Reserve
            // the content rows first; the preview is the only shrinking row.
            layoutMain.PerformLayout();
            int previewRow = layoutMain.GetRow(pnlPreview);
            int[] rowHeights = layoutMain.GetRowHeights();
            int minimumHeight = layoutMain.Padding.Vertical + pnlPreview.Margin.Vertical +
                pnlPreview.Height - pnlPreview.ClientSize.Height +
                dgvFiles.ColumnHeadersHeight + 2 * dgvFiles.RowTemplate.Height;
            for (int row = 0; row < rowHeights.Length; row++)
            {
                if (row != previewRow) minimumHeight += rowHeights[row];
            }

            int headerWidth = pnlHeader.Padding.Horizontal + (int)layoutHeader.ColumnStyles[0].Width +
                Math.Max(lblTitle.PreferredSize.Width, lblSubtitle.PreferredSize.Width) +
                headerActions.PreferredSize.Width + headerActions.Margin.Horizontal;
            int selectionWidth = layoutSelection.Padding.Horizontal + selectionActions.PreferredSize.Width +
                lblSelectionSummary.PreferredSize.Width + lblSelectionSummary.Margin.Horizontal;
            int gridWidth = pnlPreview.Margin.Horizontal + pnlPreview.Width - pnlPreview.ClientSize.Width +
                dgvFiles.Columns.Cast<DataGridViewColumn>().Sum(column => column.MinimumWidth) +
                SystemInformation.VerticalScrollBarWidth;
            int minimumWidth = Math.Max(Math.Max(headerWidth, selectionWidth),
                Math.Max(fileActions.PreferredSize.Width, gridWidth));
            MinimumSize = SizeFromClientSize(new Size(minimumWidth, minimumHeight));
            // FitToWorkingArea caps these measured bounds on smaller screens.
        }

        private void RenderPendingFiles()
        {
            // Build the grid once after a complete scan, never after each move.
            updatingSelection = true;
            try
            {
                DataGridViewRow[] rows = new DataGridViewRow[scannedFiles.Count];
                fileRows.Clear();
                for (int index = 0; index < scannedFiles.Count; index++)
                {
                    FileItem file = scannedFiles[index];
                    DataGridViewRow row = (DataGridViewRow)dgvFiles.RowTemplate.Clone();
                    row.CreateCells(dgvFiles, file.IsSelected, file.Name, file.Category.ToString(), file.Size,
                        file.PreviewDestinationPath, "Ready");
                    row.Tag = file;
                    row.Cells[colFileName.Index].ToolTipText = file.FullPath;
                    row.Cells[colSize.Index].ToolTipText = $"{file.Size:N0} bytes";
                    row.Cells[colDestination.Index].ToolTipText =
                        $"Expected destination (checked again when moving):\n{file.PreviewDestinationPath}";
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
            finally
            {
                updatingSelection = false;
                UpdateSelectionSummary();
            }
        }

        private bool CommitCheckboxEdit()
        {
            if (dgvFiles.IsCurrentCellDirty && dgvFiles.CurrentCell?.OwningColumn == colSelected &&
                !dgvFiles.CommitEdit(DataGridViewDataErrorContexts.Commit))
            {
                return false;
            }

            return dgvFiles.EndEdit();
        }

        private void dgvFiles_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (!isProcessing && !closeRequested && dgvFiles.IsCurrentCellDirty &&
                dgvFiles.CurrentCell?.OwningColumn == colSelected)
            {
                dgvFiles.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dgvFiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (updatingSelection || isProcessing || closeRequested || e.ColumnIndex != colSelected.Index ||
                e.RowIndex < 0 || e.RowIndex >= dgvFiles.Rows.Count)
            {
                return;
            }

            DataGridViewRow row = dgvFiles.Rows[e.RowIndex];
            if (row.Tag is FileItem file)
            {
                // Row position is only used to find the row; identity is the FileItem.
                file.IsSelected = row.Cells[colSelected.Index].Value is true;
                UpdateSelectionSummary();
            }
        }

        private void dgvFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == colSize.Index && e.Value is long bytes)
            {
                // Keep the raw long in Value for numeric sorting and totals.
                e.Value = FileSizeFormatter.Format(bytes);
                e.FormattingApplied = true;
            }
        }

        private void SetAllSelected(bool selected)
        {
            if (isProcessing || closeRequested || !CommitCheckboxEdit())
            {
                return;
            }

            updatingSelection = true;
            try
            {
                FileSelectionHelper.SetAll(scannedFiles, selected);
                foreach (var entry in fileRows)
                {
                    entry.Value.Cells[colSelected.Index].Value = entry.Key.IsSelected;
                }
            }
            finally
            {
                updatingSelection = false;
                UpdateSelectionSummary();
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllSelected(true);
        }

        private void btnClearSelection_Click(object sender, EventArgs e)
        {
            SetAllSelected(false);
        }

        private void SetRowResult(DataGridViewRow row, FileItem file)
        {
            DataGridViewCell cell = row.Cells[colResult.Index];
            fileErrors.TryGetValue(file, out string? error);
            if (notProcessedFiles.Contains(file))
            {
                cell.Value = "Not processed";
                cell.Style.ForeColor = cell.Style.SelectionForeColor = Color.FromArgb(180, 83, 9);
                cell.ToolTipText = error == null ? "Not processed in this run."
                    : $"Not processed in this run.\nPrevious failure: {error}";
            }
            else
            {
                cell.Value = error == null ? "Ready" : $"Failed: {error}";
                cell.Style.ForeColor = cell.Style.SelectionForeColor = error == null
                    ? Color.FromArgb(100, 116, 139) : Color.FromArgb(185, 28, 28);
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
            ClassificationSettings settingsSnapshot = settingsStore.Current;
            ClearScanResults();
            CancellationTokenSource source = BeginOperation();
            bool acceptProgress = true;

            try
            {
                string folderPath = NormalizeFolderPath(requestedPath);
                progressFiles.Style = ProgressBarStyle.Marquee;
                SetPreviewState(PreviewState.Scanning);
                SetStatus("Scanning files...", Feedback.Busy);
                IProgress<int> progress = new Progress<int>(count =>
                {
                    if (acceptProgress && CanReportProgress(source))
                    {
                        SetStatus($"Scanning — {count:N0} file(s) read", Feedback.Busy);
                    }
                });

                List<FileItem> files;
                try
                {
                    files = await Task.Run(() =>
                    {
                        List<FileItem> scanned = scanner.Scan(folderPath, source.Token, progress, settingsSnapshot);
                        foreach (FileItem file in scanned)
                        {
                            source.Token.ThrowIfCancellationRequested();
                            file.PreviewDestinationPath = organizer.GetPreviewDestinationPath(folderPath, file);
                        }

                        source.Token.ThrowIfCancellationRequested();
                        return scanned;
                    });
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
                SetPreviewState(PreviewState.EmptyFolder);
                SetStatus($"Scan complete — {files.Count:N0} file(s) found.", Feedback.Success);
            }
            catch (OperationCanceledException) when (source.IsCancellationRequested)
            {
                ClearScanResults(PreviewState.ScanCancelled);
                SetStatus("Scan cancelled. Scan again to load a complete preview.", Feedback.Warning);
            }
            catch (Exception ex)
            {
                ClearScanResults(PreviewState.ScanFailed);
                SetStatus("Scan failed. Resolve the error and try again.", Feedback.Error);
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
                UpdateSelectionSummary();
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

        private void RecordOrganizationResult(FileOrganizationResult result)
        {
            undoHistory.RecordOrganization(result);
            UpdateControlState();
        }

        private void ApplyUndoResults(IReadOnlyList<FileMoveResult> snapshot, FileOrganizationResult result)
        {
            undoHistory.ApplyUndoResults(snapshot, result);
            if (result.SuccessCount > 0)
            {
                // Restored files invalidate the advisory preview, not the remaining
                // Undo transaction. The next scan uses the current folder/rules.
                ClearScanResults();
            }
            UpdateControlState();
        }

        private async void btnUndo_Click(object sender, EventArgs e)
        {
            await UndoAsync();
        }

        private async Task UndoAsync()
        {
            if (isProcessing || closeRequested || undoHistory.PendingCount == 0) return;

            FileMoveResult[] snapshot = undoHistory.GetSnapshot();
            if (MessageBox.Show(this,
                $"Restore {snapshot.Length:N0} file(s) from the most recent organization?\n\n" +
                "Files will return to their original paths. Existing files will not be overwritten.\n" +
                "Changed or missing files will be kept for retry.",
                "Confirm Undo", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;

            CancellationTokenSource source = BeginOperation();
            FileOrganizationResult? result = null;
            bool acceptProgress = true;
            try
            {
                ResetProgress();
                progressFiles.Maximum = snapshot.Length;
                SetStatus($"Undoing — 0/{snapshot.Length}, 0 restored, 0 failed", Feedback.Busy);
                IProgress<FileOrganizationProgress> progress = new Progress<FileOrganizationProgress>(update =>
                {
                    if (!acceptProgress || !CanReportProgress(source)) return;
                    progressFiles.Value = update.ProcessedCount;
                    SetStatus($"Undoing — {update.ProcessedCount}/{update.TotalCount}, " +
                        $"{update.SuccessCount} restored, {update.FailedCount} failed", Feedback.Busy);
                });

                try
                {
                    // Let the worker return its journal even after cancellation.
                    result = await Task.Run(() => undoService.UndoFiles(snapshot, source.Token, progress));
                }
                finally
                {
                    acceptProgress = false;
                    if (result != null) ApplyUndoResults(snapshot, result);
                }

                progressFiles.Maximum = snapshot.Length;
                progressFiles.Value = result.ProcessedCount;
                string outcome = result.Error != null ? "Undo stopped" : result.Cancelled ? "Undo cancelled" : "Undo complete";
                SetStatus($"{outcome} — {result.ProcessedCount}/{result.TotalCount}, " +
                    $"{result.SuccessCount} restored, {result.FailedCount} failed, {undoHistory.PendingCount} available to retry",
                    result.Error != null ? Feedback.Error : result.Cancelled || result.FailedCount > 0 ? Feedback.Warning : Feedback.Success);

                if (!closeRequested && !result.Cancelled && (result.Error != null || result.FailedCount > 0))
                {
                    string failures = string.Join("\n\n", result.FileResults.Where(file => !file.Success)
                        .Take(5).Select(file => file.ErrorMessage));
                    MessageBox.Show(this,
                        $"Restored: {result.SuccessCount}\nFailed: {result.FailedCount}\n" +
                        $"Available to retry: {undoHistory.PendingCount}\n\n" +
                        (result.Error?.Message ?? failures), "Undo — Remaining Files",
                        MessageBoxButtons.OK, result.Error != null ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Undo stopped — {undoHistory.PendingCount} file(s) available to retry.", Feedback.Error);
                if (!closeRequested)
                    MessageBox.Show(this, $"Unable to finish Undo:\n\n{ex.Message}",
                        "Undo Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                acceptProgress = false;
                // Release busy before recomputing availability from Undo history.
                EndOperation(source);
            }
        }

        private async void btnOrganize_Click(object sender, EventArgs e)
        {
            await OrganizeAsync();
        }

        private async Task OrganizeAsync()
        {
            if (isProcessing || closeRequested || !CommitCheckboxEdit())
            {
                return;
            }

            if (!CurrentPathMatchesScan())
            {
                ClearScanResults();
                SetStatus("Scan required. The folder path no longer matches the preview.", Feedback.Warning);
                MessageBox.Show(this, "The folder path does not match a successful scan. Please scan again.",
                    "Scan Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            FileItem[] files = FileSelectionHelper.GetSelectedFiles(scannedFiles);
            if (files.Length == 0)
            {
                UpdateSelectionSummary();
                return;
            }

            // Capture all worker inputs on the UI thread, before starting any work.
            string folderPath = scannedFolderPath!;
            long selectedSize = FileSelectionHelper.GetSummary(files).SelectedSize;
            DialogResult confirmation = MessageBox.Show(this,
                $"Organize {files.Length} selected file(s) ({FileSizeFormatter.Format(selectedSize)})?\n\n" +
                $"Source folder:\n{folderPath}\n\nFiles will be moved into category folders.",
                "Confirm Organization", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
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
                SetStatus($"Organizing — 0/{files.Length}, 0 moved, 0 failed", Feedback.Busy);
                IProgress<FileOrganizationProgress> progress = new Progress<FileOrganizationProgress>(update =>
                {
                    if (!acceptProgress || !CanReportProgress(source))
                    {
                        return;
                    }

                    ApplyOrganizationResults(files, update.FileResults,
                        update.ProcessedCount - update.FileResults.Count, applied);
                    progressFiles.Value = update.ProcessedCount;
                    SetStatus($"Organizing — {update.ProcessedCount}/{update.TotalCount}, " +
                        $"{update.SuccessCount} moved, {update.FailedCount} failed", Feedback.Busy);
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
                    // Persist the authoritative moves before updating/removing
                    // preview rows, including cancelled/partially failed batches.
                    if (result != null) RecordOrganizationResult(result);
                }

                // Reconcile from the authoritative results, even when reports were
                // delayed, skipped during cancellation, or never delivered.
                ApplyOrganizationResults(files, result.FileResults, 0, applied);
                MarkUnprocessed(files, result.ProcessedCount);
                progressFiles.Value = result.ProcessedCount;
                string outcome = result.Error != null ? "Stopped" : result.Cancelled ? "Cancelled" : "Organized";
                SetStatus($"{outcome} — {result.ProcessedCount}/{result.TotalCount}, " +
                    $"{result.SuccessCount} moved, {result.FailedCount} failed, {result.TotalCount - result.ProcessedCount} not processed",
                    result.Error != null ? Feedback.Error : result.Cancelled || result.FailedCount > 0 ? Feedback.Warning : Feedback.Success);

                if (!closeRequested && !result.Cancelled && (result.Error != null || result.FailedCount > 0))
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
                SetStatus($"Stopped — {processedCount}/{files.Length}, " +
                    $"{successCount} moved, {failedCount} failed, {files.Length - processedCount} not processed", Feedback.Error);
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
                    SetPreviewState(PreviewState.Organized);
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
            if (!DesignMode && settingsStore.LoadWarning != null)
            {
                SetStatus("Settings could not be loaded. Using temporary defaults.", Feedback.Warning);
                MessageBox.Show(this, settingsStore.LoadWarning, "Settings — Temporary Defaults",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
