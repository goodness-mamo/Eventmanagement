
namespace EventManagementApi.Models.DTOs;

public class EventCategoryResponse
{
    public int EventId { get; set; }
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int? MinimumAge { get; set; }
    public int? MaximumAge { get; set; }

    public decimal EntryFee { get; set; }

    public int MaximumEntries { get; set; }
    public int AvailableEntries { get; set; }
}

