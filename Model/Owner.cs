namespace PropertyManagementSystem.Model;

public sealed class Owner
{
    public int OwnerId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? EmailAddress { get; set; }
    public string? ContactNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastUpdatedDate { get; set; }
    public ICollection<CondominiumUnit> Units { get; set; } = new List<CondominiumUnit>();
}
