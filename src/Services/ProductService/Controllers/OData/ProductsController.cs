using BITask.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers.OData;

/// <summary>
/// OData v4 endpoint for products, mounted at /odata/Products. Supports $filter, $select,
/// $orderby, $top, $skip, $count and $expand out of the box via [EnableQuery]. The entity
/// set name ("Products") is what OData's routing convention matches this controller against,
/// independent of the plain REST <see cref="ProductService.Controllers.ProductsController"/>.
/// </summary>
[Authorize]
[ApiExplorerSettings(IgnoreApi = true)]
public class ProductsController : ODataController
{
    private readonly ProductDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [EnableQuery(PageSize = 50)]
    public IQueryable<Product> Get() => _db.Products.AsNoTracking();

    [EnableQuery]
    public SingleResult<Product> Get(int key)
    {
        var result = _db.Products.AsNoTracking().Where(p => p.Id == key);
        return SingleResult.Create(result);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Post([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        product.Id = 0;
        product.CreatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} created via OData by {User}", product.Id, User.Identity?.Name);

        return Created(product);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Patch(int key, [FromBody] Delta<Product> delta)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == key);
        if (product is null)
        {
            return NotFound();
        }

        delta.Patch(product);
        product.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} patched via OData by {User}", key, User.Identity?.Name);

        return Updated(product);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Put(int key, [FromBody] Product update)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == key);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = update.Name;
        product.Description = update.Description;
        product.Category = update.Category;
        product.Price = update.Price;
        product.Stock = update.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} replaced via OData by {User}", key, User.Identity?.Name);

        return Updated(product);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(int key)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == key);
        if (product is null)
        {
            return NotFound();
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Product {ProductId} deleted via OData by {User}", key, User.Identity?.Name);

        return NoContent();
    }
}
