using PropertyManagementSystem.Model;
using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class AddOwnerForm : Form
{
    private readonly PropertyManagementService propertyManagementService;
    private readonly Owner? existingOwner;

    internal AddOwnerForm(PropertyManagementService propertyManagementService)
        : this(propertyManagementService, null)
    {
    }

    internal AddOwnerForm(
        PropertyManagementService propertyManagementService,
        Owner? existingOwner)
    {
        this.propertyManagementService = propertyManagementService;
        this.existingOwner = existingOwner;
        ConfigureWindow();
        BuildInterface();
        if (existingOwner is not null)
        {
            ownerNameTextBox.Text = existingOwner.FullName;
            emailTextBox.Text = existingOwner.EmailAddress ?? string.Empty;
            contactTextBox.Text = existingOwner.ContactNumber ?? string.Empty;
            statusComboBox.SelectedItem = existingOwner.IsActive ? "Active" : "Inactive";
        }
    }

    internal Owner? CreatedOwner { get; private set; }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        saveButton.Enabled = false;
        try
        {
            var isActive = string.Equals(
                statusComboBox.SelectedItem?.ToString(),
                "Active",
                StringComparison.OrdinalIgnoreCase);
            var result = existingOwner is null
                ? await propertyManagementService.AddOwnerAsync(
                    ownerNameTextBox.Text,
                    emailTextBox.Text,
                    contactTextBox.Text)
                : await propertyManagementService.UpdateOwnerAsync(
                    existingOwner,
                    ownerNameTextBox.Text,
                    emailTextBox.Text,
                    contactTextBox.Text,
                    isActive);
            if (!result.Succeeded)
            {
                FadingToast.ShowMessage(this, result.Message);
                return;
            }

            CreatedOwner = result.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, existingOwner is null
                ? "Unable to save the owner."
                : "Unable to update the owner.");
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
