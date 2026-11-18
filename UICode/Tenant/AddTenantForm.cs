namespace PropertyManagementSystem;

internal partial class AddTenantForm : Form
{
    private readonly TextBox tenantNameTextBox = new();
    private readonly TextBox contactTextBox = new();
    private readonly TextBox emailTextBox = new();
    private readonly ModernActionButton saveButton = new();

    private void ConfigureWindow()
    {
        Text = "Add Tenant - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(620, 570);
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
            Text = "Create the tenant profile first, then assign it to a condominium unit.",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = AppTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = "Add tenant",
            Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });

        var card = new ModernCard
        {
            Location = new Point(32, 136),
            Size = new Size(556, 326),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 17
        };
        tenantNameTextBox.PlaceholderText = "Enter the tenant's full name";
        contactTextBox.PlaceholderText = "Example: 09XX XXX XXXX";
        emailTextBox.PlaceholderText = "Example: tenant@email.com";
        card.Controls.Add(CreateField("TENANT FULL NAME", tenantNameTextBox, 22));
        card.Controls.Add(CreateField("CONTACT NUMBER  ·  OPTIONAL", contactTextBox, 119));
        card.Controls.Add(CreateField("EMAIL ADDRESS  ·  OPTIONAL", emailTextBox, 216));

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 82,
            BackColor = AppTheme.Surface
        };
        var cancelButton = new ModernActionButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
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
        saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        saveButton.Text = "Save tenant";
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

    private static ModernFieldHost CreateField(string label, TextBox input, int top) =>
        new(label, input)
        {
            Location = new Point(22, top),
            Size = new Size(512, 72)
        };
}
