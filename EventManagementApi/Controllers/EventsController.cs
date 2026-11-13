
using EventManagementApi.Data;
using EventManagementApi.DTOs.Events;
using EventManagementApi.Services;
using EventManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly AppDbContext _context;
    private readonly BlobStorageService _blobStorage;

    public EventsController(
        IEventService eventService,
        AppDbContext context,
        BlobStorageService blobStorage)
    {
        _eventService = eventService;
        _context = context;
        _blobStorage = blobStorage;
    }

    // GET: api/Events
    [HttpGet]
    public async Task<ActionResult<List<EventResponseDto>>> GetAll()
    {
        return Ok(await _eventService.GetAllAsync());
    }

    // GET: api/Events/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventResponseDto>> GetById(int id)
    {
        var result = await _eventService.GetByIdAsync(id);

        if (result is null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        return Ok(result);
    }

    // POST: api/Events
    [HttpPost]
    public async Task<ActionResult<EventResponseDto>> Create(
        [FromBody] CreateEventDto dto)
    {
        try
        {
            var result = await _eventService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.EventId },
                result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: api/Events/1
    [HttpPut("{id:int}")]
    public async Task<ActionResult<EventResponseDto>> Update(
        int id,
        [FromBody] UpdateEventDto dto)
    {
        try
        {
            var result = await _eventService.UpdateAsync(id, dto);

            if (result is null)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: api/Events/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool deleted = await _eventService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Event not found."
                });
            }

            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict(new
            {
                message = "The event cannot be deleted because related records exist."
            });
        }
    }

    // POST: api/Events/1/banner
    // Requires an authenticated Organiser.
    [Authorize(Roles = "Organiser")]
    [HttpPost("{id:int}/banner")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadBanner(
        int id,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .FirstOrDefaultAsync(
                e => e.EventId == id,
                cancellationToken);

        if (eventEntity == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        string? newBlobName = null;

        try
        {
            newBlobName = await _blobStorage.UploadImageAsync(
                file,
                "event-banners",
                cancellationToken);

            var oldBlobName = eventEntity.BannerBlobName;

            eventEntity.BannerBlobName = newBlobName;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete the previous banner after saving the new reference.
            if (!string.IsNullOrWhiteSpace(oldBlobName))
            {
                try
                {
                    await _blobStorage.DeleteImageAsync(
                        oldBlobName,
                        "event-banners",
                        cancellationToken);
                }
                catch (Exception)
                {
                    // Previous image cleanup can be retried separately.
                }
            }

            return Ok(new
            {
                message = "Event banner uploaded successfully.",
                eventId = id,
                blobName = newBlobName
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
        catch
        {
            // Remove the newly uploaded image if the database update fails.
            if (!string.IsNullOrWhiteSpace(newBlobName))
            {
                try
                {
                    await _blobStorage.DeleteImageAsync(
                        newBlobName,
                        "event-banners",
                        cancellationToken);
                }
                catch
                {
                    // Cleanup can be retried separately.
                }
            }

            throw;
        }
    }

    // GET: api/Events/1/banner
    [HttpGet("{id:int}/banner")]
    public async Task<IActionResult> GetBanner(
        int id,
        CancellationToken cancellationToken)
    {
        var eventEntity = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.EventId == id,
                cancellationToken);

        if (eventEntity == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        if (string.IsNullOrWhiteSpace(eventEntity.BannerBlobName))
        {
            return NotFound(new
            {
                message = "This event has no banner."
            });
        }

        var image = await _blobStorage.DownloadImageAsync(
            eventEntity.BannerBlobName,
            "event-banners",
            cancellationToken);

        if (image == null)
        {
            return NotFound(new
            {
                message = "Banner image not found."
            });
        }

        return File(image.Content, image.ContentType);
    }
}
