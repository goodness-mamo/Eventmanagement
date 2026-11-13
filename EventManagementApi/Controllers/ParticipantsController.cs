
using System.Security.Claims;
using EventManagementApi.Data;
using EventManagementApi.Models.Entities;
using EventManagementApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParticipantsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BlobStorageService _blobStorage;

    public ParticipantsController(
        AppDbContext context,
        BlobStorageService blobStorage)
    {
        _context = context;
        _blobStorage = blobStorage;
    }

    // GET: api/Participants/me
    [HttpGet("me")]
    public async Task<IActionResult> GetMyParticipant()
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user identity.");
        }

        var participant = await _context.Participants
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.ParticipantId,
                p.UserId,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Email = p.User.Email,
                p.DateOfBirth,
                p.EmergencyContactName,
                p.EmergencyContactNumber,
                p.ProfilePictureBlobName
            })
            .FirstOrDefaultAsync();

        if (participant == null)
        {
            return NotFound(
                "No participant profile was found for this account.");
        }

        return Ok(participant);
    }

    // GET: api/Participants
    [HttpGet]
    public async Task<IActionResult> GetParticipants()
    {
        var participants = await _context.Participants
            .AsNoTracking()
            .Select(p => new
            {
                p.ParticipantId,
                p.UserId,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Email = p.User.Email,
                p.DateOfBirth,
                p.EmergencyContactName,
                p.EmergencyContactNumber,
                p.ProfilePictureBlobName
            })
            .ToListAsync();

        return Ok(participants);
    }

    // GET: api/Participants/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetParticipant(int id)
    {
        var participant = await _context.Participants
            .AsNoTracking()
            .Where(p => p.ParticipantId == id)
            .Select(p => new
            {
                p.ParticipantId,
                p.UserId,
                FirstName = p.User.FirstName,
                LastName = p.User.LastName,
                Email = p.User.Email,
                p.DateOfBirth,
                p.EmergencyContactName,
                p.EmergencyContactNumber,
                p.ProfilePictureBlobName
            })
            .FirstOrDefaultAsync();

        if (participant == null)
        {
            return NotFound();
        }

        return Ok(participant);
    }

    // POST: api/Participants
    [HttpPost]
    public async Task<IActionResult> CreateParticipant(
        [FromBody] Participant participant)
    {
        var userExists = await _context.Users
            .AnyAsync(u => u.UserId == participant.UserId);

        if (!userExists)
        {
            return BadRequest(
                "The specified UserId does not exist.");
        }

        var alreadyRegistered = await _context.Participants
            .AnyAsync(p => p.UserId == participant.UserId);

        if (alreadyRegistered)
        {
            return Conflict(
                "A participant record already exists for this user.");
        }

        participant.ParticipantId = 0;

        _context.Participants.Add(participant);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetParticipant),
            new { id = participant.ParticipantId },
            new
            {
                participant.ParticipantId,
                participant.UserId,
                participant.DateOfBirth,
                participant.EmergencyContactName,
                participant.EmergencyContactNumber
            });
    }

    // PUT: api/Participants/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateParticipant(
        int id,
        [FromBody] Participant participant)
    {
        if (id != participant.ParticipantId)
        {
            return BadRequest(
                "Participant ID does not match.");
        }

        var existing = await _context.Participants
            .FirstOrDefaultAsync(p => p.ParticipantId == id);

        if (existing == null)
        {
            return NotFound();
        }

        existing.DateOfBirth = participant.DateOfBirth;

        existing.EmergencyContactName =
            participant.EmergencyContactName;

        existing.EmergencyContactNumber =
            participant.EmergencyContactNumber;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Participants/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteParticipant(int id)
    {
        var participant = await _context.Participants
            .FindAsync(id);

        if (participant == null)
        {
            return NotFound();
        }

        _context.Participants.Remove(participant);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // POST: api/Participants/me/profile-picture
    [HttpPost("me/profile-picture")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user identity.");
        }

        var participant = await _context.Participants
            .FirstOrDefaultAsync(
                p => p.UserId == userId,
                cancellationToken);

        if (participant == null)
        {
            return NotFound(
                "Participant profile not found.");
        }

        string? newBlobName = null;

        try
        {
            newBlobName = await _blobStorage.UploadImageAsync(
                file,
                "profile-pictures",
                cancellationToken);

            var oldBlobName = participant.ProfilePictureBlobName;

            participant.ProfilePictureBlobName = newBlobName;

            await _context.SaveChangesAsync(cancellationToken);

            // Delete the previous picture after saving the new reference.
            if (!string.IsNullOrWhiteSpace(oldBlobName))
            {
                try
                {
                    await _blobStorage.DeleteImageAsync(
                        oldBlobName,
                        "profile-pictures",
                        cancellationToken);
                }
                catch (Exception)
                {
                    // Previous image cleanup can be retried separately.
                }
            }

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
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
            // Remove the new image if the database update fails.
            if (!string.IsNullOrWhiteSpace(newBlobName))
            {
                try
                {
                    await _blobStorage.DeleteImageAsync(
                        newBlobName,
                        "profile-pictures",
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

    // GET: api/Participants/me/profile-picture
    [HttpGet("me/profile-picture")]
    public async Task<IActionResult> GetMyProfilePicture(
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user identity.");
        }

        var participant = await _context.Participants
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.UserId == userId,
                cancellationToken);

        if (participant == null)
        {
            return NotFound(
                "Participant profile not found.");
        }

        if (string.IsNullOrWhiteSpace(
            participant.ProfilePictureBlobName))
        {
            return NotFound(
                "No profile picture has been uploaded.");
        }

        var image = await _blobStorage.DownloadImageAsync(
            participant.ProfilePictureBlobName,
            "profile-pictures",
            cancellationToken);

        if (image == null)
        {
            return NotFound(
                "Profile picture not found.");
        }

        return File(image.Content, image.ContentType);
    }
}
