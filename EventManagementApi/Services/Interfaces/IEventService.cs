using EventManagementApi.DTOs.Events;

namespace EventManagementApi.Services.Interfaces;

public interface IEventService
{
    Task<List<EventResponseDto>> GetAllAsync();

    Task<EventResponseDto?> GetByIdAsync(int id);

    Task<EventResponseDto> CreateAsync(CreateEventDto dto);

    Task<EventResponseDto?> UpdateAsync(int id, UpdateEventDto dto);

    Task<bool> DeleteAsync(int id);
}
