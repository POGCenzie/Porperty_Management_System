namespace PropertyManagementSystem.Model;

public sealed class SoaLineItem
{
    public int SoaLineItemId { get; set; }
    public int StatementOfAccountId { get; set; }
    public StatementOfAccount StatementOfAccount { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Remarks { get; set; }
    public int SortOrder { get; set; }
}
