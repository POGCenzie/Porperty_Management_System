using PropertyManagementSystem.Constants;

namespace PropertyManagementSystem;

internal partial class CreateSoaForm : Form
{
    private readonly ModernComboBox assignmentComboBox = new();
    private readonly DateTimePicker statementDatePicker = new();
    private readonly ModernComboBox paperSizeComboBox = new();
    private readonly DataGridView paymentBreakdownGrid = new();
    private readonly DataGridView deductionsGrid = new();
    private readonly Label totalDeductionsValueLabel = new();
    private readonly Label netAmountValueLabel = new();
    private readonly ModernActionButton saveButton = new();

    private void ConfigureWindow()
    {
        Text = "Create Statement of Account - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 820);
        MinimumSize = new Size(820, 720);
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    private void BuildInterface()
    {
        var isOwner = soaType == SoaTypes.Owner;
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 108,
            BackColor = AppTheme.Surface,
            Padding = new Padding(34, 18, 34, 12)
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = isOwner
                ? "Prepare the tenant payment and deductions payable to the unit owner."
                : "Prepare the security-deposit refund and move-out deductions for the tenant.",
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI", 9.2F)
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = isOwner ? "Create owner SOA" : "Create tenant refund SOA",
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold)
        });

        assignmentComboBox.DropDownWidth = 780;
        assignmentComboBox.SelectedIndexChanged += AssignmentComboBox_SelectedIndexChanged;
        paperSizeComboBox.Items.AddRange(SoaPaperSizes.All.Cast<object>().ToArray());
        paperSizeComboBox.SelectedItem = SoaPaperSizes.A4;
        statementDatePicker.Format = DateTimePickerFormat.Custom;
        statementDatePicker.CustomFormat = "MMM d, yyyy";
        statementDatePicker.CalendarForeColor = AppTheme.TextPrimary;
        statementDatePicker.CalendarMonthBackground = AppTheme.InputBackground;
        ConfigurePaymentBreakdownGrid();
        ConfigureDeductionsGrid();

        var detailsCard = new ModernCard
        {
            Margin = Padding.Empty,
            Padding = new Padding(18, 70, 12, 16),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 16
        };
        var detailsTitle = CreateSectionTitle(
            "SOA details",
            "Select the assignment to calculate the starting amount automatically.");
        detailsTitle.SetBounds(18, 7, 700, 60);
        detailsTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        detailsCard.Controls.Add(detailsTitle);
        var detailsGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = AppTheme.Surface
        };
        detailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        detailsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        detailsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.34F));
        detailsGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        detailsGrid.Controls.Add(CreateField("TENANT ASSIGNMENT", assignmentComboBox), 0, 0);
        detailsGrid.Controls.Add(CreateField("STATEMENT DATE", statementDatePicker), 0, 1);
        detailsGrid.Controls.Add(CreateField("PRINT PAPER SIZE", paperSizeComboBox), 0, 2);
        detailsCard.Controls.Add(detailsGrid);
        detailsGrid.BringToFront();

        var paymentsCard = new ModernCard
        {
            Margin = Padding.Empty,
            Padding = new Padding(18, 70, 18, 16),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 16
        };
        var paymentsTitle = CreateSectionTitle(
            "Payment breakdown",
            "Amounts are calculated from the rent, advance and deposit saved in the tenant assignment.");
        paymentsTitle.SetBounds(18, 7, 760, 60);
        paymentsTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        paymentsCard.Controls.Add(paymentsTitle);
        paymentsCard.Controls.Add(paymentBreakdownGrid);
        paymentBreakdownGrid.BringToFront();

        var deductionsCard = new ModernCard
        {
            Margin = Padding.Empty,
            Padding = new Padding(18, 70, 18, 16),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 16
        };
        var removeButton = new ModernActionButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(112, 34),
            Text = "Remove row",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            Cursor = Cursors.Hand
        };
        removeButton.Click += (_, _) =>
        {
            if (deductionsGrid.CurrentRow is { IsNewRow: false } row)
            {
                deductionsGrid.Rows.Remove(row);
                CalculateTotals();
            }
        };
        deductionsCard.Resize += (_, _) => removeButton.SetBounds(deductionsCard.ClientSize.Width - 136, 18, 112, 34);
        var deductionsTitle = CreateSectionTitle("Deductions", "Edit the prepared rows or add another line at the bottom.");
        deductionsTitle.SetBounds(18, 7, 620, 60);
        deductionsTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        deductionsCard.Controls.Add(deductionsTitle);
        deductionsCard.Controls.Add(removeButton);
        deductionsCard.Controls.Add(deductionsGrid);
        deductionsGrid.BringToFront();
        removeButton.BringToFront();

        var pages = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase)
        {
            ["Details"] = detailsCard,
            ["Payments"] = paymentsCard,
            ["Deductions"] = deductionsCard
        };
        var pagesHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = AppTheme.PageBackground
        };
        foreach (var page in pages.Values)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            pagesHost.Controls.Add(page);
        }

        var tabSelector = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(5),
            BackColor = AppTheme.InputBackground,
            BorderColor = AppTheme.Border,
            CornerRadius = 15
        };
        var tabGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = AppTheme.InputBackground,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        tabGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tabGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        tabGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        tabGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        tabSelector.Controls.Add(tabGrid);

        var tabButtons = new Dictionary<string, ModernActionButton>(StringComparer.OrdinalIgnoreCase);
        var tabNames = new[] { "Details", "Payments", "Deductions" };
        for (var index = 0; index < tabNames.Length; index++)
        {
            var tabName = tabNames[index];
            var button = new ModernActionButton
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(index == 0 ? 0 : 4, 0, index == tabNames.Length - 1 ? 0 : 4, 0),
                Text = tabName,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                BorderColor = AppTheme.InputBackground,
                Cursor = Cursors.Hand
            };
            tabButtons[tabName] = button;
            tabGrid.Controls.Add(button, index, 0);
        }

        void ShowTab(string tabName)
        {
            foreach (var pair in pages)
            {
                pair.Value.Visible = string.Equals(pair.Key, tabName, StringComparison.OrdinalIgnoreCase);
                if (pair.Value.Visible)
                {
                    pair.Value.BringToFront();
                }
            }

            foreach (var pair in tabButtons)
            {
                var selected = string.Equals(pair.Key, tabName, StringComparison.OrdinalIgnoreCase);
                pair.Value.BackColor = selected ? AppTheme.PrimaryBlue : AppTheme.Surface;
                pair.Value.ForeColor = selected ? Color.White : AppTheme.TextPrimary;
                pair.Value.BorderColor = selected ? AppTheme.PrimaryBlue : AppTheme.InputBackground;
                pair.Value.Invalidate();
            }
        }
        foreach (var pair in tabButtons)
        {
            var tabName = pair.Key;
            pair.Value.Click += (_, _) => ShowTab(tabName);
        }

        var content = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(26, 22, 26, 18),
            BackColor = AppTheme.PageBackground,
            Margin = Padding.Empty
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        content.Controls.Add(tabSelector, 0, 0);
        content.Controls.Add(pagesHost, 0, 1);
        ShowTab("Details");

        var footer = CreateFooter(isOwner);
        Controls.Add(content);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private Panel CreateFooter(bool isOwner)
    {
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 88, BackColor = AppTheme.Surface };
        var summary = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 480,
            Padding = new Padding(28, 15, 0, 0),
            BackColor = AppTheme.Surface,
            WrapContents = false
        };
        summary.Controls.Add(CreateTotalBlock("TOTAL DEDUCTIONS", totalDeductionsValueLabel));
        summary.Controls.Add(CreateTotalBlock(isOwner ? "PAYABLE TO OWNER" : "TENANT REFUND", netAmountValueLabel));
        var cancelButton = new ModernActionButton
        {
            Size = new Size(108, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.Size = new Size(142, 48);
        saveButton.Text = "File and print";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        void PositionButtons()
        {
            saveButton.SetBounds(footer.ClientSize.Width - saveButton.Width - 28, 19, saveButton.Width, saveButton.Height);
            cancelButton.SetBounds(saveButton.Left - cancelButton.Width - 12, 19, cancelButton.Width, cancelButton.Height);
        }
        footer.Resize += (_, _) => PositionButtons();
        footer.Controls.Add(summary);
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);
        PositionButtons();
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        return footer;
    }

    private void ConfigureDeductionsGrid()
    {
        deductionsGrid.Dock = DockStyle.Fill;
        deductionsGrid.BackgroundColor = AppTheme.Surface;
        deductionsGrid.BorderStyle = BorderStyle.None;
        deductionsGrid.RowHeadersVisible = false;
        deductionsGrid.AllowUserToAddRows = true;
        deductionsGrid.AllowUserToDeleteRows = true;
        deductionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        deductionsGrid.ColumnHeadersHeight = 42;
        deductionsGrid.RowTemplate.Height = 38;
        deductionsGrid.EnableHeadersVisualStyles = false;
        deductionsGrid.GridColor = AppTheme.Border;
        deductionsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.TableHeader,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            SelectionBackColor = AppTheme.TableHeader
        };
        deductionsGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.InputBackground,
            ForeColor = AppTheme.TextPrimary,
            SelectionBackColor = AppTheme.Selection,
            SelectionForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI", 9.2F)
        };
        deductionsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Deduction", FillWeight = 44F });
        deductionsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Amount", FillWeight = 22F });
        deductionsGrid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Remarks", FillWeight = 34F });
        deductionsGrid.CellValueChanged += (_, _) => CalculateTotals();
        deductionsGrid.RowsRemoved += (_, _) => CalculateTotals();
        deductionsGrid.DataError += (_, eventArgs) => eventArgs.ThrowException = false;
    }

    private void ConfigurePaymentBreakdownGrid()
    {
        paymentBreakdownGrid.Dock = DockStyle.Fill;
        paymentBreakdownGrid.Margin = new Padding(4, 5, 10, 3);
        paymentBreakdownGrid.BackgroundColor = AppTheme.Surface;
        paymentBreakdownGrid.BorderStyle = BorderStyle.None;
        paymentBreakdownGrid.RowHeadersVisible = false;
        paymentBreakdownGrid.AllowUserToAddRows = false;
        paymentBreakdownGrid.AllowUserToDeleteRows = false;
        paymentBreakdownGrid.AllowUserToResizeRows = false;
        paymentBreakdownGrid.ReadOnly = true;
        paymentBreakdownGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        paymentBreakdownGrid.ColumnHeadersHeight = 36;
        paymentBreakdownGrid.RowTemplate.Height = 32;
        paymentBreakdownGrid.EnableHeadersVisualStyles = false;
        paymentBreakdownGrid.GridColor = AppTheme.Border;
        paymentBreakdownGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.TableHeader,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            SelectionBackColor = AppTheme.TableHeader,
            Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        paymentBreakdownGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = AppTheme.InputBackground,
            ForeColor = AppTheme.TextPrimary,
            SelectionBackColor = AppTheme.InputBackground,
            SelectionForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 9.2F),
            Padding = new Padding(8, 0, 8, 0)
        };
        paymentBreakdownGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "DETAILS",
            FillWeight = 64F
        });
        paymentBreakdownGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            HeaderText = "AMOUNT",
            FillWeight = 36F,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleRight,
                Format = "N2",
                BackColor = AppTheme.InputBackground,
                ForeColor = AppTheme.TextPrimary,
                SelectionBackColor = AppTheme.InputBackground,
                SelectionForeColor = AppTheme.TextPrimary,
                Padding = new Padding(8, 0, 8, 0)
            }
        });
    }

    private static ModernFieldHost CreateField(string label, Control input) =>
        new(label, input) { Dock = DockStyle.Fill, Margin = new Padding(4, 3, 10, 3) };

    private static Panel CreateSectionTitle(string title, string subtitle)
    {
        var panel = new Panel { Height = 62, BackColor = AppTheme.Surface };
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(4, 4),
            Text = title,
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 12.5F, FontStyle.Bold)
        });
        panel.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(5, 34),
            Text = subtitle,
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI", 8.7F)
        });
        return panel;
    }

    private static Panel CreateTotalBlock(string title, Label valueLabel)
    {
        var block = new Panel { Width = 210, Height = 58, Margin = new Padding(0, 0, 16, 0), BackColor = AppTheme.Surface };
        block.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 20,
            Text = title,
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI Semibold", 8F, FontStyle.Bold)
        });
        valueLabel.Dock = DockStyle.Fill;
        valueLabel.Text = "0.00";
        valueLabel.ForeColor = AppTheme.PrimaryBlue;
        valueLabel.Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold);
        valueLabel.TextAlign = ContentAlignment.MiddleLeft;
        block.Controls.Add(valueLabel);
        return block;
    }
}
