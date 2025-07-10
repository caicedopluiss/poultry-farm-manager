namespace PoultryFarmManager.Core.Finances.Models;

public class FinancialEntity : DbEntityBase
{
    public string Name { get; set; } = string.Empty;
    public string? ContactPhoneNumber { get; set; }
    public FinancialEntityType Type { get; set; }
}