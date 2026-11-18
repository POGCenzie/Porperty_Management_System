using PropertyManagementSystem.Model;

namespace PropertyManagementSystem.Repository;

public interface IPropertyRepository
{
    Task<IReadOnlyList<Condominium>> GetCondominiumsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tenant>> GetTenantsAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Owner>> GetOwnersAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CondominiumUnit>> GetCondominiumUnitsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TenantAssignment>> GetAssignmentsAsync(
        int? condominiumId = null,
        CancellationToken cancellationToken = default);

    Task<bool> CondominiumNameExistsAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<bool> CondominiumUnitExistsAsync(
        int condominiumId,
        string unitNumber,
        int? excludedUnitId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ActiveUnitAssignmentExistsAsync(
        int condominiumId,
        string unitNumber,
        int? excludedAssignmentId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ActiveTenantAssignmentExistsAsync(
        int tenantId,
        int? excludedAssignmentId = null,
        CancellationToken cancellationToken = default);

    Task<Condominium> AddCondominiumAsync(
        Condominium condominium,
        CancellationToken cancellationToken = default);

    Task<Owner> AddOwnerAsync(
        Owner owner,
        CancellationToken cancellationToken = default);

    Task<Owner?> UpdateOwnerAsync(
        int ownerId,
        string fullName,
        string? emailAddress,
        string? contactNumber,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<Owner?> SetOwnerActiveAsync(
        int ownerId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<CondominiumUnit> AddCondominiumUnitAsync(
        CondominiumUnit unit,
        CancellationToken cancellationToken = default);

    Task<CondominiumUnit?> SetCondominiumUnitActiveAsync(
        int condominiumUnitId,
        bool isActive,
        CancellationToken cancellationToken = default);

    Task<CondominiumUnit?> UpdateCondominiumUnitAsync(
        int condominiumUnitId,
        string unitNumber,
        int ownerId,
        string? meralcoSin,
        CancellationToken cancellationToken = default);

    Task<Tenant> AddTenantAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default);

    Task<TenantAssignment> AddAssignmentAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default);

    Task<TenantAssignment> UpdateAssignmentAsync(
        TenantAssignment assignment,
        CancellationToken cancellationToken = default);

    Task<TenantAssignment?> EndAssignmentAsync(
        int tenantAssignmentId,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StatementOfAccount>> GetStatementsOfAccountAsync(
        CancellationToken cancellationToken = default);

    Task<StatementOfAccount> AddStatementOfAccountAsync(
        StatementOfAccount statement,
        CancellationToken cancellationToken = default);

    Task<AttentionCounts> GetAttentionCountsAsync(
        DateTime today,
        DateTime upcomingDate,
        CancellationToken cancellationToken = default);
}

public sealed record AttentionCounts(
    int ContractsEndingSoon,
    int AirconMaintenanceDue,
    int VacantUnits,
    int TenantsWithoutAssignment);
