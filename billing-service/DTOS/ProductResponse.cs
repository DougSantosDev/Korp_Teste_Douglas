namespace BillingService.DTOs;

public class ProductResponse
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int StockQuantity { get; set; }
}