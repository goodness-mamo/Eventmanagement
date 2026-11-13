
using System.Security.Claims;
using EventManagementApi.Data;
using EventManagementApi.DTOs.Auth;
using EventManagementApi.Models.DTOs;
using EventManagementApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrolmentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public EnrolmentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Enrolments
    // Administrative access only
    [Authorize(Roles = "Admin,Organiser")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetEnrolments()
    {
        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Select(e => new EnrolmentResponse
            {
                EnrolmentId = e.EnrolmentId,
                EventId = e.EventId,
                EventName = e.EventCategory.Event.EventName,
                CategoryId = e.CategoryId,
                CategoryName = e.EventCategory.Category.CategoryName,
                ParticipantId = e.ParticipantId,
                ParticipantName =
                    e.Participant.User.FirstName + " " +
                    e.Participant.User.LastName,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status,
                RaceNumber = e.RaceNumber,
                EntryFee = e.EventCategory.EntryFee
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // GET: api/Enrolments/1
    // Administrative access only
    [Authorize(Roles = "Admin,Organiser")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EnrolmentResponse>> GetEnrolment(int id)
    {
        var enrolment = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EnrolmentId == id)
            .Select(e => new EnrolmentResponse
            {
                EnrolmentId = e.EnrolmentId,
                EventId = e.EventId,
                EventName = e.EventCategory.Event.EventName,
                CategoryId = e.CategoryId,
                CategoryName = e.EventCategory.Category.CategoryName,
                ParticipantId = e.ParticipantId,
                ParticipantName =
                    e.Participant.User.FirstName + " " +
                    e.Participant.User.LastName,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status,
                RaceNumber = e.RaceNumber,
                EntryFee = e.EventCategory.EntryFee
            })
            .FirstOrDefaultAsync();

        if (enrolment == null)
            return NotFound("Enrolment not found.");

        return Ok(enrolment);
    }

    // GET: api/Enrolments/my
    // Returns only the logged-in participant's enrolments
    [Authorize(Roles = "Participant")]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<EnrolmentResponse>>> GetMyEnrolments()
    {
        var userIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user identity.");

        var enrolments = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.Participant.UserId == userId)
            .Select(e => new EnrolmentResponse
            {
                EnrolmentId = e.EnrolmentId,
                EventId = e.EventId,
                EventName = e.EventCategory.Event.EventName,
                CategoryId = e.CategoryId,
                CategoryName = e.EventCategory.Category.CategoryName,
                ParticipantId = e.ParticipantId,
                ParticipantName =
                    e.Participant.User.FirstName + " " +
                    e.Participant.User.LastName,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status,
                RaceNumber = e.RaceNumber,
                EntryFee = e.EventCategory.EntryFee
            })
            .ToListAsync();

        return Ok(enrolments);
    }

    // POST: api/Enrolments
    // A participant can enrol themselves only
    [Authorize(Roles = "Participant")]
    [HttpPost]
    public async Task<ActionResult<EnrolmentResponse>> CreateEnrolment(
        CreateEnrolmentRequest request)
    {
        var userIdValue = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
            return Unauthorized("Invalid user identity.");

        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (participant == null)
            return NotFound("Participant profile not found.");

        var eventCategory = await _context.EventCategories
            .FirstOrDefaultAsync(ec =>
                ec.EventId == request.EventId &&
                ec.CategoryId == request.CategoryId);

        if (eventCategory == null)
            return BadRequest(
                "This category is not associated with the selected event.");

        var duplicateExists = await _context.Enrolments
            .AnyAsync(e =>
                e.EventId == request.EventId &&
                e.CategoryId == request.CategoryId &&
                e.ParticipantId == participant.ParticipantId);

        if (duplicateExists)
            return Conflict(
                "You are already enrolled in this event category.");

        var currentEntries = await _context.Enrolments
            .CountAsync(e =>
                e.EventId == request.EventId &&
                e.CategoryId == request.CategoryId);

        if (currentEntries >= eventCategory.MaximumEntries)
            return Conflict(
                "This event category has reached its maximum entries.");

        var enrolment = new Enrolments
        {
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            ParticipantId = participant.ParticipantId,
            EnrolledAt = DateTime.UtcNow,
            Status = "Pending",
            RaceNumber = string.Empty
        };

        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync();

        // Return a DTO rather than the EF entity
        var response = await _context.Enrolments
            .AsNoTracking()
            .Where(e => e.EnrolmentId == enrolment.EnrolmentId)
            .Select(e => new EnrolmentResponse
            {
                EnrolmentId = e.EnrolmentId,
                EventId = e.EventId,
                EventName = e.EventCategory.Event.EventName,
                CategoryId = e.CategoryId,
                CategoryName = e.EventCategory.Category.CategoryName,
                ParticipantId = e.ParticipantId,
                ParticipantName =
                    e.Participant.User.FirstName + " " +
                    e.Participant.User.LastName,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status,
                RaceNumber = e.RaceNumber,
                EntryFee = e.EventCategory.EntryFee
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetEnrolment),
            new { id = enrolment.EnrolmentId },
            response);
    }

    // PUT: api/Enrolments/1
    // Administrative access only
    [Authorize(Roles = "Admin,Organiser")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEnrolment(
        int id,
        Enrolments enrolment)
    {
        if (id != enrolment.EnrolmentId)
            return BadRequest("Enrolment ID does not match.");

        var existing = await _context.Enrolments.FindAsync(id);

        if (existing == null)
            return NotFound("Enrolment not found.");

        var eventCategoryExists = await _context.EventCategories
            .AnyAsync(ec =>
                ec.EventId == enrolment.EventId &&
                ec.CategoryId == enrolment.CategoryId);

        if (!eventCategoryExists)
            return BadRequest(
                "This category is not associated with the selected event.");

        var participantExists = await _context.Participants
            .AnyAsync(p => p.ParticipantId == enrolment.ParticipantId);

        if (!participantExists)
            return BadRequest("Participant does not exist.");

        var duplicateExists = await _context.Enrolments
            .AnyAsync(e =>
                e.EnrolmentId != id &&
                e.EventId == enrolment.EventId &&
                e.CategoryId == enrolment.CategoryId &&
                e.ParticipantId == enrolment.ParticipantId);

        if (duplicateExists)
            return Conflict(
                "This participant is already enrolled in this event category.");

        existing.EventId = enrolment.EventId;
        existing.CategoryId = enrolment.CategoryId;
        existing.ParticipantId = enrolment.ParticipantId;
        existing.Status = enrolment.Status;
        existing.RaceNumber = enrolment.RaceNumber;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Enrolments/1
    // Administrative access only
    [Authorize(Roles = "Admin,Organiser")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEnrolment(int id)
    {
        var enrolment = await _context.Enrolments.FindAsync(id);

        if (enrolment == null)
            return NotFound("Enrolment not found.");

        _context.Enrolments.Remove(enrolment);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
