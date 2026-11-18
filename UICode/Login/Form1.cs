using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;

namespace PropertyManagementSystem
{
    public partial class Form1 : Form
    {
        private static Color PrimaryBlue => AppTheme.PrimaryBlue;
        private static Color DarkBlue => AppTheme.PrimaryBlueHover;
        private static Color TextPrimary => AppTheme.TextPrimary;
        private static Color TextSecondary => AppTheme.TextSecondary;
        private static Color BorderColor => AppTheme.Border;
        private static Color SurfaceColor => AppTheme.Surface;

        private readonly ModernTextBox usernameTextBox = new();
        private readonly ModernTextBox passwordTextBox = new();
        private readonly CheckBox rememberCheckBox = new();
        private ToastNotification? activeToast;

        private void ConfigureWindow()
        {
            Text = "Syrento Property Management Service";
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(1180, 720);
            MinimumSize = new Size(960, 620);
            BackColor = SurfaceColor;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            using var applicationIcon = System.Drawing.Icon.ExtractAssociatedIcon(
                Application.ExecutablePath);
            if (applicationIcon is not null)
            {
                Icon = (System.Drawing.Icon)applicationIcon.Clone();
            }
        }

        private void BuildLoginInterface()
        {
            SuspendLayout();
            Controls.Clear();

            var pageLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = SurfaceColor
            };
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44F));
            pageLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56F));
            pageLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            pageLayout.Controls.Add(CreateBrandPanel(), 0, 0);
            pageLayout.Controls.Add(CreateLoginPanel(), 1, 0);
            Controls.Add(pageLayout);

            var themeButton = new ModernActionButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ClientSize.Width - 142, 24),
                Size = new Size(116, 40),
                Text = AppTheme.IsDark ? "White mode" : "Dark mode",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = BorderColor,
                Cursor = Cursors.Hand,
                TabStop = false
            };
            themeButton.Click += (_, _) =>
            {
                AppTheme.Toggle();
                BackColor = SurfaceColor;
                BuildLoginInterface();
            };
            Controls.Add(themeButton);
            themeButton.BringToFront();

            ResumeLayout(true);
        }

        private Control CreateBrandPanel()
        {
            return new BrandArtworkPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
        }

        private Control CreateLoginPanel()
        {
            var hostLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                ColumnCount = 3,
                RowCount = 3,
                Margin = Padding.Empty
            };
            hostLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            hostLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 440F));
            hostLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            hostLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            hostLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 470F));
            hostLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var loginCard = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Margin = Padding.Empty
            };

            var welcomeLabel = new Label
            {
                AutoSize = true,
                Location = new Point(0, 8),
                Text = "Welcome back",
                Font = new Font("Segoe UI Semibold", 25F, FontStyle.Bold),
                ForeColor = TextPrimary
            };

            var instructionLabel = new Label
            {
                AutoSize = true,
                Location = new Point(3, 62),
                Text = "Sign in to continue to your workspace.",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = TextSecondary
            };

            var usernameLabel = CreateFieldLabel("Username or email", 118);
            usernameTextBox.Location = new Point(0, 146);
            usernameTextBox.Size = new Size(440, 54);
            usernameTextBox.PlaceholderText = "Enter your username or email";
            usernameTextBox.TabIndex = 0;
            usernameTextBox.AccessibleName = "Username or email";

            var passwordLabel = CreateFieldLabel("Password", 226);
            passwordTextBox.Location = new Point(0, 254);
            passwordTextBox.Size = new Size(440, 54);
            passwordTextBox.PlaceholderText = "Enter your password";
            passwordTextBox.IsPassword = true;
            passwordTextBox.ShowPasswordToggle = true;
            passwordTextBox.TabIndex = 1;
            passwordTextBox.AccessibleName = "Password";

            rememberCheckBox.Location = new Point(0, 329);
            rememberCheckBox.AutoSize = true;
            rememberCheckBox.Text = "Remember me";
            rememberCheckBox.Font = new Font("Segoe UI", 9.5F);
            rememberCheckBox.ForeColor = TextSecondary;
            rememberCheckBox.Cursor = Cursors.Hand;
            rememberCheckBox.TabIndex = 2;

            var forgotPasswordLink = new LinkLabel
            {
                AutoSize = true,
                Location = new Point(323, 330),
                Text = "Forgot password?",
                Font = new Font("Segoe UI Semibold", 9.5F),
                LinkColor = PrimaryBlue,
                ActiveLinkColor = DarkBlue,
                VisitedLinkColor = PrimaryBlue,
                LinkBehavior = LinkBehavior.NeverUnderline,
                Cursor = Cursors.Hand,
                TabIndex = 3
            };
            forgotPasswordLink.LinkClicked += (_, _) => ShowToast(
                "Password recovery is not connected yet.",
                ToastKind.Information);

            var loginButton = new RoundedButton
            {
                Location = new Point(0, 375),
                Size = new Size(440, 54),
                Text = "Sign in",
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabIndex = 4
            };
            loginButton.Click += LoginButton_Click;
            AcceptButton = loginButton;

            loginCard.Controls.Add(welcomeLabel);
            loginCard.Controls.Add(instructionLabel);
            loginCard.Controls.Add(usernameLabel);
            loginCard.Controls.Add(usernameTextBox);
            loginCard.Controls.Add(passwordLabel);
            loginCard.Controls.Add(passwordTextBox);
            loginCard.Controls.Add(rememberCheckBox);
            loginCard.Controls.Add(forgotPasswordLink);
            loginCard.Controls.Add(loginButton);

            hostLayout.Controls.Add(loginCard, 1, 1);
            return hostLayout;
        }

        private static Label CreateFieldLabel(string text, int top)
        {
            return new Label
            {
                AutoSize = true,
                Location = new Point(1, top),
                Text = text,
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                ForeColor = TextPrimary
            };
        }

        private void ShowToast(string message, ToastKind kind)
        {
            activeToast?.Close();
            activeToast?.Dispose();

            var toast = new ToastNotification(message, kind);
            activeToast = toast;
            toast.FormClosed += (_, _) =>
            {
                if (ReferenceEquals(activeToast, toast))
                {
                    activeToast = null;
                }
            };

            toast.Location = PointToScreen(new Point(
                ClientSize.Width - toast.Width - 28,
                28));
            toast.Show(this);
        }

        private enum ToastKind
        {
            Information,
            Error
        }

        private sealed class ToastNotification : Form
        {
            private const int AnimationInterval = 16;
            private const int FadeInDuration = 192;
            private const int VisibleDuration = 2400;
            private const int FadeOutDuration = 320;

            private readonly System.Windows.Forms.Timer animationTimer = new()
            {
                Interval = AnimationInterval
            };
            private readonly string message;
            private readonly ToastKind kind;
            private int elapsed;

            public ToastNotification(string message, ToastKind kind)
            {
                this.message = message;
                this.kind = kind;

                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ShowInTaskbar = false;
                Size = new Size(390, 82);
                BackColor = kind == ToastKind.Error
                    ? Color.FromArgb(26, 43, 73)
                    : PrimaryBlue;
                Opacity = 0D;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;

                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 14);
                Region = new Region(path);

                animationTimer.Tick += AnimateToast;
                Click += (_, _) => Close();
            }

            protected override bool ShowWithoutActivation => true;

            protected override void OnShown(EventArgs e)
            {
                base.OnShown(e);
                animationTimer.Start();
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                var accentColor = kind == ToastKind.Error
                    ? Color.FromArgb(255, 190, 92)
                    : Color.FromArgb(155, 205, 255);

                using var accentBrush = new SolidBrush(accentColor);
                using var iconBackgroundBrush = new SolidBrush(Color.FromArgb(38, Color.White));
                using var iconFont = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
                using var messageFont = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);

                e.Graphics.FillRectangle(accentBrush, 0, 0, 5, Height);
                e.Graphics.FillEllipse(iconBackgroundBrush, 20, 23, 36, 36);

                var icon = kind == ToastKind.Error ? "!" : "i";
                TextRenderer.DrawText(
                    e.Graphics,
                    icon,
                    iconFont,
                    new Rectangle(20, 23, 36, 36),
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(
                    e.Graphics,
                    message,
                    messageFont,
                    new Rectangle(72, 14, Width - 94, Height - 28),
                    Color.White,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    animationTimer.Dispose();
                }

                base.Dispose(disposing);
            }

            private void AnimateToast(object? sender, EventArgs e)
            {
                elapsed += AnimationInterval;

                if (elapsed <= FadeInDuration)
                {
                    Opacity = Math.Min(1D, (double)elapsed / FadeInDuration);
                    return;
                }

                var fadeOutStart = FadeInDuration + VisibleDuration;
                if (elapsed <= fadeOutStart)
                {
                    Opacity = 1D;
                    return;
                }

                var fadeOutElapsed = elapsed - fadeOutStart;
                if (fadeOutElapsed < FadeOutDuration)
                {
                    Opacity = Math.Max(0D, 1D - ((double)fadeOutElapsed / FadeOutDuration));
                    return;
                }

                animationTimer.Stop();
                Close();
            }
        }

        private sealed class BrandArtworkPanel : Panel
        {
            private readonly Image? artwork;

            public BrandArtworkPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = SurfaceColor;

                var artworkPath = Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "SyrentoBrand.png");

                if (File.Exists(artworkPath))
                {
                    using var source = Image.FromFile(artworkPath);
                    artwork = new Bitmap(source);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (artwork is null)
                {
                    return;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                const int padding = 10;
                var availableWidth = Math.Max(1, Width - (padding * 2));
                var availableHeight = Math.Max(1, Height - (padding * 2));
                var scale = Math.Min(
                    (float)availableWidth / artwork.Width,
                    (float)availableHeight / artwork.Height);
                var imageWidth = (int)(artwork.Width * scale);
                var imageHeight = (int)(artwork.Height * scale);
                var imageBounds = new Rectangle(
                    (Width - imageWidth) / 2,
                    (Height - imageHeight) / 2,
                    imageWidth,
                    imageHeight);

                e.Graphics.DrawImage(artwork, imageBounds);
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    artwork?.Dispose();
                }

                base.Dispose(disposing);
            }
        }

        private sealed class RoundedButton : Button
        {
            private bool isHovered;

            public RoundedButton()
            {
                SetStyle(
                    ControlStyles.UserPaint |
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.OptimizedDoubleBuffer,
                    true);
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                DoubleBuffered = true;
            }

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);

                if (Width <= 0 || Height <= 0)
                {
                    return;
                }

                using var path = CreateRoundedPath(
                    new Rectangle(0, 0, Width, Height),
                    12);
                Region = new Region(path);
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                isHovered = true;
                Invalidate();
                base.OnMouseEnter(e);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                isHovered = false;
                Invalidate();
                base.OnMouseLeave(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
                using var path = CreateRoundedPath(bounds, 12);
                using var brush = new SolidBrush(isHovered ? DarkBlue : BackColor);
                e.Graphics.Clear(Parent?.BackColor ?? SurfaceColor);
                e.Graphics.FillPath(brush, path);
                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    bounds,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private sealed class ModernTextBox : UserControl
        {
            private readonly TextBox input = new();
            private readonly Button passwordToggle = new();
            private bool isFocused;
            private bool showPasswordToggle;

            public ModernTextBox()
            {
                BackColor = AppTheme.InputBackground;
                DoubleBuffered = true;
                Padding = new Padding(16, 8, 12, 8);

                input.BorderStyle = BorderStyle.None;
                input.Font = new Font("Segoe UI", 10.5F);
                input.ForeColor = TextPrimary;
                input.BackColor = AppTheme.InputBackground;
                input.Location = new Point(16, 16);
                input.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                input.GotFocus += (_, _) =>
                {
                    isFocused = true;
                    Invalidate();
                };
                input.LostFocus += (_, _) =>
                {
                    isFocused = false;
                    Invalidate();
                };

                passwordToggle.FlatStyle = FlatStyle.Flat;
                passwordToggle.FlatAppearance.BorderSize = 0;
                passwordToggle.BackColor = AppTheme.InputBackground;
                passwordToggle.ForeColor = PrimaryBlue;
                passwordToggle.Font = new Font("Segoe UI Semibold", 8.5F);
                passwordToggle.Text = "Show";
                passwordToggle.Cursor = Cursors.Hand;
                passwordToggle.TabStop = false;
                passwordToggle.Visible = false;
                passwordToggle.Click += (_, _) => TogglePasswordVisibility();

                Controls.Add(input);
                Controls.Add(passwordToggle);
                Cursor = Cursors.IBeam;
                Click += (_, _) => input.Focus();
            }

            [AllowNull]
            public override string Text
            {
                get => input.Text;
                set => input.Text = value ?? string.Empty;
            }

            public string PlaceholderText
            {
                get => input.PlaceholderText;
                set => input.PlaceholderText = value;
            }

            public bool IsPassword
            {
                get => input.UseSystemPasswordChar;
                set => input.UseSystemPasswordChar = value;
            }

            public bool ShowPasswordToggle
            {
                get => showPasswordToggle;
                set
                {
                    showPasswordToggle = value;
                    passwordToggle.Visible = value;
                    PerformLayout();
                }
            }

            public void FocusInput() => input.Focus();

            protected override void OnResize(EventArgs e)
            {
                base.OnResize(e);
                passwordToggle.SetBounds(Width - 67, 9, 54, Height - 18);
                input.SetBounds(16, (Height - input.PreferredHeight) / 2, Width - (showPasswordToggle ? 92 : 32), input.PreferredHeight);
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 11);
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
                using var path = CreateRoundedPath(bounds, 11);
                using var pen = new Pen(isFocused ? PrimaryBlue : BorderColor, isFocused ? 2F : 1F);
                e.Graphics.DrawPath(pen, path);
            }

            private void TogglePasswordVisibility()
            {
                input.UseSystemPasswordChar = !input.UseSystemPasswordChar;
                passwordToggle.Text = input.UseSystemPasswordChar ? "Show" : "Hide";
                input.Focus();
                input.SelectionStart = input.TextLength;
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
