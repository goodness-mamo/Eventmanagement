
using EventManagementApi.Data;
using EventManagementApi.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Participant")]
public class EventCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public EventCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/EventCategories/event/1
    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<IEnumerable<EventCategoryResponse>>>
        GetCategoriesForEvent(int eventId)
    {
        // Check whether the event exists
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        // Retrieve categories linked to this event
        var categories = await _context.EventCategories
            .AsNoTracking()
            .Where(ec => ec.EventId == eventId)
            .Select(ec => new EventCategoryResponse
            {
                EventId = ec.EventId,
                CategoryId = ec.CategoryId,

                CategoryName = ec.Category.CategoryName,
                Description = ec.Category.Description,

                MinimumAge = ec.Category.MinimumAge,
                MaximumAge = ec.Category.MaximumAge,

                EntryFee = ec.EntryFee,
                MaximumEntries = ec.MaximumEntries,

                AvailableEntries = ec.MaximumEntries -
                    _context.Enrolments.Count(e =>
                        e.EventId == ec.EventId &&
                        e.CategoryId == ec.CategoryId)
            })
            .Where(c => c.AvailableEntries > 0)
            .ToListAsync();

        return Ok(categories);
    }
}

