namespace SmartFileOrganizer
{
    partial class SettingsForm
    {
        private System.ComponentModel.IContainer components = null;
        private DataGridView dgvCategories;
        private DataGridViewTextBoxColumn colGroupName;
        private DataGridViewTextBoxColumn colFolderName;
        private DataGridViewTextBoxColumn colExtensions;
        private Label lblHeader;
        private Label lblHelp;
        private Label lblError;
        private Button btnAddGroup;
        private Button btnDeleteGroup;
        private Button btnRestoreDefaults;
        private Button btnSaveSettings;
        private Button btnCancelSettings;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            DataGridViewCellStyle headerStyle = new DataGridViewCellStyle();
            dgvCategories = new DataGridView();
            colGroupName = new DataGridViewTextBoxColumn();
            colFolderName = new DataGridViewTextBoxColumn();
            colExtensions = new DataGridViewTextBoxColumn();
            lblHeader = new Label();
            lblHelp = new Label();
            lblError = new Label();
            btnAddGroup = new Button();
            btnDeleteGroup = new Button();
            btnRestoreDefaults = new Button();
            btnSaveSettings = new Button();
            btnCancelSettings = new Button();
            ((System.ComponentModel.ISupportInitialize)dgvCategories).BeginInit();
            SuspendLayout();
            //
            // dgvCategories
            //
            dgvCategories.AllowUserToAddRows = false;
            dgvCategories.AllowUserToDeleteRows = false;
            dgvCategories.AllowUserToResizeRows = false;
            dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCategories.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.DisplayedCells;
            dgvCategories.BackgroundColor = Color.White;
            headerStyle.BackColor = Color.FromArgb(225, 239, 255);
            headerStyle.ForeColor = Color.FromArgb(30, 50, 80);
            headerStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dgvCategories.ColumnHeadersDefaultCellStyle = headerStyle;
            dgvCategories.ColumnHeadersHeight = 35;
            dgvCategories.Columns.AddRange(new DataGridViewColumn[] { colGroupName, colFolderName, colExtensions });
            dgvCategories.EnableHeadersVisualStyles = false;
            dgvCategories.GridColor = Color.FromArgb(225, 230, 238);
            dgvCategories.Location = new Point(20, 100);
            dgvCategories.MultiSelect = false;
            dgvCategories.Name = "dgvCategories";
            dgvCategories.RowHeadersWidth = 28;
            dgvCategories.RowTemplate.Height = 35;
            dgvCategories.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgvCategories.ShowCellToolTips = true;
            dgvCategories.Size = new Size(900, 305);
            dgvCategories.TabIndex = 0;
            dgvCategories.EditingControlShowing += dgvCategories_EditingControlShowing;
            dgvCategories.SelectionChanged += dgvCategories_SelectionChanged;
            //
            // columns
            //
            colGroupName.HeaderText = "Group";
            colGroupName.FillWeight = 25F;
            colGroupName.Name = "colGroupName";
            colGroupName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colFolderName.HeaderText = "Destination subfolder";
            colFolderName.FillWeight = 25F;
            colFolderName.Name = "colFolderName";
            colFolderName.SortMode = DataGridViewColumnSortMode.NotSortable;
            colFolderName.ToolTipText = "One Windows subfolder name; paths and reserved device names are not allowed.";
            colExtensions.HeaderText = "Extensions (comma, semicolon or newline)";
            colExtensions.FillWeight = 50F;
            colExtensions.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            colExtensions.Name = "colExtensions";
            colExtensions.SortMode = DataGridViewColumnSortMode.NotSortable;
            colExtensions.ToolTipText = "Final extension only: PDF or .pdf; use .gz for file.tar.gz. Others has no extensions.";
            //
            // labels
            //
            lblHeader.BackColor = Color.FromArgb(30, 115, 220);
            lblHeader.Dock = DockStyle.Top;
            lblHeader.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.Padding = new Padding(20, 0, 0, 0);
            lblHeader.Size = new Size(940, 55);
            lblHeader.Name = "lblHeader";
            lblHeader.Text = "Classification settings";
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            lblHelp.Location = new Point(20, 64);
            lblHelp.Size = new Size(900, 30);
            lblHelp.Name = "lblHelp";
            lblHelp.Text = "Edit this draft, then Save to apply. Extensions match the final suffix only (.gz, not .tar.gz). Shift+Enter adds a line.";
            lblError.ForeColor = Color.FromArgb(180, 40, 40);
            lblError.Location = new Point(20, 415);
            lblError.Size = new Size(900, 55);
            lblError.Name = "lblError";
            //
            // buttons
            //
            btnAddGroup.BackColor = Color.FromArgb(225, 239, 255);
            btnAddGroup.FlatStyle = FlatStyle.Flat;
            btnAddGroup.Location = new Point(20, 480);
            btnAddGroup.Name = "btnAddGroup";
            btnAddGroup.Size = new Size(100, 34);
            btnAddGroup.TabIndex = 1;
            btnAddGroup.Text = "Add Group";
            btnAddGroup.UseVisualStyleBackColor = false;
            btnAddGroup.Click += btnAddGroup_Click;
            btnDeleteGroup.BackColor = Color.FromArgb(225, 239, 255);
            btnDeleteGroup.Enabled = false;
            btnDeleteGroup.FlatStyle = FlatStyle.Flat;
            btnDeleteGroup.Location = new Point(130, 480);
            btnDeleteGroup.Name = "btnDeleteGroup";
            btnDeleteGroup.Size = new Size(100, 34);
            btnDeleteGroup.TabIndex = 2;
            btnDeleteGroup.Text = "Delete Group";
            btnDeleteGroup.UseVisualStyleBackColor = false;
            btnDeleteGroup.Click += btnDeleteGroup_Click;
            btnRestoreDefaults.BackColor = Color.FromArgb(225, 239, 255);
            btnRestoreDefaults.FlatStyle = FlatStyle.Flat;
            btnRestoreDefaults.Location = new Point(245, 480);
            btnRestoreDefaults.Name = "btnRestoreDefaults";
            btnRestoreDefaults.Size = new Size(180, 34);
            btnRestoreDefaults.TabIndex = 3;
            btnRestoreDefaults.Text = "Restore Defaults";
            btnRestoreDefaults.UseVisualStyleBackColor = false;
            btnRestoreDefaults.Click += btnRestoreDefaults_Click;
            btnSaveSettings.BackColor = Color.FromArgb(30, 115, 220);
            btnSaveSettings.FlatStyle = FlatStyle.Flat;
            btnSaveSettings.ForeColor = Color.White;
            btnSaveSettings.Location = new Point(710, 480);
            btnSaveSettings.Name = "btnSaveSettings";
            btnSaveSettings.Size = new Size(95, 34);
            btnSaveSettings.TabIndex = 4;
            btnSaveSettings.Text = "Save";
            btnSaveSettings.UseVisualStyleBackColor = false;
            btnSaveSettings.Click += btnSaveSettings_Click;
            btnCancelSettings.CausesValidation = false;
            btnCancelSettings.DialogResult = DialogResult.Cancel;
            btnCancelSettings.Location = new Point(815, 480);
            btnCancelSettings.Name = "btnCancelSettings";
            btnCancelSettings.Size = new Size(105, 34);
            btnCancelSettings.TabIndex = 5;
            btnCancelSettings.Text = "Cancel";
            //
            // SettingsForm
            //
            AcceptButton = btnSaveSettings;
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 252);
            CancelButton = btnCancelSettings;
            ClientSize = new Size(940, 535);
            Controls.Add(dgvCategories);
            Controls.Add(lblHeader);
            Controls.Add(lblHelp);
            Controls.Add(lblError);
            Controls.Add(btnAddGroup);
            Controls.Add(btnDeleteGroup);
            Controls.Add(btnRestoreDefaults);
            Controls.Add(btnSaveSettings);
            Controls.Add(btnCancelSettings);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SettingsForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Settings — Smart File Organizer";
            ((System.ComponentModel.ISupportInitialize)dgvCategories).EndInit();
            ResumeLayout(false);
        }
        #endregion
    }
}
