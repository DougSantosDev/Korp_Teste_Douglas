using InventoryService.Data;
using InventoryService.DTOs;
using InventoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly InventoryDbContext _context;

    public ProductsController(InventoryDbContext context)
    {
        _context = context;
    }

    // GET: /api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAll()
    {
        var products = await _context.Products
            .ToListAsync();

        return Ok(products);
    }

    // GET: /api/products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(int id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        return Ok(product);
    }

    // POST: /api/products
    [HttpPost]
    public async Task<ActionResult<Product>> Create(
        [FromBody] Product product)
    {
        var codeExists = await _context.Products
            .AnyAsync(p => p.Code == product.Code);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "A product with this code already exists."
            });
        }

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        );
    }

    // POST: /api/products/decrease-stock
    [HttpPost("decrease-stock")]
    public async Task<ActionResult> DecreaseStockBatch(
        [FromBody] DecreaseStockBatchRequest request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in request.Items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == item.ProductId
                    );

                if (product is null)
                {
                    await transaction.RollbackAsync();

                    return NotFound(new
                    {
                        message =
                            $"Product {item.ProductId} not found."
                    });
                }

                if (product.StockQuantity < item.Quantity)
                {
                    await transaction.RollbackAsync();

                    return Conflict(new
                    {
                        message =
                            $"Insufficient stock for product {product.Code}."
                    });
                }

                product.StockQuantity -= item.Quantity;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                message = "Stock updated successfully."
            });
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}