using System;

namespace PoultryFarmManager.Core.Models.Finance;

public class Vendor : DbEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }

    public Guid ContactPersonId { get; set; }
    public Person? ContactPerson { get; set; }
}
