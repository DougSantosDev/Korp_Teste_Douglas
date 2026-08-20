using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTOs;

public class DecreaseStockBatchRequest
{
    [Required]
    [MinLength(1)]
    public List<DecreaseStockItemRequest> Items { get; set; } = [];
}

public class DecreaseStockItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}