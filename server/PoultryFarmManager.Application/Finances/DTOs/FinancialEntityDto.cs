using System;
using PoultryFarmManager.Core.Finances;
using PoultryFarmManager.Core.Finances.Models;

namespace PoultryFarmManager.Application.Finances.DTOs;

public record NewFinancialEntityDto
{
    public string Name { get; set; } = string.Empty;
    public string ContactPhoneNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;

    public virtual FinancialEntity ToCoreModel() => new()
    {
        Name = Name,
        ContactPhoneNumber = ContactPhoneNumber,
        Type = Enum.Parse<FinancialEntityType>(Type),
    };
}

public record FinancialEntityDto : NewFinancialEntityDto
{
    public Guid Id { get; set; } = Guid.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }

    public static FinancialEntityDto FromCore(FinancialEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        ContactPhoneNumber = entity.ContactPhoneNumber ?? string.Empty,
        Type = entity.Type.ToString(),
        CreatedAt = entity.CreatedAt,
        ModifiedAt = entity.ModifiedAt,
    };
}