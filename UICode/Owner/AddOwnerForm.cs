namespace PropertyManagementSystem;

internal partial class AddOwnerForm : Form
{
    private readonly TextBox ownerNameTextBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly TextBox contactTextBox = new();
    private readonly ModernComboBox statusComboBox = new();
    private readonly ModernActionButton saveButton = new();

    private void ConfigureWindow()
    {
        Text = existingOwner is null ? "Add Owner - Syrento" : "Update Owner - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(620, 660);
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
            Height = 108,
            Padding = new Padding(36, 18, 36, 12),
            BackColor = AppTheme.Surface
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = existingOwner is null
                ? "Owner information is reused by every condominium unit linked to this owner."
                : "Changes are reflected wherever this owner is linked to an active assignment.",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = AppTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = existingOwner is null ? "Add unit owner" : "Update unit owner",
            Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });

        ownerNameTextBox.PlaceholderText = "Enter the owner's full name";
        emailTextBox.PlaceholderText = "owner@email.com";
        contactTextBox.PlaceholderText = "Owner contact number";
        statusComboBox.Items.AddRange(new object[] { "Active", "Inactive" });
        statusComboBox.SelectedIndex = 0;
        statusComboBox.Enabled = existingOwner is not null;
        var card = new ModernCard
        {
            Location = new Point(32, 136),
            Size = new Size(556, 420),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 17
        };
        card.Controls.Add(CreateField("OWNER FULL NAME", ownerNameTextBox, 22));
        card.Controls.Add(CreateField("EMAIL ADDRESS - OPTIONAL", emailTextBox, 119));
        card.Controls.Add(CreateField("CONTACT NUMBER - OPTIONAL", contactTextBox, 216));
        card.Controls.Add(CreateField("STATUS", statusComboBox, 313));

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 82, BackColor = AppTheme.Surface };
        var cancelButton = new ModernActionButton
        {
            Location = new Point(364, 17),
            Size = new Size(102, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.SetBounds(478, 17, 110, 48);
        saveButton.Text = existingOwner is null ? "Save owner" : "Save changes";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);

        Controls.Add(card);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    private static ModernFieldHost CreateField(string label, Control input, int top) =>
        new(label, input) { Location = new Point(22, top), Size = new Size(512, 72) };
}
