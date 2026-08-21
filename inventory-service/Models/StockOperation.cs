using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models;

public class StockOperation
{
    [Key]
    [MaxLength(100)]
    public string IdempotencyKey { get; set; } = string.Empty;

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
