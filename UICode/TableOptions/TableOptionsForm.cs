using System.Drawing.Drawing2D;

namespace PropertyManagementSystem
{
    internal partial class TableOptionsForm : Form
    {
        private static Color PrimaryBlue => AppTheme.PrimaryBlue;
        private static Color PageBackground => AppTheme.PageBackground;
        private static Color BorderColor => AppTheme.Border;
        private static Color TextPrimary => AppTheme.TextPrimary;
        private static Color TextSecondary => AppTheme.TextSecondary;
        private static Color SurfaceColor => AppTheme.Surface;

        private readonly ModernCheckedListBox optionsList = new();
        private readonly ModernSearchBox optionsSearchTextBox = new();

        private void ConfigureWindow()
        {
            Text = mode switch
            {
                TableOptionsMode.FilterFields => "Filter Fields - Syrento",
                TableOptionsMode.VisibleColumns => "Showing Columns - Syrento",
                _ => "Needs Attention - Syrento"
            };
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(500, 590);
            MinimumSize = new Size(500, 590);
            MaximumSize = new Size(500, 590);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            BackColor = PageBackground;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;

            using var applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (applicationIcon is not null)
            {
                Icon = (Icon)applicationIcon.Clone();
            }
        }

        private void BuildOptionsInterface()
        {
            var isFilterMode = mode == TableOptionsMode.FilterFields;
            var isAttentionMode = mode == TableOptionsMode.AttentionItems;
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = SurfaceColor,
                Padding = new Padding(30, 18, 30, 10)
            };
            header.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 37,
                Text = isFilterMode
                    ? "Only checked fields are included when using the table search box."
                    : isAttentionMode
                        ? "Choose the reminders that appear on your dashboard."
                        : "Only checked columns are shown in the table.",
                Font = new Font("Segoe UI", 9.3F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            header.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 40,
                Text = isFilterMode
                    ? "Filter fields"
                    : isAttentionMode
                        ? "Needs attention"
                        : "Showing columns",
                Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            });

