using Microsoft.EntityFrameworkCore;
using PropertyManagementSystem.Data;
using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Repository;

public sealed class PropertyRepository : IPropertyRepository
{
    private readonly IDbContextFactory<AppDbContext> contextFactory;

    public PropertyRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        this.contextFactory = contextFactory;
    }

    public async Task<IReadOnlyList<Condominium>> GetCondominiumsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.Condominiums
            .AsNoTracking()
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.Tenants
            .AsNoTracking()
            .OrderBy(item => item.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Owner>> GetOwnersAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.Owners
            .AsNoTracking()
            .OrderBy(item => item.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CondominiumUnit>> GetCondominiumUnitsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = database.CondominiumUnits
            .AsNoTracking()
            .Include(item => item.Condominium)
            .Include(item => item.Owner)
            .AsQueryable();
        if (condominiumId.HasValue)
        {
            query = query.Where(item => item.CondominiumId == condominiumId.Value);
        }

        return await query
            .OrderBy(item => item.Condominium.Name)
            .ThenBy(item => item.UnitNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TenantAssignment>> GetAssignmentsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = database.TenantAssignments
            .AsNoTracking()
            .Include(item => item.Condominium)
            .Include(item => item.Tenant)
            .AsQueryable();
        if (condominiumId.HasValue)
        {
            query = query.Where(item => item.CondominiumId == condominiumId.Value);
        }

        return await query
            .OrderBy(item => item.Condominium.Name)
            .ThenBy(item => item.UnitNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CondominiumNameExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.Condominiums.AnyAsync(
            item => item.Name == name,
            cancellationToken);
    }

    public async Task<bool> CondominiumUnitExistsAsync(
        int condominiumId,
        string unitNumber,
        int? excludedUnitId = null,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.CondominiumUnits.AnyAsync(
            item => item.CondominiumId == condominiumId &&
                item.UnitNumber == unitNumber &&
                (!excludedUnitId.HasValue || item.CondominiumUnitId != excludedUnitId.Value),
            cancellationToken);
    }

    public async Task<bool> ActiveUnitAssignmentExistsAsync(
        int condominiumId,
        string unitNumber,
        int? excludedAssignmentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.TenantAssignments.AnyAsync(
            item => item.CondominiumId == condominiumId &&
                item.UnitNumber == unitNumber &&
                item.IsActive &&
                (!excludedAssignmentId.HasValue ||
                    item.TenantAssignmentId != excludedAssignmentId.Value),
            cancellationToken);
    }

    public async Task<bool> ActiveTenantAssignmentExistsAsync(
        int tenantId,
        int? excludedAssignmentId = null,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.TenantAssignments.AnyAsync(
            item => item.TenantId == tenantId &&
                item.IsActive &&
                (!excludedAssignmentId.HasValue ||
                    item.TenantAssignmentId != excludedAssignmentId.Value),
            cancellationToken);
    }

    public async Task<Condominium> AddCondominiumAsync(
        Condominium condominium,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.Condominiums.Add(condominium);
        await database.SaveChangesAsync(cancellationToken);
        return condominium;
    }

    public async Task<Owner> AddOwnerAsync(
        Owner owner,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.Owners.Add(owner);
        await database.SaveChangesAsync(cancellationToken);
        return owner;
    }

    public async Task<Owner?> UpdateOwnerAsync(
        int ownerId,
        string fullName,
        string? emailAddress,
        string? contactNumber,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var owner = await database.Owners.FirstOrDefaultAsync(
            item => item.OwnerId == ownerId,
            cancellationToken);
        if (owner is null)
        {
            return null;
        }

        owner.FullName = fullName;
        owner.EmailAddress = emailAddress;
        owner.ContactNumber = contactNumber;
        owner.IsActive = isActive;
        owner.LastUpdatedDate = DateTime.UtcNow;

        var activeAssignments = await database.TenantAssignments
            .Where(assignment => assignment.IsActive &&
                database.CondominiumUnits.Any(unit =>
                    unit.OwnerId == ownerId &&
                    unit.CondominiumId == assignment.CondominiumId &&
                    unit.UnitNumber == assignment.UnitNumber))
            .ToListAsync(cancellationToken);
        foreach (var assignment in activeAssignments)
        {
            assignment.OwnerName = fullName;
            assignment.OwnerEmailAddress = emailAddress;
            assignment.OwnerContactNumber = contactNumber;
            assignment.LastUpdatedDate = DateTime.UtcNow;
        }

        await database.SaveChangesAsync(cancellationToken);
        return owner;
    }

    public async Task<Owner?> SetOwnerActiveAsync(
        int ownerId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var owner = await database.Owners.FirstOrDefaultAsync(
            item => item.OwnerId == ownerId,
            cancellationToken);
        if (owner is null)
        {
            return null;
        }

        owner.IsActive = isActive;
        owner.LastUpdatedDate = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return owner;
    }

    public async Task<CondominiumUnit> AddCondominiumUnitAsync(
        CondominiumUnit unit,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.CondominiumUnits.Add(unit);
        await database.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<CondominiumUnit?> SetCondominiumUnitActiveAsync(
        int condominiumUnitId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var unit = await database.CondominiumUnits.FirstOrDefaultAsync(
            item => item.CondominiumUnitId == condominiumUnitId,
            cancellationToken);
        if (unit is null)
        {
            return null;
        }

        unit.IsActive = isActive;
        unit.LastUpdatedDate = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<CondominiumUnit?> UpdateCondominiumUnitAsync(
        int condominiumUnitId,
        string unitNumber,
        int ownerId,
        string? meralcoSin,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var unit = await database.CondominiumUnits.FirstOrDefaultAsync(
            item => item.CondominiumUnitId == condominiumUnitId,
            cancellationToken);
        if (unit is null)
        {
            return null;
        }

        var owner = await database.Owners.FirstOrDefaultAsync(
            item => item.OwnerId == ownerId && item.IsActive,
            cancellationToken);
        if (owner is null)
        {
            return null;
        }

        var previousUnitNumber = unit.UnitNumber;
        unit.UnitNumber = unitNumber;
        unit.OwnerId = ownerId;
        unit.MeralcoSin = meralcoSin;
        unit.LastUpdatedDate = DateTime.UtcNow;
        var activeAssignments = await database.TenantAssignments
            .Where(item => item.IsActive &&
                item.CondominiumId == unit.CondominiumId &&
                item.UnitNumber == previousUnitNumber)
            .ToListAsync(cancellationToken);
        foreach (var assignment in activeAssignments)
        {
            assignment.UnitNumber = unitNumber;
            assignment.OwnerName = owner.FullName;
            assignment.OwnerEmailAddress = owner.EmailAddress;
            assignment.OwnerContactNumber = owner.ContactNumber;
            assignment.MeralcoSin = meralcoSin;
            assignment.LastUpdatedDate = DateTime.UtcNow;
        }
        await database.SaveChangesAsync(cancellationToken);
        return unit;
    }

    public async Task<Tenant> AddTenantAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.Tenants.Add(tenant);
        await database.SaveChangesAsync(cancellationToken);
        return tenant;
    }

    public async Task<TenantAssignment> AddAssignmentAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.TenantAssignments.Add(assignment);
        await database.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<TenantAssignment> UpdateAssignmentAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.Entry(assignment).State = EntityState.Modified;
        await database.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<TenantAssignment?> EndAssignmentAsync(
        int tenantAssignmentId,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var assignment = await database.TenantAssignments.FirstOrDefaultAsync(
            item => item.TenantAssignmentId == tenantAssignmentId,
            cancellationToken);
        if (assignment is null)
        {
            return null;
        }

        assignment.EndOfContract = endDate.Date;
        assignment.IsActive = false;
        assignment.LastUpdatedDate = DateTime.UtcNow;
        await database.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<IReadOnlyList<StatementOfAccount>> GetStatementsOfAccountAsync(
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.StatementsOfAccount
            .AsNoTracking()
            .Include(item => item.TenantAssignment)
                .ThenInclude(item => item.Condominium)
            .Include(item => item.TenantAssignment)
                .ThenInclude(item => item.Tenant)
            .Include(item => item.LineItems)
            .OrderByDescending(item => item.StatementDate)
            .ThenByDescending(item => item.StatementOfAccountId)
            .ToListAsync(cancellationToken);
    }

    public async Task<StatementOfAccount> AddStatementOfAccountAsync(
        StatementOfAccount statement,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        database.StatementsOfAccount.Add(statement);
        await database.SaveChangesAsync(cancellationToken);
        return statement;
    }

    public async Task<AttentionCounts> GetAttentionCountsAsync(
        DateTime today,
        DateTime upcomingDate,
        CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var activeAssignments = database.TenantAssignments.Where(item => item.IsActive);
        var contractsEndingSoon = await activeAssignments.CountAsync(
            item => item.EndOfContract.HasValue &&
                item.EndOfContract.Value >= today &&
                item.EndOfContract.Value <= upcomingDate,
            cancellationToken);
        var airconMaintenanceDue = await activeAssignments.CountAsync(
            item => item.AirconMaintenanceDate.HasValue &&
                item.AirconMaintenanceDate.Value <= upcomingDate,
            cancellationToken);
        var vacantUnits = await database.CondominiumUnits.CountAsync(
            unit => unit.IsActive &&
                !database.TenantAssignments.Any(assignment =>
                    assignment.IsActive &&
                    assignment.CondominiumId == unit.CondominiumId &&
                    assignment.UnitNumber == unit.UnitNumber),
            cancellationToken);
        var tenantsWithoutAssignment = await database.Tenants.CountAsync(
            tenant => tenant.IsActive &&
                !tenant.Assignments.Any(assignment => assignment.IsActive),
            cancellationToken);
        return new AttentionCounts(
            contractsEndingSoon,
            airconMaintenanceDue,
            vacantUnits,
            tenantsWithoutAssignment);
    }
}
