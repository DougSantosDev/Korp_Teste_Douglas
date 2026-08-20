using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class CreateInvoiceRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required.")]
    public List<CreateInvoiceItemRequest> Items { get; set; } = [];
}

public class CreateInvoiceItemRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "ProductId must be greater than zero.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
    public int Quantity { get; set; }
}