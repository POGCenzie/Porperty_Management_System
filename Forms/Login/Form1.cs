using PropertyManagementSystem.Services;

namespace PropertyManagementSystem
{
    public partial class Form1 : Form
    {
        private AuthenticationService? authenticationService;
        private CurrentUserSession? currentUserSession;
        private UserAccountService? userAccountService;
        private PropertyManagementService? propertyManagementService;

        public Form1()
        {
            InitializeComponent();
            ConfigureWindow();
            BuildLoginInterface();
        }

        internal Form1(
            AuthenticationService authenticationService,
            CurrentUserSession currentUserSession,
            UserAccountService userAccountService,
            PropertyManagementService propertyManagementService)
            : this()
        {
            this.authenticationService = authenticationService;
            this.currentUserSession = currentUserSession;
            this.userAccountService = userAccountService;
            this.propertyManagementService = propertyManagementService;
        }

        private async void LoginButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text))
            {
                ShowToast("Please enter your username or email.", ToastKind.Error);
                usernameTextBox.FocusInput();
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordTextBox.Text))
            {
                ShowToast("Please enter your password.", ToastKind.Error);
                passwordTextBox.FocusInput();
                return;
            }

            if (authenticationService is null ||
                currentUserSession is null ||
                userAccountService is null ||
                propertyManagementService is null)
            {
                ShowToast(
                    "Login services are not configured.",
                    ToastKind.Error);
                return;
            }

            try
            {
                var result = await authenticationService.AuthenticateAsync(
                    usernameTextBox.Text,
                    passwordTextBox.Text);

                if (result.Status ==
                    AuthenticationStatus.PasswordDoesNotMeetRequirements)
                {
                    ShowToast(
                        PasswordPolicy.RequirementMessage,
                        ToastKind.Error);
                    passwordTextBox.FocusInput();
                    return;
                }

                if (result.Status == AuthenticationStatus.InactiveAccount)
                {
                    ShowToast(
                        "This user account is inactive. Contact an administrator.",
                        ToastKind.Error);
                    return;
                }

                if (result.Status == AuthenticationStatus.InvalidCredentials ||
                    result.User is null)
                {
                    ShowToast(
                        "The username or password is incorrect.",
                        ToastKind.Error);
                    passwordTextBox.FocusInput();
                    return;
                }

                currentUserSession.Start(result.User);

                Hide();
                using var dashboard = new DashBoard(
                    currentUserSession,
                    userAccountService,
                    propertyManagementService);
                dashboard.ShowDialog();
                currentUserSession.Clear();
                Close();
            }
            catch (Exception)
            {
                ShowToast(
                    "Unable to connect to the Syrento database.",
                    ToastKind.Error);
            }
        }
    }
}
