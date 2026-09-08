namespace SmartFileOrganizer
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        private FontAwesome.Sharp.IconButton btnBrowse;
        private FontAwesome.Sharp.IconButton btnScan;
        private FontAwesome.Sharp.IconButton btnOrganize;

        private System.Windows.Forms.DataGridView dgvFiles;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCategory;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDestination;

        private System.Windows.Forms.Label lblFolder;
        private System.Windows.Forms.Label lblEmptyState;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;

        private System.Windows.Forms.TextBox txtFolderPath;

        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Panel pnlStatus;

        private System.Windows.Forms.PictureBox picEmptyState;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            btnBrowse = new FontAwesome.Sharp.IconButton();
            btnScan = new FontAwesome.Sharp.IconButton();
            btnOrganize = new FontAwesome.Sharp.IconButton();
            dgvFiles = new DataGridView();
            colFileName = new DataGridViewTextBoxColumn();
            colCategory = new DataGridViewTextBoxColumn();
            colDestination = new DataGridViewTextBoxColumn();
            lblFolder = new Label();
            lblEmptyState = new Label();
            lblStatus = new Label();
            lblTitle = new Label();
            lblSubtitle = new Label();
            txtFolderPath = new TextBox();
            pnlContent = new Panel();
            picEmptyState = new PictureBox();
            pnlHeader = new Panel();
            pnlStatus = new Panel();
            ((System.ComponentModel.ISupportInitialize)dgvFiles).BeginInit();
            pnlContent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picEmptyState).BeginInit();
            pnlHeader.SuspendLayout();
            pnlStatus.SuspendLayout();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.BackColor = Color.FromArgb(30, 115, 220);
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.FlatAppearance.MouseDownBackColor = Color.FromArgb(20, 90, 180);
            btnBrowse.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 130, 235);
            btnBrowse.FlatStyle = FlatStyle.Flat;
            btnBrowse.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnBrowse.ForeColor = Color.White;
            btnBrowse.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            btnBrowse.IconColor = Color.White;
            btnBrowse.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnBrowse.IconSize = 19;
            btnBrowse.ImageAlign = ContentAlignment.MiddleLeft;
            btnBrowse.Location = new Point(695, 45);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Padding = new Padding(5, 0, 5, 0);
            btnBrowse.Size = new Size(120, 32);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Browse";
            btnBrowse.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnBrowse.UseVisualStyleBackColor = false;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // btnScan
            // 
            btnScan.BackColor = Color.FromArgb(225, 239, 255);
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.FlatAppearance.MouseDownBackColor = Color.FromArgb(205, 225, 250);
            btnScan.FlatAppearance.MouseOverBackColor = Color.FromArgb(210, 230, 250);
            btnScan.FlatStyle = FlatStyle.Flat;
            btnScan.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnScan.ForeColor = Color.FromArgb(30, 100, 200);
            btnScan.IconChar = FontAwesome.Sharp.IconChar.Search;
            btnScan.IconColor = Color.FromArgb(30, 100, 200);
            btnScan.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnScan.IconSize = 19;
            btnScan.ImageAlign = ContentAlignment.MiddleLeft;
            btnScan.Location = new Point(565, 100);
            btnScan.Name = "btnScan";
            btnScan.Padding = new Padding(5, 0, 5, 0);
            btnScan.Size = new Size(115, 32);
            btnScan.TabIndex = 1;
            btnScan.Text = "Scan Files";
            btnScan.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnScan.UseVisualStyleBackColor = false;
            btnScan.Click += btnScan_Click;
            // 
            // btnOrganize
            // 
            btnOrganize.BackColor = Color.FromArgb(40, 180, 120);
            btnOrganize.Enabled = false;
            btnOrganize.FlatAppearance.BorderSize = 0;
            btnOrganize.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 155, 100);
            btnOrganize.FlatAppearance.MouseOverBackColor = Color.FromArgb(55, 195, 135);
            btnOrganize.FlatStyle = FlatStyle.Flat;
            btnOrganize.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnOrganize.ForeColor = Color.White;
            btnOrganize.IconChar = FontAwesome.Sharp.IconChar.FolderTree;
            btnOrganize.IconColor = Color.White;
            btnOrganize.IconFont = FontAwesome.Sharp.IconFont.Auto;
            btnOrganize.IconSize = 19;
            btnOrganize.ImageAlign = ContentAlignment.MiddleLeft;
            btnOrganize.Location = new Point(690, 100);
            btnOrganize.Name = "btnOrganize";
            btnOrganize.Padding = new Padding(5, 0, 5, 0);
            btnOrganize.Size = new Size(115, 32);
            btnOrganize.TabIndex = 2;
            btnOrganize.Text = "Organize";
            btnOrganize.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnOrganize.UseVisualStyleBackColor = false;
            btnOrganize.Click += btnOrganize_Click;
            // 
            // dgvFiles
            // 
            dgvFiles.AllowUserToAddRows = false;
            dgvFiles.AllowUserToDeleteRows = false;
            dgvFiles.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(248, 250, 253);
            dgvFiles.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgvFiles.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvFiles.BackgroundColor = Color.White;
            dgvFiles.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvFiles.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(225, 239, 255);
            dataGridViewCellStyle2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = Color.FromArgb(30, 50, 80);
            dataGridViewCellStyle2.SelectionBackColor = Color.FromArgb(225, 239, 255);
            dataGridViewCellStyle2.SelectionForeColor = Color.FromArgb(30, 50, 80);
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvFiles.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvFiles.ColumnHeadersHeight = 35;
            dgvFiles.Columns.AddRange(new DataGridViewColumn[] { colFileName, colCategory, colDestination });
            dataGridViewCellStyle3.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = Color.White;
            dataGridViewCellStyle3.Font = new Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = Color.FromArgb(50, 60, 70);
            dataGridViewCellStyle3.SelectionBackColor = Color.FromArgb(220, 235, 255);
            dataGridViewCellStyle3.SelectionForeColor = Color.FromArgb(30, 60, 100);
            dataGridViewCellStyle3.WrapMode = DataGridViewTriState.False;
            dgvFiles.DefaultCellStyle = dataGridViewCellStyle3;
            dgvFiles.EnableHeadersVisualStyles = false;
            dgvFiles.GridColor = Color.FromArgb(225, 230, 238);
            dgvFiles.Location = new Point(25, 155);
            dgvFiles.MultiSelect = false;
            dgvFiles.Name = "dgvFiles";
            dgvFiles.ReadOnly = true;
            dgvFiles.RowHeadersVisible = false;
            dgvFiles.RowHeadersWidth = 51;
            dgvFiles.RowTemplate.Height = 30;
            dgvFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvFiles.Size = new Size(805, 266);
            dgvFiles.TabIndex = 5;
            // 
            // colFileName
            // 
            colFileName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colFileName.FillWeight = 35F;
            colFileName.HeaderText = "File";
            colFileName.MinimumWidth = 6;
            colFileName.Name = "colFileName";
            colFileName.ReadOnly = true;
            // 
            // colCategory
            // 
            colCategory.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colCategory.FillWeight = 25F;
            colCategory.HeaderText = "Category";
            colCategory.MinimumWidth = 6;
            colCategory.Name = "colCategory";
            colCategory.ReadOnly = true;
            // 
            // colDestination
            // 
            colDestination.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colDestination.FillWeight = 40F;
            colDestination.HeaderText = "Destination";
            colDestination.MinimumWidth = 6;
            colDestination.Name = "colDestination";
            colDestination.ReadOnly = true;
            // 
            // lblFolder
            // 
            lblFolder.AutoSize = true;
            lblFolder.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblFolder.ForeColor = Color.FromArgb(30, 50, 80);
            lblFolder.Location = new Point(25, 12);
            lblFolder.Name = "lblFolder";
            lblFolder.Size = new Size(136, 20);
            lblFolder.TabIndex = 4;
            lblFolder.Text = "Folder to organize";
            // 
            // lblEmptyState
            // 
            lblEmptyState.BackColor = Color.Transparent;
            lblEmptyState.Font = new Font("Segoe UI", 10F);
            lblEmptyState.ForeColor = Color.FromArgb(100, 120, 145);
            lblEmptyState.Location = new Point(190, 270);
            lblEmptyState.Name = "lblEmptyState";
            lblEmptyState.Size = new Size(520, 60);
            lblEmptyState.TabIndex = 7;
            lblEmptyState.Text = "No files to display\r\nSelect a folder and click \"Scan Files\" to get started.";
            lblEmptyState.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.ForeColor = Color.FromArgb(45, 55, 72);
            lblStatus.Location = new Point(15, 8);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(114, 20);
            lblStatus.TabIndex = 0;
            lblStatus.Text = "●  Status: Ready";
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(25, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(280, 37);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "Smart File Organizer";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 9F);
            lblSubtitle.ForeColor = Color.White;
            lblSubtitle.Location = new Point(25, 48);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(227, 20);
            lblSubtitle.TabIndex = 1;
            lblSubtitle.Text = "Organize your files automatically";
            // 
            // txtFolderPath
            // 
            txtFolderPath.BackColor = Color.White;
            txtFolderPath.BorderStyle = BorderStyle.FixedSingle;
            txtFolderPath.Font = new Font("Segoe UI", 9F);
            txtFolderPath.ForeColor = Color.FromArgb(50, 60, 70);
            txtFolderPath.Location = new Point(25, 45);
            txtFolderPath.Name = "txtFolderPath";
            txtFolderPath.Size = new Size(655, 27);
            txtFolderPath.TabIndex = 3;
            // 
            // pnlContent
            // 
            pnlContent.BackColor = Color.FromArgb(245, 248, 252);
            pnlContent.Controls.Add(dgvFiles);
            pnlContent.Controls.Add(picEmptyState);
            pnlContent.Controls.Add(lblEmptyState);
            pnlContent.Controls.Add(txtFolderPath);
            pnlContent.Controls.Add(lblFolder);
            pnlContent.Controls.Add(btnBrowse);
            pnlContent.Controls.Add(btnScan);
            pnlContent.Controls.Add(btnOrganize);
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Location = new Point(0, 80);
            pnlContent.Name = "pnlContent";
            pnlContent.Size = new Size(882, 438);
            pnlContent.TabIndex = 8;
            // 
            // picEmptyState
            // 
            picEmptyState.BackColor = Color.Transparent;
            picEmptyState.Location = new Point(417, 215);
            picEmptyState.Name = "picEmptyState";
            picEmptyState.Size = new Size(48, 48);
            picEmptyState.SizeMode = PictureBoxSizeMode.CenterImage;
            picEmptyState.TabIndex = 6;
            picEmptyState.TabStop = false;
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(30, 115, 220);
            pnlHeader.Controls.Add(lblSubtitle);
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(882, 80);
            pnlHeader.TabIndex = 9;
            // 
            // pnlStatus
            // 
            pnlStatus.BackColor = Color.FromArgb(235, 247, 242);
            pnlStatus.Controls.Add(lblStatus);
            pnlStatus.Dock = DockStyle.Bottom;
            pnlStatus.Location = new Point(0, 518);
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Size = new Size(882, 35);
            pnlStatus.TabIndex = 10;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 248, 252);
            ClientSize = new Size(882, 553);
            Controls.Add(pnlContent);
            Controls.Add(pnlStatus);
            Controls.Add(pnlHeader);
            Font = new Font("Segoe UI", 9F);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Smart File Organizer";
            Load += MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)dgvFiles).EndInit();
            pnlContent.ResumeLayout(false);
            pnlContent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picEmptyState).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            pnlStatus.ResumeLayout(false);
            pnlStatus.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
    }
}