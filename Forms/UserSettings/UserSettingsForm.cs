using PropertyManagementSystem.Services;
using PropertyManagementSystem.Model;

namespace PropertyManagementSystem
{
    public partial class UserSettingsForm : Form
    {
        private readonly UserAccountService userAccountService;
        private readonly CurrentUserSession currentUserSession;
        private readonly User targetUser;
        private readonly bool administratorEditMode;

        internal User? UpdatedUser { get; private set; }

        internal UserSettingsForm(
            UserAccountService userAccountService,
            CurrentUserSession currentUserSession)
            : this(
                userAccountService,
                currentUserSession,
                currentUserSession.User ?? throw new InvalidOperationException(
                    "The current login session is not available."),
                false)
        {
        }

        internal UserSettingsForm(
            UserAccountService userAccountService,
            CurrentUserSession currentUserSession,
            User targetUser)
            : this(userAccountService, currentUserSession, targetUser, true)
        {
        }

        private UserSettingsForm(
            UserAccountService userAccountService,
            CurrentUserSession currentUserSession,
            User targetUser,
            bool administratorEditMode)
        {
            this.userAccountService = userAccountService;
            this.currentUserSession = currentUserSession;
            this.targetUser = targetUser;
            this.administratorEditMode = administratorEditMode;

            ConfigureWindow();
            BuildAccountInterface();
            LoadTargetUser();
        }

        private void LoadTargetUser()
        {
            formTitleLabel.Text = administratorEditMode ? "Edit user" : "Edit my account";
            formDescriptionLabel.Text = administratorEditMode
                ? $"Update @{targetUser.Username}. Administrator password confirmation is required."
                : "Change your display name, login username or password.";
            displayNameTextBox.Text = string.IsNullOrWhiteSpace(targetUser.DisplayName)
                ? targetUser.Username
                : targetUser.DisplayName;
            usernameTextBox.Text = targetUser.Username;
            accountTypeValueLabel.Text = targetUser.UserGroup;
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            var currentUser = currentUserSession.User;
            if (currentUser is null)
            {
                ShowToast("The current login session is no longer available.");
                return;
            }

            if (string.IsNullOrWhiteSpace(currentPasswordTextBox.Text))
            {
                ShowToast("Enter your current password to confirm the changes.");
                currentPasswordTextBox.Focus();
                return;
            }

            if (newPasswordTextBox.Text != confirmPasswordTextBox.Text)
            {
                ShowToast("The new password and confirmation do not match.");
                confirmPasswordTextBox.Focus();
                return;
            }

            saveButton.Enabled = false;
            try
            {
                var newPassword = string.IsNullOrWhiteSpace(newPasswordTextBox.Text)
                    ? null
                    : newPasswordTextBox.Text;
                var result = administratorEditMode
                    ? await userAccountService.UpdateUserAsAdminAsync(
                        currentUser,
                        targetUser.UserId,
                        displayNameTextBox.Text,
                        usernameTextBox.Text,
                        currentPasswordTextBox.Text,
                        newPassword)
                    : await userAccountService.UpdateCurrentUserAsync(
                        currentUser,
                        displayNameTextBox.Text,
                        usernameTextBox.Text,
                        currentPasswordTextBox.Text,
                        newPassword);

                switch (result.Status)
                {
                    case AccountUpdateStatus.InvalidDisplayName:
                        ShowToast("Enter a display name of 150 characters or fewer.");
                        displayNameTextBox.Focus();
                        return;

                    case AccountUpdateStatus.InvalidUsername:
                        ShowToast("Enter a username of 100 characters or fewer.");
                        usernameTextBox.Focus();
                        return;

                    case AccountUpdateStatus.InvalidCurrentPassword:
                        ShowToast("The current password is incorrect.");
                        currentPasswordTextBox.Focus();
                        return;

                    case AccountUpdateStatus.UsernameAlreadyExists:
                        ShowToast("That username is already being used.");
                        usernameTextBox.Focus();
                        return;

                    case AccountUpdateStatus.PasswordDoesNotMeetRequirements:
                        ShowToast(PasswordPolicy.RequirementMessage);
                        newPasswordTextBox.Focus();
                        return;

                    case AccountUpdateStatus.Unauthorized:
                        ShowToast("Administrator access is required to edit users.");
                        return;

                    case AccountUpdateStatus.Success when result.User is not null:
                        UpdatedUser = result.User;
                        if (result.User.UserId == currentUser.UserId)
                        {
                            currentUserSession.Start(result.User);
                        }
                        DialogResult = DialogResult.OK;
                        Close();
                        return;
                }
            }
            catch (Exception)
            {
                ShowToast("Unable to update the user account in the database.");
            }
            finally
            {
                if (!IsDisposed)
                {
                    saveButton.Enabled = true;
                }
            }
        }
    }
}
