using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventManagementApi.Data;
using EventManagementApi.Models.Entities;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Categories>>> GetCategories()
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync();
    }

    // GET: api/Categories/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Categories>> GetCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        return Ok(category);
    }

    // POST: api/Categories
    [HttpPost]
    public async Task<ActionResult<Categories>> CreateCategory(
        Categories category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.CategoryId },
            category);
    }

    // PUT: api/Categories/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCategory(
        int id,
        Categories category)
    {
        if (id != category.CategoryId)
            return BadRequest("Category ID does not match.");

        var existing = await _context.Categories.FindAsync(id);

        if (existing == null)
            return NotFound();

        existing.CategoryName = category.CategoryName;
        existing.Description = category.Description;
        existing.MinimumAge = category.MinimumAge;
        existing.MaximumAge = category.MaximumAge;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Categories/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories.FindAsync(id);

        if (category == null)
            return NotFound();

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}