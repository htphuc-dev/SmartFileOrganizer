namespace SmartFileOrganizer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layoutMain;
        private TableLayoutPanel layoutHeader;
        private TableLayoutPanel layoutFolder;
        private TableLayoutPanel layoutFolderPath;
        private TableLayoutPanel layoutSelection;
        private TableLayoutPanel layoutEmpty;
        private TableLayoutPanel layoutProgress;
        private TableLayoutPanel layoutStatus;
        private Panel pnlHeader;
        private Panel pnlPreview;
        private Panel pnlEmptyState;
        private Panel pnlUndo;
        private FlowLayoutPanel headerActions;
        private FlowLayoutPanel fileActions;
        private FlowLayoutPanel selectionActions;
        private FontAwesome.Sharp.IconButton btnSettings;
        private FontAwesome.Sharp.IconButton btnAbout;
        private FontAwesome.Sharp.IconButton btnBrowse;
        private FontAwesome.Sharp.IconButton btnScan;
        private FontAwesome.Sharp.IconButton btnOrganize;
        private FontAwesome.Sharp.IconButton btnUndo;
        private FontAwesome.Sharp.IconButton btnSelectAll;
        private FontAwesome.Sharp.IconButton btnClearSelection;
        private FontAwesome.Sharp.IconButton btnCancel;
        private FontAwesome.Sharp.IconPictureBox picLogo;
        private FontAwesome.Sharp.IconPictureBox picEmptyState;
        private FontAwesome.Sharp.IconPictureBox picStatus;
        private Label lblTitle;
        private Label lblSubtitle;
        private Label lblFolder;
        private Label lblSelectionSummary;
        private Label lblEmptyTitle;
        private Label lblEmptyState;
        private Label lblStatus;
        private TextBox txtFolderPath;
        private ProgressBar progressFiles;
        private DataGridView dgvFiles;
        private DataGridViewCheckBoxColumn colSelected;
        private DataGridViewTextBoxColumn colFileName;
        private DataGridViewTextBoxColumn colCategory;
        private DataGridViewTextBoxColumn colSize;
        private DataGridViewTextBoxColumn colDestination;
        private DataGridViewTextBoxColumn colResult;
        private ToolTip tips;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle4 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            layoutMain = new TableLayoutPanel();
            pnlHeader = new Panel();
            layoutHeader = new TableLayoutPanel();
            picLogo = new FontAwesome.Sharp.IconPictureBox();
            lblTitle = new Label();
            lblSubtitle = new Label();
            headerActions = new FlowLayoutPanel();
            btnSettings = new FontAwesome.Sharp.IconButton();
            btnAbout = new FontAwesome.Sharp.IconButton();
            layoutFolder = new TableLayoutPanel();
            layoutFolderPath = new TableLayoutPanel();
            lblFolder = new Label();
            txtFolderPath = new TextBox();
            btnBrowse = new FontAwesome.Sharp.IconButton();
            fileActions = new FlowLayoutPanel();
            btnScan = new FontAwesome.Sharp.IconButton();
            btnOrganize = new FontAwesome.Sharp.IconButton();
            pnlUndo = new Panel();
            btnUndo = new FontAwesome.Sharp.IconButton();
            layoutSelection = new TableLayoutPanel();
            selectionActions = new FlowLayoutPanel();
            btnSelectAll = new FontAwesome.Sharp.IconButton();
            btnClearSelection = new FontAwesome.Sharp.IconButton();
            lblSelectionSummary = new Label();
            pnlPreview = new Panel();
            dgvFiles = new DataGridView();
            colSelected = new DataGridViewCheckBoxColumn();
            colFileName = new DataGridViewTextBoxColumn();
            colCategory = new DataGridViewTextBoxColumn();
            colSize = new DataGridViewTextBoxColumn();
            colDestination = new DataGridViewTextBoxColumn();
            colResult = new DataGridViewTextBoxColumn();
            pnlEmptyState = new Panel();
            layoutEmpty = new TableLayoutPanel();
            picEmptyState = new FontAwesome.Sharp.IconPictureBox();
            lblEmptyTitle = new Label();
            lblEmptyState = new Label();
            layoutProgress = new TableLayoutPanel();
            progressFiles = new ProgressBar();
            btnCancel = new FontAwesome.Sharp.IconButton();
            layoutStatus = new TableLayoutPanel();
            picStatus = new FontAwesome.Sharp.IconPictureBox();
            lblStatus = new Label();
            tips = new ToolTip(components);
            layoutMain.SuspendLayout();
            pnlHeader.SuspendLayout();
            layoutHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            headerActions.SuspendLayout();
            layoutFolder.SuspendLayout();
            layoutFolderPath.SuspendLayout();
            fileActions.SuspendLayout();
            pnlUndo.SuspendLayout();
            layoutSelection.SuspendLayout();
            selectionActions.SuspendLayout();
            pnlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvFiles).BeginInit();
            pnlEmptyState.SuspendLayout();
            layoutEmpty.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picEmptyState).BeginInit();
            layoutProgress.SuspendLayout();
            layoutStatus.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picStatus).BeginInit();
            SuspendLayout();
            //
            // layoutMain
            //
            layoutMain.ColumnCount = 1;
            layoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutMain.Controls.Add(pnlHeader, 0, 0);
            layoutMain.Controls.Add(layoutFolder, 0, 1);
            layoutMain.Controls.Add(fileActions, 0, 2);
            layoutMain.Controls.Add(layoutSelection, 0, 3);
            layoutMain.Controls.Add(pnlPreview, 0, 4);
            layoutMain.Controls.Add(layoutProgress, 0, 5);
            layoutMain.Controls.Add(layoutStatus, 0, 6);
            layoutMain.Dock = DockStyle.Fill;
            layoutMain.Location = new Point(0, 0);
            layoutMain.Margin = new Padding(0);
            layoutMain.Name = "layoutMain";
            layoutMain.RowCount = 7;
            layoutMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutMain.RowStyles.Add(new RowStyle());
            layoutMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutMain.Size = new Size(1234, 907);
            layoutMain.TabIndex = 0;
            //
            // pnlHeader
            //
            pnlHeader.BackColor = Color.FromArgb(30, 115, 220);
            pnlHeader.AutoSize = true;
            pnlHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            pnlHeader.Controls.Add(layoutHeader);
            pnlHeader.Dock = DockStyle.Fill;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Margin = new Padding(0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.MinimumSize = new Size(0, 107);
            pnlHeader.Padding = new Padding(23, 16, 23, 16);
            pnlHeader.Size = new Size(1234, 107);
            pnlHeader.TabIndex = 4;
            //
            // layoutHeader
            //
            layoutHeader.ColumnCount = 3;
            layoutHeader.AutoSize = true;
            layoutHeader.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64F));
            layoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutHeader.ColumnStyles.Add(new ColumnStyle());
            layoutHeader.Controls.Add(picLogo, 0, 0);
            layoutHeader.Controls.Add(lblTitle, 1, 0);
            layoutHeader.Controls.Add(lblSubtitle, 1, 1);
            layoutHeader.Controls.Add(headerActions, 2, 0);
            layoutHeader.Dock = DockStyle.Fill;
            layoutHeader.Location = new Point(23, 16);
            layoutHeader.Margin = new Padding(0);
            layoutHeader.Name = "layoutHeader";
            layoutHeader.RowCount = 2;
            layoutHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutHeader.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutHeader.Size = new Size(1188, 75);
            layoutHeader.TabIndex = 0;
            //
            // picLogo
            //
            picLogo.Anchor = AnchorStyles.Left;
            picLogo.BackColor = Color.Transparent;
            picLogo.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            picLogo.IconColor = Color.White;
            picLogo.IconFont = FontAwesome.Sharp.IconFont.Auto;
            picLogo.IconSize = 50;
            picLogo.Location = new Point(3, 8);
            picLogo.Margin = new Padding(3, 4, 3, 4);
            picLogo.Name = "picLogo";
            layoutHeader.SetRowSpan(picLogo, 2);
            picLogo.Size = new Size(50, 59);
            picLogo.TabIndex = 0;
            picLogo.TabStop = false;
            //
            // lblTitle
            //
            lblTitle.AutoEllipsis = true;
            lblTitle.AutoSize = true;
            lblTitle.Dock = DockStyle.Fill;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(64, 0);
            lblTitle.Margin = new Padding(0);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(859, 41);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Smart File Organizer";
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // lblSubtitle
            //
            lblSubtitle.AutoEllipsis = true;
            lblSubtitle.AutoSize = true;
            lblSubtitle.Dock = DockStyle.Fill;
            lblSubtitle.ForeColor = Color.FromArgb(225, 239, 255);
            lblSubtitle.Location = new Point(64, 41);
            lblSubtitle.Margin = new Padding(0);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(859, 34);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = "Preview your files. Organize with confidence.";
            lblSubtitle.TextAlign = ContentAlignment.MiddleLeft;
            //
            // headerActions
            //
            headerActions.Anchor = AnchorStyles.Right;
            headerActions.AutoSize = true;
            headerActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            headerActions.Controls.Add(btnSettings);
            headerActions.Controls.Add(btnAbout);
            headerActions.Location = new Point(937, 13);
            headerActions.Margin = new Padding(14, 0, 0, 0);
            headerActions.Name = "headerActions";
            layoutHeader.SetRowSpan(headerActions, 2);
            headerActions.Size = new Size(251, 48);
            headerActions.TabIndex = 0;
            headerActions.WrapContents = false;
            //
            // btnSettings
            //
            btnSettings.BackColor = Color.FromArgb(49, 128, 226);
            btnSettings.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnSettings.FlatAppearance.BorderSize = 0;
            btnSettings.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 138, 236);
            btnSettings.FlatStyle = FlatStyle.Flat;
            btnSettings.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSettings.ForeColor = Color.White;
            btnSettings.IconChar = FontAwesome.Sharp.IconChar.Cog;
            btnSettings.IconColor = Color.White;
            btnSettings.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSettings.IconSize = 17;
            btnSettings.Location = new Point(0, 0);
            btnSettings.Margin = new Padding(0, 0, 9, 0);
            btnSettings.Name = "btnSettings";
            btnSettings.Padding = new Padding(7, 0, 7, 0);
            btnSettings.Size = new Size(123, 48);
            btnSettings.TabIndex = 0;
            btnSettings.Text = "Settings";
            btnSettings.TextImageRelation = TextImageRelation.ImageBeforeText;
            tips.SetToolTip(btnSettings, "Manage classification rules. Available when no task is running.");
            btnSettings.UseVisualStyleBackColor = false;
            btnSettings.Click += btnSettings_Click;
            //
            // btnAbout
            //
            btnAbout.BackColor = Color.FromArgb(49, 128, 226);
            btnAbout.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.FlatAppearance.MouseOverBackColor = Color.FromArgb(59, 138, 236);
            btnAbout.FlatStyle = FlatStyle.Flat;
            btnAbout.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnAbout.ForeColor = Color.White;
            btnAbout.IconChar = FontAwesome.Sharp.IconChar.CircleInfo;
            btnAbout.IconColor = Color.White;
            btnAbout.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnAbout.IconSize = 17;
            btnAbout.Location = new Point(132, 0);
            btnAbout.Margin = new Padding(0, 0, 9, 0);
            btnAbout.Name = "btnAbout";
            btnAbout.Padding = new Padding(7, 0, 7, 0);
            btnAbout.Size = new Size(110, 48);
            btnAbout.TabIndex = 1;
            btnAbout.Text = "About";
            btnAbout.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnAbout.UseVisualStyleBackColor = false;
            btnAbout.Click += btnAbout_Click;
            //
            // layoutFolder
            //
            layoutFolder.AutoSize = true;
            layoutFolder.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutFolder.ColumnCount = 2;
            layoutFolder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutFolder.ColumnStyles.Add(new ColumnStyle());
            layoutFolder.Controls.Add(lblFolder, 0, 0);
            layoutFolder.Controls.Add(layoutFolderPath, 0, 1);
            layoutFolder.Controls.Add(btnBrowse, 1, 1);
            layoutFolder.Dock = DockStyle.Fill;
            layoutFolder.Location = new Point(0, 107);
            layoutFolder.Margin = new Padding(0);
            layoutFolder.Name = "layoutFolder";
            layoutFolder.Padding = new Padding(23, 19, 23, 0);
            layoutFolder.RowCount = 2;
            layoutFolder.RowStyles.Add(new RowStyle());
            layoutFolder.RowStyles.Add(new RowStyle());
            layoutFolder.Size = new Size(1234, 95);
            layoutFolder.TabIndex = 0;
            //
            // lblFolder
            //
            lblFolder.AutoSize = true;
            layoutFolder.SetColumnSpan(lblFolder, 2);
            lblFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFolder.Location = new Point(23, 19);
            lblFolder.Margin = new Padding(0, 0, 0, 8);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(136, 20);
            lblFolder.TabIndex = 0;
            lblFolder.Text = "Folder to organize";
            //
            // layoutFolderPath
            //
            layoutFolderPath.AutoSize = true;
            layoutFolderPath.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutFolderPath.BackColor = Color.White;
            layoutFolderPath.BorderStyle = BorderStyle.FixedSingle;
            layoutFolderPath.ColumnCount = 1;
            layoutFolderPath.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutFolderPath.Controls.Add(txtFolderPath, 0, 0);
            layoutFolderPath.Dock = DockStyle.Fill;
            layoutFolderPath.Location = new Point(23, 47);
            layoutFolderPath.Margin = new Padding(0, 0, 14, 0);
            layoutFolderPath.MinimumSize = new Size(0, 48);
            layoutFolderPath.Name = "layoutFolderPath";
            layoutFolderPath.Padding = new Padding(11, 0, 11, 0);
            layoutFolderPath.RowCount = 1;
            layoutFolderPath.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layoutFolderPath.Size = new Size(1037, 48);
            layoutFolderPath.TabIndex = 0;
            //
            // txtFolderPath
            //
            txtFolderPath.AccessibleName = "Folder path";
            txtFolderPath.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtFolderPath.BackColor = Color.White;
            txtFolderPath.BorderStyle = BorderStyle.None;
            txtFolderPath.Font = new Font("Segoe UI", 10F);
            txtFolderPath.Location = new Point(11, 11);
            txtFolderPath.Margin = new Padding(0);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.PlaceholderText = "Choose a folder to get started";
            txtFolderPath.Size = new Size(1013, 23);
            txtFolderPath.TabIndex = 0;
            txtFolderPath.TextChanged += txtFolderPath_TextChanged;
            //
            // btnBrowse
            //
            btnBrowse.BackColor = Color.White;
            btnBrowse.AutoSize = true;
            btnBrowse.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnBrowse.Dock = DockStyle.Fill;
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnBrowse.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBrowse.ForeColor = Color.FromArgb(36, 50, 71);
            btnBrowse.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnBrowse.IconColor = Color.FromArgb(36, 50, 71);
            btnBrowse.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnBrowse.IconSize = 17;
            btnBrowse.Location = new Point(1065, 47);
            btnBrowse.Margin = new Padding(0);
            btnBrowse.MinimumSize = new Size(137, 48);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Padding = new Padding(7, 0, 7, 0);
            btnBrowse.Size = new Size(137, 48);
            btnBrowse.TabIndex = 1;
            btnBrowse.Text = "Browse";
            btnBrowse.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnBrowse.UseVisualStyleBackColor = false;
            btnBrowse.Click += btnBrowse_Click;
            //
            // fileActions
            //
            fileActions.AutoSize = true;
            fileActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            fileActions.Controls.Add(btnScan);
            fileActions.Controls.Add(btnOrganize);
            fileActions.Controls.Add(pnlUndo);
            fileActions.Dock = DockStyle.Fill;
            fileActions.Location = new Point(0, 202);
            fileActions.Margin = new Padding(0);
            fileActions.Name = "fileActions";
            fileActions.Padding = new Padding(23, 16, 23, 5);
            fileActions.Size = new Size(1234, 69);
            fileActions.TabIndex = 1;
            fileActions.WrapContents = false;
            //
            // btnScan
            //
            btnScan.BackColor = Color.White;
            btnScan.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnScan.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnScan.ForeColor = Color.FromArgb(36, 50, 71);
            btnScan.IconChar = FontAwesome.Sharp.IconChar.Search;
            btnScan.IconColor = Color.FromArgb(36, 50, 71);
            btnScan.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnScan.IconSize = 17;
            btnScan.Location = new Point(23, 16);
            btnScan.Margin = new Padding(0, 0, 9, 0);
            btnScan.Name = "btnScan";
            btnScan.Padding = new Padding(7, 0, 7, 0);
            btnScan.Size = new Size(153, 48);
            btnScan.TabIndex = 0;
            btnScan.Text = "Scan Files";
            btnScan.TextImageRelation = TextImageRelation.ImageBeforeText;
            tips.SetToolTip(btnScan, "Read files in the selected folder. Subfolders are not scanned.");
            btnScan.UseVisualStyleBackColor = false;
            btnScan.Click += btnScan_Click;
            //
            // btnOrganize
            //
            btnOrganize.BackColor = Color.FromArgb(30, 115, 220);
            btnOrganize.Enabled = false;
            btnOrganize.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnOrganize.FlatAppearance.BorderSize = 0;
            btnOrganize.FlatAppearance.MouseOverBackColor = Color.FromArgb(25, 98, 190);
            btnOrganize.FlatStyle = FlatStyle.Flat;
            btnOrganize.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOrganize.ForeColor = Color.White;
            btnOrganize.IconChar = FontAwesome.Sharp.IconChar.FolderBlank;
            btnOrganize.IconColor = Color.White;
            btnOrganize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnOrganize.IconSize = 17;
            btnOrganize.Location = new Point(185, 16);
            btnOrganize.Margin = new Padding(0, 0, 9, 0);
            btnOrganize.Name = "btnOrganize";
            btnOrganize.Padding = new Padding(7, 0, 7, 0);
            btnOrganize.Size = new Size(171, 48);
            btnOrganize.TabIndex = 1;
            btnOrganize.Text = "Organize";
            btnOrganize.TextImageRelation = TextImageRelation.ImageBeforeText;
            tips.SetToolTip(btnOrganize, "Move only selected files after confirmation.");
            btnOrganize.UseVisualStyleBackColor = false;
            btnOrganize.Click += btnOrganize_Click;
            //
            // pnlUndo
            //
            pnlUndo.Controls.Add(btnUndo);
            pnlUndo.Location = new Point(365, 16);
            pnlUndo.Margin = new Padding(0, 0, 9, 0);
            pnlUndo.Name = "pnlUndo";
            pnlUndo.Size = new Size(121, 48);
            pnlUndo.TabIndex = 2;
            tips.SetToolTip(pnlUndo, "No files available to undo in this session.");
            //
            // btnUndo
            //
            btnUndo.BackColor = Color.White;
            btnUndo.Dock = DockStyle.Fill;
            btnUndo.Enabled = false;
            btnUndo.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnUndo.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnUndo.FlatStyle = FlatStyle.Flat;
            btnUndo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnUndo.ForeColor = Color.FromArgb(36, 50, 71);
            btnUndo.IconChar = FontAwesome.Sharp.IconChar.ArrowRotateLeft;
            btnUndo.IconColor = Color.FromArgb(36, 50, 71);
            btnUndo.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnUndo.IconSize = 17;
            btnUndo.Location = new Point(0, 0);
            btnUndo.Margin = new Padding(0, 0, 9, 0);
            btnUndo.Name = "btnUndo";
            btnUndo.Padding = new Padding(7, 0, 7, 0);
            btnUndo.Size = new Size(121, 48);
            btnUndo.TabIndex = 2;
            btnUndo.Text = "Undo";
            btnUndo.TextImageRelation = TextImageRelation.ImageBeforeText;
            tips.SetToolTip(btnUndo, "No files available to undo in this session.");
            btnUndo.UseVisualStyleBackColor = false;
            btnUndo.Click += btnUndo_Click;
            //
            // layoutSelection
            //
            layoutSelection.AutoSize = true;
            layoutSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutSelection.ColumnCount = 2;
            layoutSelection.ColumnStyles.Add(new ColumnStyle());
            layoutSelection.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutSelection.Controls.Add(selectionActions, 0, 0);
            layoutSelection.Controls.Add(lblSelectionSummary, 1, 0);
            layoutSelection.Dock = DockStyle.Fill;
            layoutSelection.Location = new Point(0, 271);
            layoutSelection.Margin = new Padding(0);
            layoutSelection.Name = "layoutSelection";
            layoutSelection.Padding = new Padding(23, 5, 23, 11);
            layoutSelection.RowCount = 1;
            layoutSelection.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutSelection.Size = new Size(1234, 64);
            layoutSelection.TabIndex = 2;
            //
            // selectionActions
            //
            selectionActions.AutoSize = true;
            selectionActions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            selectionActions.Controls.Add(btnSelectAll);
            selectionActions.Controls.Add(btnClearSelection);
            selectionActions.Dock = DockStyle.Fill;
            selectionActions.Location = new Point(23, 5);
            selectionActions.Margin = new Padding(0);
            selectionActions.Name = "selectionActions";
            selectionActions.Size = new Size(315, 48);
            selectionActions.TabIndex = 0;
            selectionActions.WrapContents = false;
            //
            // btnSelectAll
            //
            btnSelectAll.BackColor = Color.White;
            btnSelectAll.AutoSize = true;
            btnSelectAll.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnSelectAll.Enabled = false;
            btnSelectAll.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnSelectAll.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnSelectAll.FlatStyle = FlatStyle.Flat;
            btnSelectAll.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnSelectAll.ForeColor = Color.FromArgb(36, 50, 71);
            btnSelectAll.IconChar = FontAwesome.Sharp.IconChar.Check;
            btnSelectAll.IconColor = Color.FromArgb(36, 50, 71);
            btnSelectAll.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnSelectAll.IconSize = 17;
            btnSelectAll.Location = new Point(0, 0);
            btnSelectAll.Margin = new Padding(0, 0, 9, 0);
            btnSelectAll.Name = "btnSelectAll";
            btnSelectAll.MinimumSize = new Size(128, 48);
            btnSelectAll.Padding = new Padding(7, 0, 7, 0);
            btnSelectAll.Size = new Size(128, 48);
            btnSelectAll.TabIndex = 0;
            btnSelectAll.Text = "Select All";
            btnSelectAll.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnSelectAll.UseVisualStyleBackColor = false;
            btnSelectAll.Click += btnSelectAll_Click;
            //
            // btnClearSelection
            //
            btnClearSelection.BackColor = Color.White;
            btnClearSelection.AutoSize = true;
            btnClearSelection.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnClearSelection.Enabled = false;
            btnClearSelection.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnClearSelection.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 246, 255);
            btnClearSelection.FlatStyle = FlatStyle.Flat;
            btnClearSelection.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnClearSelection.ForeColor = Color.FromArgb(36, 50, 71);
            btnClearSelection.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            btnClearSelection.IconColor = Color.FromArgb(36, 50, 71);
            btnClearSelection.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnClearSelection.IconSize = 17;
            btnClearSelection.Location = new Point(137, 0);
            btnClearSelection.Margin = new Padding(0, 0, 9, 0);
            btnClearSelection.Name = "btnClearSelection";
            btnClearSelection.MinimumSize = new Size(169, 48);
            btnClearSelection.Padding = new Padding(7, 0, 7, 0);
            btnClearSelection.Size = new Size(169, 48);
            btnClearSelection.TabIndex = 1;
            btnClearSelection.Text = "Clear Selection";
            btnClearSelection.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClearSelection.UseVisualStyleBackColor = false;
            btnClearSelection.Click += btnClearSelection_Click;
            //
            // lblSelectionSummary
            //
            lblSelectionSummary.AutoEllipsis = true;
            lblSelectionSummary.AutoSize = true;
            lblSelectionSummary.Dock = DockStyle.Fill;
            lblSelectionSummary.ForeColor = Color.FromArgb(100, 116, 139);
            lblSelectionSummary.Location = new Point(352, 5);
            lblSelectionSummary.Margin = new Padding(14, 0, 0, 0);
            lblSelectionSummary.Name = "lblSelectionSummary";
            lblSelectionSummary.Size = new Size(859, 20);
            lblSelectionSummary.TabIndex = 1;
            lblSelectionSummary.Text = "Files: 0 | Selected: 0 | Selected size: 0 B";
            lblSelectionSummary.TextAlign = ContentAlignment.MiddleRight;
            //
            // pnlPreview
            //
            pnlPreview.BackColor = Color.White;
            pnlPreview.BorderStyle = BorderStyle.FixedSingle;
            pnlPreview.Controls.Add(dgvFiles);
            pnlPreview.Controls.Add(pnlEmptyState);
            pnlPreview.Dock = DockStyle.Fill;
            pnlPreview.Location = new Point(23, 307);
            pnlPreview.Margin = new Padding(23, 0, 23, 16);
            pnlPreview.Name = "pnlPreview";
            pnlPreview.Size = new Size(1188, 503);
            pnlPreview.TabIndex = 3;
            //
            // dgvFiles
            //
            dgvFiles.AccessibleName = "File preview and selection";
            dgvFiles.AllowUserToAddRows = false;
            dgvFiles.AllowUserToDeleteRows = false;
            dgvFiles.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(249, 251, 254);
            dgvFiles.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFiles.BackgroundColor = Color.White;
            dgvFiles.BorderStyle = BorderStyle.None;
            dgvFiles.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFiles.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(239, 246, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(36, 50, 71);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(239, 246, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(36, 50, 71);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvFiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvFiles.ColumnHeadersHeight = 38;
            dgvFiles.Columns.AddRange(new DataGridViewColumn[] { colSelected, colFileName, colCategory, colSize, colDestination, colResult });
            dataGridViewCellStyle4.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = Color.White;
            dataGridViewCellStyle4.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle4.ForeColor = Color.FromArgb(36, 50, 71);
            dataGridViewCellStyle4.Padding = new Padding(4, 0, 4, 0);
            dataGridViewCellStyle4.SelectionBackColor = Color.FromArgb(219, 234, 254);
            dataGridViewCellStyle4.SelectionForeColor = Color.FromArgb(36, 50, 71);
            dataGridViewCellStyle4.WrapMode = DataGridViewTriState.False;
            dgvFiles.DefaultCellStyle = dataGridViewCellStyle4;
            dgvFiles.Dock = DockStyle.Fill;
            dgvFiles.EditMode = DataGridViewEditMode.EditOnEnter;
            dgvFiles.EnableHeadersVisualStyles = false;
            dgvFiles.GridColor = Color.FromArgb(226, 232, 240);
            dgvFiles.Location = new Point(0, 0);
            dgvFiles.Margin = new Padding(3, 4, 3, 4);
            dgvFiles.MultiSelect = false;
            dgvFiles.Name = "dgvFiles";
            dgvFiles.RowHeadersVisible = false;
            dgvFiles.RowHeadersWidth = 51;
            dgvFiles.RowTemplate.Height = 32;
            dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiles.Size = new Size(1186, 501);
            dgvFiles.TabIndex = 0;
            dgvFiles.CellFormatting += dgvFiles_CellFormatting;
            dgvFiles.CellValueChanged += dgvFiles_CellValueChanged;
            dgvFiles.CurrentCellDirtyStateChanged += dgvFiles_CurrentCellDirtyStateChanged;
            //
            // colSelected
            //
            colSelected.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colSelected.HeaderText = "";
            colSelected.MinimumWidth = 38;
            colSelected.Name = "colSelected";
            colSelected.Resizable = DataGridViewTriState.False;
            colSelected.ToolTipText = "Select a file to organize. Press Space to toggle.";
            colSelected.Width = 38;
            //
            // colFileName
            //
            colFileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colFileName.FillWeight = 28F;
            colFileName.HeaderText = "File";
            colFileName.MinimumWidth = 110;
            colFileName.Name = "colFileName";
            colFileName.ReadOnly = true;
            //
            // colCategory
            //
            colCategory.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colCategory.FillWeight = 15F;
            colCategory.HeaderText = "Group";
            colCategory.MinimumWidth = 90;
            colCategory.Name = "colCategory";
            colCategory.ReadOnly = true;
            //
            // colSize
            //
            colSize.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleRight;
            colSize.DefaultCellStyle = dataGridViewCellStyle3;
            colSize.HeaderText = "Size";
            colSize.MinimumWidth = 74;
            colSize.Name = "colSize";
            colSize.ReadOnly = true;
            colSize.Width = 82;
            //
            // colDestination
            //
            colDestination.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDestination.FillWeight = 39F;
            colDestination.HeaderText = "Destination (preview)";
            colDestination.MinimumWidth = 180;
            colDestination.Name = "colDestination";
            colDestination.ReadOnly = true;
            colDestination.ToolTipText = "Expected destination. The name is checked again when moving.";
            //
            // colResult
            //
            colResult.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colResult.FillWeight = 18F;
            colResult.HeaderText = "Result";
            colResult.MinimumWidth = 110;
            colResult.Name = "colResult";
            colResult.ReadOnly = true;
            //
            // pnlEmptyState
            //
            pnlEmptyState.BackColor = Color.White;
            pnlEmptyState.Controls.Add(layoutEmpty);
            pnlEmptyState.Dock = DockStyle.Fill;
            pnlEmptyState.Location = new Point(0, 0);
            pnlEmptyState.Margin = new Padding(3, 4, 3, 4);
            pnlEmptyState.Name = "pnlEmptyState";
            pnlEmptyState.Size = new Size(1186, 501);
            pnlEmptyState.TabIndex = 1;
            //
            // layoutEmpty
            //
            layoutEmpty.ColumnCount = 1;
            layoutEmpty.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutEmpty.Controls.Add(picEmptyState, 0, 1);
            layoutEmpty.Controls.Add(lblEmptyTitle, 0, 2);
            layoutEmpty.Controls.Add(lblEmptyState, 0, 3);
            layoutEmpty.Dock = DockStyle.Fill;
            layoutEmpty.Location = new Point(0, 0);
            layoutEmpty.Margin = new Padding(3, 4, 3, 4);
            layoutEmpty.Name = "layoutEmpty";
            layoutEmpty.Padding = new Padding(32, 16, 32, 16);
            layoutEmpty.RowCount = 5;
            layoutEmpty.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layoutEmpty.RowStyles.Add(new RowStyle(SizeType.Absolute, 85F));
            layoutEmpty.RowStyles.Add(new RowStyle());
            layoutEmpty.RowStyles.Add(new RowStyle());
            layoutEmpty.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            layoutEmpty.Size = new Size(1186, 501);
            layoutEmpty.TabIndex = 0;
            //
            // picEmptyState
            //
            picEmptyState.Anchor = AnchorStyles.None;
            picEmptyState.BackColor = Color.White;
            picEmptyState.ForeColor = Color.FromArgb(30, 115, 220);
            picEmptyState.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            picEmptyState.IconColor = Color.FromArgb(30, 115, 220);
            picEmptyState.IconFont = FontAwesome.Sharp.IconFont.Auto;
            picEmptyState.IconSize = 59;
            picEmptyState.Location = new Point(563, 181);
            picEmptyState.Margin = new Padding(3, 4, 3, 4);
            picEmptyState.Name = "picEmptyState";
            picEmptyState.Size = new Size(59, 69);
            picEmptyState.TabIndex = 0;
            picEmptyState.TabStop = false;
            //
            // lblEmptyTitle
            //
            lblEmptyTitle.AutoSize = true;
            lblEmptyTitle.Dock = DockStyle.Fill;
            lblEmptyTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblEmptyTitle.Location = new Point(32, 269);
            lblEmptyTitle.Margin = new Padding(0, 11, 0, 0);
            lblEmptyTitle.Name = "lblEmptyTitle";
            lblEmptyTitle.Size = new Size(1122, 28);
            lblEmptyTitle.TabIndex = 1;
            lblEmptyTitle.Text = "Choose a folder";
            lblEmptyTitle.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblEmptyState
            //
            lblEmptyState.AutoSize = true;
            lblEmptyState.Dock = DockStyle.Fill;
            lblEmptyState.ForeColor = Color.FromArgb(100, 116, 139);
            lblEmptyState.Location = new Point(32, 308);
            lblEmptyState.Margin = new Padding(0, 11, 0, 0);
            lblEmptyState.Name = "lblEmptyState";
            lblEmptyState.Size = new Size(1122, 20);
            lblEmptyState.TabIndex = 2;
            lblEmptyState.Text = "Browse for a folder, then scan to preview your files.";
            lblEmptyState.TextAlign = ContentAlignment.MiddleCenter;
            //
            // layoutProgress
            //
            layoutProgress.AutoSize = true;
            layoutProgress.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutProgress.ColumnCount = 2;
            layoutProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutProgress.ColumnStyles.Add(new ColumnStyle());
            layoutProgress.Controls.Add(progressFiles, 0, 0);
            layoutProgress.Controls.Add(btnCancel, 1, 0);
            layoutProgress.Dock = DockStyle.Fill;
            layoutProgress.Location = new Point(0, 826);
            layoutProgress.Margin = new Padding(0);
            layoutProgress.Name = "layoutProgress";
            layoutProgress.Padding = new Padding(23, 0, 23, 16);
            layoutProgress.RowCount = 1;
            layoutProgress.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutProgress.Size = new Size(1234, 64);
            layoutProgress.TabIndex = 5;
            //
            // progressFiles
            //
            progressFiles.AccessibleName = "Task progress";
            progressFiles.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            progressFiles.Location = new Point(23, 12);
            progressFiles.Margin = new Padding(0, 0, 18, 0);
            progressFiles.MarqueeAnimationSpeed = 30;
            progressFiles.Maximum = 1;
            progressFiles.MinimumSize = new Size(91, 24);
            progressFiles.Name = "progressFiles";
            progressFiles.Size = new Size(1033, 24);
            progressFiles.TabIndex = 0;
            progressFiles.TabStop = false;
            //
            // btnCancel
            //
            btnCancel.BackColor = Color.FromArgb(254, 242, 242);
            btnCancel.Anchor = AnchorStyles.Right;
            btnCancel.AutoSize = true;
            btnCancel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            btnCancel.Enabled = false;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(226, 232, 240);
            btnCancel.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 226, 226);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnCancel.ForeColor = Color.FromArgb(185, 28, 28);
            btnCancel.IconChar = FontAwesome.Sharp.IconChar.Cancel;
            btnCancel.IconColor = Color.FromArgb(185, 28, 28);
            btnCancel.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnCancel.IconSize = 17;
            btnCancel.Location = new Point(1074, 0);
            btnCancel.Margin = new Padding(0);
            btnCancel.Name = "btnCancel";
            btnCancel.MinimumSize = new Size(137, 48);
            btnCancel.Padding = new Padding(7, 0, 7, 0);
            btnCancel.Size = new Size(137, 48);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.TextImageRelation = TextImageRelation.ImageBeforeText;
            tips.SetToolTip(btnCancel, "Request cancellation. The current file operation finishes before stopping.");
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            //
            // layoutStatus
            //
            layoutStatus.BackColor = Color.White;
            layoutStatus.AutoSize = true;
            layoutStatus.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            layoutStatus.ColumnCount = 2;
            layoutStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 27F));
            layoutStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layoutStatus.Controls.Add(picStatus, 0, 0);
            layoutStatus.Controls.Add(lblStatus, 1, 0);
            layoutStatus.Dock = DockStyle.Fill;
            layoutStatus.Location = new Point(0, 862);
            layoutStatus.Margin = new Padding(0);
            layoutStatus.Name = "layoutStatus";
            layoutStatus.MinimumSize = new Size(0, 45);
            layoutStatus.Padding = new Padding(23, 7, 23, 7);
            layoutStatus.RowCount = 1;
            layoutStatus.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layoutStatus.Size = new Size(1234, 45);
            layoutStatus.TabIndex = 6;
            //
            // picStatus
            //
            picStatus.BackColor = Color.White;
            picStatus.Anchor = AnchorStyles.Left;
            picStatus.ForeColor = Color.FromArgb(100, 116, 139);
            picStatus.IconChar = FontAwesome.Sharp.IconChar.CircleInfo;
            picStatus.IconColor = Color.FromArgb(100, 116, 139);
            picStatus.IconFont = FontAwesome.Sharp.IconFont.Auto;
            picStatus.IconSize = 20;
            picStatus.Location = new Point(23, 10);
            picStatus.Margin = new Padding(0, 0, 7, 0);
            picStatus.Name = "picStatus";
            picStatus.Size = new Size(20, 24);
            picStatus.TabIndex = 0;
            picStatus.TabStop = false;
            //
            // lblStatus
            //
            lblStatus.AccessibleName = "Current status";
            lblStatus.AutoEllipsis = true;
            lblStatus.AutoSize = true;
            lblStatus.Dock = DockStyle.Fill;
            lblStatus.ForeColor = Color.FromArgb(100, 116, 139);
            lblStatus.Location = new Point(50, 7);
            lblStatus.Margin = new Padding(0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(1161, 31);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Ready. Choose a folder to begin.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            //
            // tips
            //
            tips.AutoPopDelay = 15000;
            tips.InitialDelay = 400;
            tips.ReshowDelay = 100;
            tips.ShowAlways = true;
            //
            // MainForm
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 252);
            ClientSize = new Size(1234, 907);
            Controls.Add(layoutMain);
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(36, 50, 71);
            Margin = new Padding(3, 4, 3, 4);
            MinimumSize = new Size(980, 640);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Smart File Organizer";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            Shown += MainForm_Shown;
            layoutMain.ResumeLayout(false);
            layoutMain.PerformLayout();
            pnlHeader.ResumeLayout(false);
            layoutHeader.ResumeLayout(false);
            layoutHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            headerActions.ResumeLayout(false);
            layoutFolder.ResumeLayout(false);
            layoutFolder.PerformLayout();
            layoutFolderPath.ResumeLayout(false);
            layoutFolderPath.PerformLayout();
            fileActions.ResumeLayout(false);
            pnlUndo.ResumeLayout(false);
            layoutSelection.ResumeLayout(false);
            layoutSelection.PerformLayout();
            selectionActions.ResumeLayout(false);
            pnlPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvFiles).EndInit();
            pnlEmptyState.ResumeLayout(false);
            layoutEmpty.ResumeLayout(false);
            layoutEmpty.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picEmptyState).EndInit();
            layoutProgress.ResumeLayout(false);
            layoutStatus.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picStatus).EndInit();
            ResumeLayout(false);
        }
        #endregion
    }
}
