using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;

namespace PropertyManagementSystem;

internal sealed class ModernCard : Panel
{
    internal int CornerRadius { get; set; } = 12;
    internal Color BorderColor { get; set; } = AppTheme.Border;

    internal ModernCard()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (Width < 2 || Height < 2)
        {
            return;
        }

        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
        using var pen = new Pen(BorderColor);
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class ModernFieldHost : Panel
{
    private bool focused;

    internal ModernFieldHost(string label, Control input)
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = AppTheme.InputBackground;
        Padding = new Padding(13, 25, 13, 8);
        Controls.Add(new Label
        {
            AutoSize = true,
            Location = new Point(14, 7),
            Text = label,
            Font = new Font("Segoe UI Semibold", 8.1F, FontStyle.Bold),
            ForeColor = AppTheme.TextSecondary,
            BackColor = Color.Transparent
        });

        input.Dock = DockStyle.Fill;
        input.Font = new Font("Segoe UI", 9.3F);
        input.BackColor = AppTheme.InputBackground;
        input.ForeColor = AppTheme.TextPrimary;
        if (input is TextBox textBox)
        {
            textBox.BorderStyle = BorderStyle.None;
        }
        else if (input is ComboBox comboBox)
        {
            comboBox.FlatStyle = FlatStyle.Flat;
        }
        else if (input is NumericUpDown numeric)
        {
            numeric.BorderStyle = BorderStyle.None;
        }

        input.Enter += (_, _) => { focused = true; Invalidate(); };
        input.Leave += (_, _) => { focused = false; Invalidate(); };
        Controls.Add(input);
        input.BringToFront();
        Click += (_, _) => input.Focus();
    }

    protected override void OnResize(EventArgs eventargs)
    {
        base.OnResize(eventargs);
        if (Width < 2 || Height < 2)
        {
            return;
        }

        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(0, 0, Width, Height), 9);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(1, 1, Width - 3, Height - 3), 8);
        using var pen = new Pen(focused ? AppTheme.PrimaryBlue : AppTheme.Border, focused ? 2F : 1F);
        e.Graphics.DrawPath(pen, path);
    }
}

internal sealed class ModernActionButton : Button
{
    private bool hovered;
    internal Color BorderColor { get; set; } = AppTheme.Border;

