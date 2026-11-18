using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class AddCondominiumForm : Form
{
    private readonly PropertyManagementService propertyManagementService;

    internal AddCondominiumForm(PropertyManagementService propertyManagementService)
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
            var result = await propertyManagementService.AddCondominiumAsync(
                condominiumNameTextBox.Text);
            if (!result.Succeeded)
            {
                FadingToast.ShowMessage(this, result.Message);
                condominiumNameTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to save the condominium.");
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
