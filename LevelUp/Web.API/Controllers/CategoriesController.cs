using DataAccessLayer;
using DataAccessLayer.Models;
using DataAccessLayer.UoW;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;

namespace Web.API.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly MyDbContext _ctx;
    
    public CategoriesController(MyDbContext ctx)
    {
        _ctx = ctx;
    }
    
    [HttpGet("allSimple")]
    public async Task<IEnumerable<Category>> GetAllCategories()
    {       
        return await _ctx.Categories.ToArrayAsync();
    }
    
    [HttpGet("allAsSplitQuery")]
    public IEnumerable<Category> GetAllAsSplitQuery()
    {       
        return _ctx.Categories
            .Include(x=>x.Products)
            .AsSplitQuery()
            .ToArray();
    }
    
        
    [HttpGet("allAsNoTracking")]
    public IEnumerable<Category> GetAllAsNoTracking()
    {       
        return _ctx.Categories
            .Include(x=>x.Products)
            .AsNoTracking()
            .ToArray();
    }
    
    [HttpGet("allAsSplitAsNoTracking")]
    public IEnumerable<Category> GetAllAsSplitAsNoTracking()
    {       
        return _ctx.Categories
            .Include(x=>x.Products)
            .AsNoTracking()
            .AsSplitQuery()
            .ToArray();
    }
    
    [HttpGet("ienumerable/{id}")]
    public Category? GetByIdIEnumerable(Guid id)
    {       
        return _ctx.Categories.ToList().FirstOrDefault(x => x.Id == id);
    }
    
    [HttpGet("iqueryable/{id}")]
    public Category? GetByIdIQueryable(Guid id)
    {       
        return _ctx.Categories.Where(x => x.Id == id).FirstOrDefault();
    }
    
    [HttpGet("names")]
    public IEnumerable<object> GetNames()
    {
        return _ctx.Categories
                    .Select(x => new { x.Name })
                    .ToList();
    }
    
    [Authorize()]
    [HttpPost]
    public IActionResult CreateCategory([FromBody] Category category)
    {
        //Detached → Added → Unchanged
        
        // Initially, this newCategory is not being tracked by EF Core.
        Console.WriteLine($"Category {category.Name} (before Add): {_ctx.Entry(category).State}"); 
        // Output: Detached

        // Mark it as to be added to the database
        _ctx.Categories.Add(category);
        Console.WriteLine($"Category {category.Name} (after Add): {_ctx.Entry(category).State}");
        // Output: Added

        // Once we call SaveChanges, EF will insert the record and mark it as Unchanged
        _ctx.SaveChanges();
        Console.WriteLine($"Category {category.Name} (after SaveChanges): {_ctx.Entry(category).State}");
        // Output: Unchanged
        
        return CreatedAtAction(nameof(CreateCategory), new { id = category.Id }, category);
    }  
    
    [HttpPut("{id}")]
    public IActionResult ModifyCategory(Guid id, [FromBody] Category category)
    {
        //Unchanged → Modified → Unchanged
        
        // Retrieve entity 
        var entity = _ctx.Categories.Where(x => x.Id == id).FirstOrDefault();
        Console.WriteLine($"entity (after retrieval): {_ctx.Entry(entity).State}");
        
        // Change a property on the existing category
        entity.Name = category.Name;
        Console.WriteLine($"entity (after property change): {_ctx.Entry(entity).State}");
        // Output: Modified

        // Once we save changes, EF will apply the update to the DB, and state becomes Unchanged
        _ctx.SaveChanges();
        Console.WriteLine($"entity (after SaveChanges on modification): {_ctx.Entry(entity).State}");
        // Output: Unchanged

        
        return CreatedAtAction(nameof(ModifyCategory), new { id = entity.Id }, entity);
    }
    
    [HttpDelete("{id}")]
    public IActionResult DeleteCategory(Guid id)
    {
        //Unchanged → Deleted → Detached
        
        // Retrieve entity 
        var entity = _ctx.Categories.Where(x => x.Id == id).FirstOrDefault();
        Console.WriteLine($"Entity (after retrieval): {_ctx.Entry(entity).State}");
        
        _ctx.Categories.Remove(entity);
        Console.WriteLine($"Entity (after Remove): {_ctx.Entry(entity).State}");
        // Output: Deleted

        // Once we save changes, it will be removed from the DB, 
        // but the in-memory tracker will change the state to Detached AFTER the deletion.
        _ctx.SaveChanges();
        Console.WriteLine($"Entity (after SaveChanges on removal): {_ctx.Entry(entity).State}");
        // Output: Detached (in most cases after the entity is fully removed from the context)
        
        return CreatedAtAction(nameof(DeleteCategory), new { id = entity.Id });
    }
    
    [HttpPost("attachDemo")]
    public IActionResult AttachDemo()
    {
        var id = Guid.NewGuid();
        var category = new Category()
        {
            Id = id,
            Name = "Category"+id,
            Products = new List<Product>()
        };
        
        // The context doesn’t know about anotherCategory yet
        Console.WriteLine($"Entity (initially): {_ctx.Entry(category).State}");
        // Output: Detached

        // If we want EF to track it as an existing entity without marking it as changed, we use Attach:
        _ctx.Categories.Attach(category);
        Console.WriteLine($"Entity (after Attach): {_ctx.Entry(category).State}");
        // Output: Unchanged (assuming EF tries to match the primary key but doesn't find it in DB, it might remain Unchanged in memory)

        // If we know it’s new, we can mark it as Added manually:
        _ctx.Entry(category).State = EntityState.Added;
        Console.WriteLine($"Entity (set to Added): {_ctx.Entry(category).State}");
        // Output: Added

        // Save to commit
        _ctx.SaveChanges();
        Console.WriteLine($"Entity (after SaveChanges): {_ctx.Entry(category).State}");
        // Output: Unchanged
       
        
        return CreatedAtAction(nameof(AttachDemo), new { id = category.Id }, category);
    }


    [HttpPost("unitOfWorkDemo")]
    public async Task<IActionResult> UnitOfWorkDemo()
    {
    
        var category = new Category { Name = "UoWDemoCategory" };
        var product1 = new Product 
        { 
            Name = "UoWDemoProduct1", 
            Description = "Something heavy processed or fetched from an API", 
            Price = 11m,
            Category = category
        };

        var product2 = new Product
        {
            Name = "UoWDemoProduct2", 
            Description = "Another product from external system",
            Price = 13m,
            Category = category
        };
    
        var reviewForProduct1 = new Review
        {
            Content = "Some validated review content",
            Product = product1
        };
        
        using var uow = new UnitOfWork(_ctx);
        try
        {
            await uow.BeginTransactionAsync();
            
            uow.Context.Categories.Add(category);
            uow.Context.Products.AddRange(product1, product2);
            uow.Context.Reviews.Add(reviewForProduct1);
            
            await uow.SaveChangesAsync();
            await uow.CommitTransactionAsync();
        }
        catch
        {
            await uow.RollbackTransactionAsync();
            return Problem();
        }

        return Ok("Success");
    }
}