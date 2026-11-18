using PropertyManagementSystem.Model;
using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class AddUnitForm : Form
{
    private readonly PropertyManagementService propertyManagementService;
    private readonly Condominium condominium;
    private readonly CondominiumUnit? existingUnit;
    private IReadOnlyList<Owner> owners = Array.Empty<Owner>();

    internal AddUnitForm(
        PropertyManagementService propertyManagementService,
        Condominium condominium)
        : this(propertyManagementService, condominium, null)
    {
    }

    internal AddUnitForm(
        PropertyManagementService propertyManagementService,
        Condominium condominium,
        CondominiumUnit? existingUnit)
    {
        this.propertyManagementService = propertyManagementService;
        this.condominium = condominium;
        this.existingUnit = existingUnit;
        ConfigureWindow();
        BuildInterface();
        if (existingUnit is not null)
        {
            unitNumberTextBox.Text = existingUnit.UnitNumber;
            meralcoSinTextBox.Text = existingUnit.MeralcoSin ?? string.Empty;
        }
        Shown += LoadOwnersAsync;
    }

    private async void LoadOwnersAsync(object? sender, EventArgs e) =>
        await ReloadOwnersAsync(existingUnit?.OwnerId);

    private async Task ReloadOwnersAsync(int? selectedOwnerId)
    {
        try
        {
            owners = await propertyManagementService.GetOwnersAsync();
            ownerComboBox.DataSource = owners
                .Where(item => item.IsActive)
                .Select(item => new OwnerLookup(item.OwnerId, item.FullName))
                .ToList();
            ownerComboBox.SelectedIndex = -1;
            if (selectedOwnerId.HasValue)
            {
                SelectOwner(selectedOwnerId.Value);
            }
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to load owners.");
        }
    }

    private async void AddOwnerButton_Click(object? sender, EventArgs e)
    {
        using var form = new AddOwnerForm(propertyManagementService);
        if (form.ShowDialog(this) == DialogResult.OK && form.CreatedOwner is not null)
        {
            await ReloadOwnersAsync(form.CreatedOwner.OwnerId);
        }
    }

    private void SelectOwner(int ownerId)
    {
        for (var index = 0; index < ownerComboBox.Items.Count; index++)
        {
            if (ownerComboBox.Items[index] is OwnerLookup owner && owner.Id == ownerId)
            {
                ownerComboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        saveButton.Enabled = false;
        try
        {
            if (ownerComboBox.SelectedItem is not OwnerLookup owner)
            {
                FadingToast.ShowMessage(this, "Select a unit owner.");
                return;
            }

            var result = existingUnit is null
                ? await propertyManagementService.AddCondominiumUnitAsync(
                    condominium.CondominiumId,
                    unitNumberTextBox.Text,
                    owner.Id,
                    meralcoSinTextBox.Text)
                : await propertyManagementService.UpdateCondominiumUnitAsync(
                    existingUnit,
                    unitNumberTextBox.Text,
                    owner.Id,
                    meralcoSinTextBox.Text);
            if (!result.Succeeded)
            {
                FadingToast.ShowMessage(this, result.Message);
                unitNumberTextBox.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to save the condominium unit.");
        }
        finally
        {
            if (!IsDisposed)
            {
                saveButton.Enabled = true;
            }
        }
    }

    private sealed record OwnerLookup(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
