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
    public async Task<ActionResult<IEnumerable<Product>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _context.Products
            .OrderBy(product => product.Code)
            .ToListAsync(cancellationToken);

        return Ok(products);
    }

    // GET: /api/products/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

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
        [FromBody] Product product,
        CancellationToken cancellationToken)
    {
        product.Code = product.Code.Trim();
        product.Description = product.Description.Trim();

        if (string.IsNullOrWhiteSpace(product.Code) ||
            string.IsNullOrWhiteSpace(product.Description))
        {
            return BadRequest(new
            {
                message = "Code and description cannot be blank."
            });
        }

        var codeExists = await _context.Products
            .AnyAsync(p => p.Code == product.Code, cancellationToken);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "A product with this code already exists."
            });
        }

        _context.Products.Add(product);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            _context.ChangeTracker.Clear();

            if (await _context.Products
                .AsNoTracking()
                .AnyAsync(
                    candidate => candidate.Code == product.Code,
                    cancellationToken))
            {
                return Conflict(new
                {
                    message = "A product with this code already exists."
                });
            }

            throw;
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product
        );
    }

    // POST: /api/products/decrease-stock
    [HttpPost("decrease-stock")]
    public async Task<ActionResult> DecreaseStockBatch(
        [FromBody] DecreaseStockBatchRequest request,
        CancellationToken cancellationToken)
    {
        request.IdempotencyKey = request.IdempotencyKey.Trim();

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return BadRequest(new
            {
                message = "IdempotencyKey is required."
            });
        }

        List<DecreaseStockItemRequest> items;

        try
        {
            items = request.Items
                .GroupBy(item => item.ProductId)
                .Select(group => new DecreaseStockItemRequest
                {
                    ProductId = group.Key,
                    Quantity = checked(group.Sum(item => item.Quantity))
                })
                .OrderBy(item => item.ProductId)
                .ToList();
        }
        catch (OverflowException)
        {
            return BadRequest(new
            {
                message = "The total quantity for a product is too large."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            if (await _context.StockOperations.AnyAsync(
                operation => operation.IdempotencyKey == request.IdempotencyKey,
                cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);

                return Ok(new
                {
                    message = "Stock had already been updated for this operation.",
                    idempotentReplay = true
                });
            }

            _context.StockOperations.Add(new StockOperation
            {
                IdempotencyKey = request.IdempotencyKey,
                ProcessedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);

            foreach (var item in items)
            {
                var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Products SET StockQuantity = StockQuantity - {item.Quantity} WHERE Id = {item.ProductId} AND StockQuantity >= {item.Quantity}",
                    cancellationToken);

                if (affectedRows == 1)
                {
                    continue;
                }

                var product = await _context.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        candidate => candidate.Id == item.ProductId,
                        cancellationToken);

                await transaction.RollbackAsync(cancellationToken);

                if (product is null)
                {

                    return NotFound(new
                    {
                        message =
                            $"Product {item.ProductId} not found."
                    });
                }

                return Conflict(new
                {
                    message =
                        $"Insufficient stock for product {product.Code}."
                });
            }

            await transaction.CommitAsync(cancellationToken);

            return Ok(new
            {
                message = "Stock updated successfully.",
                idempotentReplay = false
            });
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();

            if (await _context.StockOperations
                .AsNoTracking()
                .AnyAsync(
                    operation => operation.IdempotencyKey == request.IdempotencyKey,
                    cancellationToken))
            {
                return Ok(new
                {
                    message = "Stock had already been updated for this operation.",
                    idempotentReplay = true
                });
            }

            throw;
        }
    }
}
