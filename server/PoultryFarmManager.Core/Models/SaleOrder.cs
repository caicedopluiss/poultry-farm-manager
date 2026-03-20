using System;
using System.Collections.Generic;
using System.Linq;
using PoultryFarmManager.Core.Enums;
using PoultryFarmManager.Core.Models.Finance;

namespace PoultryFarmManager.Core.Models;

public class SaleOrder : DbEntity
{
    public Guid BatchId { get; set; }
    public Batch? Batch { get; set; }

    public Guid CustomerId { get; set; }
    public Person? Customer { get; set; }

    public DateTime Date { get; set; }
    public SaleOrderStatus Status { get; set; } = SaleOrderStatus.Pending;
    public decimal PricePerKg { get; set; }
    public string? Notes { get; set; }

    public ICollection<SaleOrderItem> Items { get; set; } = [];
    public ICollection<Transaction> Payments { get; set; } = [];

    // Computed properties
    public decimal TotalAmount => Items.Sum(i => i.Weight) * PricePerKg;
    public decimal TotalPaid => Payments.Where(p => p.Type == TransactionType.Income).Sum(p => p.TransactionAmount);
    public decimal PendingAmount => TotalAmount - TotalPaid;
}
