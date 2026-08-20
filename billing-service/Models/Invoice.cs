namespace BillingService.Models;

public class Invoice
{
    public int Id { get; set; }

    public int Number { get; set; }

    public InvoiceStatus Status { get; set; } = InvoiceStatus.Open;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<InvoiceItem> Items { get; set; } = [];
}