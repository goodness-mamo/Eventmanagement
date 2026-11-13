
namespace EventManagementApi.Models.Entities;

public class Event
{
    public int EventId { get; set; }

    public int OrganiserId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = string.Empty;

    // Azure Blob Storage image reference
    public string? BannerBlobName { get; set; }

    public Organiser Organiser { get; set; } = null!;

    public ICollection<EventCategory> EventCategories { get; set; }
        = new List<EventCategory>();
}
