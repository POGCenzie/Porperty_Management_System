namespace PropertyManagementSystem.Model;

public sealed class StatementOfAccount
{
    public int StatementOfAccountId { get; set; }
    public string SoaNumber { get; set; } = string.Empty;
    public string SoaType { get; set; } = string.Empty;
    public int TenantAssignmentId { get; set; }
    public TenantAssignment TenantAssignment { get; set; } = null!;
    public DateTime StatementDate { get; set; }
    public decimal? RentalAmount { get; set; }
    public decimal? AdvanceAmount { get; set; }
    public decimal? DepositAmount { get; set; }
    public decimal BaseAmount { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetAmount { get; set; }
    public string Status { get; set; } = Constants.SoaStatuses.Filed;
    public string CreatedByUsername { get; set; } = string.Empty;
    public DateTime LastUpdatedDate { get; set; }
    public ICollection<SoaLineItem> LineItems { get; set; } = new List<SoaLineItem>();
}
