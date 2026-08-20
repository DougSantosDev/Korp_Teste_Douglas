using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using BillingService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly BillingDbContext _context;
    private readonly InventoryServiceClient _inventoryServiceClient;

    public InvoicesController(
        BillingDbContext context,
        InventoryServiceClient inventoryServiceClient)
    {
        _context = context;
        _inventoryServiceClient = inventoryServiceClient;
    }

    // GET: /api/invoices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAll()
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .OrderBy(i => i.Number)
            .ToListAsync();

        return Ok(invoices);
    }

    // GET: /api/invoices/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Invoice>> GetById(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return NotFound(new
            {
                message = "Invoice not found."
            });
        }

        return Ok(invoice);
    }

    // POST: /api/invoices
    [HttpPost]
    public async Task<ActionResult<Invoice>> Create(
        [FromBody] CreateInvoiceRequest request)
    {
        foreach (var item in request.Items)
        {
            var product = await _inventoryServiceClient
                .GetProductByIdAsync(item.ProductId);

            if (product is null)
            {
                return BadRequest(new
                {
                    message = $"Product {item.ProductId} does not exist."
                });
            }
        }

        var lastNumber = await _context.Invoices
            .MaxAsync(i => (int?)i.Number) ?? 0;

        var invoice = new Invoice
        {
            Number = lastNumber + 1,
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow,
            Items = request.Items
                .Select(item => new InvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice
        );
    }

    [HttpPost("{id:int}/print")]
    public async Task<ActionResult> PrintInvoice(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return NotFound(new
            {
                message = "Invoice not found."
            });
        }

        if (invoice.Status != InvoiceStatus.Open)
        {
            return Conflict(new
            {
                message = "Only open invoices can be printed."
            });
        }

        var stockRequest = new DecreaseStockRequest
        {
            Items = invoice.Items
                .Select(item => new DecreaseStockItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        try
        {
            var stockResponse = await _inventoryServiceClient
                .DecreaseStockAsync(stockRequest);

            if (!stockResponse.IsSuccessStatusCode)
            {
                var errorMessage = await stockResponse.Content.ReadAsStringAsync();

                return StatusCode(
                    (int)stockResponse.StatusCode,
                    new
                    {
                        message = "Could not update inventory.",
                        details = errorMessage
                    }
                );
            }

            invoice.Status = InvoiceStatus.Closed;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Invoice printed successfully.",
                invoice
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    message = "Inventory service is temporarily unavailable."
                }
            );
        }
    }
}