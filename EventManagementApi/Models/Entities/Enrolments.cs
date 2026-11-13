
namespace EventManagementApi.Models.Entities;

public class Enrolments
{
    public int EnrolmentId { get; set; }

    public int EventId { get; set; }

    public int CategoryId { get; set; }

    public int ParticipantId { get; set; }

    public DateTime EnrolledAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string RaceNumber { get; set; } = string.Empty;

    // Navigation properties
    public Participant Participant { get; set; } = null!;

    public EventCategory EventCategory { get; set; } = null!;
}
