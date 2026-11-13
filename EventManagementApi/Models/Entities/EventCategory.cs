namespace EventManagementApi.Models.Entities;

public class EventCategory
{
    public int EventId { get; set; }

    public int CategoryId { get; set; }

    public decimal EntryFee { get; set; }

    public int MaximumEntries { get; set; }

    public Event Event { get; set; } = null!;

    public Categories Category { get; set; } = null!;

    public ICollection<Enrolments> Enrolments { get; set; }
        = new List<Enrolments>();
}
