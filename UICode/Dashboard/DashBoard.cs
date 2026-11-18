using System.Drawing.Drawing2D;
using PropertyManagementSystem.Configuration;
using PropertyManagementSystem.Model;
using PropertyManagementSystem.Repository;

namespace PropertyManagementSystem
{
    public partial class DashBoard : Form
    {
        private static Color PrimaryBlue => AppTheme.PrimaryBlue;
        private static Color DarkBlue => AppTheme.PrimaryBlueHover;
        private static Color SidebarBlue => AppTheme.Sidebar;
        private static Color PageBackground => AppTheme.PageBackground;
        private static Color CardBorder => AppTheme.Border;
        private static Color TextPrimary => AppTheme.TextPrimary;
        private static Color TextSecondary => AppTheme.TextSecondary;
        private static Color SurfaceColor => AppTheme.Surface;
        private static readonly Image? BrandArtwork = LoadBrandArtwork();

        private readonly Panel contentHost = new();
        private readonly Label pageTitleLabel = new();
        private readonly Label pageSubtitleLabel = new();
        private readonly Label userNameLabel = new();
        private readonly Label userGroupLabel = new();
        private readonly Label welcomeUserLabel = new();
        private readonly DashboardActionButton accountButton = new();
        private readonly Dictionary<string, DashboardNavButton> navigationButtons = new();
        private static readonly string[] DefaultAttentionItems =
        {
            "ContractsEndingSoon",
            "AirconMaintenanceDue",
            "VacantUnits",
            "TenantsWithoutAssignment"
        };
        private readonly HashSet<string> selectedAttentionItems = new(
            DefaultAttentionItems,
            StringComparer.OrdinalIgnoreCase);
        private AttentionCounts attentionCounts = new(0, 0, 0, 0);
        private int condominiumCount;
        private int assignmentCount;
        private int assignedTenantCount;

        private void ConfigureWindow()
        {
            Text = "Syrento Property Management Service";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1080, 700);
            BackColor = PageBackground;
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            using var applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (applicationIcon is not null)
            {
                Icon = (Icon)applicationIcon.Clone();
            }

            Shown += (_, _) => WindowState = FormWindowState.Maximized;
        }

