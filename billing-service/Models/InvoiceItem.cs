using System.Text.Json.Serialization;

namespace BillingService.Models;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    [JsonIgnore]
    public Invoice Invoice { get; set; } = null!;
}