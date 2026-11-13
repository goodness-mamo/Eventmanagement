using EventManagementApi.Data;
using EventManagementApi.DTOs.Events;
using EventManagementApi.Models.Entities;
using EventManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventManagementApi.Services.Implementations;

public class EventService : IEventService
{
    private readonly AppDbContext _context;

    public EventService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventResponseDto>> GetAllAsync()
    {
        return await _context.Events
            .AsNoTracking()
            .Select(e => new EventResponseDto
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                EventName = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location
            })
            .ToListAsync();
    }

    public async Task<EventResponseDto?> GetByIdAsync(int id)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.EventId == id)
            .Select(e => new EventResponseDto
            {
                EventId = e.EventId,
                OrganiserId = e.OrganiserId,
                EventName = e.EventName,
                Description = e.Description,
                EventDate = e.EventDate,
                Location = e.Location
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EventResponseDto> CreateAsync(CreateEventDto dto)
    {
        bool organiserExists = await _context.Organisers
            .AnyAsync(o => o.OrganiserId == dto.OrganiserId);

        if (!organiserExists)
        {
            throw new ArgumentException(
                "The specified organiser does not exist.");
        }

        var entity = new Event
        {
            EventName = dto.EventName,
            Description = dto.Description,
            EventDate = dto.EventDate!.Value,
            Location = dto.Location,
            OrganiserId = dto.OrganiserId
        };

        _context.Events.Add(entity);

        await _context.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<EventResponseDto?> UpdateAsync(
        int id, UpdateEventDto dto)
    {
        var entity = await _context.Events.FindAsync(id);

        if (entity is null)
            return null;

        bool organiserExists = await _context.Organisers
            .AnyAsync(o => o.OrganiserId == dto.OrganiserId);

        if (!organiserExists)
        {
            throw new ArgumentException(
                "The specified organiser does not exist.");
        }

        entity.EventName = dto.EventName;
        entity.Description = dto.Description;
        entity.EventDate = dto.EventDate!.Value;
        entity.Location = dto.Location;
        entity.OrganiserId = dto.OrganiserId;

        await _context.SaveChangesAsync();

        return MapToResponse(entity);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Events.FindAsync(id);

        if (entity is null)
            return false;

        _context.Events.Remove(entity);

        await _context.SaveChangesAsync();

        return true;
    }

    private static EventResponseDto MapToResponse(Event entity)
    {
        return new EventResponseDto
        {
            EventId = entity.EventId,
            OrganiserId = entity.OrganiserId,
            EventName = entity.EventName,
            Description = entity.Description,
            EventDate = entity.EventDate,
            Location = entity.Location
        };
    }
}
