namespace PoultryFarmManager.Core.Enums
{
    public enum AssetStatus : byte
    {
        Available = 0,
        InUse = 1,
        Damaged = 2,
        UnderMaintenance = 3,
        Obsolete = 4,
        Disposed = 5,
        Sold = 6,
        Leased = 7,
        Lost = 8,
    }
}
