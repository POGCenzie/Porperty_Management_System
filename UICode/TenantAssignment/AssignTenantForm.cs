using System.Globalization;
using System.Drawing.Drawing2D;

namespace PropertyManagementSystem;

internal partial class AssignTenantForm : Form
{
    private readonly ModernComboBox condominiumComboBox = new();
    private readonly ModernComboBox tenantComboBox = new();
    private readonly ModernComboBox unitNumberComboBox = new();
    private readonly TextBox ownerNameTextBox = new();
    private readonly ModernComboBox occupancyStatusComboBox = new();
    private readonly TextBox meralcoSinTextBox = new();
    private readonly NumericUpDown keyReleaseNumeric = new();
    private readonly NumericUpDown accessCardNumeric = new();
    private readonly NumericUpDown rentalAmountNumeric = new();
    private readonly ModernComboBox withDuesComboBox = new();
    private readonly NumericUpDown stayDurationNumeric = new();
    private readonly ModernComboBox stayDurationUnitComboBox = new();
    private readonly DateTimePicker startDatePicker = new();
    private readonly DateTimePicker endDatePicker = new();
    private readonly DateTimePicker airconDatePicker = new();
    private readonly TextBox maintenanceStatusTextBox = new();
    private readonly NumericUpDown ownerAdvanceMonthsNumeric = new();
    private readonly NumericUpDown ownerDepositMonthsNumeric = new();
    private readonly TextBox ownerEmailTextBox = new();
    private readonly TextBox ownerContactTextBox = new();
    private readonly TextBox tenantContactTextBox = new();
    private readonly TextBox tenantEmailTextBox = new();
    private readonly ModernActionButton saveButton = new();
    private readonly ModernActionButton endContractButton = new();
    private readonly Label paymentSummaryLabel = new();
    private bool calculatingContractEndDate;

