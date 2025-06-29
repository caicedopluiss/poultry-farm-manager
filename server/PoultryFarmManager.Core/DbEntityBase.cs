namespace PoultryFarmManager.Core;

public class DbEntityBase : IDbEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }

    public void UpdateModifiedAt()
    {
        ModifiedAt = DateTime.UtcNow;
    }
}