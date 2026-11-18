using PropertyManagementSystem.Constants;
using PropertyManagementSystem.Model;
using PropertyManagementSystem.Services;

namespace PropertyManagementSystem;

internal partial class CreateSoaForm : Form
{
    private readonly PropertyManagementService propertyManagementService;
    private readonly CurrentUserSession currentUserSession;
    private readonly string soaType;
    private readonly int? initialAssignmentId;
    private IReadOnlyList<TenantAssignment> assignments = Array.Empty<TenantAssignment>();
    private decimal computedBaseAmount;
    private decimal rentalAmount;
    private decimal advanceAmount;
    private decimal depositAmount;
    private StatementOfAccount? filedStatement;
    private TenantAssignment? filedAssignment;

    internal CreateSoaForm(
        PropertyManagementService propertyManagementService,
        CurrentUserSession currentUserSession,
        string soaType,
        int? initialAssignmentId = null)
    {
        this.propertyManagementService = propertyManagementService;
        this.currentUserSession = currentUserSession;
        this.soaType = soaType;
        this.initialAssignmentId = initialAssignmentId;
        ConfigureWindow();
        BuildInterface();
        LoadDefaultDeductions();
        Shown += LoadAssignmentsAsync;
    }

    private async void LoadAssignmentsAsync(object? sender, EventArgs e)
    {
        try
        {
            assignments = await propertyManagementService.GetAssignmentsAsync();
            assignmentComboBox.DataSource = assignments
                .OrderByDescending(item => item.IsActive)
                .ThenByDescending(item => item.EndOfContract ?? item.LastUpdatedDate)
                .Select(item => new AssignmentLookup(
                    item.TenantAssignmentId,
                    $"{item.Condominium.Name} · Unit {item.UnitNumber} · {item.Tenant.FullName}"))
                .ToList();
            assignmentComboBox.DataSource = assignments
                .OrderByDescending(item => item.IsActive)
                .ThenByDescending(item => item.EndOfContract ?? item.LastUpdatedDate)
                .Select(item => new AssignmentLookup(
                    item.TenantAssignmentId,
                    BuildAssignmentDisplay(item)))
                .ToList();
            assignmentComboBox.SelectedIndex = -1;
            if (initialAssignmentId.HasValue)
            {
                for (var index = 0; index < assignmentComboBox.Items.Count; index++)
                {
                    if (assignmentComboBox.Items[index] is AssignmentLookup option &&
                        option.Id == initialAssignmentId.Value)
                    {
                        assignmentComboBox.SelectedIndex = index;
                        break;
                    }
                }
            }
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to load tenant assignments.");
        }
    }

    private void LoadDefaultDeductions()
    {
        var descriptions = soaType == SoaTypes.Owner
            ? new[] { "Commission", "Notary", "Print out", "Reservation", "Recleaning" }
            : new[] { "Unit cleaning", "Meralco", "Water", "Repairs / repainting" };
        foreach (var description in descriptions)
        {
            deductionsGrid.Rows.Add(description, 0M, string.Empty);
        }
        CalculateTotals();
    }

