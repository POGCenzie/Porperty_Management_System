namespace PropertyManagementSystem;

internal partial class SoaPrintOptionsForm : Form
{
    private readonly ModernComboBox paperSizeComboBox = new();

    private void ConfigureWindow()
    {
        Text = "Print SOA - Syrento";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 330);
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
            Padding = new Padding(32, 18, 32, 12),
            BackColor = AppTheme.Surface
        };
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Text = statement.SoaNumber,
            ForeColor = AppTheme.TextSecondary,
            Font = new Font("Segoe UI", 9.2F)
        });
        header.Controls.Add(new Label
        {
            Dock = DockStyle.Top,
            Height = 44,
            Text = "Print statement of account",
            ForeColor = AppTheme.TextPrimary,
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold)
        });

        paperSizeComboBox.Items.AddRange(SoaPaperSizes.All.Cast<object>().ToArray());
        paperSizeComboBox.SelectedItem = SoaPaperSizes.A4;
        var paperField = new ModernFieldHost("PAPER SIZE", paperSizeComboBox)
        {
            Location = new Point(30, 138),
            Size = new Size(460, 72)
        };

        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 88,
            BackColor = AppTheme.Surface
        };
        var cancelButton = new ModernActionButton
        {
            Location = new Point(244, 19),
            Size = new Size(112, 48),
            Text = "Cancel",
            BackColor = AppTheme.Surface,
            ForeColor = AppTheme.TextSecondary,
            BorderColor = AppTheme.Border,
            DialogResult = DialogResult.Cancel,
            Cursor = Cursors.Hand
        };
        var previewButton = new ModernActionButton
        {
            Location = new Point(368, 19),
            Size = new Size(122, 48),
            Text = "Print preview",
            BackColor = AppTheme.PrimaryBlue,
            ForeColor = Color.White,
            BorderColor = AppTheme.PrimaryBlue,
            DialogResult = DialogResult.OK,
            Cursor = Cursors.Hand
        };
        footer.Controls.Add(cancelButton);
        footer.Controls.Add(previewButton);

        Controls.Add(paperField);
        Controls.Add(footer);
        Controls.Add(header);
        AcceptButton = previewButton;
        CancelButton = cancelButton;
    }
}