    internal ModernActionButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
        Font = new Font("Segoe UI Semibold", 8.8F, FontStyle.Bold);
        DoubleBuffered = true;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        hovered = true;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        hovered = false;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width < 2 || Height < 2)
        {
            return;
        }

        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(0, 0, Width, Height), 8);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
        var fill = hovered && BackColor == AppTheme.PrimaryBlue
            ? AppTheme.PrimaryBlueHover
            : BackColor;
        using var path = ModernUiGeometry.CreateRoundedPath(bounds, 8);
        using var brush = new SolidBrush(fill);
        using var pen = new Pen(BorderColor);
        e.Graphics.Clear(Parent?.BackColor ?? AppTheme.Surface);
        e.Graphics.FillPath(brush, path);
        e.Graphics.DrawPath(pen, path);
        TextRenderer.DrawText(e.Graphics, Text, Font, bounds, ForeColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

internal sealed class ModernComboBox : ComboBox
{
    private const int WmPaint = 0x000F;
    private const int ArrowAreaWidth = 38;

    internal ModernComboBox()
    {
        DropDownStyle = ComboBoxStyle.DropDownList;
        FlatStyle = FlatStyle.Flat;
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 32;
        DropDownHeight = 232;
        IntegralHeight = false;
        MaxDropDownItems = 8;
        BackColor = AppTheme.InputBackground;
        ForeColor = AppTheme.TextPrimary;
        Font = new Font("Segoe UI", 9.3F);
        SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count)
        {
            return;
        }

        var isEditArea = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;
        var selected = !isEditArea &&
            (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        using var background = new SolidBrush(
            isEditArea
                ? AppTheme.InputBackground
                : selected
                    ? AppTheme.Selection
                    : AppTheme.ElevatedSurface);
        e.Graphics.FillRectangle(background, e.Bounds);

        var textBounds = new Rectangle(
            e.Bounds.Left + 14,
            e.Bounds.Top,
            Math.Max(1, e.Bounds.Width - 28),
            e.Bounds.Height);
        TextRenderer.DrawText(
            e.Graphics,
            GetItemText(Items[e.Index]),
            Font,
            textBounds,
            selected || isEditArea ? AppTheme.TextPrimary : AppTheme.TextSecondary,
            TextFormatFlags.Left |
            TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis |
            TextFormatFlags.NoPrefix);

        if ((e.State & DrawItemState.Focus) == DrawItemState.Focus)
        {
            e.DrawFocusRectangle();
        }
    }

    protected override void WndProc(ref Message message)
    {
        base.WndProc(ref message);
        if (message.Msg != WmPaint || !IsHandleCreated || Width < ArrowAreaWidth)
        {
            return;
        }

        using var graphics = Graphics.FromHwnd(Handle);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var arrowBounds = new Rectangle(
            ClientSize.Width - ArrowAreaWidth,
            1,
            ArrowAreaWidth - 1,
            Math.Max(1, ClientSize.Height - 2));
        using var background = new SolidBrush(
            DroppedDown ? AppTheme.SoftBlue : AppTheme.InputBackground);
        graphics.FillRectangle(background, arrowBounds);

        using var separator = new Pen(AppTheme.Border);
        graphics.DrawLine(
            separator,
            arrowBounds.Left,
            8,
            arrowBounds.Left,
            Math.Max(8, ClientSize.Height - 8));

        var centerX = arrowBounds.Left + (arrowBounds.Width / 2);
        var centerY = arrowBounds.Top + (arrowBounds.Height / 2);
        using var arrowPen = new Pen(AppTheme.PrimaryBlue, 2F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };
        var direction = DroppedDown ? -1 : 1;
        graphics.DrawLines(arrowPen, new[]
        {
            new Point(centerX - 5, centerY - (2 * direction)),
            new Point(centerX, centerY + (3 * direction)),
            new Point(centerX + 5, centerY - (2 * direction))
        });
    }

    protected override void OnDropDown(EventArgs e)
    {
        DropDownWidth = Math.Max(Width, MeasureDropDownWidth());
        base.OnDropDown(e);
        Invalidate();
    }

    protected override void OnDropDownClosed(EventArgs e)
    {
        base.OnDropDownClosed(e);
        Invalidate();
    }

    private int MeasureDropDownWidth()
    {
        var width = Width;
        foreach (var item in Items)
        {
            width = Math.Max(
                width,
                TextRenderer.MeasureText(GetItemText(item), Font).Width + 42);
        }

        return Math.Min(520, width);
    }
}

internal sealed class ModernSearchBox : UserControl
{
    private readonly TextBox input = new();
    private bool focused;
    private bool hoveringClear;

    internal ModernSearchBox()
    {
        Height = 36;
        MinimumSize = new Size(140, 34);
        BackColor = AppTheme.InputBackground;
        DoubleBuffered = true;
        Cursor = Cursors.IBeam;

        input.BorderStyle = BorderStyle.None;
        input.BackColor = AppTheme.InputBackground;
        input.ForeColor = AppTheme.TextPrimary;
        input.Font = new Font("Segoe UI", 9F);
        input.TextChanged += (_, _) =>
        {
            OnTextChanged(EventArgs.Empty);
            Invalidate();
        };
        input.Enter += (_, _) =>
        {
            focused = true;
            Invalidate();
        };
        input.Leave += (_, _) =>
        {
            focused = false;
            Invalidate();
        };
        Controls.Add(input);
        Click += (_, _) => input.Focus();
    }

    [AllowNull]
    public override string Text
    {
        get => input.Text;
        set => input.Text = value ?? string.Empty;
    }

    internal string PlaceholderText
    {
        get => input.PlaceholderText;
        set => input.PlaceholderText = value;
    }

    internal void FocusInput() => input.Focus();

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (Width < 2 || Height < 2)
        {
            return;
        }

        var clearSpace = string.IsNullOrEmpty(input.Text) ? 16 : 43;
        input.SetBounds(
            42,
            Math.Max(1, (Height - input.PreferredHeight) / 2),
            Math.Max(1, Width - 42 - clearSpace),
            input.PreferredHeight);
        using var path = ModernUiGeometry.CreateRoundedPath(new Rectangle(0, 0, Width, Height), 9);
        Region = new Region(path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = ModernUiGeometry.CreateRoundedPath(bounds, 8);
        using var background = new SolidBrush(AppTheme.InputBackground);
        using var border = new Pen(focused ? AppTheme.PrimaryBlue : AppTheme.Border, focused ? 2F : 1F);
        e.Graphics.FillPath(background, path);
        e.Graphics.DrawPath(border, path);

        var iconColor = focused ? AppTheme.PrimaryBlue : AppTheme.TextSecondary;
        using var iconPen = new Pen(iconColor, 1.8F)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        var iconTop = (Height - 15) / 2;
        e.Graphics.DrawEllipse(iconPen, 15, iconTop, 10, 10);
        e.Graphics.DrawLine(iconPen, 24, iconTop + 9, 29, iconTop + 14);

        if (!string.IsNullOrEmpty(input.Text))
        {
            var clearBounds = new Rectangle(Width - 37, (Height - 28) / 2, 28, 28);
            if (hoveringClear)
            {
                using var clearBackground = new SolidBrush(AppTheme.SoftBlue);
                e.Graphics.FillEllipse(clearBackground, clearBounds);
            }

            using var clearPen = new Pen(AppTheme.TextSecondary, 1.7F)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            e.Graphics.DrawLine(clearPen, clearBounds.Left + 9, clearBounds.Top + 9, clearBounds.Right - 9, clearBounds.Bottom - 9);
            e.Graphics.DrawLine(clearPen, clearBounds.Right - 9, clearBounds.Top + 9, clearBounds.Left + 9, clearBounds.Bottom - 9);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var nowHoveringClear = !string.IsNullOrEmpty(input.Text) && e.X >= Width - 43;
        if (nowHoveringClear == hoveringClear)
        {
            return;
        }

        hoveringClear = nowHoveringClear;
        Cursor = hoveringClear ? Cursors.Hand : Cursors.IBeam;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        hoveringClear = false;
        Cursor = Cursors.IBeam;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (!string.IsNullOrEmpty(input.Text) && e.X >= Width - 43)
        {
            input.Clear();
            input.Focus();
            return;
        }

        input.Focus();
    }
}

internal sealed class ModernLineListBox : ListBox
{
    private int hoveredIndex = -1;

    internal ModernLineListBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        ItemHeight = 52;
        BorderStyle = BorderStyle.None;
        IntegralHeight = false;
        BackColor = AppTheme.Surface;
        ForeColor = AppTheme.TextPrimary;
        Font = new Font("Segoe UI", 9.5F);
        DoubleBuffered = true;
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0 || e.Index >= Items.Count)
        {
            return;
        }

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var hovered = e.Index == hoveredIndex;
        var rowBounds = new Rectangle(
            e.Bounds.Left + 2,
            e.Bounds.Top + 3,
            Math.Max(1, e.Bounds.Width - 5),
            Math.Max(1, e.Bounds.Height - 6));
        var rowColor = selected
            ? AppTheme.Selection
            : hovered
                ? AppTheme.SoftBlue
                : AppTheme.Surface;
        using (var path = ModernUiGeometry.CreateRoundedPath(rowBounds, 7))
        using (var brush = new SolidBrush(rowColor))
        using (var border = new Pen(selected ? AppTheme.PrimaryBlue : AppTheme.Border))
        {
            e.Graphics.FillPath(brush, path);
            e.Graphics.DrawPath(border, path);
        }

        var iconBounds = new Rectangle(rowBounds.Left + 10, rowBounds.Top + 8, 29, 29);
        using (var iconBrush = new SolidBrush(selected ? AppTheme.PrimaryBlue : AppTheme.SoftBlue))
        {
            e.Graphics.FillEllipse(iconBrush, iconBounds);
        }
        var itemText = GetItemText(Items[e.Index]);
        var initial = string.IsNullOrWhiteSpace(itemText)
            ? "C"
            : char.ToUpperInvariant(itemText[0]).ToString();
        using var initialFont = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
        TextRenderer.DrawText(
            e.Graphics,
            initial,
            initialFont,
            iconBounds,
            selected ? Color.White : AppTheme.PrimaryBlue,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        var nameBounds = new Rectangle(
            iconBounds.Right + 11,
            rowBounds.Top + 5,
            Math.Max(1, rowBounds.Right - iconBounds.Right - 18),
            25);
        using var nameFont = new Font("Segoe UI Semibold", 9.4F, FontStyle.Bold);
        TextRenderer.DrawText(
            e.Graphics,
            itemText,
            nameFont,
            nameBounds,
            AppTheme.TextPrimary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        TextRenderer.DrawText(
            e.Graphics,
            "Condominium",
            Font,
            new Rectangle(nameBounds.Left, nameBounds.Bottom - 1, nameBounds.Width, 18),
            AppTheme.TextSecondary,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var index = IndexFromPoint(e.Location);
        if (index == hoveredIndex)
        {
            return;
        }

        var previous = hoveredIndex;
        hoveredIndex = index;
        if (previous >= 0)
        {
            Invalidate(GetItemRectangle(previous));
        }
        if (hoveredIndex >= 0)
        {
            Invalidate(GetItemRectangle(hoveredIndex));
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        var previous = hoveredIndex;
        hoveredIndex = -1;
        if (previous >= 0)
        {
            Invalidate(GetItemRectangle(previous));
        }
    }
}

internal static class ModernUiGeometry
{
    internal static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        if (diameter <= 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

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
