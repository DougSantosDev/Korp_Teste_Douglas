using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product code is required.")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required.")]
    [MaxLength(150)]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
    public int StockQuantity { get; set; }
}