    private void CalculateTotals()
    {
        var deductions = deductionsGrid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow)
            .Sum(row => ParseAmount(row.Cells[1].Value));
        totalDeductionsValueLabel.Text = deductions.ToString("N2");
        netAmountValueLabel.Text = Math.Max(0M, computedBaseAmount - deductions).ToString("N2");
    }

    private void AssignmentComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (assignmentComboBox.SelectedItem is not AssignmentLookup selection)
        {
            SetPaymentBreakdown(null);
            return;
        }

        var assignment = assignments.FirstOrDefault(
            item => item.TenantAssignmentId == selection.Id);
        if (assignment is null)
        {
            SetPaymentBreakdown(null);
            return;
        }

        SetPaymentBreakdown(assignment);
    }

    private void SetPaymentBreakdown(TenantAssignment? assignment)
    {
        rentalAmount = assignment?.RentalAmountToOwner ?? 0M;
        advanceAmount = rentalAmount * (assignment?.OwnerAdvanceMonths ?? 0);
        depositAmount = rentalAmount * (assignment?.OwnerDepositMonths ?? 0);
        computedBaseAmount = soaType == SoaTypes.Owner
            ? advanceAmount + depositAmount
            : depositAmount;

        paymentBreakdownGrid.Rows.Clear();
        if (rentalAmount > 0M)
        {
            paymentBreakdownGrid.Rows.Add("MONTHLY RENT - REFERENCE", rentalAmount);
        }
        if (advanceAmount > 0M)
        {
            paymentBreakdownGrid.Rows.Add(
                CreateMonthDescription(advanceAmount, rentalAmount, "ADVANCE"),
                advanceAmount);
        }
        if (depositAmount > 0M)
        {
            paymentBreakdownGrid.Rows.Add(
                CreateMonthDescription(depositAmount, rentalAmount, "DEPOSIT"),
                depositAmount);
        }
        paymentBreakdownGrid.Rows.Add(
            soaType == SoaTypes.Owner
                ? "TOTAL TENANT PAYMENT TO OWNER"
                : "SECURITY DEPOSIT FOR REFUND",
            computedBaseAmount);
        CalculateTotals();
    }

    private static string CreateMonthDescription(
        decimal amount,
        decimal monthlyRent,
        string paymentType)
    {
        if (monthlyRent <= 0M)
        {
            return $"OWNER {paymentType}";
        }

        var months = decimal.Round(amount / monthlyRent, 2);
        var monthLabel = months == 1M ? "MONTH" : "MONTHS";
        return $"{months:0.##} {monthLabel} {paymentType}";
    }

    private async void SaveButton_Click(object? sender, EventArgs e)
    {
        if (filedStatement is not null && filedAssignment is not null)
        {
            OpenPrintPreview(filedStatement, filedAssignment);
            return;
        }

        if (assignmentComboBox.SelectedItem is not AssignmentLookup assignment)
        {
            FadingToast.ShowMessage(this, "Select a tenant assignment.");
            return;
        }

        if (paperSizeComboBox.SelectedItem is not string)
        {
            FadingToast.ShowMessage(this, "Select A4, Short Bond or Long Bond paper.");
            return;
        }

        if (computedBaseAmount <= 0M)
        {
            FadingToast.ShowMessage(this, soaType == SoaTypes.Owner
                ? "Enter the monthly rent and advance or deposit months in the tenant assignment first."
                : "Enter the monthly rent and deposit months in the tenant assignment first.");
            return;
        }

        var items = deductionsGrid.Rows
            .Cast<DataGridViewRow>()
            .Where(row => !row.IsNewRow)
            .Select(row => new SoaLineItemInput(
                row.Cells[0].Value?.ToString() ?? string.Empty,
                ParseAmount(row.Cells[1].Value),
                row.Cells[2].Value?.ToString()))
            .ToList();
        saveButton.Enabled = false;
        try
        {
            var result = await propertyManagementService.CreateStatementOfAccountAsync(
                soaType,
                assignment.Id,
                statementDatePicker.Value,
                rentalAmount,
                advanceAmount,
                depositAmount,
                computedBaseAmount,
                items,
                currentUserSession.User?.Username ?? "system");
            if (!result.Succeeded)
            {
                FadingToast.ShowMessage(this, result.Message);
                return;
            }

            filedStatement = result.Value;
            filedAssignment = assignments.FirstOrDefault(item =>
                item.TenantAssignmentId == assignment.Id);
            if (filedStatement is null || filedAssignment is null)
            {
                FadingToast.ShowMessage(this, "The SOA was filed, but its print data could not be loaded.");
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            saveButton.Text = "Open print preview";
            OpenPrintPreview(filedStatement, filedAssignment);
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(this, "Unable to file the SOA.");
        }
        finally
        {
            if (!IsDisposed)
            {
                saveButton.Enabled = true;
            }
        }
    }

    private void OpenPrintPreview(
        StatementOfAccount statement,
        TenantAssignment assignment)
    {
        try
        {
            SoaPrintService.ShowPreview(
                this,
                statement,
                assignment,
                paperSizeComboBox.SelectedItem?.ToString() ?? SoaPaperSizes.A4);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception)
        {
            FadingToast.ShowMessage(
                this,
                "The SOA is filed, but Print Preview could not open. Check the installed printer or PDF printer.");
        }
    }

    private static decimal ParseAmount(object? value) =>
        decimal.TryParse(value?.ToString(), out var amount) ? Math.Max(0M, amount) : 0M;

    private static string BuildAssignmentDisplay(TenantAssignment assignment)
    {
        var status = assignment.IsActive
            ? "ACTIVE"
            : $"CLOSED {assignment.EndOfContract?.ToString("MMM d, yyyy") ?? string.Empty}";
        return $"{assignment.Condominium.Name} • Unit {assignment.UnitNumber} • " +
            $"{assignment.Tenant.FullName} • {status}";
    }

    private sealed record AssignmentLookup(int Id, string Display)
    {
        public override string ToString() => Display;
    }
}
