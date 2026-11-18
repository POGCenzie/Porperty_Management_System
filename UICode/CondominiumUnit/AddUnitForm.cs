namespace PropertyManagementSystem;

internal partial class AddUnitForm : Form
{
    private readonly TextBox unitNumberTextBox = new();
    private readonly TextBox meralcoSinTextBox = new();
    private readonly ModernComboBox ownerComboBox = new();
    private readonly ModernActionButton addOwnerButton = new();
    private readonly ModernActionButton saveButton = new();

    private void ConfigureWindow()
    {
        Text = "Add Unit - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(580, 570);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);
        AutoScaleMode = AutoScaleMode.Dpi;
    }

    private void BuildInterface()
    {
        var header = new Panel
        {
            Dock = DockStyle.Top,
            Height = 112,
            Padding = new Padding(34, 18, 34, 12),
            BackColor = AppTheme.Surface
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 31,
            Text = existingUnit is null
                ? $"Add a unit under {condominium.Name}."
                : $"Update the unit, owner or Meralco SIN under {condominium.Name}.",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = AppTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = existingUnit is null ? "Add unit number" : "Update unit",
            Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });

        unitNumberTextBox.PlaceholderText = "Example: 20-M or 1724";
        var field = new ModernFieldHost("UNIT NUMBER", unitNumberTextBox)
        {
            Location = new Point(32, 148),
            Size = new Size(516, 70)
        };
        ownerComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        var ownerField = new ModernFieldHost("UNIT OWNER", ownerComboBox)
        {
            Location = new Point(32, 232),
            Size = new Size(382, 70)
        };
        addOwnerButton.SetBounds(426, 232, 122, 70);
        addOwnerButton.Text = "+ New owner";
        addOwnerButton.BackColor = AppTheme.Surface;
        addOwnerButton.ForeColor = AppTheme.PrimaryBlue;
        addOwnerButton.BorderColor = AppTheme.Border;
        addOwnerButton.Cursor = Cursors.Hand;
        addOwnerButton.Click += AddOwnerButton_Click;
        meralcoSinTextBox.PlaceholderText = "Enter the unit's Meralco service identifier";
        var meralcoField = new ModernFieldHost("MERALCO SIN", meralcoSinTextBox)
        {
            Location = new Point(32, 316),
            Size = new Size(516, 70)
        };
        var info = new ModernCard
        {
            Location = new Point(32, 402),
            Size = new Size(516, 58),
            Padding = new Padding(16, 0, 16, 0),
            BackColor = AppTheme.SoftBlue,
            BorderColor = AppTheme.Border,
            CornerRadius = 12
        };
        info.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "This unit becomes available for tenant assignment after saving.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 84, BackColor = AppTheme.Surface };
        var cancelButton = new ModernActionButton
        {
            Location = new Point(324, 18),
            Size = new Size(102, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.SetBounds(438, 18, 110, 48);
        saveButton.Text = existingUnit is null ? "Save unit" : "Save changes";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);

        Controls.Add(info);
        Controls.Add(meralcoField);
        Controls.Add(addOwnerButton);
        Controls.Add(ownerField);
        Controls.Add(field);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }
}
