
using System.Security.Claims;
using EventManagementApi.Data;
using EventManagementApi.Models.DTOs;
using EventManagementApi.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ResultsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ResultsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Results/my
    // Participant can only view their own results.
    [HttpGet("my")]
    [Authorize(Roles = "Participant")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetMyResults()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.Participant != null &&
                r.Enrolment.Participant.UserId == userId)
            .OrderBy(r => r.Position)
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results
    // Organiser and Admin can view all results.
    [HttpGet]
    [Authorize(Roles = "Organiser,Admin")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>> GetResults()
    {
        var results = await _context.Results
            .AsNoTracking()
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results/1
    // Organiser and Admin can view a specific result.
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Organiser,Admin")]
    public async Task<ActionResult<ResultResponse>> GetResult(int id)
    {
        var result = await _context.Results
            .AsNoTracking()
            .Where(r => r.ResultId == id)
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return NotFound("Result not found.");
        }

        return Ok(result);
    }

    // GET: api/Results/event/1
    // Organiser and Admin can view results for an event.
    [HttpGet("event/{eventId:int}")]
    [Authorize(Roles = "Organiser,Admin")]
    public async Task<ActionResult<IEnumerable<ResultResponse>>>
        GetResultsByEvent(int eventId)
    {
        var eventExists = await _context.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
        {
            return NotFound("Event not found.");
        }

        var results = await _context.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment != null &&
                r.Enrolment.EventId == eventId)
            .OrderBy(r => r.Position)
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .ToListAsync();

        return Ok(results);
    }

    // GET: api/Results/enrolment/1
    // Organiser and Admin can view a result by enrolment.
    [HttpGet("enrolment/{enrolmentId:int}")]
    [Authorize(Roles = "Organiser,Admin")]
    public async Task<ActionResult<ResultResponse>>
        GetResultByEnrolment(int enrolmentId)
    {
        var result = await _context.Results
            .AsNoTracking()
            .Where(r => r.EnrolmentId == enrolmentId)
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .FirstOrDefaultAsync();

        if (result == null)
        {
            return NotFound("No result found for this enrolment.");
        }

        return Ok(result);
    }

    // POST: api/Results
    // Only Organiser can create results.
    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<ActionResult<ResultResponse>>
        CreateResult(CreateResultRequest request)
    {
        var enrolmentExists = await _context.Enrolments
            .AnyAsync(e => e.EnrolmentId == request.EnrolmentId);

        if (!enrolmentExists)
        {
            return NotFound("The selected enrolment does not exist.");
        }

        var resultExists = await _context.Results
            .AnyAsync(r => r.EnrolmentId == request.EnrolmentId);

        if (resultExists)
        {
            return Conflict("A result already exists for this enrolment.");
        }

        var result = new Result
        {
            EnrolmentId = request.EnrolmentId,
            Position = request.Position,
            FinishTime = request.FinishTime,
            ResultStatus = request.ResultStatus.Trim()
        };

        _context.Results.Add(result);

        await _context.SaveChangesAsync();

        var response = await _context.Results
            .AsNoTracking()
            .Where(r => r.ResultId == result.ResultId)
            .Select(r => new ResultResponse
            {
                ResultId = r.ResultId,
                EnrolmentId = r.EnrolmentId,
                EventId = r.Enrolment!.EventId,
                EventName = r.Enrolment.EventCategory.Event.EventName,
                CategoryName = r.Enrolment.EventCategory.Category.CategoryName,
                RaceNumber = r.Enrolment.RaceNumber,
                Position = r.Position,
                FinishTime = r.FinishTime,
                ResultStatus = r.ResultStatus
            })
            .FirstAsync();

        return CreatedAtAction(
            nameof(GetResult),
            new { id = result.ResultId },
            response);
    }

    // PUT: api/Results/1
    // Only Organiser can update results.
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> UpdateResult(
        int id,
        UpdateResultRequest request)
    {
        var result = await _context.Results
            .FirstOrDefaultAsync(r => r.ResultId == id);

        if (result == null)
        {
            return NotFound("Result not found.");
        }

        result.Position = request.Position;
        result.FinishTime = request.FinishTime;
        result.ResultStatus = request.ResultStatus.Trim();

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
