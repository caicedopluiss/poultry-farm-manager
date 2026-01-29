using System;
using PoultryFarmManager.Core.Models.Inventory;

namespace PoultryFarmManager.Core.Models.Finance;

public class Transaction : DbEntity
{
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType Type { get; set; }
    public decimal UnitPrice { get; set; }
    public int? Quantity { get; set; } // null if it's an expense without quantity
    public decimal TotalAmount => (Quantity ?? 1) * UnitPrice;
    public decimal TransactionAmount { get; set; } // Final recorded amount (may include taxes, discounts, etc.)

    // Relationship with Product
    public Guid? ProductVariantId { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    // Relationship with batch (if applicable)
    public Guid? BatchId { get; set; }
    public Batch? Batch { get; set; }

    // Relationship with counterpart
    public Guid? VendorId { get; set; }
    public Vendor? Vendor { get; set; }

    public Guid? CustomerId { get; set; }
    public Person? Customer { get; set; }

    // Free field for description
    public string? Notes { get; set; } = string.Empty;
}
