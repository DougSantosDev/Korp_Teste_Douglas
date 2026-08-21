namespace BillingService.DTOs;

public class DecreaseStockRequest
{
    public string IdempotencyKey { get; set; } = string.Empty;

    public List<DecreaseStockItemRequest> Items { get; set; } = [];
}

public class DecreaseStockItemRequest
{
    public int ProductId { get; set; }

    public int Quantity { get; set; }
}
