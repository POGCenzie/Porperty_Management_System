namespace PropertyManagementSystem
{
    internal sealed record TableColumnOption(string Key, string HeaderText)
    {
        public override string ToString() => HeaderText;
    }

    internal enum TableOptionsMode
    {
        FilterFields,
        VisibleColumns,
        AttentionItems
    }

    internal partial class TableOptionsForm : Form
    {
        private readonly IReadOnlyList<TableColumnOption> columns;
        private readonly IReadOnlySet<string> selectedColumns;
        private readonly TableOptionsMode mode;
        private readonly Dictionary<string, bool> checkedStates =
            new(StringComparer.OrdinalIgnoreCase);
        private bool loadingOptions;

        internal TableOptionsForm(
            IReadOnlyList<TableColumnOption> columns,
            IReadOnlySet<string> selectedColumns,
            TableOptionsMode mode)
        {
            this.columns = columns;
            this.selectedColumns = selectedColumns;
            this.mode = mode;
            foreach (var column in columns)
            {
                checkedStates[column.Key] = selectedColumns.Contains(column.Key);
            }

            ConfigureWindow();
            BuildOptionsInterface();
            LoadOptions();
        }

        internal IReadOnlySet<string> CheckedColumns { get; private set; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private void LoadOptions()
        {
            loadingOptions = true;
            optionsList.Items.Clear();
            var search = optionsSearchTextBox.Text.Trim();
            foreach (var column in columns.Where(column =>
                string.IsNullOrWhiteSpace(search) ||
                column.HeaderText.Contains(search, StringComparison.OrdinalIgnoreCase)))
            {
                var index = optionsList.Items.Add(column);
                optionsList.SetItemChecked(
                    index,
                    checkedStates.GetValueOrDefault(column.Key));
            }
            loadingOptions = false;
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            CaptureVisibleChecks();
            var checkedColumns = checkedStates
                .Where(pair => pair.Value)
                .Select(pair => pair.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (checkedColumns.Count == 0 && columns.Count > 0 &&
                mode != TableOptionsMode.AttentionItems)
            {
                checkedColumns.Add(columns[0].Key);
            }

            CheckedColumns = checkedColumns;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OptionsSearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            CaptureVisibleChecks();
            LoadOptions();
        }

        private void OptionsList_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (loadingOptions ||
                e.Index < 0 ||
                optionsList.Items[e.Index] is not TableColumnOption column)
            {
                return;
            }

            checkedStates[column.Key] = e.NewValue == CheckState.Checked;
        }

        private void CaptureVisibleChecks()
        {
            if (loadingOptions)
            {
                return;
            }

            for (var index = 0; index < optionsList.Items.Count; index++)
            {
                if (optionsList.Items[index] is TableColumnOption column)
                {
                    checkedStates[column.Key] = optionsList.GetItemChecked(index);
                }
            }
        }

        private void SetAllChecks(bool isChecked)
        {
            foreach (var column in columns)
            {
                checkedStates[column.Key] = isChecked;
            }

            LoadOptions();
        }
    }
}
