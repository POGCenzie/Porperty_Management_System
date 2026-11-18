namespace PropertyManagementSystem.Model;

public sealed class CondominiumUnit
{
    public int CondominiumUnitId { get; set; }
    public int CondominiumId { get; set; }
    public Condominium Condominium { get; set; } = null!;
    public string UnitNumber { get; set; } = string.Empty;
    public string? MeralcoSin { get; set; }
    public int? OwnerId { get; set; }
    public Owner? Owner { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime LastUpdatedDate { get; set; }
}
