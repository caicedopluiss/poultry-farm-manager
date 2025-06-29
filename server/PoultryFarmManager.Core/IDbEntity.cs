namespace PoultryFarmManager.Core;

public interface IDbEntity
{
    Guid Id { get; set; }
    DateTime CreatedAt { get; set; }
    DateTime? ModifiedAt { get; set; }
}