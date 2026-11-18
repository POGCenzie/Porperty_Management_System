namespace PropertyManagementSystem;

internal partial class AddCondominiumForm : Form
{
    private readonly TextBox condominiumNameTextBox = new();
    private readonly ModernActionButton saveButton = new();

    private void ConfigureWindow()
    {
        Text = "Add Condominium - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(580, 390);
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
            Padding = new Padding(34, 18, 34, 12),
            BackColor = AppTheme.Surface
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = "Create a building record before adding tenant assignments.",
            Font = new Font("Segoe UI", 9.2F),
            ForeColor = AppTheme.TextSecondary
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = "Add condominium",
            Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });

        condominiumNameTextBox.PlaceholderText = "Example: Vista 878 España";
        var field = new ModernFieldHost("CONDOMINIUM NAME", condominiumNameTextBox)
        {
            Location = new Point(32, 143),
            Size = new Size(516, 68)
        };

        var noteCard = new ModernCard
        {
            Location = new Point(32, 228),
            Size = new Size(516, 55),
            Padding = new Padding(16, 0, 16, 0),
            BackColor = AppTheme.SoftBlue,
            BorderColor = AppTheme.Border,
            CornerRadius = 12
        };
        noteCard.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "You can assign units and tenants after saving this condominium.",
            Font = new Font("Segoe UI", 9F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.MiddleLeft
        });

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 82,
            Padding = new Padding(32, 16, 32, 16),
            BackColor = AppTheme.Surface
        };
        var cancelButton = new ModernActionButton
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(324, 17),
            Size = new Size(102, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        saveButton.SetBounds(438, 17, 110, 48);
        saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        saveButton.Text = "Save";
        saveButton.BackColor = AppTheme.PrimaryBlue;
        saveButton.ForeColor = Color.White;
        saveButton.BorderColor = AppTheme.PrimaryBlue;
        saveButton.Cursor = Cursors.Hand;
        saveButton.Click += SaveButton_Click;
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(saveButton);

        Controls.Add(noteCard);
        Controls.Add(field);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }
}
