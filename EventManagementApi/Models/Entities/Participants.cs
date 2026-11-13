
namespace EventManagementApi.Models.Entities;

public class Participant
{
    public int ParticipantId { get; set; }

    public int UserId { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactNumber { get; set; }

    // Azure Blob Storage image reference
    public string? ProfilePictureBlobName { get; set; }

    public User User { get; set; } = null!;

    public ICollection<Enrolments> Enrolments { get; set; }
        = new List<Enrolments>();
}
