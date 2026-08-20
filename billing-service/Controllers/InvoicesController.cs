using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly BillingDbContext _context;

    public InvoicesController(BillingDbContext context)
    {
        _context = context;
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
}