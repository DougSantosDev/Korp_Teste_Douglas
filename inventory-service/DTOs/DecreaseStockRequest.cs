using System.ComponentModel.DataAnnotations;

namespace InventoryService.DTOs;

public class DecreaseStockRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}