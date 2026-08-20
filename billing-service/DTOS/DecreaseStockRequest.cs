namespace BillingService.DTOs;

public class DecreaseStockRequest
{
    public List<DecreaseStockItemRequest> Items { get; set; } = [];
}

public class DecreaseStockItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}