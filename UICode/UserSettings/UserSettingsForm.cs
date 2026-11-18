using System.Drawing.Drawing2D;

namespace PropertyManagementSystem
{
    public partial class UserSettingsForm : Form
    {
        private static Color PrimaryBlue => AppTheme.PrimaryBlue;
        private static Color DarkBlue => AppTheme.PrimaryBlueHover;
        private static Color PageBackground => AppTheme.PageBackground;
        private static Color BorderColor => AppTheme.Border;
        private static Color TextPrimary => AppTheme.TextPrimary;
        private static Color TextSecondary => AppTheme.TextSecondary;
        private static Color SurfaceColor => AppTheme.Surface;

        private readonly TextBox displayNameTextBox = new();
        private readonly TextBox usernameTextBox = new();
        private readonly TextBox currentPasswordTextBox = new();
        private readonly TextBox newPasswordTextBox = new();
        private readonly TextBox confirmPasswordTextBox = new();
        private readonly Label accountTypeValueLabel = new();
        private readonly Label formTitleLabel = new();
        private readonly Label formDescriptionLabel = new();
        private readonly AccountButton saveButton = new();
        private AccountToast? activeToast;

        private void ConfigureWindow()
        {
            Text = "Edit User Account - Syrento";
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(620, 690);
            MinimumSize = new Size(620, 690);
            MaximumSize = new Size(620, 690);
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

        private void BuildAccountInterface()
        {
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 112,
                BackColor = SurfaceColor,
                Padding = new Padding(36, 20, 36, 12)
            };
            formDescriptionLabel.Dock = DockStyle.Bottom;
            formDescriptionLabel.Height = 31;
            formDescriptionLabel.Font = new Font("Segoe UI", 9.5F);
            formDescriptionLabel.ForeColor = TextSecondary;
            formDescriptionLabel.TextAlign = ContentAlignment.MiddleLeft;
            formTitleLabel.Dock = DockStyle.Top;
            formTitleLabel.Height = 42;
            formTitleLabel.Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold);
            formTitleLabel.ForeColor = TextPrimary;
            formTitleLabel.TextAlign = ContentAlignment.MiddleLeft;
            header.Controls.Add(formDescriptionLabel);
            header.Controls.Add(formTitleLabel);

            var card = new RoundedAccountPanel
            {
                Location = new Point(32, 136),
                Size = new Size(556, 508),
                BackColor = SurfaceColor,
                BorderColor = BorderColor,
                CornerRadius = 16
            };

            AddField(card, "Display name", displayNameTextBox, 25, false);
            AddField(card, "Login username", usernameTextBox, 105, false);

            var accountTypeLabel = CreateFieldLabel("Account type", 185);
            accountTypeValueLabel.SetBounds(24, 211, 508, 42);
            accountTypeValueLabel.BackColor = AppTheme.InputBackground;
            accountTypeValueLabel.ForeColor = TextSecondary;
            accountTypeValueLabel.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            accountTypeValueLabel.TextAlign = ContentAlignment.MiddleLeft;
            accountTypeValueLabel.Padding = new Padding(12, 0, 12, 0);
            card.Controls.Add(accountTypeLabel);
            card.Controls.Add(accountTypeValueLabel);

            AddField(
                card,
                administratorEditMode ? "Administrator password" : "Current password",
                currentPasswordTextBox,
                274,
                true);
            AddField(card, "New password (optional)", newPasswordTextBox, 354, true);
            AddField(card, "Confirm new password", confirmPasswordTextBox, 434, true);

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 78,
                BackColor = SurfaceColor,
                Padding = new Padding(32, 13, 32, 13)
            };

            var cancelButton = new AccountButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(364, 13),
                Size = new Size(104, 46),
                Text = "Cancel",
                BackColor = SurfaceColor,
                ForeColor = TextSecondary,
                BorderColor = BorderColor,
                DialogResult = DialogResult.Cancel,
                Cursor = Cursors.Hand
            };
            saveButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            saveButton.SetBounds(478, 13, 110, 46);
            saveButton.Text = "Save changes";
            saveButton.BackColor = PrimaryBlue;
            saveButton.ForeColor = Color.White;
            saveButton.BorderColor = PrimaryBlue;
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

        private static void AddField(
            Control parent,
            string labelText,
            TextBox textBox,
            int top,
            bool password)
        {
            parent.Controls.Add(CreateFieldLabel(labelText, top));
            textBox.SetBounds(24, top + 26, 508, 42);
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.BackColor = AppTheme.InputBackground;
            textBox.ForeColor = TextPrimary;
            textBox.Font = new Font("Segoe UI", 10F);
            textBox.UseSystemPasswordChar = password;
            parent.Controls.Add(textBox);
        }

        private static Label CreateFieldLabel(string text, int top)
        {
            return new Label
            {
                AutoSize = true,
                Location = new Point(24, top),
                Text = text,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = TextPrimary
            };
        }

        private void ShowToast(string message)
        {
            activeToast?.Close();
            activeToast?.Dispose();

            var toast = new AccountToast(message);
            activeToast = toast;
            toast.FormClosed += (_, _) =>
            {
                if (ReferenceEquals(activeToast, toast))
                {
                    activeToast = null;
                }
            };
            toast.Location = PointToScreen(new Point(
                ClientSize.Width - toast.Width - 24,
                24));
            toast.Show(this);
        }

        private sealed class RoundedAccountPanel : Panel
        {
            public int CornerRadius { get; set; } = 14;

            public Color BorderColor { get; set; } = Color.Transparent;

            public RoundedAccountPanel()
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

        private sealed class AccountButton : Button
        {
            private bool hovered;

            public Color BorderColor { get; set; } = UserSettingsForm.BorderColor;

            public AccountButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
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
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 10);
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
                var fill = hovered && BackColor == PrimaryBlue ? DarkBlue : BackColor;
                using var path = CreateRoundedPath(bounds, 10);
                using var brush = new SolidBrush(fill);
                using var pen = new Pen(BorderColor);
                e.Graphics.Clear(Parent?.BackColor ?? SurfaceColor);
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    bounds,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private sealed class AccountToast : Form
        {
            private const int Interval = 16;
            private readonly System.Windows.Forms.Timer timer = new() { Interval = Interval };
            private readonly string message;
            private int elapsed;

            public AccountToast(string message)
            {
                this.message = message;
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                Size = new Size(380, 76);
                BackColor = Color.FromArgb(26, 43, 73);
                Opacity = 0D;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 13);
                Region = new Region(path);
                timer.Tick += Animate;
                Click += (_, _) => Close();
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
