using PropertyManagementSystem.Model;
using PropertyManagementSystem.Repository;

namespace PropertyManagementSystem.Services;

public sealed record PropertyOperationResult<T>(
    bool Succeeded,
    string Message,
    T? Value = default);

public sealed record SoaLineItemInput(string Description, decimal Amount, string? Remarks);

public sealed class PropertyManagementService
{
    private readonly IPropertyRepository propertyRepository;

    public PropertyManagementService(IPropertyRepository propertyRepository)
    {
        this.propertyRepository = propertyRepository;
    }

    public Task<IReadOnlyList<Condominium>> GetCondominiumsAsync(
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetCondominiumsAsync(cancellationToken);

    public Task<IReadOnlyList<Tenant>> GetTenantsAsync(
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetTenantsAsync(cancellationToken);

    public Task<IReadOnlyList<Owner>> GetOwnersAsync(
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetOwnersAsync(cancellationToken);

    public async Task<PropertyOperationResult<Owner>> AddOwnerAsync(
        string fullName,
        string? emailAddress,
        string? contactNumber,
        CancellationToken cancellationToken = default)
    {
        fullName = fullName.Trim();
        emailAddress = NullIfWhiteSpace(emailAddress);
        contactNumber = NullIfWhiteSpace(contactNumber);
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 200)
        {
            return new PropertyOperationResult<Owner>(false, "Enter an owner name of 200 characters or fewer.");
        }
        if (emailAddress?.Length > 200 || contactNumber?.Length > 100)
        {
            return new PropertyOperationResult<Owner>(false, "Owner contact information is too long.");
        }

        var owner = await propertyRepository.AddOwnerAsync(new Owner
        {
            FullName = fullName,
            EmailAddress = emailAddress,
            ContactNumber = contactNumber,
            IsActive = true,
            LastUpdatedDate = DateTime.UtcNow
        }, cancellationToken);
        return new PropertyOperationResult<Owner>(true, "Owner added successfully.", owner);
    }

    public async Task<PropertyOperationResult<Owner>> UpdateOwnerAsync(
        Owner owner,
        string fullName,
        string? emailAddress,
        string? contactNumber,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateOwnerDetails(fullName, emailAddress, contactNumber);
        if (!validation.Succeeded)
        {
            return validation;
        }

        var updated = await propertyRepository.UpdateOwnerAsync(
            owner.OwnerId,
            fullName.Trim(),
            NullIfWhiteSpace(emailAddress),
            NullIfWhiteSpace(contactNumber),
            isActive,
            cancellationToken);
        return updated is null
            ? new PropertyOperationResult<Owner>(false, "The selected owner no longer exists.")
            : new PropertyOperationResult<Owner>(true, "Owner updated successfully.", updated);
    }

    public async Task<PropertyOperationResult<Owner>> DeleteOwnerAsync(
        Owner owner,
        CancellationToken cancellationToken = default)
    {
        if (!owner.IsActive)
        {
            return new PropertyOperationResult<Owner>(false, "This owner is already inactive.");
        }

        var updated = await propertyRepository.SetOwnerActiveAsync(
            owner.OwnerId,
            false,
            cancellationToken);
        return updated is null
            ? new PropertyOperationResult<Owner>(false, "The selected owner no longer exists.")
            : new PropertyOperationResult<Owner>(true, "Owner marked inactive.", updated);
    }

    private static PropertyOperationResult<Owner> ValidateOwnerDetails(
        string fullName,
        string? emailAddress,
        string? contactNumber)
    {
        fullName = fullName.Trim();
        emailAddress = NullIfWhiteSpace(emailAddress);
        contactNumber = NullIfWhiteSpace(contactNumber);
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 200)
        {
            return new PropertyOperationResult<Owner>(false, "Enter an owner name of 200 characters or fewer.");
        }
        if (emailAddress?.Length > 200 || contactNumber?.Length > 100)
        {
            return new PropertyOperationResult<Owner>(false, "Owner contact information is too long.");
        }

        return new PropertyOperationResult<Owner>(true, string.Empty);
    }

    public Task<IReadOnlyList<CondominiumUnit>> GetCondominiumUnitsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetCondominiumUnitsAsync(condominiumId, cancellationToken);

    public Task<IReadOnlyList<TenantAssignment>> GetAssignmentsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetAssignmentsAsync(condominiumId, cancellationToken);

    public Task<AttentionCounts> GetAttentionCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        return propertyRepository.GetAttentionCountsAsync(
            today,
            today.AddDays(30),
            cancellationToken);
    }

    public Task<IReadOnlyList<StatementOfAccount>> GetStatementsOfAccountAsync(
        CancellationToken cancellationToken = default) =>
        propertyRepository.GetStatementsOfAccountAsync(cancellationToken);

