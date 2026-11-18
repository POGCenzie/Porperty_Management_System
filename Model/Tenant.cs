namespace PropertyManagementSystem.Model;

public sealed class Tenant
{
    public int TenantId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? ContactNumber { get; set; }

    public string? EmailAddress { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime LastUpdatedDate { get; set; }

    public ICollection<TenantAssignment> Assignments { get; set; } =
        new List<TenantAssignment>();
}