            var card = new RoundedOptionsPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(24, 22, 24, 20),
                Padding = new Padding(16, 66, 16, 16),
                BackColor = SurfaceColor,
                BorderColor = BorderColor,
                CornerRadius = 14
            };
            optionsSearchTextBox.SetBounds(16, 15, 420, 40);
            optionsSearchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            optionsSearchTextBox.PlaceholderText = isFilterMode
                ? "Search filter fields"
                : isAttentionMode
                    ? "Search reminders"
                    : "Search column names";
            optionsSearchTextBox.TextChanged += OptionsSearchTextBox_TextChanged;
            optionsList.Dock = DockStyle.Fill;
            optionsList.BorderStyle = BorderStyle.None;
            optionsList.BackColor = SurfaceColor;
            optionsList.ForeColor = TextPrimary;
            optionsList.Font = new Font("Segoe UI", 9.5F);
            optionsList.CheckOnClick = true;
            optionsList.IntegralHeight = false;
            optionsList.HorizontalScrollbar = false;
            optionsList.ItemCheck += OptionsList_ItemCheck;
            card.Controls.Add(optionsSearchTextBox);
            card.Controls.Add(optionsList);
            optionsList.BringToFront();

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 74,
                BackColor = SurfaceColor
            };
            var cancelButton = CreateButton("Cancel", SurfaceColor, TextSecondary, BorderColor);
            cancelButton.SetBounds(252, 14, 102, 44);
            cancelButton.DialogResult = DialogResult.Cancel;
            var applyButton = CreateButton("Apply", PrimaryBlue, Color.White, PrimaryBlue);
            applyButton.SetBounds(364, 14, 106, 44);
            applyButton.Click += ApplyButton_Click;
            if (isAttentionMode)
            {
                var selectAllButton = CreateButton("Select all", SurfaceColor, PrimaryBlue, BorderColor);
                selectAllButton.SetBounds(24, 14, 96, 44);
                selectAllButton.Click += (_, _) => SetAllChecks(true);
                var clearButton = CreateButton("Clear", SurfaceColor, TextSecondary, BorderColor);
                clearButton.SetBounds(130, 14, 84, 44);
                clearButton.Click += (_, _) => SetAllChecks(false);
                footer.Controls.Add(selectAllButton);
                footer.Controls.Add(clearButton);
            }
            footer.Controls.Add(cancelButton);
            footer.Controls.Add(applyButton);

            Controls.Add(card);
            Controls.Add(footer);
            Controls.Add(header);
            AcceptButton = applyButton;
            CancelButton = cancelButton;
        }

        private static Button CreateButton(
            string text,
            Color background,
            Color foreground,
            Color border)
        {
            var button = new Button
            {
                Text = text,
                BackColor = background,
                ForeColor = foreground,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            button.FlatAppearance.BorderColor = border;
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private sealed class ModernCheckedListBox : CheckedListBox
        {
            private int hoveredIndex = -1;

            public ModernCheckedListBox()
            {
                DrawMode = DrawMode.OwnerDrawFixed;
                ItemHeight = 42;
                BorderStyle = BorderStyle.None;
                BackColor = SurfaceColor;
                DoubleBuffered = true;
                SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                ItemCheck += (_, _) =>
                {
                    if (IsHandleCreated && !IsDisposed && !Disposing)
                    {
                        BeginInvoke((Action)Invalidate);
                    }
                };
            }

            protected override void OnDrawItem(DrawItemEventArgs e)
            {
                if (e.Index < 0 || e.Index >= Items.Count)
                {
                    return;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rowBounds = new Rectangle(
                    e.Bounds.X + 4,
                    e.Bounds.Y + 2,
                    Math.Max(1, e.Bounds.Width - 8),
                    Math.Max(1, e.Bounds.Height - 4));
                var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                var isHovered = e.Index == hoveredIndex;
                var rowColor = isSelected
                    ? AppTheme.Selection
                    : isHovered
                        ? AppTheme.SoftBlue
                        : SurfaceColor;

                using (var rowPath = CreateRoundedPath(rowBounds, 8))
                using (var rowBrush = new SolidBrush(rowColor))
                {
                    e.Graphics.FillPath(rowBrush, rowPath);
                }

                var checkboxBounds = new Rectangle(
                    rowBounds.Left + 12,
                    rowBounds.Top + ((rowBounds.Height - 20) / 2),
                    20,
                    20);
                var isChecked = GetItemChecked(e.Index);
                using (var checkboxPath = CreateRoundedPath(checkboxBounds, 5))
                using (var checkboxBrush = new SolidBrush(
                    isChecked ? PrimaryBlue : AppTheme.InputBackground))
                using (var checkboxPen = new Pen(
                    isChecked ? PrimaryBlue : AppTheme.Border,
                    1.4F))
                {
                    e.Graphics.FillPath(checkboxBrush, checkboxPath);
                    e.Graphics.DrawPath(checkboxPen, checkboxPath);
                }

                if (isChecked)
                {
                    using var checkPen = new Pen(Color.White, 2.2F)
                    {
                        StartCap = LineCap.Round,
                        EndCap = LineCap.Round,
                        LineJoin = LineJoin.Round
                    };
                    e.Graphics.DrawLines(
                        checkPen,
                        new[]
                        {
                            new Point(checkboxBounds.Left + 5, checkboxBounds.Top + 10),
                            new Point(checkboxBounds.Left + 9, checkboxBounds.Top + 14),
                            new Point(checkboxBounds.Left + 16, checkboxBounds.Top + 6)
                        });
                }

                var textBounds = new Rectangle(
                    checkboxBounds.Right + 12,
                    rowBounds.Top,
                    Math.Max(1, rowBounds.Right - checkboxBounds.Right - 22),
                    rowBounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    GetItemText(Items[e.Index]),
                    Font,
                    textBounds,
                    TextPrimary,
                    TextFormatFlags.Left |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis |
                    TextFormatFlags.NoPrefix);
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);
                var index = IndexFromPoint(e.Location);
                if (index == hoveredIndex)
                {
                    return;
                }

                var previousIndex = hoveredIndex;
                hoveredIndex = index;
                if (previousIndex >= 0)
                {
                    Invalidate(GetItemRectangle(previousIndex));
                }

                if (hoveredIndex >= 0)
                {
                    Invalidate(GetItemRectangle(hoveredIndex));
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                var previousIndex = hoveredIndex;
                hoveredIndex = -1;
                if (previousIndex >= 0)
                {
                    Invalidate(GetItemRectangle(previousIndex));
                }
            }

            protected override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);
                if (e.KeyCode == Keys.Space)
                {
                    BeginInvoke(Invalidate);
                }
            }
        }

        private sealed class RoundedOptionsPanel : Panel
        {
            public int CornerRadius { get; set; } = 14;

            public Color BorderColor { get; set; } = Color.Transparent;

            public RoundedOptionsPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawPath(pen, path);
            }
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
}
