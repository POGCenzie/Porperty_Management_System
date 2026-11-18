using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class AddTenantForm : Form
{
    private readonly PropertyManagementService propertyManagementService;

    internal AddTenantForm(PropertyManagementService propertyManagementService)
    {
        this.propertyManagementService = propertyManagementService;
        ConfigureWindow();
        BuildInterface();
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        saveButton.Enabled = false;
        try
        {
            var result = await propertyManagementService.AddTenantAsync(
                tenantNameTextBox.Text,
                contactTextBox.Text,
                emailTextBox.Text);
            if (!result.Succeeded)
            {
                FadingToast.ShowMessage(this, result.Message);
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to save the tenant.");
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