    private void ConfigureWindow()
    {
            Text = existingAssignment is null
                ? "Assign Tenant - Syrento"
                : "Update Tenant Assignment - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1320, 900);
        MinimumSize = new Size(1100, 780);
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    private void BuildInterface()
    {
        ConfigureInputs();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 108,
            BackColor = AppTheme.Surface,
            Padding = new Padding(34, 16, 34, 10)
        };
        header.Paint += (_, e) =>
        {
            using var stripe = new LinearGradientBrush(
                new Rectangle(0, 0, Math.Max(1, header.Width), 6),
                AppTheme.Cyan,
                AppTheme.PrimaryBlue,
                LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(stripe, 0, 0, header.Width, 6);
            using var blue = new SolidBrush(Color.FromArgb(28, AppTheme.PrimaryBlue));
            e.Graphics.FillPolygon(blue, new[]
            {
                new Point(Math.Max(0, header.Width - 370), 6),
                new Point(header.Width - 270, 6),
                new Point(header.Width - 205, header.Height),
                new Point(header.Width - 305, header.Height)
            });
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = existingAssignment is null
                ? "Manage the property unit, owner, tenant, payments and contract from one accurate record."
                : "Update the selected unit and tenant information from one workspace.",
            Font = new Font("Segoe UI", 9.3F),
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Text = existingAssignment is null ? "⌂  Unit Information" : "⌂  Update Unit Information",
            Font = new Font("Segoe UI Semibold", 23F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            BackColor = Color.Transparent
        });
        header.Controls.Add(new Label
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(header.Width - 246, 26),
            Size = new Size(210, 54),
            Text = "SYRENTO\r\nPROPERTY MANAGEMENT SERVICES",
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
            ForeColor = AppTheme.PrimaryBlue,
            TextAlign = ContentAlignment.MiddleRight,
            BackColor = Color.Transparent
        });

        var informationCard = new ModernCard
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(18, 66, 18, 14),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 20
        };
        informationCard.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(24, 15),
            Text = "Complete unit and tenant details",
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });
        informationCard.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(25, 42),
            Text = "Required fields unlock in order and fetched owner information remains read-only.",
            Font = new Font("Segoe UI", 8.6F),
            ForeColor = AppTheme.TextSecondary
        });
        paymentSummaryLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        paymentSummaryLabel.SetBounds(710, 14, 500, 38);
        paymentSummaryLabel.TextAlign = ContentAlignment.MiddleRight;
        paymentSummaryLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        paymentSummaryLabel.ForeColor = AppTheme.Green;
        paymentSummaryLabel.BackColor = Color.Transparent;
        informationCard.Resize += (_, _) =>
            paymentSummaryLabel.Left = informationCard.ClientSize.Width - paymentSummaryLabel.Width - 24;
        informationCard.Controls.Add(paymentSummaryLabel);

        var fieldGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 11,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = AppTheme.Surface
        };
        fieldGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        fieldGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        for (var row = 0; row < 11; row++)
        {
            fieldGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / 11F));
        }

        var rows = new (string LeftLabel, string LeftGlyph, Color LeftColor, Control LeftInput,
            string RightLabel, string RightGlyph, Color RightColor, Control RightInput)[]
        {
            ("CONDOMINIUM", "▥", AppTheme.PrimaryBlue, condominiumComboBox,
                "UNIT NUMBER", "#", AppTheme.Violet, unitNumberComboBox),
            ("UNIT OWNER", "●", AppTheme.Green, ownerNameTextBox,
                "TENANT", "♟", AppTheme.Orange, tenantComboBox),
            ("OWNER EMAIL", "✉", AppTheme.Violet, ownerEmailTextBox,
                "OWNER CONTACT", "☎", AppTheme.Cyan, ownerContactTextBox),
            ("TENANT EMAIL", "✉", AppTheme.Pink, tenantEmailTextBox,
                "TENANT CONTACT", "☎", AppTheme.Green, tenantContactTextBox),
            ("MONTHLY RENTAL AMOUNT", "₱", AppTheme.Pink, rentalAmountNumeric,
                "DUES", "%", AppTheme.Green, withDuesComboBox),
            ("ADVANCE MONTHS", "A", AppTheme.Orange, ownerAdvanceMonthsNumeric,
                "DEPOSIT MONTHS", "D", AppTheme.PrimaryBlue, ownerDepositMonthsNumeric),
            ("MERALCO SIN", "⚡", Color.FromArgb(0, 161, 175), meralcoSinTextBox,
                "OCCUPANCY STATUS", "⌂", AppTheme.PrimaryBlue, occupancyStatusComboBox),
            ("KEY RELEASE", "K", AppTheme.Violet, keyReleaseNumeric,
                "ACCESS CARD RELEASE", "C", AppTheme.Cyan, accessCardNumeric),
            ("DURATION UNIT", "U", AppTheme.Orange, stayDurationUnitComboBox,
                "STAY DURATION", "#", AppTheme.PrimaryBlue, stayDurationNumeric),
            ("START OF CONTRACT", "▣", AppTheme.Violet, startDatePicker,
                "END OF CONTRACT (AUTOMATIC)", "▣", AppTheme.Pink, endDatePicker),
            ("AIRCON MAINTENANCE", "❄", AppTheme.Cyan, airconDatePicker,
                "MAINTENANCE STATUS", "✓", AppTheme.Green, maintenanceStatusTextBox)
        };
        for (var row = 0; row < rows.Length; row++)
        {
            var fields = rows[row];
            fieldGrid.Controls.Add(CreateConceptField(
                fields.LeftLabel,
                fields.LeftGlyph,
                fields.LeftColor,
                fields.LeftInput), 0, row);
            fieldGrid.Controls.Add(CreateConceptField(
                fields.RightLabel,
                fields.RightGlyph,
                fields.RightColor,
                fields.RightInput), 1, row);
        }
        informationCard.Controls.Add(fieldGrid);
        fieldGrid.BringToFront();

        var content = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24, 18, 24, 18),
            BackColor = AppTheme.PageBackground
        };
        content.Controls.Add(informationCard);

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 84,
            BackColor = AppTheme.Surface
        };
        var cancelButton = new ModernActionButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(120, 48),
            Text = "✕  Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        saveButton.Size = new Size(168, 48);
        saveButton.Text = existingAssignment is null ? "▣  Assign tenant" : "▣  Save changes";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        endContractButton.Size = new Size(150, 48);
        endContractButton.Text = "End contract";
        endContractButton.BackColor = Color.FromArgb(225, 56, 76);
        endContractButton.ForeColor = Color.White;
        endContractButton.BorderColor = Color.FromArgb(225, 56, 76);
        endContractButton.Cursor = Cursors.Hand;
        endContractButton.Visible = existingAssignment is not null;
        endContractButton.Click += EndContractButton_Click;
        void PositionButtons()
        {
            saveButton.SetBounds(footer.ClientSize.Width - saveButton.Width - 28, 18, saveButton.Width, saveButton.Height);
            cancelButton.SetBounds(saveButton.Left - cancelButton.Width - 12, 18, cancelButton.Width, cancelButton.Height);
        }
        footer.Resize += (_, _) => PositionButtons();
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);
        footer.Controls.Add(endContractButton);
        endContractButton.SetBounds(28, 18, 150, 48);
        PositionButtons();

        Controls.Add(content);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
        UpdatePaymentSummary();
    }

    private static Control CreateConceptField(
        string label,
        string glyph,
        Color accent,
        Control input)
    {
        var container = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(5, 2, 8, 3),
            Padding = new Padding(48, 0, 0, 0),
            BackColor = AppTheme.Surface
        };
        var badge = new ConceptIconBadge
        {
            Location = new Point(3, 9),
            Size = new Size(36, 36),
            Text = glyph,
            Font = new Font("Segoe UI Symbol", 11F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = accent,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var field = new ModernFieldHost(label, input)
        {
            Dock = DockStyle.Fill,
            Margin = Padding.Empty,
            Padding = new Padding(12, 20, 12, 5),
            BackColor = AppTheme.InputBackground
        };
        container.Resize += (_, _) => badge.Top = Math.Max(1, (container.ClientSize.Height - badge.Height) / 2);
        container.Controls.Add(field);
        container.Controls.Add(badge);
        badge.BringToFront();
        return container;
    }

    private void UpdatePaymentSummary()
    {
        paymentSummaryLabel.Text =
            $"ⓘ  {ownerAdvanceMonthsNumeric.Value:0} MONTH(S) ADVANCE  •  " +
            $"{ownerDepositMonthsNumeric.Value:0} MONTH(S) DEPOSIT";
    }

    private void BuildLegacyInterface()
    {
        ConfigureInputs();

        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 112,
            BackColor = AppTheme.Surface,
            Padding = new Padding(36, 18, 36, 12)
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 32,
            Text = existingAssignment is null
                ? "Complete each field in order. Later fields unlock after the required selection is made."
                : "Update the selected assignment information and save the changes.",
            Font = new Font("Segoe UI", 9.3F),
            ForeColor = AppTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 46,
            Text = existingAssignment is null
                ? "Assign tenant to condominium"
                : "Update tenant assignment",
            Font = new Font("Segoe UI Semibold", 22F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });

        var pages = new Dictionary<string, Control>(StringComparer.OrdinalIgnoreCase)
        {
            ["Placement"] = CreateTabPage(
                "Placement",
                "Choose the condominium and its registered unit before continuing.",
                ("CONDOMINIUM", (Control)condominiumComboBox),
                ("UNIT NUMBER", unitNumberComboBox),
                ("OCCUPANCY STATUS", occupancyStatusComboBox)),
            ["Owner"] = CreateTabPage(
                "Unit owner information",
                "These details are fetched from the owner linked to the selected unit.",
                ("OWNER'S NAME", ownerNameTextBox),
                ("OWNER EMAIL ADDRESS", ownerEmailTextBox),
                ("OWNER CONTACT NUMBER", ownerContactTextBox)),
            ["Tenant"] = CreateTabPage(
                "Tenant information",
                "Select the tenant and confirm the contact information for this assignment.",
                ("TENANT", tenantComboBox),
                ("MAIN TENANT CONTACT NUMBER", tenantContactTextBox),
                ("MAIN TENANT EMAIL ADDRESS", tenantEmailTextBox)),
            ["Releases"] = CreateTabPage(
                "Release and utility information",
                "Enter the utility reference, release quantities and dues status.",
                ("MERALCO SIN", meralcoSinTextBox),
                ("KEY RELEASE", keyReleaseNumeric),
                ("ACCESS CARD RELEASE", accessCardNumeric),
                ("DUES", withDuesComboBox)),
            ["Payments"] = CreateTabPage(
                "Owner payment information",
                "Enter the monthly rent, then the number of advance and deposit months.",
                ("RENTAL AMOUNT TO OWNER", rentalAmountNumeric),
                ("OWNER ADVANCE (MONTHS)", ownerAdvanceMonthsNumeric),
                ("OWNER DEPOSIT (MONTHS)", ownerDepositMonthsNumeric)),
            ["Contract"] = CreateTabPage(
                "Contract and maintenance",
                "Choose Days, Weeks or Months first, then enter the duration and contract start date.",
                ("DURATION UNIT", stayDurationUnitComboBox),
                ("STAY DURATION", stayDurationNumeric),
                ("START OF CONTRACT", startDatePicker),
                ("END OF CONTRACT (AUTOMATIC)", endDatePicker),
                ("6 MONTHS MAINTENANCE AIRCON", airconDatePicker),
                ("MAINTENANCE STATUS", maintenanceStatusTextBox))
        };

        var pagesHost = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.PageBackground };
        foreach (var page in pages.Values)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            pagesHost.Controls.Add(page);
        }

        var tabBar = new Panel { Dock = DockStyle.Fill, BackColor = AppTheme.PageBackground };
        var tabSelector = new ModernCard
        {
            Location = new Point(0, 5),
            Size = new Size(914, 50),
            BackColor = AppTheme.InputBackground,
            BorderColor = AppTheme.Border,
            CornerRadius = 15
        };
        tabBar.Controls.Add(tabSelector);
        var tabButtons = new Dictionary<string, ModernActionButton>(StringComparer.OrdinalIgnoreCase);
        var tabNames = new[] { "Placement", "Owner", "Tenant", "Releases", "Payments", "Contract" };
        for (var index = 0; index < tabNames.Length; index++)
        {
            var tabName = tabNames[index];
            var button = new ModernActionButton
            {
                Location = new Point(5 + (index * 151), 5),
                Size = new Size(145, 40),
                Text = tabName,
                BackColor = AppTheme.Surface,
                ForeColor = AppTheme.TextPrimary,
                BorderColor = AppTheme.InputBackground,
                Cursor = Cursors.Hand
            };
            tabButtons[tabName] = button;
            tabSelector.Controls.Add(button);
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
            Padding = new Padding(28, 12, 28, 18),
            BackColor = AppTheme.PageBackground
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        content.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        content.Controls.Add(tabBar, 0, 0);
        content.Controls.Add(pagesHost, 0, 1);
        ShowTab("Placement");

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 84,
            BackColor = AppTheme.Surface
        };
        var cancelButton = new ModernActionButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(112, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        saveButton.Size = new Size(146, 48);
        saveButton.Text = existingAssignment is null ? "Assign tenant" : "Save changes";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        endContractButton.Size = new Size(138, 48);
        endContractButton.Text = "End contract";
        endContractButton.BackColor = Color.FromArgb(210, 55, 65);
        endContractButton.ForeColor = Color.White;
        endContractButton.BorderColor = Color.FromArgb(210, 55, 65);
        endContractButton.Cursor = Cursors.Hand;
        endContractButton.Visible = existingAssignment is not null;
        endContractButton.Click += EndContractButton_Click;
        void PositionButtons()
        {
            saveButton.SetBounds(footer.ClientSize.Width - saveButton.Width - 28, 18, saveButton.Width, saveButton.Height);
            cancelButton.SetBounds(saveButton.Left - cancelButton.Width - 12, 18, cancelButton.Width, cancelButton.Height);
        }
        footer.Resize += (_, _) => PositionButtons();
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);
        footer.Controls.Add(endContractButton);
        endContractButton.SetBounds(28, 18, 138, 48);
        PositionButtons();

        Controls.Add(content);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private void ConfigureInputs()
    {
        ConfigureComboBox(condominiumComboBox);
        condominiumComboBox.SelectedIndexChanged += CondominiumComboBox_SelectedIndexChanged;
        ConfigureComboBox(tenantComboBox);
        tenantComboBox.SelectedIndexChanged += TenantComboBox_SelectedIndexChanged;
        ConfigureComboBox(occupancyStatusComboBox);
        ConfigureComboBox(unitNumberComboBox);
        unitNumberComboBox.SelectedIndexChanged += (_, eventArgs) =>
        {
            UnitOwnerSelectionChanged();
            UnitNumberComboBox_SelectedIndexChanged(_, eventArgs);
        };
        ConfigureComboBox(withDuesComboBox);
        withDuesComboBox.Items.AddRange(new object[] { "No Dues", "With Dues" });
        withDuesComboBox.SelectedIndex = -1;
        withDuesComboBox.SelectedIndexChanged += (_, _) => UpdateFieldAvailability();
        ConfigureComboBox(stayDurationUnitComboBox);
        stayDurationUnitComboBox.Items.AddRange(new object[] { "Days", "Weeks", "Months" });
        stayDurationUnitComboBox.SelectedIndex = -1;
        stayDurationUnitComboBox.SelectedIndexChanged += (_, _) =>
        {
            UpdateFieldAvailability();
            RecalculateContractEndDate();
        };
        occupancyStatusComboBox.Items.AddRange(new object[] { "RENTED", "VACANT" });
        occupancyStatusComboBox.SelectedIndex = 0;

        ownerNameTextBox.PlaceholderText = "Property owner's full name";
        meralcoSinTextBox.PlaceholderText = "Service identification number";
        maintenanceStatusTextBox.PlaceholderText = "Scheduled / Completed / Pending";
        ownerEmailTextBox.PlaceholderText = "owner@email.com";
        ownerContactTextBox.PlaceholderText = "Owner contact number";
        tenantContactTextBox.PlaceholderText = "Tenant contact number";
        tenantEmailTextBox.PlaceholderText = "tenant@email.com";

        ConfigureWholeNumberInput(keyReleaseNumeric, 100000M);
        ConfigureWholeNumberInput(accessCardNumeric, 100000M);
        ConfigureMoneyInput(rentalAmountNumeric);
        rentalAmountNumeric.ValueChanged += (_, _) => UpdateFieldAvailability();
        rentalAmountNumeric.TextChanged += (_, _) => UpdateFieldAvailability();
        ConfigureWholeNumberInput(ownerAdvanceMonthsNumeric, 120M);
        ConfigureWholeNumberInput(ownerDepositMonthsNumeric, 120M);
        ownerAdvanceMonthsNumeric.ValueChanged += (_, _) => UpdatePaymentSummary();
        ownerDepositMonthsNumeric.ValueChanged += (_, _) => UpdatePaymentSummary();
        stayDurationNumeric.Maximum = 3650M;
        stayDurationNumeric.ValueChanged += (_, _) => ContractDurationChanged();
        stayDurationNumeric.TextChanged += (_, _) => ContractDurationChanged();
        ownerNameTextBox.ReadOnly = true;
        ownerEmailTextBox.ReadOnly = true;
        ownerContactTextBox.ReadOnly = true;
        meralcoSinTextBox.ReadOnly = true;
        ConfigureDatePicker(startDatePicker);
        ConfigureDatePicker(endDatePicker);
        ConfigureDatePicker(airconDatePicker);
        startDatePicker.ValueChanged += (_, _) =>
        {
            UpdateFieldAvailability();
            RecalculateContractEndDate();
        };
        airconDatePicker.ValueChanged += (_, _) => UpdateFieldAvailability();
        UpdateFieldAvailability();
    }

    private static ModernCard CreateTabPage(
        string title,
        string subtitle,
        params (string Label, Control Input)[] fields)
    {
        var card = new ModernCard
        {
            Margin = Padding.Empty,
            Padding = new Padding(18, 76, 12, 14),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 17
        };
        card.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(22, 18),
            Text = title,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });
        card.Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(23, 49),
            Text = subtitle,
            Font = new Font("Segoe UI", 8.8F),
            ForeColor = AppTheme.TextSecondary
        });

        var fieldGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = fields.Length,
            BackColor = AppTheme.Surface,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        fieldGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        for (var row = 0; row < fields.Length; row++)
        {
            fieldGrid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / fields.Length));
        }

        for (var index = 0; index < fields.Length; index++)
        {
            var host = new ModernFieldHost(fields[index].Label, fields[index].Input)
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 3, 10, 6)
            };
            fieldGrid.Controls.Add(host, 0, index);
        }
        card.Controls.Add(fieldGrid);
        return card;
    }

    private void UpdateFieldAvailability()
    {
        var hasCondominium = condominiumComboBox.SelectedItem is not null;
        unitNumberComboBox.Enabled = hasCondominium;
        var hasUnit = hasCondominium && unitNumberComboBox.SelectedItem is not null;
        tenantComboBox.Enabled = hasUnit;
        var hasTenant = hasUnit && tenantComboBox.SelectedItem is not null;

        foreach (var input in new Control[]
        {
            occupancyStatusComboBox,
            keyReleaseNumeric,
            accessCardNumeric,
            rentalAmountNumeric,
            withDuesComboBox,
            stayDurationNumeric,
            ownerAdvanceMonthsNumeric,
            ownerDepositMonthsNumeric,
            tenantContactTextBox,
            tenantEmailTextBox
        })
        {
            input.Enabled = hasTenant;
        }

        var hasRentalAmount = hasTenant && GetEnteredNumericValue(rentalAmountNumeric) > 0M;
        ownerAdvanceMonthsNumeric.Enabled = hasRentalAmount;
        ownerDepositMonthsNumeric.Enabled = hasRentalAmount;

        stayDurationUnitComboBox.Enabled = hasTenant;
        var hasDurationUnit = hasTenant && stayDurationUnitComboBox.SelectedItem is not null;
        stayDurationNumeric.Enabled = hasDurationUnit;
        var duration = GetEnteredStayDuration();
        var hasDuration = hasTenant &&
            hasDurationUnit &&
            duration > 0;
        startDatePicker.Enabled = hasDuration;
        endDatePicker.Enabled = false;
        airconDatePicker.Enabled = hasDuration && startDatePicker.Checked;
        maintenanceStatusTextBox.Enabled = hasDuration && airconDatePicker.Checked;
        var hasDuesChoice = withDuesComboBox.SelectedItem is not null;
        saveButton.Enabled = hasDuration && hasDuesChoice;
    }

    private static decimal GetEnteredNumericValue(NumericUpDown input) =>
        decimal.TryParse(
            input.Text,
            NumberStyles.Number,
            CultureInfo.CurrentCulture,
            out var value)
                ? value
                : 0M;

    private void ContractDurationChanged()
    {
        UpdateFieldAvailability();
        RecalculateContractEndDate();
    }

    private int GetEnteredStayDuration()
    {
        if (!decimal.TryParse(
                stayDurationNumeric.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out var duration) ||
            duration <= 0 ||
            duration > stayDurationNumeric.Maximum)
        {
            return 0;
        }

        return decimal.ToInt32(decimal.Truncate(duration));
    }

    private void RecalculateContractEndDate()
    {
        if (calculatingContractEndDate)
        {
            return;
        }

        var duration = GetEnteredStayDuration();
        if (duration <= 0 ||
            stayDurationUnitComboBox.SelectedItem is null ||
            !startDatePicker.Checked)
        {
            endDatePicker.Checked = false;
            return;
        }

        var contractEndExclusive = stayDurationUnitComboBox.Text switch
        {
            "Days" => startDatePicker.Value.Date.AddDays(duration),
            "Weeks" => startDatePicker.Value.Date.AddDays(duration * 7D),
            "Months" => startDatePicker.Value.Date.AddMonths(duration),
            _ => startDatePicker.Value.Date
        };

        calculatingContractEndDate = true;
        try
        {
            endDatePicker.Value = contractEndExclusive.AddDays(-1);
            endDatePicker.Checked = true;
        }
        finally
        {
            calculatingContractEndDate = false;
        }
    }

    private static void ConfigureComboBox(ModernComboBox comboBox)
    {
        comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        comboBox.FlatStyle = FlatStyle.Flat;
        comboBox.DrawMode = DrawMode.OwnerDrawFixed;
        comboBox.ItemHeight = 36;
    }

    private static void ConfigureDatePicker(DateTimePicker picker)
    {
        picker.Format = DateTimePickerFormat.Custom;
        picker.CustomFormat = "MMM d, yyyy";
        picker.ShowCheckBox = true;
        picker.Checked = false;
        picker.CalendarForeColor = AppTheme.TextPrimary;
        picker.CalendarMonthBackground = AppTheme.InputBackground;
    }

    private static void ConfigureWholeNumberInput(NumericUpDown input, decimal maximum)
    {
        input.DecimalPlaces = 0;
        input.Maximum = maximum;
        input.ThousandsSeparator = true;
    }

    private static void ConfigureMoneyInput(NumericUpDown input)
    {
        input.DecimalPlaces = 2;
        input.Maximum = 100000000M;
        input.ThousandsSeparator = true;
    }

}

internal sealed class ConceptIconBadge : Label
{
    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width < 2 || Height < 2)
        {
            return;
        }

        using var path = new GraphicsPath();
        path.AddEllipse(0, 0, Width, Height);
        Region = new Region(path);
    }
}
