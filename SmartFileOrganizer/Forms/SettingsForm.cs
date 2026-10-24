using SmartFileOrganizer.Models;
using SmartFileOrganizer.Services;

namespace SmartFileOrganizer
{
    public partial class SettingsForm : Form
    {
        private readonly SettingsStore? settingsStore;
        public bool RulesChanged { get; private set; }

        // The parameterless constructor keeps the Visual Studio Designer independent of disk I/O.
        public SettingsForm()
        {
            InitializeComponent();
        }

        public SettingsForm(SettingsStore store) : this()
        {
            settingsStore = store;
            LoadDraft(store.Current);
        }

        private void LoadDraft(ClassificationSettings settings)
        {
            dgvCategories.Rows.Clear();
            foreach (FileCategory category in settings.Categories)
            {
                int index = dgvCategories.Rows.Add(category.Name, category.FolderName, string.Join(", ", category.Extensions));
                DataGridViewRow row = dgvCategories.Rows[index];
                row.Tag = category.Id;
                if (string.Equals(category.Id, FileCategory.FallbackId, StringComparison.OrdinalIgnoreCase))
                {
                    row.Cells[colExtensions.Index].ReadOnly = true;
                    row.Cells[colExtensions.Index].ToolTipText = "Fallback group: catches unmatched files and files without extensions. Cannot be deleted.";
                }
            }

            lblError.Text = string.Empty;
            UpdateDeleteButton();
        }

        private void UpdateDeleteButton()
        {
            btnDeleteGroup.Enabled = dgvCategories.CurrentRow?.Tag is string id &&
                !string.Equals(id, FileCategory.FallbackId, StringComparison.OrdinalIgnoreCase);
        }

        private void dgvCategories_SelectionChanged(object sender, EventArgs e)
        {
            UpdateDeleteButton();
        }

        private void dgvCategories_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox textBox)
            {
                bool extensions = dgvCategories.CurrentCell?.OwningColumn == colExtensions;
                textBox.Multiline = extensions;
                textBox.AcceptsReturn = extensions;
                textBox.WordWrap = extensions;
            }
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            dgvCategories.EndEdit();
            int suffix = 1;
            string name;
            do
            {
                name = $"New group {suffix++}";
            }
            while (dgvCategories.Rows.Cast<DataGridViewRow>().Any(row =>
                string.Equals(row.Cells[colGroupName.Index].Value?.ToString(), name, StringComparison.OrdinalIgnoreCase)));
            int index = dgvCategories.Rows.Add(name, $"NewGroup{suffix - 1}", "");
            DataGridViewRow added = dgvCategories.Rows[index];
            added.Tag = Guid.NewGuid().ToString("N");
            dgvCategories.CurrentCell = added.Cells[colGroupName.Index];
            dgvCategories.BeginEdit(true);
            UpdateDeleteButton();
        }

        private void btnDeleteGroup_Click(object sender, EventArgs e)
        {
            DataGridViewRow? row = dgvCategories.CurrentRow;
            if (row?.Tag is not string id || string.Equals(id, FileCategory.FallbackId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (MessageBox.Show(this, $"Delete group '{row.Cells[colGroupName.Index].Value}' from this draft?",
                "Delete Group", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dgvCategories.Rows.Remove(row);
                UpdateDeleteButton();
            }
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Replace this draft with the default groups? Changes apply only after Save.",
                "Restore Defaults", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                dgvCategories.CancelEdit();
                LoadDraft(ClassificationSettings.Defaults);
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            if (settingsStore == null || !dgvCategories.EndEdit())
            {
                return;
            }

            foreach (DataGridViewRow row in dgvCategories.Rows)
            {
                row.ErrorText = string.Empty;
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.ErrorText = string.Empty;
                }
            }

            try
            {
                List<FileCategory> categories = new();
                foreach (DataGridViewRow row in dgvCategories.Rows)
                {
                    string id = (string)row.Tag!;
                    string name = row.Cells[colGroupName.Index].Value?.ToString() ?? string.Empty;
                    try
                    {
                        categories.Add(new FileCategory(id, name,
                            row.Cells[colFolderName.Index].Value?.ToString() ?? string.Empty,
                            SettingsValidator.ParseExtensions(row.Cells[colExtensions.Index].Value?.ToString() ?? string.Empty)));
                    }
                    catch (SettingsValidationException ex)
                    {
                        throw new SettingsValidationException($"Group '{name}': {ex.Message}", id, "extensions");
                    }
                }

                ClassificationSettings draft = SettingsValidator.NormalizeAndValidate(
                    new ClassificationSettings(ClassificationSettings.CurrentVersion, categories));
                if (settingsStore.RequiresOverwriteConfirmation && MessageBox.Show(this,
                    $"The existing settings file could not be loaded:\n{settingsStore.SettingsPath}\n\n" +
                    "Replace it with this draft? The original file will be replaced only if saving succeeds.",
                    "Replace Invalid Settings", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2) != DialogResult.Yes)
                {
                    return;
                }

                RulesChanged = settingsStore.Save(draft, replaceInvalidFile: settingsStore.RequiresOverwriteConfirmation);
                DialogResult = DialogResult.OK;
            }
            catch (SettingsValidationException ex)
            {
                lblError.Text = ex.Message;
                DataGridViewRow? row = dgvCategories.Rows.Cast<DataGridViewRow>()
                    .FirstOrDefault(item => string.Equals(item.Tag as string, ex.CategoryId, StringComparison.OrdinalIgnoreCase));
                if (row != null)
                {
                    int column = ex.Field == "folderName" ? colFolderName.Index
                        : ex.Field == "extensions" ? colExtensions.Index : colGroupName.Index;
                    row.ErrorText = ex.Message;
                    row.Cells[column].ErrorText = ex.Message;
                    dgvCategories.CurrentCell = row.Cells[column];
                }
            }
            catch (Exception ex)
            {
                lblError.Text = $"Save failed: {ex.Message}";
                MessageBox.Show(this, $"Unable to save settings:\n\n{ex.Message}\n\nActive rules have not changed.",
                    "Settings Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
