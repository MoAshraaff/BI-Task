using BITask.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.DTOs;
using ProductService.Models;

namespace ProductService.Controllers;

/// <summary>
/// Plain RESTful CRUD endpoints for products. All endpoints require a valid JWT; mutating
/// endpoints additionally require the Admin role. See <see cref="ProductsODataController"/>
/// for the equivalent OData surface with $filter/$select/$orderby support.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly ProductDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        var products = await _db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        return Ok(products.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        return Ok(ToDto(product));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductDto>> Create(CreateProductRequest request)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            Stock = request.Stock,
            CreatedAt = DateTime.UtcNow
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} created by {User}", product.Id, User.Identity?.Name);

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToDto(product));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateProductRequest request)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} updated by {User}", product.Id, User.Identity?.Name);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product is null)
        {
            return NotFound(new { message = $"Product {id} was not found." });
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted by {User}", id, User.Identity?.Name);

        return NoContent();
    }

    private static ProductDto ToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Category = product.Category,
        Price = product.Price,
        Stock = product.Stock,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt
    };
}
