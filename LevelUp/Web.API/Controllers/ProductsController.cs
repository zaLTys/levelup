using DataAccessLayer;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly MyDbContext _ctx;
    
    public ProductsController(MyDbContext ctx)
    {
        _ctx = ctx;
    }
    
    [HttpGet("allSimple")]
    public async Task<Product[]> GetAllProducts()
    {
            return await _ctx.Products
                .Include(x=>x.Category)
                .ToArrayAsync();
    }
    
    [HttpGet("allAsSplitQuery")]
    public Task<Product[]> GetAllAsSplitQuery()
    {       
        return _ctx.Products
            .Include(x=>x.Category)
            .AsSplitQuery()
            .ToArrayAsync();
    }
    
        
    [HttpGet("allAsNoTracking")]
    public Task<Product[]> GetAllAsNoTracking()
    {       
        return _ctx.Products
            .Include(x=>x.Category)
            .AsNoTracking()
            .ToArrayAsync();
    }
    
    [HttpGet("allAsSplitAsNoTracking")]
    public IEnumerable<Product> GetAllAsSplitAsNoTracking()
    {       
        return _ctx.Products
            .Include(x=>x.Category)
            .AsNoTracking()
            .AsSplitQuery()
            .ToArray();
    }
    
    [HttpGet("ienumerable/{id}")]
    public Product? GetByIdIEnumerable(Guid id)
    {       
        return _ctx.Products.ToList().FirstOrDefault(x => x.Id == id);
    }
    
    [HttpGet("iqueryable/{id}")]
    public Product? GetByIdIQueryable(Guid id)
    {       
        return _ctx.Products.Where(x => x.Id == id).FirstOrDefault();
    }
    
    [HttpGet("names")]
    public IEnumerable<object> GetNames()
    {
        return _ctx.Products
            .Select(x => new { x.Name })
            .ToList();
    }
    
    [HttpPost]
    public IActionResult CreateProduct([FromBody] Product product)
    {
        var category = _ctx.Categories.FirstOrDefault(c => c.Id == product.CategoryId);
        if (category == null)
        {
            return NotFound("Category not found.");
        }

        product.Category = category;
        _ctx.Products.Add(product);
        _ctx.SaveChanges();

        return CreatedAtAction(nameof(GetAllProducts), new { id = product.Id }, product);
    }
}