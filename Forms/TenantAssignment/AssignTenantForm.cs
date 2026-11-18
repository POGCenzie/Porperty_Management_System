using PropertyManagementSystem.Model;
using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class AssignTenantForm : Form
{
    private readonly PropertyManagementService propertyManagementService;
    private readonly TenantAssignment? existingAssignment;
    private readonly int? initialTenantId;
    private IReadOnlyList<Tenant> tenants = Array.Empty<Tenant>();
    private IReadOnlyList<CondominiumUnit> units = Array.Empty<CondominiumUnit>();
    private IReadOnlySet<string> occupiedUnitKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    internal AssignTenantForm(PropertyManagementService propertyManagementService)
        : this(propertyManagementService, null, null)
    {
    }

    internal AssignTenantForm(
        PropertyManagementService propertyManagementService,
        Tenant initialTenant)
        : this(propertyManagementService, null, initialTenant.TenantId)
    {
    }

    internal AssignTenantForm(
        PropertyManagementService propertyManagementService,
        TenantAssignment? existingAssignment)
        : this(propertyManagementService, existingAssignment, null)
    {
    }

    private AssignTenantForm(
        PropertyManagementService propertyManagementService,
        TenantAssignment? existingAssignment,
        int? initialTenantId)
    {
        this.propertyManagementService = propertyManagementService;
        this.existingAssignment = existingAssignment;
        this.initialTenantId = initialTenantId;
        ConfigureWindow();
        BuildInterface();
        Shown += LoadLookupsAsync;
    }

    private async void LoadLookupsAsync(object? sender, EventArgs e)
    {
        try
        {
            var condominiums = await propertyManagementService.GetCondominiumsAsync();
            tenants = await propertyManagementService.GetTenantsAsync();
            units = await propertyManagementService.GetCondominiumUnitsAsync();
            var assignments = await propertyManagementService.GetAssignmentsAsync();
            occupiedUnitKeys = assignments
                .Where(item => item.IsActive &&
                    item.TenantAssignmentId != existingAssignment?.TenantAssignmentId)
                .Select(item => $"{item.CondominiumId}|{item.UnitNumber}")
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            condominiumComboBox.DataSource = condominiums
                .Where(item => item.IsActive)
                .Select(item => new LookupItem(item.CondominiumId, item.Name))
                .ToList();
            condominiumComboBox.SelectedIndex = -1;
            tenantComboBox.DataSource = tenants
                .Where(item => item.IsActive)
                .Select(item => new LookupItem(item.TenantId, item.FullName))
                .ToList();
            tenantComboBox.SelectedIndex = -1;
            if (existingAssignment is not null)
            {
                LoadExistingAssignment();
            }
            else if (initialTenantId.HasValue)
            {
                SelectLookupById(tenantComboBox, initialTenantId.Value);
            }
            UpdateFieldAvailability();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to load condominiums and tenants.");
        }
    }

    private void CondominiumComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        LoadUnitsForSelectedCondominium();
        UpdateFieldAvailability();
    }

    private void UnitNumberComboBox_SelectedIndexChanged(object? sender, EventArgs e) =>
        UpdateFieldAvailability();

    private void LoadUnitsForSelectedCondominium()
    {
        if (condominiumComboBox.SelectedItem is not LookupItem condominium)
        {
            unitNumberComboBox.DataSource = null;
            return;
        }

        unitNumberComboBox.DataSource = units
            .Where(item => item.IsActive &&
                item.CondominiumId == condominium.Id &&
                !occupiedUnitKeys.Contains($"{item.CondominiumId}|{item.UnitNumber}"))
            .Select(item => new LookupItem(
                item.CondominiumUnitId,
                item.UnitNumber))
            .ToList();
        unitNumberComboBox.SelectedIndex = -1;
    }

    private void TenantComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (tenantComboBox.SelectedItem is not LookupItem selected)
        {
            return;
        }

        var tenant = tenants.FirstOrDefault(item => item.TenantId == selected.Id);
        if (tenant is null)
        {
            return;
        }

        tenantContactTextBox.Text = tenant.ContactNumber ?? string.Empty;
        tenantEmailTextBox.Text = tenant.EmailAddress ?? string.Empty;
        UpdateFieldAvailability();
    }

    private void UnitOwnerSelectionChanged()
    {
        var unit = unitNumberComboBox.SelectedItem is LookupItem selection
            ? units.FirstOrDefault(item => item.CondominiumUnitId == selection.Id)
            : null;
        ownerNameTextBox.Text = unit?.Owner?.FullName ?? string.Empty;
        ownerEmailTextBox.Text = unit?.Owner?.EmailAddress ?? string.Empty;
        ownerContactTextBox.Text = unit?.Owner?.ContactNumber ?? string.Empty;
        meralcoSinTextBox.Text = unit?.MeralcoSin ?? string.Empty;
    }

    private void LoadExistingAssignment()
    {
        if (existingAssignment is null)
        {
            return;
        }

        SelectLookupById(condominiumComboBox, existingAssignment.CondominiumId);
        LoadUnitsForSelectedCondominium();
        SelectLookupByName(unitNumberComboBox, existingAssignment.UnitNumber);
        SelectLookupById(tenantComboBox, existingAssignment.TenantId);
        occupancyStatusComboBox.Text = existingAssignment.OccupancyStatus;
        keyReleaseNumeric.Value = Math.Min(
            keyReleaseNumeric.Maximum,
            existingAssignment.KeyRelease ?? 0);
        accessCardNumeric.Value = Math.Min(
            accessCardNumeric.Maximum,
            existingAssignment.AccessCardRelease ?? 0);
        rentalAmountNumeric.Value = Math.Min(
            rentalAmountNumeric.Maximum,
            existingAssignment.RentalAmountToOwner ?? 0M);
        withDuesComboBox.Text = existingAssignment.WithDues ?? string.Empty;
        stayDurationNumeric.Value = Math.Min(
            stayDurationNumeric.Maximum,
            existingAssignment.StayDuration ?? 0);
        stayDurationUnitComboBox.Text = existingAssignment.StayDurationUnit ?? string.Empty;
        SetDate(startDatePicker, existingAssignment.StartOfContract);
        SetDate(endDatePicker, existingAssignment.EndOfContract);
        SetDate(airconDatePicker, existingAssignment.AirconMaintenanceDate);
        maintenanceStatusTextBox.Text = existingAssignment.MaintenanceStatus ?? string.Empty;
        ownerAdvanceMonthsNumeric.Value = Math.Min(
            ownerAdvanceMonthsNumeric.Maximum,
            existingAssignment.OwnerAdvanceMonths ?? 0);
        ownerDepositMonthsNumeric.Value = Math.Min(
            ownerDepositMonthsNumeric.Maximum,
            existingAssignment.OwnerDepositMonths ?? 0);
        tenantContactTextBox.Text = existingAssignment.MainTenantContactNumber ?? string.Empty;
        tenantEmailTextBox.Text = existingAssignment.MainTenantEmailAddress ?? string.Empty;
        UnitOwnerSelectionChanged();
        UpdateFieldAvailability();
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (condominiumComboBox.SelectedItem is not LookupItem condominium ||
            tenantComboBox.SelectedItem is not LookupItem tenant ||
            unitNumberComboBox.SelectedItem is not LookupItem unit)
        {
            FadingToast.ShowMessage(this, "Select a condominium, unit and tenant.");
            return;
        }

        saveButton.Enabled = false;
        try
        {
            var selectedUnit = units.FirstOrDefault(item =>
                item.CondominiumUnitId == unit.Id);
            if (selectedUnit?.Owner is null)
            {
                FadingToast.ShowMessage(this, "The selected unit does not have an active owner record.");
                return;
            }

            var stayDuration = GetEnteredStayDuration();
            RecalculateContractEndDate();
            var assignment = new TenantAssignment
            {
                TenantAssignmentId = existingAssignment?.TenantAssignmentId ?? 0,
                CondominiumId = condominium.Id,
                TenantId = tenant.Id,
                UnitNumber = unit.Name,
                OwnerName = selectedUnit.Owner.FullName,
                OccupancyStatus = occupancyStatusComboBox.Text,
                MeralcoSin = Clean(selectedUnit.MeralcoSin ?? string.Empty),
                KeyRelease = keyReleaseNumeric.Value <= 0
                    ? null
                    : decimal.ToInt32(keyReleaseNumeric.Value),
                AccessCardRelease = accessCardNumeric.Value <= 0
                    ? null
                    : decimal.ToInt32(accessCardNumeric.Value),
                RentalAmountToOwner = rentalAmountNumeric.Value <= 0
                    ? null
                    : rentalAmountNumeric.Value,
                WithDues = Clean(withDuesComboBox.Text),
                StayDuration = stayDuration <= 0
                    ? null
                    : stayDuration,
                StayDurationUnit = stayDuration <= 0
                    ? null
                    : Clean(stayDurationUnitComboBox.Text),
                StartOfContract = GetDate(startDatePicker),
                EndOfContract = GetDate(endDatePicker),
                AirconMaintenanceDate = GetDate(airconDatePicker),
                MaintenanceStatus = Clean(maintenanceStatusTextBox.Text),
                OwnerAdvanceMonths = ownerAdvanceMonthsNumeric.Value <= 0
                    ? null
                    : decimal.ToInt32(ownerAdvanceMonthsNumeric.Value),
                OwnerDepositMonths = ownerDepositMonthsNumeric.Value <= 0
                    ? null
                    : decimal.ToInt32(ownerDepositMonthsNumeric.Value),
                OwnerEmailAddress = Clean(selectedUnit.Owner.EmailAddress ?? string.Empty),
                OwnerContactNumber = Clean(selectedUnit.Owner.ContactNumber ?? string.Empty),
                MainTenantContactNumber = Clean(tenantContactTextBox.Text),
                MainTenantEmailAddress = Clean(tenantEmailTextBox.Text)
            };
            var result = existingAssignment is null
                ? await propertyManagementService.AssignTenantAsync(assignment)
                : await propertyManagementService.UpdateTenantAssignmentAsync(assignment);
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
            FadingToast.ShowMessage(this, "Unable to save the tenant assignment.");
        }
        finally
        {
            if (!IsDisposed)
            {
                saveButton.Enabled = true;
            }
        }
    }

    private async void EndContractButton_Click(object? sender, EventArgs e)
    {
        if (existingAssignment is null)
        {
            return;
        }

        using var confirmation = new ConfirmationDialog(
            "End tenant contract?",
            "This closes the assignment immediately, sets the contract end date to today, and makes the unit available for another tenant.",
            "End contract");
        if (confirmation.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        endContractButton.Enabled = false;
        try
        {
            var result = await propertyManagementService.EndTenantAssignmentAsync(
                existingAssignment.TenantAssignmentId);
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
            FadingToast.ShowMessage(this, "Unable to end the tenant contract.");
        }
        finally
        {
            if (!IsDisposed)
            {
                endContractButton.Enabled = true;
            }
        }
    }

    private static DateTime? GetDate(DateTimePicker picker) =>
        picker.Checked ? picker.Value.Date : null;

    private static string? Clean(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void SetDate(DateTimePicker picker, DateTime? value)
    {
        picker.Checked = value.HasValue;
        if (value.HasValue)
        {
            picker.Value = value.Value;
        }
    }

    private static void SelectLookupById(ComboBox comboBox, int id)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is LookupItem item && item.Id == id)
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private static void SelectLookupByName(ComboBox comboBox, string name)
    {
        for (var index = 0; index < comboBox.Items.Count; index++)
        {
            if (comboBox.Items[index] is LookupItem item &&
                string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                comboBox.SelectedIndex = index;
                return;
            }
        }
    }

    private sealed record LookupItem(int Id, string Name)
    {
        public override string ToString() => Name;
    }
}
