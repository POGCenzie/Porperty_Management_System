namespace PropertyManagementSystem;

internal partial class ConfirmationDialog : Form
{
    private void ConfigureWindow(string title)
    {
        Text = $"{title} - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(500, 285);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = AppTheme.PageBackground;
        Font = new Font("Segoe UI", 10F);
    }

    private void BuildInterface(string title, string message, string confirmText)
    {
        var card = new ModernCard
        {
            Location = new Point(24, 24),
            Size = new Size(452, 168),
            Padding = new Padding(24),
            BackColor = AppTheme.Surface,
            BorderColor = AppTheme.Border,
            CornerRadius = 16
        };
        card.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 10, 0, 0),
            Text = message,
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = AppTheme.TextSecondary,
            TextAlign = ContentAlignment.TopLeft
        });
        card.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = title,
            Font = new Font("Segoe UI Semibold", 17F, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary
        });
        var cancelButton = new ModernActionButton
        {
            Location = new Point(252, 214),
            Size = new Size(104, 46),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        var confirmButton = new ModernActionButton
        {
            Location = new Point(368, 214),
            Size = new Size(108, 46),
            Text = confirmText,
            BackColor = Color.FromArgb(210, 55, 65),
            ForeColor = Color.White,
            BorderColor = Color.FromArgb(210, 55, 65),
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        Controls.Add(card);
        Controls.Add(cancelButton);
        Controls.Add(confirmButton);
        AcceptButton = confirmButton;
        CancelButton = cancelButton;
    }
}
