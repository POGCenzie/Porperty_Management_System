using System.Drawing.Drawing2D;

namespace PropertyManagementSystem;

internal sealed class FadingToast : Form
{
    private const int Interval = 16;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = Interval };
    private readonly string message;
    private int elapsed;

    private FadingToast(string message)
    {
        this.message = message;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        Size = new Size(390, 78);
        BackColor = Color.FromArgb(26, 43, 73);
        Opacity = 0D;
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 13);
        Region = new Region(path);
        timer.Tick += Animate;
        Click += (_, _) => Close();
    }

    internal static void ShowMessage(Form owner, string message)
    {
        var toast = new FadingToast(message);
        toast.Location = owner.PointToScreen(new Point(
            owner.ClientSize.Width - toast.Width - 24,
            24));
        toast.Show(owner);
    }

    protected override bool ShowWithoutActivation => true;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        timer.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        using var accent = new SolidBrush(Color.FromArgb(255, 190, 92));
        using var font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
        e.Graphics.FillRectangle(accent, 0, 0, 5, Height);
        TextRenderer.DrawText(
            e.Graphics,
            message,
            font,
            new Rectangle(22, 12, Width - 42, Height - 24),
            Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Animate(object? sender, EventArgs e)
    {
        elapsed += Interval;
        if (elapsed <= 192)
        {
            Opacity = Math.Min(1D, elapsed / 192D);
            return;
        }

        if (elapsed <= 2592)
        {
            Opacity = 1D;
            return;
        }

        if (elapsed <= 2912)
        {
            Opacity = Math.Max(0D, 1D - ((elapsed - 2592D) / 320D));
            return;
        }

        timer.Stop();
        Close();
    }

    private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;
        var arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }
}
