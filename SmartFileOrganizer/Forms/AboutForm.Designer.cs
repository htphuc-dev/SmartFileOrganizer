namespace SmartFileOrganizer
{
    partial class AboutForm
    {
        private System.ComponentModel.IContainer components = null;
        private TableLayoutPanel layout;
        private FontAwesome.Sharp.IconPictureBox picLogo;
        private Label lblName;
        private Label lblVersion;
        private Label lblAuthor;
        private Label lblDescription;
        private FontAwesome.Sharp.IconButton btnClose;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            layout = new TableLayoutPanel();
            picLogo = new FontAwesome.Sharp.IconPictureBox();
            lblName = new Label();
            lblVersion = new Label();
            lblAuthor = new Label();
            lblDescription = new Label();
            btnClose = new FontAwesome.Sharp.IconButton();
            ((System.ComponentModel.ISupportInitialize)picLogo).BeginInit();
            layout.SuspendLayout();
            SuspendLayout();
            layout.ColumnCount = 1;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowCount = 6;
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            layout.Dock = DockStyle.Fill;
            layout.Padding = new Padding(24);
            layout.Name = "layout";
            layout.Controls.Add(picLogo, 0, 0);
            layout.Controls.Add(lblName, 0, 1);
            layout.Controls.Add(lblVersion, 0, 2);
            layout.Controls.Add(lblAuthor, 0, 3);
            layout.Controls.Add(lblDescription, 0, 4);
            layout.Controls.Add(btnClose, 0, 5);
            picLogo.Anchor = AnchorStyles.None;
            picLogo.BackColor = Color.Transparent;
            picLogo.IconChar = FontAwesome.Sharp.IconChar.FolderOpen;
            picLogo.IconColor = Color.FromArgb(30, 115, 220);
            picLogo.IconSize = 52;
            picLogo.Size = new Size(56, 56);
            picLogo.Name = "picLogo";
            picLogo.TabStop = false;
            lblName.AutoSize = true;
            lblName.Dock = DockStyle.Fill;
            lblName.Font = new Font("Segoe UI", 17F, FontStyle.Bold);
            lblName.Name = "lblName";
            lblName.Text = "Smart File Organizer";
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            lblVersion.AutoSize = true;
            lblVersion.Dock = DockStyle.Fill;
            lblVersion.ForeColor = Color.FromArgb(100, 116, 139);
            lblVersion.Margin = new Padding(0, 8, 0, 0);
            lblVersion.Name = "lblVersion";
            lblVersion.Text = "Version";
            lblVersion.TextAlign = ContentAlignment.MiddleCenter;
            lblAuthor.AutoSize = true;
            lblAuthor.Dock = DockStyle.Fill;
            lblAuthor.Margin = new Padding(0, 8, 0, 0);
            lblAuthor.Name = "lblAuthor";
            lblAuthor.Text = "By Hoàng Phúc";
            lblAuthor.TextAlign = ContentAlignment.MiddleCenter;
            lblDescription.Dock = DockStyle.Fill;
            lblDescription.ForeColor = Color.FromArgb(100, 116, 139);
            lblDescription.Margin = new Padding(8, 14, 8, 10);
            lblDescription.Name = "lblDescription";
            lblDescription.Text = "Preview, select and organize files with your own classification rules.";
            lblDescription.TextAlign = ContentAlignment.MiddleCenter;
            btnClose.Anchor = AnchorStyles.None;
            btnClose.BackColor = Color.FromArgb(30, 115, 220);
            btnClose.DialogResult = DialogResult.Cancel;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.ForeColor = Color.White;
            btnClose.IconChar = FontAwesome.Sharp.IconChar.Check;
            btnClose.IconColor = Color.White;
            btnClose.IconSize = 16;
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(110, 36);
            btnClose.TabIndex = 0;
            btnClose.Text = "Close";
            btnClose.TextImageRelation = TextImageRelation.ImageBeforeText;
            btnClose.UseVisualStyleBackColor = false;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            CancelButton = btnClose;
            ClientSize = new Size(470, 350);
            Controls.Add(layout);
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.FromArgb(36, 50, 71);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "About Smart File Organizer";
            Shown += AboutForm_Shown;
            ((System.ComponentModel.ISupportInitialize)picLogo).EndInit();
            layout.ResumeLayout(false);
            layout.PerformLayout();
            ResumeLayout(false);
        }
        #endregion
    }
}
