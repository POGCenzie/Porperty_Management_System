using PropertyManagementSystem.Services;

namespace PropertyManagementSystem
{
    public partial class DashBoard : Form
    {
        private CurrentUserSession? currentUserSession;
        private UserAccountService? userAccountService;
        private PropertyManagementService? propertyManagementService;

        public DashBoard()
        {
            InitializeComponent();
            ConfigureWindow();
            BuildDashboardInterface();
        }

        internal DashBoard(
            CurrentUserSession currentUserSession,
            UserAccountService userAccountService,
            PropertyManagementService propertyManagementService)
            : this()
        {
            this.currentUserSession = currentUserSession;
            this.userAccountService = userAccountService;
            this.propertyManagementService = propertyManagementService;
            LoadAttentionPreferences();
            ApplySessionDetails();
            ShowOverview();
        }
    }
}
