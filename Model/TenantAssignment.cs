namespace PropertyManagementSystem.Model;

public sealed class TenantAssignment
{
    public int TenantAssignmentId { get; set; }

    public int CondominiumId { get; set; }

    public Condominium Condominium { get; set; } = null!;

    public int TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public string UnitNumber { get; set; } = string.Empty;

    public string? OwnerName { get; set; }

    public string OccupancyStatus { get; set; } = "RENTED";

    public string? MeralcoSin { get; set; }

    public int? KeyRelease { get; set; }

    public int? AccessCardRelease { get; set; }

    public decimal? RentalAmountToOwner { get; set; }

    public string? WithDues { get; set; }

    public int? StayDuration { get; set; }

    public string? StayDurationUnit { get; set; }

    public DateTime? StartOfContract { get; set; }

    public DateTime? EndOfContract { get; set; }

    public DateTime? AirconMaintenanceDate { get; set; }

    public string? MaintenanceStatus { get; set; }

    public int? OwnerAdvanceMonths { get; set; }

    public int? OwnerDepositMonths { get; set; }

    public string? OwnerEmailAddress { get; set; }

    public string? OwnerContactNumber { get; set; }

    public string? MainTenantContactNumber { get; set; }

    public string? MainTenantEmailAddress { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime LastUpdatedDate { get; set; }
}