    public async Task<PropertyOperationResult<StatementOfAccount>> CreateStatementOfAccountAsync(
        string soaType,
        int tenantAssignmentId,
        DateTime statementDate,
        decimal rentalAmount,
        decimal advanceAmount,
        decimal depositAmount,
        decimal baseAmount,
        IEnumerable<SoaLineItemInput> lineItems,
        string createdByUsername,
        CancellationToken cancellationToken = default)
    {
        if (soaType != Constants.SoaTypes.Owner && soaType != Constants.SoaTypes.Tenant)
        {
            return new PropertyOperationResult<StatementOfAccount>(false, "Select a valid SOA type.");
        }

        if (tenantAssignmentId <= 0)
        {
            return new PropertyOperationResult<StatementOfAccount>(false, "Select a tenant assignment.");
        }

        if (baseAmount <= 0)
        {
            return new PropertyOperationResult<StatementOfAccount>(false, "Enter an amount greater than zero.");
        }

        var cleanedItems = lineItems
            .Where(item => !string.IsNullOrWhiteSpace(item.Description))
            .Select((item, index) => new SoaLineItem
            {
                Description = item.Description.Trim(),
                Amount = Math.Max(0M, item.Amount),
                Remarks = NullIfWhiteSpace(item.Remarks),
                SortOrder = index + 1
            })
            .ToList();
        var deductions = cleanedItems.Sum(item => item.Amount);
        if (deductions > baseAmount)
        {
            return new PropertyOperationResult<StatementOfAccount>(
                false,
                "Total deductions cannot be greater than the starting amount.");
        }

        var typeCode = soaType == Constants.SoaTypes.Owner ? "OWN" : "TEN";
        var statement = new StatementOfAccount
        {
            SoaNumber = $"SOA-{typeCode}-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            SoaType = soaType,
            TenantAssignmentId = tenantAssignmentId,
            StatementDate = statementDate.Date,
            RentalAmount = rentalAmount > 0M ? rentalAmount : null,
            AdvanceAmount = advanceAmount > 0M ? advanceAmount : null,
            DepositAmount = depositAmount > 0M ? depositAmount : null,
            BaseAmount = baseAmount,
            TotalDeductions = deductions,
            NetAmount = baseAmount - deductions,
            Status = Constants.SoaStatuses.Filed,
            CreatedByUsername = createdByUsername.Trim(),
            LastUpdatedDate = DateTime.UtcNow,
            LineItems = cleanedItems
        };
        var saved = await propertyRepository.AddStatementOfAccountAsync(statement, cancellationToken);
        return new PropertyOperationResult<StatementOfAccount>(true, "SOA filed successfully.", saved);
    }

