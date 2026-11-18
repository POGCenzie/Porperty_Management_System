namespace PropertyManagementSystem.Model;

public sealed class Condominium
{
    public int CondominiumId { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime LastUpdatedDate { get; set; }

    public ICollection<TenantAssignment> TenantAssignments { get; set; } =
        new List<TenantAssignment>();

    public ICollection<CondominiumUnit> Units { get; set; } =
        new List<CondominiumUnit>();
}
