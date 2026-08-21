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
    public async Task<ActionResult<IEnumerable<Invoice>>> GetAll(
        CancellationToken cancellationToken)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Items)
            .OrderBy(i => i.Number)
            .ToListAsync(cancellationToken);

        return Ok(invoices);
    }

    // GET: /api/invoices/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Invoice>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

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
        [FromBody] CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        List<CreateInvoiceItemRequest> items;

        try
        {
            items = request.Items
                .GroupBy(item => item.ProductId)
                .Select(group => new CreateInvoiceItemRequest
                {
                    ProductId = group.Key,
                    Quantity = checked(group.Sum(item => item.Quantity))
                })
                .ToList();
        }
        catch (OverflowException)
        {
            return BadRequest(new
            {
                message = "The total quantity for a product is too large."
            });
        }

        try
        {
            foreach (var item in items)
            {
                var product = await _inventoryServiceClient
                    .GetProductByIdAsync(item.ProductId, cancellationToken);

                if (product is null)
                {
                    return BadRequest(new
                    {
                        message = $"Product {item.ProductId} does not exist."
                    });
                }
            }
        }
        catch (HttpRequestException)
        {
            return InventoryUnavailable();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return InventoryUnavailable();
        }

        await using var transaction = await _context.Database
            .BeginTransactionAsync(cancellationToken);

        var sequence = await _context.InvoiceSequences
            .FromSqlRaw("SELECT * FROM InvoiceSequences WHERE Id = 1 FOR UPDATE")
            .SingleAsync(cancellationToken);

        sequence.LastNumber++;

        var invoice = new Invoice
        {
            Number = sequence.LastNumber,
            Status = InvoiceStatus.Open,
            CreatedAt = DateTime.UtcNow,
            Items = items
                .Select(item => new InvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        _context.Invoices.Add(invoice);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = invoice.Id },
            invoice
        );
    }

    [HttpPost("{id:int}/print")]
    public async Task<ActionResult> PrintInvoice(
        int id,
        CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

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
            IdempotencyKey = $"billing-invoice-{invoice.Id}",
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
            using var stockResponse = await _inventoryServiceClient
                .DecreaseStockAsync(stockRequest, cancellationToken);

            if (!stockResponse.IsSuccessStatusCode)
            {
                var errorMessage = await stockResponse.Content
                    .ReadAsStringAsync(cancellationToken);

                return StatusCode(
                    (int)stockResponse.StatusCode,
                    new
                    {
                        message = "Could not update inventory.",
                        details = errorMessage
                    }
                );
            }

            var updatedRows = await _context.Invoices
                .Where(candidate =>
                    candidate.Id == invoice.Id &&
                    candidate.Status == InvoiceStatus.Open)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(
                        candidate => candidate.Status,
                        InvoiceStatus.Closed),
                    cancellationToken);

            if (updatedRows == 0)
            {
                return Conflict(new
                {
                    message = "The invoice was already closed."
                });
            }

            invoice.Status = InvoiceStatus.Closed;

            return Ok(new
            {
                message = "Invoice printed successfully.",
                invoice
            });
        }
        catch (HttpRequestException)
        {
            return InventoryUnavailable();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return InventoryUnavailable();
        }
    }

    private ObjectResult InventoryUnavailable()
    {
        return StatusCode(
            StatusCodes.Status503ServiceUnavailable,
            new
            {
                message = "Inventory service is temporarily unavailable. Try again shortly."
            });
    }
}