        private void BuildDashboardInterface()
        {
            SuspendLayout();
            Controls.Clear();
            navigationButtons.Clear();

            var rootLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = PageBackground
            };
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 276F));
            rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            rootLayout.Controls.Add(CreateSidebar(), 0, 0);
            rootLayout.Controls.Add(CreateWorkspace(), 1, 0);
            Controls.Add(rootLayout);

            ShowOverview();
            ResumeLayout(true);
        }

        private Control CreateSidebar()
        {
            var sidebar = new GradientDashboardPanel
            {
                Dock = DockStyle.Fill,
                StartColor = Color.FromArgb(5, 82, 204),
                EndColor = Color.FromArgb(2, 25, 85),
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };

            var brandPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 286,
                BackColor = Color.Transparent,
                Padding = Padding.Empty
            };
            brandPanel.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 65,
                Padding = new Padding(18, 0, 18, 10),
                Text = "BETTER HOMES  •  SMOOTHER RENTALS\r\nSTRONGER COMMUNITIES",
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            });
            brandPanel.Controls.Add(new PictureBox
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(14, 8, 14, 0),
                Image = BrandArtwork,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            });

            var navigation = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(12, 8, 12, 10),
                Margin = Padding.Empty
            };

            AddNavigationButton(navigation, "Dashboard", "Dashboard overview and reminders");
            AddNavigationButton(navigation, "Condominiums", "Condominiums, units and assigned tenants");
            AddNavigationButton(navigation, "Owners", "Manage unit owners and account status");
            AddNavigationButton(navigation, "Tenant Assignments", "Assign tenants to condominium units");
            AddNavigationButton(navigation, "Past Transactions", "View closed contracts and create historical SOAs");
            AddNavigationButton(navigation, "Statements of Account", "File, print and reprint owner and tenant SOAs");
            AddNavigationButton(navigation, "User Settings", "Manage system users and access");

            var footer = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 82,
                Padding = new Padding(20, 4, 20, 15),
                Text = "SYRENTO  •  2026",
                Font = new Font("Segoe UI Semibold", 8.8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(183, 226, 255),
                TextAlign = ContentAlignment.BottomCenter,
                BackColor = Color.Transparent
            };

            sidebar.Controls.Add(navigation);
            sidebar.Controls.Add(footer);
            sidebar.Controls.Add(brandPanel);
            return sidebar;
        }

        private void AddNavigationButton(
            FlowLayoutPanel navigation,
            string moduleName,
            string description)
        {
            var button = new DashboardNavButton
            {
                Width = 248,
                Height = 50,
                Margin = new Padding(0, 0, 0, 5),
                Text = moduleName,
                IconGlyph = GetNavigationGlyph(moduleName),
                Cursor = Cursors.Hand,
                TabStop = true
            };
            button.Click += (_, _) =>
            {
                if (moduleName == "Dashboard")
                {
                    ShowOverview();
                    return;
                }

                if (moduleName == "Condominiums")
                {
                    ShowCondominiumsModule();
                    return;
                }

                if (moduleName == "Tenant Assignments")
                {
                    ShowTenantAssignmentsModule();
                    return;
                }

                if (moduleName == "Owners")
                {
                    ShowOwnersModule();
                    return;
                }

                if (moduleName == "Statements of Account")
                {
                    ShowStatementsOfAccountModule();
                    return;
                }

                if (moduleName == "Past Transactions")
                {
                    ShowPastTransactionsModule();
                    return;
                }

                ShowUserSettingsModule();
            };

            navigationButtons[moduleName] = button;
            navigation.Controls.Add(button);
        }

        private Control CreateWorkspace()
        {
            var workspace = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 94F));
            workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            workspace.Controls.Add(CreateHeader(), 0, 0);

            contentHost.Dock = DockStyle.Fill;
            contentHost.Margin = Padding.Empty;
            contentHost.Padding = new Padding(26, 22, 26, 26);
            contentHost.BackColor = PageBackground;
            contentHost.AutoScroll = true;
            workspace.Controls.Add(contentHost, 0, 1);

            return workspace;
        }

        private Control CreateHeader()
        {
            var header = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Margin = Padding.Empty,
                Padding = new Padding(28, 0, 24, 0)
            };
            header.Paint += (_, e) =>
            {
                using var pen = new Pen(CardBorder);
                e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
                using var accent = new LinearGradientBrush(
                    new Rectangle(0, 0, Math.Max(1, header.Width), 6),
                    AppTheme.Cyan,
                    PrimaryBlue,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(accent, 0, 0, header.Width, 6);
                using var decoration = new SolidBrush(Color.FromArgb(34, AppTheme.Cyan));
                e.Graphics.FillPolygon(decoration, new[]
                {
                    new Point(Math.Max(0, header.Width - 420), 6),
                    new Point(header.Width - 260, 6),
                    new Point(header.Width - 210, header.Height),
                    new Point(header.Width - 350, header.Height)
                });
            };

            pageTitleLabel.AutoSize = true;
            pageTitleLabel.Location = new Point(28, 19);
            pageTitleLabel.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
            pageTitleLabel.ForeColor = TextPrimary;

            pageSubtitleLabel.AutoSize = true;
            pageSubtitleLabel.Location = new Point(29, 57);
            pageSubtitleLabel.Font = new Font("Segoe UI", 9F);
            pageSubtitleLabel.ForeColor = TextSecondary;

            var signOutButton = new DashboardActionButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(header.Width - 104, 29),
                Size = new Size(80, 38),
                Text = "Sign out",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            signOutButton.Click += (_, _) => Close();

            accountButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            accountButton.Location = new Point(header.Width - 208, 29);
            accountButton.Size = new Size(96, 38);
            accountButton.Text = "Edit account";
            accountButton.BackColor = AppTheme.SoftBlue;
            accountButton.ForeColor = PrimaryBlue;
            accountButton.BorderColor = CardBorder;
            accountButton.Cursor = Cursors.Hand;
            accountButton.Click += (_, _) => OpenCurrentUserSettings();

            var themeButton = new DashboardActionButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(82, 38),
                Text = AppTheme.IsDark ? "Light" : "Dark",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = AppTheme.Border,
                Cursor = Cursors.Hand
            };
            themeButton.Click += (_, _) =>
            {
                AppTheme.Toggle();
                BackColor = PageBackground;
                BuildDashboardInterface();
                ApplySessionDetails();
            };

            var userPanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(header.Width - 520, 18),
                Size = new Size(210, 58),
                BackColor = SurfaceColor
            };
            userNameLabel.Dock = DockStyle.Top;
            userNameLabel.Height = 27;
            userNameLabel.TextAlign = ContentAlignment.BottomRight;
            userNameLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            userNameLabel.ForeColor = TextPrimary;
            userGroupLabel.Dock = DockStyle.Bottom;
            userGroupLabel.Height = 23;
            userGroupLabel.TextAlign = ContentAlignment.TopRight;
            userGroupLabel.Font = new Font("Segoe UI", 8.5F);
            userGroupLabel.ForeColor = TextSecondary;
            userPanel.Controls.Add(userNameLabel);
            userPanel.Controls.Add(userGroupLabel);

            header.Resize += (_, _) =>
            {
                signOutButton.Left = header.ClientSize.Width - signOutButton.Width - 24;
                accountButton.Left = signOutButton.Left - accountButton.Width - 8;
                themeButton.Left = accountButton.Left - themeButton.Width - 8;
                themeButton.Top = 29;
                userPanel.Left = themeButton.Left - userPanel.Width - 14;
            };

            header.Controls.Add(pageTitleLabel);
            header.Controls.Add(pageSubtitleLabel);
            header.Controls.Add(userPanel);
            header.Controls.Add(themeButton);
            header.Controls.Add(accountButton);
            header.Controls.Add(signOutButton);
            return header;
        }

        private void ApplySessionDetails()
        {
            var user = currentUserSession?.User;
            var isAdministrator = user is not null &&
                user.IsActive &&
                string.Equals(
                    user.UserGroup,
                    Constants.UserGroups.Admin,
                    StringComparison.OrdinalIgnoreCase);
            accountButton.Visible = user?.IsActive == true;
            if (navigationButtons.TryGetValue("User Settings", out var settingsButton))
            {
                settingsButton.Visible = isAdministrator;
            }

            userNameLabel.Text = string.IsNullOrWhiteSpace(user?.DisplayName)
                ? user?.Username ?? "Syrento User"
                : user.DisplayName;
            userGroupLabel.Text = user is null
                ? "Not signed in"
                : $"@{user.Username} · {ToTitleCase(user.UserGroup)}";
            var welcomeName = string.IsNullOrWhiteSpace(user?.DisplayName)
                ? user?.Username ?? "Admin"
                : user.DisplayName;
            welcomeUserLabel.Text = $"Welcome back, {welcomeName}";
        }

        private async void ShowOverview()
        {
            SetSelectedNavigation("Dashboard");
            pageTitleLabel.Text = "Dashboard";
            pageSubtitleLabel.Text = "Portfolio status, financial activity and upcoming work";

            if (propertyManagementService is not null)
            {
                try
                {
                    var condominiumsTask = propertyManagementService.GetCondominiumsAsync();
                    var tenantsTask = propertyManagementService.GetTenantsAsync();
                    var unitsTask = propertyManagementService.GetCondominiumUnitsAsync();
                    var assignmentsTask = propertyManagementService.GetAssignmentsAsync();
                    var attentionTask = propertyManagementService.GetAttentionCountsAsync();
                    await Task.WhenAll(condominiumsTask, tenantsTask, unitsTask, assignmentsTask, attentionTask);
                    condominiumCount = condominiumsTask.Result.Count(item => item.IsActive);
                    assignmentCount = unitsTask.Result.Count(item => item.IsActive);
                    assignedTenantCount = assignmentsTask.Result
                        .Where(item => item.IsActive)
                        .Select(item => item.TenantId)
                        .Distinct()
                        .Count();
                    attentionCounts = attentionTask.Result;
                }
                catch (Exception)
                {
                    pageSubtitleLabel.Text = "Unable to load the latest dashboard totals from the database.";
                }
            }

            contentHost.SuspendLayout();
            contentHost.Controls.Clear();
            contentHost.Controls.Add(CreateOverviewPage());
            contentHost.ResumeLayout(true);
        }

        private Control CreateOverviewPage()
        {
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            page.Controls.Add(CreateWelcomeBanner(), 0, 0);
            page.Controls.Add(CreateMetricGrid(), 0, 1);
            page.Controls.Add(CreateDashboardDetails(), 0, 3);
            return page;
        }

        private Control CreateWelcomeBanner()
        {
            var banner = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(28, 22, 28, 14),
                BackColor = PrimaryBlue,
                BorderColor = Color.FromArgb(64, 178, 247),
                CornerRadius = 18
            };
            banner.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var glow = new SolidBrush(Color.FromArgb(42, Color.White));
                e.Graphics.FillEllipse(glow, banner.Width - 270, -115, 340, 250);
                using var cyan = new SolidBrush(Color.FromArgb(110, AppTheme.Cyan));
                e.Graphics.FillPolygon(cyan, new[]
                {
                    new Point(banner.Width - 390, 0),
                    new Point(banner.Width - 330, 0),
                    new Point(banner.Width - 250, banner.Height),
                    new Point(banner.Width - 310, banner.Height)
                });
            };

            welcomeUserLabel.Dock = DockStyle.Top;
            welcomeUserLabel.Height = 40;
            welcomeUserLabel.Text = $"Welcome back, {currentUserSession?.User?.Username ?? "Admin"}";
            welcomeUserLabel.Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold);
            welcomeUserLabel.ForeColor = Color.White;
            welcomeUserLabel.TextAlign = ContentAlignment.MiddleLeft;
            var description = new Label
            {
                Dock = DockStyle.Top,
                Height = 38,
                Text = "Monitor units, contracts, owner payouts, tenant refunds and maintenance from one workspace.",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(222, 242, 255),
                TextAlign = ContentAlignment.MiddleLeft
            };

            banner.Controls.Add(description);
            banner.Controls.Add(welcomeUserLabel);
            return banner;
        }

        private Control CreateMetricGrid()
        {
            var grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            for (var index = 0; index < 4; index++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            grid.Controls.Add(CreateMetricCard("Condominiums", condominiumCount.ToString("N0"), "Managed buildings"), 0, 0);
            grid.Controls.Add(CreateMetricCard("Managed units", assignmentCount.ToString("N0"), "Unit records"), 1, 0);
            grid.Controls.Add(CreateMetricCard("Assigned tenants", assignedTenantCount.ToString("N0"), "Current occupancy"), 2, 0);
            grid.Controls.Add(CreateMetricCard("Vacant units", attentionCounts.VacantUnits.ToString("N0"), "Available inventory"), 3, 0);
            return grid;
        }

        private static Control CreateMetricCard(string title, string value, string note)
        {
            var card = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 12, 0),
                Padding = new Padding(18, 14, 15, 10),
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 16
            };
            var accent = title switch
            {
                "Condominiums" => AppTheme.Cyan,
                "Managed units" => AppTheme.Violet,
                "Assigned tenants" => AppTheme.Green,
                _ => AppTheme.Orange
            };
            card.Paint += (_, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var badge = new SolidBrush(Color.FromArgb(32, accent));
                using var dot = new SolidBrush(accent);
                e.Graphics.FillEllipse(badge, card.Width - 58, 14, 38, 38);
                e.Graphics.FillEllipse(dot, card.Width - 44, 28, 10, 10);
            };
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 20,
                Text = note,
                Font = new Font("Segoe UI", 7.8F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Text = value,
                Font = new Font("Segoe UI Semibold", 21F, FontStyle.Bold),
                ForeColor = accent,
                TextAlign = ContentAlignment.MiddleLeft
            });
            card.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = title,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            return card;
        }

        private Control CreateDashboardDetails()
        {
            var card = CreateAttentionCard();
            card.Margin = Padding.Empty;
            return card;
        }

        private Control CreateAttentionCard()
        {
            var card = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 16
            };
            var heading = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = SurfaceColor,
                Padding = new Padding(17, 9, 17, 5)
            };
            var customizeButton = new DashboardActionButton
            {
                Location = new Point(18, 14),
                Size = new Size(96, 32),
                Text = "Edit display",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            customizeButton.Click += (_, _) => CustomizeAttentionItems();
            var actionHost = new Panel
            {
                Dock = DockStyle.Right,
                Width = 128,
                BackColor = SurfaceColor
            };
            actionHost.Controls.Add(customizeButton);
            var headingText = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor
            };
            headingText.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 21,
                Text = "Choose the reminders you want to monitor from current records.",
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.TopLeft
            });
            headingText.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 25,
                Text = "Needs attention",
                Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            heading.Controls.Add(headingText);
            heading.Controls.Add(actionHost);
            actionHost.BringToFront();

            var selectedRows = GetAttentionOptions()
                .Where(item => selectedAttentionItems.Contains(item.Key))
                .ToList();
            var list = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = Math.Max(1, selectedRows.Count),
                Padding = new Padding(17, 4, 17, 14),
                Margin = Padding.Empty,
                BackColor = SurfaceColor
            };
            list.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            for (var index = 0; index < Math.Max(1, selectedRows.Count); index++)
            {
                list.RowStyles.Add(new RowStyle(SizeType.Percent, 100F / Math.Max(1, selectedRows.Count)));
            }

            if (selectedRows.Count == 0)
            {
                list.Controls.Add(CreateEmptyState(
                    "No reminders selected",
                    "Use Edit display to choose the information shown here."), 0, 0);
            }
            else
            {
                for (var index = 0; index < selectedRows.Count; index++)
                {
                    list.Controls.Add(
                        CreateAttentionRow(
                            selectedRows[index].Key,
                            selectedRows[index].HeaderText,
                            GetAttentionValue(selectedRows[index].Key).ToString("N0")),
                        0,
                        index);
                }
            }

            card.Controls.Add(list);
            card.Controls.Add(heading);
            return card;
        }

        private static IReadOnlyList<TableColumnOption> GetAttentionOptions() =>
            new[]
            {
                new TableColumnOption("ContractsEndingSoon", "Contracts ending soon"),
                new TableColumnOption("AirconMaintenanceDue", "Aircon maintenance due"),
                new TableColumnOption("VacantUnits", "Vacant units"),
                new TableColumnOption("TenantsWithoutAssignment", "Tenants without assignment")
            };

        private int GetAttentionValue(string key) => key switch
        {
            "ContractsEndingSoon" => attentionCounts.ContractsEndingSoon,
            "AirconMaintenanceDue" => attentionCounts.AirconMaintenanceDue,
            "VacantUnits" => attentionCounts.VacantUnits,
            "TenantsWithoutAssignment" => attentionCounts.TenantsWithoutAssignment,
            _ => 0
        };

        private void CustomizeAttentionItems()
        {
            using var optionsForm = new TableOptionsForm(
                GetAttentionOptions(),
                selectedAttentionItems,
                TableOptionsMode.AttentionItems);
            if (optionsForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            selectedAttentionItems.Clear();
            selectedAttentionItems.UnionWith(optionsForm.CheckedColumns);
            if (currentUserSession?.User is { } user)
            {
                DashboardPreferences.SaveAttentionItems(
                    user.UserId,
                    selectedAttentionItems);
            }
            ShowOverview();
        }

        private void LoadAttentionPreferences()
        {
            if (currentUserSession?.User is not { } user)
            {
                return;
            }

            var validKeys = GetAttentionOptions()
                .Select(item => item.Key)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            selectedAttentionItems.Clear();
            selectedAttentionItems.UnionWith(
                DashboardPreferences.LoadAttentionItems(
                    user.UserId,
                    validKeys,
                    DefaultAttentionItems));
        }

        private static RoundedDashboardPanel CreateSectionCard(string title, string subtitle)
        {
            var card = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 16, 0),
                Padding = Padding.Empty,
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 10
            };
            var heading = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = SurfaceColor,
                Padding = new Padding(16, 9, 16, 5)
            };
            heading.Controls.Add(new Label
            {
                Dock = DockStyle.Bottom,
                Height = 21,
                Text = subtitle,
                Font = new Font("Segoe UI", 8F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.TopLeft
            });
            heading.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 25,
                Text = title,
                Font = new Font("Segoe UI Semibold", 11.5F, FontStyle.Bold),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft
            });
            card.Controls.Add(heading);
            card.ControlAdded += (_, eventArgs) =>
            {
                if (!ReferenceEquals(eventArgs.Control, heading))
                {
                    heading.BringToFront();
                }
            };
            return card;
        }

        private Control CreateAttentionRow(string key, string text, string value)
        {
            var row = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                Margin = new Padding(0, 0, 0, 2),
                Cursor = Cursors.Hand
            };
            row.Paint += (_, e) =>
            {
                using var pen = new Pen(CardBorder);
                e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
            };
            var valueLabel = new Label
            {
                Dock = DockStyle.Right,
                Width = 48,
                Text = value,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                ForeColor = PrimaryBlue,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            var textLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = text,
                Font = new Font("Segoe UI Semibold", 8.7F),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            void OpenDetails(object? sender, EventArgs e) => ShowAttentionDetailsModule(key);
            void ShowHover(object? sender, EventArgs e)
            {
                row.BackColor = AppTheme.SoftBlue;
                row.Invalidate();
            }
            void HideHover(object? sender, EventArgs e)
            {
                var mouse = row.PointToClient(Cursor.Position);
                if (!row.ClientRectangle.Contains(mouse))
                {
                    row.BackColor = SurfaceColor;
                    row.Invalidate();
                }
            }
            row.Click += OpenDetails;
            textLabel.Click += OpenDetails;
            valueLabel.Click += OpenDetails;
            row.MouseEnter += ShowHover;
            textLabel.MouseEnter += ShowHover;
            valueLabel.MouseEnter += ShowHover;
            row.MouseLeave += HideHover;
            textLabel.MouseLeave += HideHover;
            valueLabel.MouseLeave += HideHover;
            row.Controls.Add(valueLabel);
            row.Controls.Add(textLabel);
            return row;
        }

        private async void ShowCondominiumsModule()
        {
            SetSelectedNavigation("Condominiums");
            pageTitleLabel.Text = "Condominiums";
            pageSubtitleLabel.Text = "Select a condominium to view its units and assigned tenants";

            if (propertyManagementService is null)
            {
                pageSubtitleLabel.Text = "Property services are not available.";
                return;
            }

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var recordsGrid = CreateDataGrid(
                "Unit #",
                "Unit Owner",
                "Meralco SIN",
                "Occupancy",
                "Last Updated");
            Condominium? selectedCondominium = null;
            async void DeleteSelectedUnit()
            {
                if (recordsGrid.CurrentRow?.Tag is not CondominiumUnit unit)
                {
                    pageSubtitleLabel.Text = "Select a unit row before deleting.";
                    return;
                }

                using var confirmation = new ConfirmationDialog(
                    "Delete unit?",
                    $"Remove Unit {unit.UnitNumber} from {selectedCondominium?.Name}? Units with active tenant assignments cannot be deleted.",
                    "Delete");
                if (confirmation.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var result = await propertyManagementService.DeleteCondominiumUnitAsync(unit);
                if (!result.Succeeded)
                {
                    FadingToast.ShowMessage(this, result.Message);
                    return;
                }

                ShowCondominiumsModule();
            }
            void UpdateSelectedUnit()
            {
                if (recordsGrid.CurrentRow?.Tag is not CondominiumUnit unit ||
                    selectedCondominium is null)
                {
                    pageSubtitleLabel.Text = "Select a unit row before updating.";
                    return;
                }

                using var form = new AddUnitForm(
                    propertyManagementService,
                    selectedCondominium,
                    unit);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ShowCondominiumsModule();
                }
            }
            page.Controls.Add(CreateModuleToolbar(
                "Search unit or assigned tenant",
                recordsGrid,
                (search, fields) => ApplyGridFilter(recordsGrid, search, fields),
                "condominium-units",
                ("Add condominium", () =>
                {
                    using var form = new AddCondominiumForm(propertyManagementService);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowCondominiumsModule();
                    }
                }),
                ("Add unit", () =>
                {
                    if (selectedCondominium is null)
                    {
                        pageSubtitleLabel.Text = "Select a condominium before adding a unit.";
                        return;
                    }

                    using var form = new AddUnitForm(
                        propertyManagementService,
                        selectedCondominium);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowCondominiumsModule();
                    }
                }),
                ("Update unit", UpdateSelectedUnit),
                ("Delete unit", DeleteSelectedUnit)), 0, 0);

            var body = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330F));
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            body.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var condominiumListCard = CreateModuleCard(
                "Condominium list",
                "Select a building to filter the unit records.");
            condominiumListCard.Margin = new Padding(0, 0, 16, 0);
            var condominiumList = CreateCondominiumList();
            var condominiumSearchBox = new ModernSearchBox
            {
                Dock = DockStyle.Top,
                Height = 40,
                PlaceholderText = "Search condominiums"
            };
            var listSpacer = new Panel
            {
                Dock = DockStyle.Top,
                Height = 10,
                BackColor = SurfaceColor
            };
            var condominiumListHost = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor
            };
            condominiumListHost.Controls.Add(condominiumList);
            condominiumListHost.Controls.Add(listSpacer);
            condominiumListHost.Controls.Add(condominiumSearchBox);
            condominiumListCard.Controls.Add(condominiumListHost);

            var recordsCard = CreateModuleCard(
                "Unit numbers",
                "Unit inventory for the selected condominium.");
            recordsCard.Margin = Padding.Empty;
            recordsCard.Controls.Add(recordsGrid);

            body.Controls.Add(condominiumListCard, 0, 0);
            body.Controls.Add(recordsCard, 1, 0);
            page.Controls.Add(body, 0, 1);
            DisplayModulePage(page);

            try
            {
                var condominiums = await propertyManagementService.GetCondominiumsAsync();
                IReadOnlyList<CondominiumUnit> units = Array.Empty<CondominiumUnit>();
                IReadOnlyList<TenantAssignment> assignments = Array.Empty<TenantAssignment>();
                try
                {
                    units = await propertyManagementService.GetCondominiumUnitsAsync();
                }
                catch (Exception exception)
                {
                    pageSubtitleLabel.Text =
                        $"Condominiums loaded, but units could not be read: {exception.GetBaseException().Message}";
                }

                try
                {
                    assignments = await propertyManagementService.GetAssignmentsAsync();
                }
                catch (Exception exception)
                {
                    pageSubtitleLabel.Text =
                        $"Condominiums loaded, but assignments need a database update: {exception.GetBaseException().Message}";
                }
                var activeCondominiums = condominiums
                    .Where(item => item.IsActive)
                    .ToList();
                condominiumList.DisplayMember = nameof(Condominium.Name);

                void BindCondominiumList()
                {
                    var selectedId = (condominiumList.SelectedItem as Condominium)?.CondominiumId;
                    var search = condominiumSearchBox.Text.Trim();
                    var filtered = activeCondominiums
                        .Where(item => string.IsNullOrWhiteSpace(search) ||
                            item.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    condominiumList.DataSource = null;
                    condominiumList.DisplayMember = nameof(Condominium.Name);
                    condominiumList.DataSource = filtered;
                    if (selectedId.HasValue)
                    {
                        var selectedIndex = filtered.FindIndex(item => item.CondominiumId == selectedId.Value);
                        condominiumList.SelectedIndex = selectedIndex;
                    }
                }

                void ShowSelectedCondominium()
                {
                    recordsGrid.Rows.Clear();
                    if (condominiumList.SelectedItem is not Condominium selected)
                    {
                        selectedCondominium = null;
                        return;
                    }

                    selectedCondominium = selected;

                    foreach (var unit in units.Where(item =>
                        item.IsActive && item.CondominiumId == selected.CondominiumId))
                    {
                        var assignment = assignments.FirstOrDefault(item =>
                            item.IsActive &&
                            item.CondominiumId == selected.CondominiumId &&
                            string.Equals(
                                item.UnitNumber,
                                unit.UnitNumber,
                                StringComparison.OrdinalIgnoreCase));
                        var rowIndex = recordsGrid.Rows.Add(
                            unit.UnitNumber,
                            unit.Owner?.FullName,
                            unit.MeralcoSin,
                            assignment is null ? "Vacant" : "Assigned",
                            DateTime.SpecifyKind(unit.LastUpdatedDate, DateTimeKind.Utc)
                                .ToLocalTime()
                                .ToString("MMM d, yyyy h:mm tt"));
                        recordsGrid.Rows[rowIndex].Tag = unit;
                    }
                }

                condominiumList.SelectedIndexChanged += (_, _) => ShowSelectedCondominium();
                condominiumSearchBox.TextChanged += (_, _) => BindCondominiumList();
                BindCondominiumList();
                if (condominiumList.Items.Count > 0)
                {
                    condominiumList.SelectedIndex = 0;
                    ShowSelectedCondominium();
                }
            }
            catch (Exception exception)
            {
                pageSubtitleLabel.Text =
                    $"Unable to load condominium records: {exception.GetBaseException().Message}";
            }
        }

        private async void ShowOwnersModule()
        {
            SetSelectedNavigation("Owners");
            pageTitleLabel.Text = "Owners";
            pageSubtitleLabel.Text = "Manage unit-owner details and active status";

            if (propertyManagementService is null)
            {
                pageSubtitleLabel.Text = "Property services are not available.";
                return;
            }

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var grid = CreateDataGrid(
                "Owner Name",
                "Email Address",
                "Contact Number",
                "Linked Units",
                "Status",
                "Last Updated");

            void AddOwner()
            {
                using var form = new AddOwnerForm(propertyManagementService);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ShowOwnersModule();
                }
            }

            void UpdateOwner()
            {
                if (grid.CurrentRow?.Tag is not Owner owner)
                {
                    pageSubtitleLabel.Text = "Select an owner row before updating.";
                    return;
                }

                using var form = new AddOwnerForm(propertyManagementService, owner);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ShowOwnersModule();
                }
            }

            async void DeleteOwner()
            {
                if (grid.CurrentRow?.Tag is not Owner owner)
                {
                    pageSubtitleLabel.Text = "Select an owner row before deleting.";
                    return;
                }

                using var confirmation = new ConfirmationDialog(
                    "Delete owner?",
                    $"{owner.FullName} will be marked inactive. Existing unit links and historical records will be preserved.",
                    "Delete owner");
                if (confirmation.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                var result = await propertyManagementService.DeleteOwnerAsync(owner);
                if (!result.Succeeded)
                {
                    FadingToast.ShowMessage(this, result.Message);
                    return;
                }

                ShowOwnersModule();
            }

            page.Controls.Add(CreateModuleToolbar(
                "Search owner name, email, contact or status",
                grid,
                (search, fields) => ApplyGridFilter(grid, search, fields),
                "owners",
                ("Add owner", AddOwner),
                ("Update owner", UpdateOwner),
                ("Delete owner", DeleteOwner)), 0, 0);

            var card = CreateModuleCard(
                "Unit owners",
                "Inactive owners remain linked to existing units but cannot be selected for new unit records.");
            card.Margin = Padding.Empty;
            card.Controls.Add(grid);
            page.Controls.Add(card, 0, 1);
            DisplayModulePage(page);

            try
            {
                var owners = await propertyManagementService.GetOwnersAsync();
                var units = await propertyManagementService.GetCondominiumUnitsAsync();
                foreach (var owner in owners)
                {
                    var lastUpdated = DateTime.SpecifyKind(
                        owner.LastUpdatedDate,
                        DateTimeKind.Utc).ToLocalTime();
                    var rowIndex = grid.Rows.Add(
                        owner.FullName,
                        owner.EmailAddress,
                        owner.ContactNumber,
                        units.Count(unit => unit.IsActive && unit.OwnerId == owner.OwnerId),
                        owner.IsActive ? "Active" : "Inactive",
                        lastUpdated.ToString("MMM d, yyyy h:mm tt"));
                    grid.Rows[rowIndex].Tag = owner;
                }
            }
            catch (Exception exception)
            {
                pageSubtitleLabel.Text =
                    $"Unable to load owners. Apply database script 010 if needed: {exception.GetBaseException().Message}";
            }
        }

        private async void ShowTenantAssignmentsModule()
        {
            SetSelectedNavigation("Tenant Assignments");
            pageTitleLabel.Text = "Tenant Assignments";
            pageSubtitleLabel.Text = "View every tenant, assign available units, and maintain the master-list details";

            if (propertyManagementService is null)
            {
                pageSubtitleLabel.Text = "Property services are not available.";
                return;
            }

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 64F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var recordsGrid = CreateAssignmentGrid(includeCondominium: true);
            var showAssignedTenants = true;
            var currentSearch = string.Empty;
            IReadOnlySet<string> currentFilterColumns = recordsGrid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => column.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            void ApplyTenantView(string search, IReadOnlySet<string> filterColumns)
            {
                currentSearch = search;
                currentFilterColumns = filterColumns.ToHashSet(StringComparer.OrdinalIgnoreCase);
                ApplyGridFilter(recordsGrid, search, filterColumns);
                foreach (DataGridViewRow row in recordsGrid.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    var belongsToSelectedTab = showAssignedTenants
                        ? row.Tag is TenantAssignment
                        : row.Tag is Tenant;
                    row.Visible = row.Visible && belongsToSelectedTab;
                }
            }

            var toolbar = CreateModuleToolbar(
                "Search tenant, condominium, unit or maintenance",
                recordsGrid,
                ApplyTenantView,
                "tenant-assignments",
                ("Add tenant", () =>
                {
                    using var form = new AddTenantForm(propertyManagementService);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowTenantAssignmentsModule();
                    }
                }),
                ("Assign tenant", () =>
                {
                    using var form = recordsGrid.CurrentRow?.Tag is Tenant selectedTenant
                        ? new AssignTenantForm(propertyManagementService, selectedTenant)
                        : new AssignTenantForm(propertyManagementService);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowTenantAssignmentsModule();
                    }
                }),
                ("Update selected", () =>
                {
                    if (recordsGrid.CurrentRow?.Tag is not TenantAssignment assignment)
                    {
                        pageSubtitleLabel.Text = "Select a tenant assignment row to update.";
                        return;
                    }

                    using var form = new AssignTenantForm(
                        propertyManagementService,
                        assignment);
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        ShowTenantAssignmentsModule();
                    }
                }));
            page.Controls.Add(toolbar, 0, 0);

            var assignedTabButton = new DashboardActionButton
            {
                Location = new Point(5, 5),
                Size = new Size(190, 40),
                Text = "Assigned",
                Cursor = Cursors.Hand
            };
            var unassignedTabButton = new DashboardActionButton
            {
                Location = new Point(201, 5),
                Size = new Size(198, 40),
                Text = "Unassigned",
                Cursor = Cursors.Hand
            };
            var tabSelector = new RoundedDashboardPanel
            {
                Location = new Point(0, 3),
                Size = new Size(404, 50),
                BackColor = AppTheme.InputBackground,
                BorderColor = CardBorder,
                CornerRadius = 15
            };
            tabSelector.Controls.Add(assignedTabButton);
            tabSelector.Controls.Add(unassignedTabButton);
            var tabsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                BackColor = PageBackground
            };
            tabsPanel.Controls.Add(tabSelector);

            void RefreshTenantTabs()
            {
                assignedTabButton.BackColor = showAssignedTenants ? PrimaryBlue : SurfaceColor;
                assignedTabButton.ForeColor = showAssignedTenants ? Color.White : TextPrimary;
                assignedTabButton.BorderColor = showAssignedTenants ? PrimaryBlue : AppTheme.InputBackground;
                unassignedTabButton.BackColor = showAssignedTenants ? SurfaceColor : PrimaryBlue;
                unassignedTabButton.ForeColor = showAssignedTenants ? TextPrimary : Color.White;
                unassignedTabButton.BorderColor = showAssignedTenants ? AppTheme.InputBackground : PrimaryBlue;
                assignedTabButton.Invalidate();
                unassignedTabButton.Invalidate();
                ApplyTenantView(currentSearch, currentFilterColumns);
            }
            assignedTabButton.Click += (_, _) =>
            {
                showAssignedTenants = true;
                RefreshTenantTabs();
            };
            unassignedTabButton.Click += (_, _) =>
            {
                showAssignedTenants = false;
                RefreshTenantTabs();
            };
            page.Controls.Add(tabsPanel, 0, 1);

            var recordsCard = CreateModuleCard(
                "Tenants and condominium assignments",
                "All active tenants are listed. Tenants without a unit are marked as unassigned.");
            recordsCard.Margin = Padding.Empty;
            recordsCard.Controls.Add(recordsGrid);
            page.Controls.Add(recordsCard, 0, 2);
            DisplayModulePage(page);
            RefreshTenantTabs();

            try
            {
                var assignments = await propertyManagementService.GetAssignmentsAsync();
                var tenants = await propertyManagementService.GetTenantsAsync();
                var activeAssignments = assignments
                    .Where(item => item.IsActive)
                    .ToList();
                foreach (var assignment in activeAssignments)
                {
                    AddAssignmentRow(recordsGrid, assignment, includeCondominium: true);
                }

                var assignedTenantIds = activeAssignments
                    .Select(item => item.TenantId)
                    .ToHashSet();
                foreach (var tenant in tenants.Where(item =>
                    item.IsActive && !assignedTenantIds.Contains(item.TenantId)))
                {
                    AddUnassignedTenantRow(recordsGrid, tenant, includeCondominium: true);
                }

                assignedTabButton.Text = $"Assigned  {activeAssignments.Count}";
                unassignedTabButton.Text =
                    $"Unassigned  {tenants.Count(item => item.IsActive && !assignedTenantIds.Contains(item.TenantId))}";
                RefreshTenantTabs();
            }
            catch (Exception exception)
            {
                pageSubtitleLabel.Text =
                    $"Unable to load tenant assignments. Apply the latest database script: {exception.GetBaseException().Message}";
            }
        }

        private async void ShowPastTransactionsModule()
        {
            SetSelectedNavigation("Past Transactions");
            pageTitleLabel.Text = "Past Transactions";
            pageSubtitleLabel.Text = "Closed tenant contracts retained for owner and tenant SOA processing";
            if (propertyManagementService is null || currentUserSession is null)
            {
                pageSubtitleLabel.Text = "Past transaction services are not available.";
                return;
            }

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var grid = CreateDataGrid(
                "Condominium",
                "Unit #",
                "Tenant",
                "Unit Owner",
                "Contract Start",
                "Contract End",
                "Monthly Rent",
                "Advance Months",
                "Deposit Months",
                "Dues",
                "Closed");

            void CreatePastSoa(string type)
            {
                if (grid.CurrentRow?.Tag is not TenantAssignment assignment)
                {
                    pageSubtitleLabel.Text = "Select a past transaction before creating an SOA.";
                    return;
                }

                using var form = new CreateSoaForm(
                    propertyManagementService,
                    currentUserSession,
                    type,
                    assignment.TenantAssignmentId);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ShowPastTransactionsModule();
                }
            }

            page.Controls.Add(CreateModuleToolbar(
                "Search closed tenant, condominium, unit or owner",
                grid,
                (search, fields) => ApplyGridFilter(grid, search, fields),
                "past-transactions",
                ("Owner SOA", () => CreatePastSoa(Constants.SoaTypes.Owner)),
                ("Tenant refund SOA", () => CreatePastSoa(Constants.SoaTypes.Tenant))), 0, 0);

            var card = CreateModuleCard(
                "Closed contracts",
                "Ending a contract vacates the unit but keeps this read-only transaction history available for SOA creation.");
            card.Margin = Padding.Empty;
            card.Controls.Add(grid);
            page.Controls.Add(card, 0, 1);
            DisplayModulePage(page);

            try
            {
                var assignments = await propertyManagementService.GetAssignmentsAsync();
                var closedAssignments = assignments
                    .Where(item => !item.IsActive)
                    .OrderByDescending(item => item.EndOfContract ?? item.LastUpdatedDate)
                    .ToList();
                foreach (var assignment in closedAssignments)
                {
                    var rowIndex = grid.Rows.Add(
                        assignment.Condominium?.Name,
                        assignment.UnitNumber,
                        assignment.Tenant?.FullName,
                        assignment.OwnerName,
                        FormatDate(assignment.StartOfContract),
                        FormatDate(assignment.EndOfContract),
                        assignment.RentalAmountToOwner?.ToString("N2"),
                        assignment.OwnerAdvanceMonths,
                        assignment.OwnerDepositMonths,
                        assignment.WithDues,
                        "Closed");
                    grid.Rows[rowIndex].Tag = assignment;
                }

                pageSubtitleLabel.Text = closedAssignments.Count == 0
                    ? "No closed tenant contracts are available yet."
                    : $"{closedAssignments.Count} closed contract{(closedAssignments.Count == 1 ? string.Empty : "s")} available for SOA processing";
            }
            catch (Exception exception)
            {
                pageSubtitleLabel.Text =
                    $"Unable to load past transactions: {exception.GetBaseException().Message}";
            }
        }

        private async void ShowStatementsOfAccountModule()
        {
            SetSelectedNavigation("Statements of Account");
            pageTitleLabel.Text = "Statements of Account";
            pageSubtitleLabel.Text = "File, print and reprint owner settlements and tenant refunds";
            if (propertyManagementService is null || currentUserSession is null)
            {
                pageSubtitleLabel.Text = "SOA services are not available.";
                return;
            }

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            var grid = CreateDataGrid(
                "SOA Number",
                "Type",
                "Statement Date",
                "Condominium",
                "Unit #",
                "Tenant",
                "Owner",
                "Monthly Rent",
                "Advance",
                "Deposit",
                "Starting Amount",
                "Deductions",
                "Net Amount",
                "Created By");
            void CreateSoa(string type)
            {
                using var form = new CreateSoaForm(
                    propertyManagementService,
                    currentUserSession,
                    type);
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ShowStatementsOfAccountModule();
                }
            }
            void PrintSelectedSoa()
            {
                if (grid.CurrentRow?.Tag is not StatementOfAccount statement)
                {
                    pageSubtitleLabel.Text = "Select an SOA row before printing.";
                    return;
                }

                using var options = new SoaPrintOptionsForm(statement);
                if (options.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    SoaPrintService.ShowPreview(
                        this,
                        statement,
                        statement.TenantAssignment,
                        options.SelectedPaperSize);
                }
                catch (Exception)
                {
                    FadingToast.ShowMessage(
                        this,
                        "Print Preview could not open. Check the installed printer or PDF printer.");
                }
            }
            page.Controls.Add(CreateModuleToolbar(
                "Search SOA, tenant, condominium or unit",
                grid,
                (search, fields) => ApplyGridFilter(grid, search, fields),
                "statements-of-account",
                ("Owner SOA", () => CreateSoa(Constants.SoaTypes.Owner)),
                ("Tenant SOA", () => CreateSoa(Constants.SoaTypes.Tenant)),
                ("Print selected", PrintSelectedSoa)), 0, 0);

            var card = CreateModuleCard(
                "Filed statements",
                "Owner payable and tenant refund calculations ready to print or reprint.");
            card.Margin = Padding.Empty;
            card.Controls.Add(grid);
            page.Controls.Add(card, 0, 1);
            DisplayModulePage(page);

            try
            {
                var statements = await propertyManagementService.GetStatementsOfAccountAsync();
                foreach (var statement in statements)
                {
                    var assignment = statement.TenantAssignment;
                    var rowIndex = grid.Rows.Add(
                        statement.SoaNumber,
                        statement.SoaType == Constants.SoaTypes.Owner ? "Owner SOA" : "Tenant Refund",
                        statement.StatementDate.ToString("MMM d, yyyy"),
                        assignment.Condominium?.Name,
                        assignment.UnitNumber,
                        assignment.Tenant?.FullName,
                        assignment.OwnerName,
                        statement.RentalAmount?.ToString("N2"),
                        statement.AdvanceAmount?.ToString("N2"),
                        statement.DepositAmount?.ToString("N2"),
                        statement.BaseAmount.ToString("N2"),
                        statement.TotalDeductions.ToString("N2"),
                        statement.NetAmount.ToString("N2"),
                        statement.CreatedByUsername);
                    grid.Rows[rowIndex].Tag = statement;
                }
            }
            catch (Exception)
            {
                pageSubtitleLabel.Text = "Unable to load statements of account from the database.";
            }
        }

        private async void ShowAttentionDetailsModule(string key)
        {
            SetSelectedNavigation("Dashboard");
            if (propertyManagementService is null)
            {
                pageSubtitleLabel.Text = "Property services are not available.";
                return;
            }

            var option = GetAttentionOptions().First(item => item.Key == key);
            pageTitleLabel.Text = option.HeaderText;
            pageSubtitleLabel.Text = "Records currently included in this Needs Attention reminder";
            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var tenantList = key == "TenantsWithoutAssignment";
            var vacantUnitList = key == "VacantUnits";
            var grid = tenantList
                ? CreateDataGrid("Tenant Name", "Contact Number", "Email Address", "Status")
                : vacantUnitList
                    ? CreateDataGrid("Condominium", "Unit #", "Occupancy")
                    : CreateAssignmentGrid(includeCondominium: true);
            page.Controls.Add(CreateModuleToolbar(
                "Search these records",
                grid,
                (search, fields) => ApplyGridFilter(grid, search, fields),
                $"attention-{key}",
                ("Back to dashboard", ShowOverview)), 0, 0);
            var card = CreateModuleCard(
                option.HeaderText,
                "Click Back to dashboard when you are finished reviewing this list.");
            card.Margin = Padding.Empty;
            card.Controls.Add(grid);
            page.Controls.Add(card, 0, 1);
            DisplayModulePage(page);

            try
            {
                var assignments = await propertyManagementService.GetAssignmentsAsync();
                if (tenantList)
                {
                    var assignedTenantIds = assignments
                        .Where(item => item.IsActive)
                        .Select(item => item.TenantId)
                        .ToHashSet();
                    var tenants = await propertyManagementService.GetTenantsAsync();
                    foreach (var tenant in tenants.Where(item =>
                        item.IsActive && !assignedTenantIds.Contains(item.TenantId)))
                    {
                        grid.Rows.Add(
                            tenant.FullName,
                            tenant.ContactNumber,
                            tenant.EmailAddress,
                            "Not assigned");
                    }
                    return;
                }

                if (vacantUnitList)
                {
                    var activeAssignmentKeys = assignments
                        .Where(item => item.IsActive)
                        .Select(item => $"{item.CondominiumId}|{item.UnitNumber}")
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var units = await propertyManagementService.GetCondominiumUnitsAsync();
                    foreach (var unit in units.Where(item =>
                        item.IsActive &&
                        !activeAssignmentKeys.Contains($"{item.CondominiumId}|{item.UnitNumber}")))
                    {
                        grid.Rows.Add(unit.Condominium.Name, unit.UnitNumber, "Vacant");
                    }
                    return;
                }

                var today = DateTime.Today;
                var upcoming = today.AddDays(30);
                var filtered = assignments.Where(item => item.IsActive && key switch
                {
                    "ContractsEndingSoon" => item.EndOfContract.HasValue &&
                        item.EndOfContract.Value >= today &&
                        item.EndOfContract.Value <= upcoming,
                    "AirconMaintenanceDue" => item.AirconMaintenanceDate.HasValue &&
                        item.AirconMaintenanceDate.Value <= upcoming,
                    _ => false
                });
                foreach (var assignment in filtered)
                {
                    AddAssignmentRow(grid, assignment, includeCondominium: true);
                }
            }
            catch (Exception)
            {
                pageSubtitleLabel.Text = "Unable to load the selected reminder records.";
            }
        }

        private async void ShowUserSettingsModule()
        {
            var administrator = currentUserSession?.User;
            if (administrator is null ||
                userAccountService is null ||
                !string.Equals(
                    administrator.UserGroup,
                    Constants.UserGroups.Admin,
                    StringComparison.OrdinalIgnoreCase))
            {
                ShowOverview();
                return;
            }

            SetSelectedNavigation("User Settings");
            pageTitleLabel.Text = "User Settings";
            pageSubtitleLabel.Text = "Administrator-only user access and account management";

            var page = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = PageBackground,
                Margin = Padding.Empty,
                Padding = Padding.Empty
            };
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            page.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var toolbar = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 16),
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 13
            };
            var searchBox = new ModernSearchBox
            {
                Location = new Point(18, 11),
                Size = new Size(310, 40),
                PlaceholderText = "Search display name or username"
            };
            var editButton = new DashboardActionButton
            {
                Size = new Size(116, 38),
                Text = "Edit selected",
                BackColor = PrimaryBlue,
                ForeColor = Color.White,
                BorderColor = PrimaryBlue,
                Cursor = Cursors.Hand
            };
            var activeButton = new DashboardActionButton
            {
                Size = new Size(150, 38),
                Text = "Activate / Deactivate",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            var filterButton = new DashboardActionButton
            {
                Size = new Size(92, 38),
                Text = "Filter",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            var columnsButton = new DashboardActionButton
            {
                Size = new Size(100, 38),
                Text = "Columns",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            void PositionToolbarButtons()
            {
                editButton.Left = toolbar.ClientSize.Width - editButton.Width - 16;
                activeButton.Left = editButton.Left - activeButton.Width - 10;
                columnsButton.Left = activeButton.Left - columnsButton.Width - 10;
                filterButton.Left = columnsButton.Left - filterButton.Width - 10;
                editButton.Top = 11;
                activeButton.Top = 11;
                columnsButton.Top = 11;
                filterButton.Top = 11;
            }
            toolbar.Resize += (_, _) => PositionToolbarButtons();
            toolbar.Controls.Add(searchBox);
            toolbar.Controls.Add(filterButton);
            toolbar.Controls.Add(columnsButton);
            toolbar.Controls.Add(activeButton);
            toolbar.Controls.Add(editButton);
            PositionToolbarButtons();
            page.Controls.Add(toolbar, 0, 0);

            var recordsCard = CreateModuleCard(
                "System users",
                "Select a user to edit account details or change whether the account can log in.");
            recordsCard.Margin = Padding.Empty;
            var userGrid = CreateUserGrid();
            recordsCard.Controls.Add(userGrid);
            page.Controls.Add(recordsCard, 0, 1);
            DisplayModulePage(page);

            IReadOnlyList<Model.User> users = Array.Empty<Model.User>();
            var filterColumns = userGrid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => column.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            async Task ReloadUsersAsync()
            {
                users = await userAccountService.GetUsersAsync(administrator);
                BindFilteredUsers();
            }

            void BindFilteredUsers()
            {
                var search = searchBox.Text.Trim();
                userGrid.DataSource = users
                    .Where(user => string.IsNullOrWhiteSpace(search) ||
                        UserMatchesFilter(user, search, filterColumns))
                    .Select(UserListRow.FromUser)
                    .ToList();
            }

            Model.User? GetSelectedUser()
            {
                if (userGrid.CurrentRow?.DataBoundItem is not UserListRow selected)
                {
                    pageSubtitleLabel.Text = "Select a user from the list first.";
                    return null;
                }

                return users.FirstOrDefault(user => user.UserId == selected.UserId);
            }

            searchBox.TextChanged += (_, _) => BindFilteredUsers();
            var userColumns = userGrid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => new TableColumnOption(column.Name, column.HeaderText))
                .ToList();
            filterButton.Click += (_, _) =>
            {
                using var optionsForm = new TableOptionsForm(
                    userColumns,
                    filterColumns,
                    TableOptionsMode.FilterFields);
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                filterColumns.Clear();
                filterColumns.UnionWith(optionsForm.CheckedColumns);
                BindFilteredUsers();
            };
            columnsButton.Click += (_, _) =>
            {
                var visibleColumns = userGrid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(column => column.Visible)
                    .Select(column => column.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                using var optionsForm = new TableOptionsForm(
                    userColumns,
                    visibleColumns,
                    TableOptionsMode.VisibleColumns);
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                foreach (DataGridViewColumn column in userGrid.Columns)
                {
                    column.Visible = optionsForm.CheckedColumns.Contains(column.Name);
                }
            };
            editButton.Click += async (_, _) =>
            {
                var selectedUser = GetSelectedUser();
                if (selectedUser is null || !OpenUserSettings(selectedUser))
                {
                    return;
                }

                ApplySessionDetails();
                await ReloadUsersAsync();
            };
            activeButton.Click += async (_, _) =>
            {
                var selectedUser = GetSelectedUser();
                if (selectedUser is null)
                {
                    return;
                }

                var updated = await userAccountService.SetUserActiveAsync(
                    administrator,
                    selectedUser.UserId,
                    !selectedUser.IsActive);
                pageSubtitleLabel.Text = updated
                    ? $"{selectedUser.DisplayName} is now {(selectedUser.IsActive ? "inactive" : "active")}."
                    : "The currently logged-in administrator cannot be deactivated.";
                await ReloadUsersAsync();
            };

            try
            {
                await ReloadUsersAsync();
            }
            catch (Exception)
            {
                pageSubtitleLabel.Text = "Unable to load users from the Syrento database.";
            }
        }

        private static DataGridView CreateUserGrid()
        {
            var grid = CreateDataGrid(
                "User ID",
                "Display Name",
                "Username",
                "User Group",
                "Status",
                "Last Updated");
            grid.AutoGenerateColumns = false;
            grid.Columns[0].DataPropertyName = nameof(UserListRow.UserId);
            grid.Columns[0].Width = 85;
            grid.Columns[1].DataPropertyName = nameof(UserListRow.DisplayName);
            grid.Columns[1].Width = 230;
            grid.Columns[2].DataPropertyName = nameof(UserListRow.Username);
            grid.Columns[2].Width = 200;
            grid.Columns[3].DataPropertyName = nameof(UserListRow.UserGroup);
            grid.Columns[3].Width = 130;
            grid.Columns[4].DataPropertyName = nameof(UserListRow.Status);
            grid.Columns[4].Width = 120;
            grid.Columns[5].DataPropertyName = nameof(UserListRow.LastUpdated);
            grid.Columns[5].Width = 190;
            return grid;
        }

        private static bool UserMatchesFilter(
            Model.User user,
            string search,
            IReadOnlySet<string> filterColumns)
        {
            var row = UserListRow.FromUser(user);
            return filterColumns.Any(column =>
            {
                var value = column switch
                {
                    "UserID" => row.UserId.ToString(),
                    "DisplayName" => row.DisplayName,
                    "Username" => row.Username,
                    "UserGroup" => row.UserGroup,
                    "Status" => row.Status,
                    "LastUpdated" => row.LastUpdated,
                    _ => string.Empty
                };
                return value.Contains(search, StringComparison.OrdinalIgnoreCase);
            });
        }

        private bool OpenUserSettings(Model.User targetUser)
        {
            if (userAccountService is null || currentUserSession is null)
            {
                pageSubtitleLabel.Text = "User account services are not available.";
                return false;
            }

            using var settingsForm = new UserSettingsForm(
                userAccountService,
                currentUserSession,
                targetUser);
            return settingsForm.ShowDialog(this) == DialogResult.OK;
        }

        private void OpenCurrentUserSettings()
        {
            if (userAccountService is null ||
                currentUserSession is null ||
                currentUserSession.User is null)
            {
                pageSubtitleLabel.Text = "User account services are not available.";
                return;
            }

            using var settingsForm = new UserSettingsForm(
                userAccountService,
                currentUserSession);
            if (settingsForm.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            ApplySessionDetails();
            if (navigationButtons.TryGetValue("User Settings", out var settingsButton) &&
                settingsButton.Selected)
            {
                ShowUserSettingsModule();
            }
        }

        private Control CreateModuleToolbar(
            string searchPlaceholder,
            DataGridView grid,
            Action<string, IReadOnlySet<string>> applyFilter,
            string tablePreferenceKey,
            params (string Text, Action Click)[] actions)
        {
            var toolbar = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 10),
                Padding = new Padding(14, 9, 14, 9),
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 15
            };

            var searchBox = new ModernSearchBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Location = new Point(16, 10),
                Size = new Size(310, 40),
                PlaceholderText = searchPlaceholder
            };
            toolbar.Controls.Add(searchBox);

            var actionButtons = new List<DashboardActionButton>();
            var filterButton = new DashboardActionButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(78, 38),
                Text = "Filter",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            var columnsButton = new DashboardActionButton
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Size = new Size(88, 38),
                Text = "Columns",
                BackColor = SurfaceColor,
                ForeColor = PrimaryBlue,
                BorderColor = CardBorder,
                Cursor = Cursors.Hand
            };
            actionButtons.Add(filterButton);
            actionButtons.Add(columnsButton);
            toolbar.Controls.Add(filterButton);
            toolbar.Controls.Add(columnsButton);

            var allColumnKeys = grid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => column.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var columns = grid.Columns
                .Cast<DataGridViewColumn>()
                .Select(column => new TableColumnOption(column.Name, column.HeaderText))
                .ToList();
            var userId = currentUserSession?.User?.UserId ?? 0;
            var filterColumns = DashboardPreferences.LoadTableSelection(
                    userId,
                    tablePreferenceKey,
                    "filters",
                    allColumnKeys,
                    allColumnKeys)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var visibleColumns = DashboardPreferences.LoadTableSelection(
                userId,
                tablePreferenceKey,
                "columns",
                allColumnKeys,
                allColumnKeys);
            foreach (DataGridViewColumn column in grid.Columns)
            {
                column.Visible = visibleColumns.Contains(column.Name);
            }
            filterButton.Click += (_, _) =>
            {
                using var optionsForm = new TableOptionsForm(
                    columns,
                    filterColumns,
                    TableOptionsMode.FilterFields);
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                filterColumns.Clear();
                filterColumns.UnionWith(optionsForm.CheckedColumns);
                DashboardPreferences.SaveTableSelection(
                    userId,
                    tablePreferenceKey,
                    "filters",
                    filterColumns);
                applyFilter(searchBox.Text, filterColumns);
            };
            columnsButton.Click += (_, _) =>
            {
                var currentVisibleColumns = grid.Columns
                    .Cast<DataGridViewColumn>()
                    .Where(column => column.Visible)
                    .Select(column => column.Name)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                using var optionsForm = new TableOptionsForm(
                    columns,
                    currentVisibleColumns,
                    TableOptionsMode.VisibleColumns);
                if (optionsForm.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                foreach (DataGridViewColumn column in grid.Columns)
                {
                    column.Visible = optionsForm.CheckedColumns.Contains(column.Name);
                }
                DashboardPreferences.SaveTableSelection(
                    userId,
                    tablePreferenceKey,
                    "columns",
                    optionsForm.CheckedColumns);
            };
            searchBox.TextChanged += (_, _) =>
                applyFilter(searchBox.Text, filterColumns);

            foreach (var action in actions)
            {
                var button = new DashboardActionButton
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    Size = new Size(118, 38),
                    Text = action.Text,
                    BackColor = PrimaryBlue,
                    ForeColor = Color.White,
                    BorderColor = PrimaryBlue,
                    Cursor = Cursors.Hand
                };
                button.Click += (_, _) => action.Click();
                actionButtons.Add(button);
                toolbar.Controls.Add(button);
            }

            void PositionButtons()
            {
                var right = toolbar.ClientSize.Width - 14;
                for (var index = actionButtons.Count - 1; index >= 0; index--)
                {
                    var button = actionButtons[index];
                    button.Left = right - button.Width;
                    button.Top = 10;
                    right = button.Left - 7;
                }
                searchBox.Width = Math.Max(
                    searchBox.MinimumSize.Width,
                    Math.Min(310, right - searchBox.Left - 10));
            }

            toolbar.Resize += (_, _) => PositionButtons();
            PositionButtons();
            return toolbar;
        }

        private static RoundedDashboardPanel CreateModuleCard(string title, string subtitle)
        {
            var card = new RoundedDashboardPanel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor,
                BorderColor = CardBorder,
                CornerRadius = 17,
                Padding = new Padding(18, 72, 18, 18)
            };
            var titleLabel = new Label
            {
                AutoSize = true,
                Location = new Point(20, 14),
                Text = title,
                Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
                ForeColor = TextPrimary
            };
            var subtitleLabel = new Label
            {
                AutoSize = true,
                Location = new Point(21, 44),
                Text = subtitle,
                Font = new Font("Segoe UI", 8.8F),
                ForeColor = TextSecondary
            };
            card.Controls.Add(titleLabel);
            card.Controls.Add(subtitleLabel);
            return card;
        }

        private static Control CreateEmptyState(string title, string message)
        {
            var emptyState = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor
            };
            emptyState.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20, 8, 20, 20),
                Text = message,
                Font = new Font("Segoe UI", 9F),
                ForeColor = TextSecondary,
                TextAlign = ContentAlignment.TopCenter
            });
            emptyState.Controls.Add(new Label
            {
                Dock = DockStyle.Top,
                Height = 96,
                Padding = new Padding(12, 54, 12, 0),
                Text = title,
                Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
                ForeColor = TextPrimary,
                TextAlign = ContentAlignment.BottomCenter
            });
            return emptyState;
        }

        private static DataGridView CreateDataGrid(params string[] headers)
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = SurfaceColor,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 44,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersVisible = false,
                RowTemplate = { Height = 42 },
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                EnableHeadersVisualStyles = false,
                GridColor = CardBorder,
                ScrollBars = ScrollBars.Both
            };
            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(5, 78, 177),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.6F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(7, 0, 7, 0),
                SelectionBackColor = Color.FromArgb(5, 78, 177),
                SelectionForeColor = Color.White
            };
            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = SurfaceColor,
                ForeColor = TextPrimary,
                Font = new Font("Segoe UI", 8.9F),
                Padding = new Padding(6, 0, 6, 0),
                SelectionBackColor = AppTheme.Selection,
                SelectionForeColor = TextPrimary
            };
            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = AppTheme.AlternateRow
            };

            foreach (var header in headers)
            {
                var width = header.Length switch
                {
                    > 26 => 210,
                    > 18 => 180,
                    > 12 => 150,
                    _ => 115
                };
                grid.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = header,
                    Name = header.Replace(" ", string.Empty).Replace("#", "Number"),
                    MinimumWidth = 78,
                    FillWeight = width,
                    SortMode = DataGridViewColumnSortMode.Automatic
                });
            }

            return grid;
        }

        private static ModernLineListBox CreateCondominiumList()
        {
            var list = new ModernLineListBox
            {
                Dock = DockStyle.Fill,
                BackColor = SurfaceColor
            };
            return list;
        }

        private static DataGridView CreateAssignmentGrid(bool includeCondominium)
        {
            var headers = new List<string>();
            if (includeCondominium)
            {
                headers.Add("Condominium Name");
            }

            headers.AddRange(new[]
            {
                "Unit #",
                "Name of Tenant",
                "Owner's Name",
                "Status",
                "Meralco SIN",
                "Key Release",
                "Access Card Release",
                "Rental Amount to Owner",
                "With Dues",
                "Length of Stay",
                "Start of Contract",
                "End of Contract",
                "6 Months Maintenance Aircon",
                "Maintenance Status",
                "Owner Advance Months",
                "Owner Deposit Months",
                "Owner's Email Address",
                "Owner's Contact Number",
                "Main Tenant Contact Number",
                "Main Tenant Email Address"
            });
            return CreateDataGrid(headers.ToArray());
        }

        private static void AddAssignmentRow(
            DataGridView grid,
            TenantAssignment assignment,
            bool includeCondominium)
        {
            var values = new List<object?>();
            if (includeCondominium)
            {
                values.Add(assignment.Condominium?.Name ?? string.Empty);
            }

            values.AddRange(new object?[]
            {
                assignment.UnitNumber,
                assignment.Tenant?.FullName,
                assignment.OwnerName,
                assignment.OccupancyStatus,
                assignment.MeralcoSin,
                assignment.KeyRelease,
                assignment.AccessCardRelease,
                assignment.RentalAmountToOwner?.ToString("N2"),
                assignment.WithDues,
                assignment.StayDuration.HasValue
                    ? $"{assignment.StayDuration.Value} {assignment.StayDurationUnit}"
                    : string.Empty,
                FormatDate(assignment.StartOfContract),
                FormatDate(assignment.EndOfContract),
                FormatDate(assignment.AirconMaintenanceDate),
                assignment.MaintenanceStatus,
                assignment.OwnerAdvanceMonths,
                assignment.OwnerDepositMonths,
                assignment.OwnerEmailAddress,
                assignment.OwnerContactNumber,
                assignment.MainTenantContactNumber ?? assignment.Tenant?.ContactNumber,
                assignment.MainTenantEmailAddress ?? assignment.Tenant?.EmailAddress
            });
            var rowIndex = grid.Rows.Add(values.ToArray());
            grid.Rows[rowIndex].Tag = assignment;
        }

        private static void AddUnassignedTenantRow(
            DataGridView grid,
            Tenant tenant,
            bool includeCondominium)
        {
            var values = new List<object?>();
            if (includeCondominium)
            {
                values.Add(string.Empty);
            }

            values.AddRange(new object?[]
            {
                string.Empty,
                tenant.FullName,
                string.Empty,
                "UNASSIGNED",
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                tenant.ContactNumber,
                tenant.EmailAddress
            });
            var rowIndex = grid.Rows.Add(values.ToArray());
            grid.Rows[rowIndex].Tag = tenant;
        }

        private static string FormatDate(DateTime? value) =>
            value?.ToString("MMM d, yyyy") ?? string.Empty;

        private static void ApplyGridFilter(
            DataGridView grid,
            string search,
            IReadOnlySet<string> filterColumns)
        {
            search = search.Trim();
            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.IsNewRow)
                {
                    continue;
                }

                row.Visible = string.IsNullOrWhiteSpace(search) ||
                    filterColumns.Any(columnName =>
                    {
                        var column = grid.Columns[columnName];
                        if (column is null)
                        {
                            return false;
                        }

                        var value = row.Cells[column.Index].FormattedValue?.ToString();
                        return value?.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase) == true;
                    });
            }
        }

        private void DisplayModulePage(Control page)
        {
            contentHost.SuspendLayout();
            contentHost.Controls.Clear();
            contentHost.Controls.Add(page);
            contentHost.ResumeLayout(true);
        }

        private void SetSelectedNavigation(string moduleName)
        {
            foreach (var pair in navigationButtons)
            {
                pair.Value.Selected = pair.Key == moduleName;
            }
        }

        private sealed record UserListRow(
            int UserId,
            string DisplayName,
            string Username,
            string UserGroup,
            string Status,
            string LastUpdated)
        {
            public static UserListRow FromUser(Model.User user)
            {
                var updated = DateTime.SpecifyKind(
                    user.LastUpdatedDate,
                    DateTimeKind.Utc).ToLocalTime();
                return new UserListRow(
                    user.UserId,
                    string.IsNullOrWhiteSpace(user.DisplayName)
                        ? user.Username
                        : user.DisplayName,
                    user.Username,
                    ToTitleCase(user.UserGroup),
                    user.IsActive ? "Active" : "Inactive",
                    updated.ToString("MMM d, yyyy h:mm tt"));
            }
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "User";
            }

            return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
        }

        private static string GetNavigationGlyph(string moduleName) => moduleName switch
        {
            "Dashboard" => "⌂",
            "Condominiums" => "▥",
            "Owners" => "●",
            "Tenant Assignments" => "♟",
            "Past Transactions" => "↶",
            "Statements of Account" => "₱",
            "User Settings" => "⚙",
            _ => "•"
        };

        private static Image? LoadBrandArtwork()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "Assets", "SyrentoBrand.png");
                if (!File.Exists(path))
                {
                    return null;
                }

                using var source = Image.FromFile(path);
                return new Bitmap(source);
            }
            catch
            {
                return null;
            }
        }

        private sealed class GradientDashboardPanel : Panel
        {
            public Color StartColor { get; set; } = Color.FromArgb(5, 82, 204);
            public Color EndColor { get; set; } = Color.FromArgb(2, 25, 85);

            public GradientDashboardPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
                BackColor = EndColor;
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                using var brush = new LinearGradientBrush(
                    ClientRectangle,
                    StartColor,
                    EndColor,
                    LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }

        private sealed class RoundedDashboardPanel : Panel
        {
            public int CornerRadius { get; set; } = 14;

            public Color BorderColor { get; set; } = Color.Transparent;

            public RoundedDashboardPanel()
            {
                DoubleBuffered = true;
                ResizeRedraw = true;
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                if (Width <= 0 || Height <= 0)
                {
                    return;
                }

                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), CornerRadius);
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                if (BorderColor == Color.Transparent)
                {
                    return;
                }

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using var path = CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), CornerRadius);
                using var pen = new Pen(BorderColor);
                e.Graphics.DrawPath(pen, path);
            }
        }

        private sealed class DashboardNavButton : Button
        {
            private bool selected;
            private bool hovered;

            public string IconGlyph { get; set; } = "•";

            public bool Selected
            {
                get => selected;
                set
                {
                    selected = value;
                    Invalidate();
                }
            }

            public DashboardNavButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                BackColor = SidebarBlue;
                ForeColor = Color.FromArgb(210, 232, 255);
                Font = new Font("Segoe UI Semibold", 9.4F, FontStyle.Bold);
                TextAlign = ContentAlignment.MiddleLeft;
                Padding = Padding.Empty;
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

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.Clear(SidebarBlue);
                var fill = Selected
                    ? Color.FromArgb(72, 193, 250)
                    : hovered
                        ? Color.FromArgb(9, 77, 169)
                        : SidebarBlue;

                using var path = CreateRoundedPath(new Rectangle(1, 1, Width - 3, Height - 3), 12);
                using var brush = new SolidBrush(fill);
                e.Graphics.FillPath(brush, path);

                if (Selected)
                {
                    using var accent = new SolidBrush(Color.White);
                    e.Graphics.FillRectangle(accent, 1, 10, 4, Height - 20);
                }

                var iconBounds = new Rectangle(13, (Height - 30) / 2, 30, 30);
                using var iconBrush = new SolidBrush(Selected
                    ? Color.White
                    : Color.FromArgb(14, 92, 188));
                e.Graphics.FillEllipse(iconBrush, iconBounds);
                using var iconFont = new Font("Segoe UI Symbol", 11F, FontStyle.Bold);
                TextRenderer.DrawText(
                    e.Graphics,
                    string.IsNullOrEmpty(Text) ? "•" : char.ToUpperInvariant(Text[0]).ToString(),
                    iconFont,
                    iconBounds,
                    Selected ? Color.FromArgb(5, 75, 177) : Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    new Rectangle(55, 0, Width - 64, Height),
                    Selected ? Color.FromArgb(4, 54, 139) : ForeColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }
        }

        private sealed class DashboardActionButton : Button
        {
            private bool hovered;

            public Color BorderColor { get; set; } = CardBorder;

            public DashboardActionButton()
            {
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
                Font = new Font("Segoe UI Semibold", 8.3F, FontStyle.Bold);
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
                if (Width <= 0 || Height <= 0)
                {
                    return;
                }

                using var path = CreateRoundedPath(new Rectangle(0, 0, Width, Height), 8);
                Region = new Region(path);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var bounds = new Rectangle(1, 1, Width - 3, Height - 3);
                var baseColor = BackColor;
                var fillColor = hovered
                    ? Blend(baseColor, PrimaryBlue, 0.08F)
                    : baseColor;
                using var path = CreateRoundedPath(bounds, 8);
                using var brush = new SolidBrush(fillColor);
                using var pen = new Pen(BorderColor);
                e.Graphics.Clear(Parent?.BackColor ?? Color.White);
                e.Graphics.FillPath(brush, path);
                e.Graphics.DrawPath(pen, path);
                TextRenderer.DrawText(
                    e.Graphics,
                    Text,
                    Font,
                    bounds,
                    ForeColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter |
                    TextFormatFlags.WordBreak);
            }

            private static Color Blend(Color first, Color second, float amount)
            {
                var inverse = 1F - amount;
                return Color.FromArgb(
                    (int)((first.R * inverse) + (second.R * amount)),
                    (int)((first.G * inverse) + (second.G * amount)),
                    (int)((first.B * inverse) + (second.B * amount)));
            }
        }

        private static GraphicsPath CreateRoundedPath(Rectangle bounds, int radius)
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
}