    public async Task<PropertyOperationResult<Condominium>> AddCondominiumAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        name = name.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 150)
        {
            return new PropertyOperationResult<Condominium>(
                false,
                "Enter a condominium name of 150 characters or fewer.");
        }

        if (await propertyRepository.CondominiumNameExistsAsync(name, cancellationToken))
        {
            return new PropertyOperationResult<Condominium>(
                false,
                "That condominium already exists.");
        }

        var condominium = await propertyRepository.AddCondominiumAsync(
            new Condominium
            {
                Name = name,
                IsActive = true,
                LastUpdatedDate = DateTime.UtcNow
            },
            cancellationToken);
        return new PropertyOperationResult<Condominium>(
            true,
            "Condominium added successfully.",
            condominium);
    }

    public async Task<PropertyOperationResult<CondominiumUnit>> AddCondominiumUnitAsync(
        int condominiumId,
        string unitNumber,
        int ownerId,
        string? meralcoSin,
        CancellationToken cancellationToken = default)
    {
        unitNumber = unitNumber.Trim();
        meralcoSin = NullIfWhiteSpace(meralcoSin);
        if (condominiumId <= 0 || ownerId <= 0 || string.IsNullOrWhiteSpace(unitNumber) || unitNumber.Length > 50)
        {
            return new PropertyOperationResult<CondominiumUnit>(
                false,
                "Select a condominium and owner, then enter a unit number of 50 characters or fewer.");
        }
        if (meralcoSin?.Length > 100)
        {
            return new PropertyOperationResult<CondominiumUnit>(false, "Meralco SIN must use 100 characters or fewer.");
        }

        if (await propertyRepository.CondominiumUnitExistsAsync(
            condominiumId,
            unitNumber,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<CondominiumUnit>(
                false,
                "That unit number already exists in the selected condominium.");
        }

        var unit = await propertyRepository.AddCondominiumUnitAsync(
            new CondominiumUnit
            {
                CondominiumId = condominiumId,
                UnitNumber = unitNumber,
                OwnerId = ownerId,
                MeralcoSin = meralcoSin,
                IsActive = true,
                LastUpdatedDate = DateTime.UtcNow
            },
            cancellationToken);
        return new PropertyOperationResult<CondominiumUnit>(true, "Unit added successfully.", unit);
    }

    public async Task<PropertyOperationResult<CondominiumUnit>> UpdateCondominiumUnitAsync(
        CondominiumUnit unit,
        string unitNumber,
        int ownerId,
        string? meralcoSin,
        CancellationToken cancellationToken = default)
    {
        unitNumber = unitNumber.Trim();
        meralcoSin = NullIfWhiteSpace(meralcoSin);
        if (ownerId <= 0 || string.IsNullOrWhiteSpace(unitNumber) || unitNumber.Length > 50)
        {
            return new PropertyOperationResult<CondominiumUnit>(false, "Select an owner and enter a unit number of 50 characters or fewer.");
        }
        if (meralcoSin?.Length > 100)
        {
            return new PropertyOperationResult<CondominiumUnit>(false, "Meralco SIN must use 100 characters or fewer.");
        }

        if (!string.Equals(unit.UnitNumber, unitNumber, StringComparison.OrdinalIgnoreCase) &&
            await propertyRepository.ActiveUnitAssignmentExistsAsync(
                unit.CondominiumId,
                unit.UnitNumber,
                cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<CondominiumUnit>(
                false,
                "End the active tenant contract before changing this unit number.");
        }

        if (await propertyRepository.CondominiumUnitExistsAsync(
            unit.CondominiumId,
            unitNumber,
            unit.CondominiumUnitId,
            cancellationToken))
        {
            return new PropertyOperationResult<CondominiumUnit>(false, "That unit number already exists in this condominium.");
        }

        var updated = await propertyRepository.UpdateCondominiumUnitAsync(
            unit.CondominiumUnitId,
            unitNumber,
            ownerId,
            meralcoSin,
            cancellationToken);
        return updated is null
            ? new PropertyOperationResult<CondominiumUnit>(false, "The selected unit no longer exists.")
            : new PropertyOperationResult<CondominiumUnit>(true, "Unit updated successfully.", updated);
    }

    public async Task<PropertyOperationResult<CondominiumUnit>> DeleteCondominiumUnitAsync(
        CondominiumUnit unit,
        CancellationToken cancellationToken = default)
    {
        if (await propertyRepository.ActiveUnitAssignmentExistsAsync(
            unit.CondominiumId,
            unit.UnitNumber,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<CondominiumUnit>(
                false,
                "This unit cannot be deleted while it has an active tenant assignment.");
        }

        var deleted = await propertyRepository.SetCondominiumUnitActiveAsync(
            unit.CondominiumUnitId,
            false,
            cancellationToken);
        return deleted is null
            ? new PropertyOperationResult<CondominiumUnit>(false, "The selected unit no longer exists.")
            : new PropertyOperationResult<CondominiumUnit>(true, "Unit deleted successfully.", deleted);
    }

    public async Task<PropertyOperationResult<Tenant>> AddTenantAsync(
        string fullName,
        string? contactNumber,
        string? emailAddress,
        CancellationToken cancellationToken = default)
    {
        fullName = fullName.Trim();
        contactNumber = NullIfWhiteSpace(contactNumber);
        emailAddress = NullIfWhiteSpace(emailAddress);
        if (string.IsNullOrWhiteSpace(fullName) || fullName.Length > 200)
        {
            return new PropertyOperationResult<Tenant>(
                false,
                "Enter a tenant name of 200 characters or fewer.");
        }

        if (contactNumber?.Length > 100 || emailAddress?.Length > 200)
        {
            return new PropertyOperationResult<Tenant>(
                false,
                "Tenant contact information is too long.");
        }

        var tenant = await propertyRepository.AddTenantAsync(
            new Tenant
            {
                FullName = fullName,
                ContactNumber = contactNumber,
                EmailAddress = emailAddress,
                IsActive = true,
                LastUpdatedDate = DateTime.UtcNow
            },
            cancellationToken);
        return new PropertyOperationResult<Tenant>(
            true,
            "Tenant added successfully.",
            tenant);
    }

    public async Task<PropertyOperationResult<TenantAssignment>> AssignTenantAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        assignment.UnitNumber = assignment.UnitNumber.Trim();
        if (assignment.CondominiumId <= 0 ||
            assignment.TenantId <= 0 ||
            string.IsNullOrWhiteSpace(assignment.UnitNumber))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Select a condominium and tenant, then enter the unit number.");
        }

        if (!await propertyRepository.CondominiumUnitExistsAsync(
            assignment.CondominiumId,
            assignment.UnitNumber,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Select a registered unit from the selected condominium.");
        }

        if (await propertyRepository.ActiveUnitAssignmentExistsAsync(
            assignment.CondominiumId,
            assignment.UnitNumber,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "That condominium unit already has an active tenant assignment.");
        }

        if (await propertyRepository.ActiveTenantAssignmentExistsAsync(
            assignment.TenantId,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "That tenant already has an active condominium assignment.");
        }

        if (assignment.StartOfContract.HasValue &&
            assignment.EndOfContract.HasValue &&
            assignment.EndOfContract < assignment.StartOfContract)
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "The contract end date cannot be earlier than the start date.");
        }

        if (!IsValidAssignmentChoices(assignment))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Select No Dues or With Dues, and choose Days, Weeks or Months for the stay duration.");
        }

        if (!HasValidPaymentMonths(assignment))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Enter a rental amount before entering advance or deposit months. Month values must be between 0 and 120.");
        }

        assignment.IsActive = true;
        assignment.LastUpdatedDate = DateTime.UtcNow;
        var saved = await propertyRepository.AddAssignmentAsync(
            assignment,
            cancellationToken);
        return new PropertyOperationResult<TenantAssignment>(
            true,
            "Tenant assigned successfully.",
            saved);
    }

    public async Task<PropertyOperationResult<TenantAssignment>> UpdateTenantAssignmentAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        assignment.UnitNumber = assignment.UnitNumber.Trim();
        if (assignment.TenantAssignmentId <= 0 ||
            assignment.CondominiumId <= 0 ||
            assignment.TenantId <= 0 ||
            string.IsNullOrWhiteSpace(assignment.UnitNumber))
        {
            return new PropertyOperationResult<TenantAssignment>(false, "Select a condominium, unit and tenant.");
        }

        if (!await propertyRepository.CondominiumUnitExistsAsync(
            assignment.CondominiumId,
            assignment.UnitNumber,
            cancellationToken: cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(false, "Select a registered unit from the selected condominium.");
        }

        if (await propertyRepository.ActiveUnitAssignmentExistsAsync(
            assignment.CondominiumId,
            assignment.UnitNumber,
            assignment.TenantAssignmentId,
            cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(false, "That unit already has another active tenant assignment.");
        }

        if (await propertyRepository.ActiveTenantAssignmentExistsAsync(
            assignment.TenantId,
            assignment.TenantAssignmentId,
            cancellationToken))
        {
            return new PropertyOperationResult<TenantAssignment>(false, "That tenant already has another active assignment.");
        }

        if (assignment.StartOfContract.HasValue &&
            assignment.EndOfContract.HasValue &&
            assignment.EndOfContract < assignment.StartOfContract)
        {
            return new PropertyOperationResult<TenantAssignment>(false, "The contract end date cannot be earlier than the start date.");
        }

        if (!IsValidAssignmentChoices(assignment))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Select No Dues or With Dues, and choose Days, Weeks or Months for the stay duration.");
        }

        if (!HasValidPaymentMonths(assignment))
        {
            return new PropertyOperationResult<TenantAssignment>(
                false,
                "Enter a rental amount before entering advance or deposit months. Month values must be between 0 and 120.");
        }

        assignment.IsActive = true;
        assignment.LastUpdatedDate = DateTime.UtcNow;
        var updated = await propertyRepository.UpdateAssignmentAsync(assignment, cancellationToken);
        return new PropertyOperationResult<TenantAssignment>(true, "Tenant assignment updated successfully.", updated);
    }

    public async Task<PropertyOperationResult<TenantAssignment>> EndTenantAssignmentAsync(
        int tenantAssignmentId,
        CancellationToken cancellationToken = default)
    {
        if (tenantAssignmentId <= 0)
        {
            return new PropertyOperationResult<TenantAssignment>(false, "The tenant assignment is not valid.");
        }

        var ended = await propertyRepository.EndAssignmentAsync(
            tenantAssignmentId,
            DateTime.Today,
            cancellationToken);
        return ended is null
            ? new PropertyOperationResult<TenantAssignment>(false, "The tenant assignment no longer exists.")
            : new PropertyOperationResult<TenantAssignment>(
                true,
                "Contract ended and the unit is now vacant.",
                ended);
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static bool IsValidAssignmentChoices(TenantAssignment assignment)
    {
        var validDues = assignment.WithDues is "No Dues" or "With Dues";
        var validDuration = assignment.StayDuration.HasValue &&
            assignment.StayDuration.Value > 0 &&
            assignment.StayDurationUnit is "Days" or "Weeks" or "Months";
        return validDues && validDuration;
    }

    private static bool HasValidPaymentMonths(TenantAssignment assignment)
    {
        var advanceMonths = assignment.OwnerAdvanceMonths ?? 0;
        var depositMonths = assignment.OwnerDepositMonths ?? 0;
        var validMonths = advanceMonths is >= 0 and <= 120 &&
            depositMonths is >= 0 and <= 120;
        var hasPaymentMonths = advanceMonths > 0 || depositMonths > 0;
        return validMonths &&
            (!hasPaymentMonths || assignment.RentalAmountToOwner is > 0M);
    }
}
