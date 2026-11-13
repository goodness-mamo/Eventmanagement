
using EventManagementApi.Data;
using EventManagementApi.Models.DTOs;
using EventManagementApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrganisersController : ControllerBase
{
    private readonly AppDbContext _context;

    public OrganisersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Organisers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganiserResponse>>>
        GetOrganisers()
    {
        var organisers = await _context.Organisers
            .AsNoTracking()
            .Select(o => new OrganiserResponse
            {
                OrganiserId = o.OrganiserId,
                UserId = o.UserId,
                OrganisationName = o.OrganisationName,
                FirstName = o.User.FirstName,
                LastName = o.User.LastName,
                Email = o.User.Email
            })
            .ToListAsync();

        return Ok(organisers);
    }

    // GET: api/Organisers/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrganiserResponse>>
        GetOrganiser(int id)
    {
        var organiser = await _context.Organisers
            .AsNoTracking()
            .Where(o => o.OrganiserId == id)
            .Select(o => new OrganiserResponse
            {
                OrganiserId = o.OrganiserId,
                UserId = o.UserId,
                OrganisationName = o.OrganisationName,
                FirstName = o.User.FirstName,
                LastName = o.User.LastName,
                Email = o.User.Email
            })
            .FirstOrDefaultAsync();

        if (organiser == null)
            return NotFound("Organiser not found.");

        return Ok(organiser);
    }

    // POST: api/Organisers
    [HttpPost]
    public async Task<ActionResult<OrganiserResponse>>
        CreateOrganiser(CreateOrganiserRequest request)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.UserId == request.UserId);

        if (!userExists)
            return NotFound("The selected user does not exist.");

        var alreadyOrganiser = await _context.Organisers
            .AnyAsync(o => o.UserId == request.UserId);

        if (alreadyOrganiser)
            return Conflict("This user is already registered as an organiser.");

        var organiser = new Organiser
        {
            UserId = request.UserId,
            OrganisationName = request.OrganisationName.Trim()
        };

        _context.Organisers.Add(organiser);

        await _context.SaveChangesAsync();

        var response = await _context.Organisers
            .AsNoTracking()
            .Where(o => o.OrganiserId == organiser.OrganiserId)
            .Select(o => new OrganiserResponse
            {
                OrganiserId = o.OrganiserId,
                UserId = o.UserId,
                OrganisationName = o.OrganisationName,
                FirstName = o.User.FirstName,
                LastName = o.User.LastName,
                Email = o.User.Email
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetOrganiser),
            new { id = organiser.OrganiserId },
            response);
    }

    // PUT: api/Organisers/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateOrganiser(
        int id,
        UpdateOrganiserRequest request)
    {
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.OrganiserId == id);

        if (organiser == null)
            return NotFound("Organiser not found.");

        organiser.OrganisationName = request.OrganisationName.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Organisers/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteOrganiser(int id)
    {
        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.OrganiserId == id);

        if (organiser == null)
            return NotFound("Organiser not found.");

        var hasEvents = await _context.Events
            .AnyAsync(e => e.OrganiserId == id);

        if (hasEvents)
        {
            return Conflict(
                "This organiser cannot be deleted because they have associated events.");
        }

        _context.Organisers.Remove(organiser);

        await _context.SaveChangesAsync();

        return NoContent();
    }
    // GET: api/Organisers/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var userIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            return Unauthorized();

        var userId = int.Parse(userIdClaim);

        var organiser = await _context.Organisers
            .FirstOrDefaultAsync(o => o.UserId == userId);

        if (organiser == null)
            return NotFound("Organiser profile not found.");

        var events = await _context.Events
            .Where(e => e.OrganiserId == organiser.OrganiserId)
            .ToListAsync();

        var eventIds = events.Select(e => e.EventId).ToList();

        var totalEnrolments = await _context.Enrolments
            .CountAsync(e => eventIds.Contains(e.EventId));

        var upcomingEvents = events
    .Where(e => e.EventDate >= DateTime.Today)
    .OrderBy(e => e.EventDate)
    .Select(e => new
    {
        e.EventId,
        e.EventName,
        e.Description,
        e.EventDate,
        e.Location
    })
    .ToList();

        var recentEnrolments = await _context.Enrolments
            .Where(e => eventIds.Contains(e.EventId))
            .OrderByDescending(e => e.EnrolledAt)
            .Take(5)
            .Select(e => new
            {
                e.EnrolmentId,
                e.EventId,
                e.EnrolledAt,
                e.Status,
                ParticipantName = e.Participant.User.FirstName
                    + " " + e.Participant.User.LastName
            })
            .ToListAsync();

        return Ok(new
        {
            OrganisationName = organiser.OrganisationName,
            TotalEvents = events.Count,
            TotalEnrolments = totalEnrolments,
            UpcomingEvents = upcomingEvents,
            RecentEnrolments = recentEnrolments
        });
    }
}
