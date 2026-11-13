
namespace EventManagement.Mvc.Models.ViewModels;

public class ParticipantDashboardViewModel
{
    // Available events
    public List<EventViewModel> AvailableEvents { get; set; }
        = new();

    public string? AvailableEventsError { get; set; }

    // Participant's existing enrolments
    public List<EnrolmentViewModel> MyEnrolments { get; set; }
        = new();

    public string? MyEnrolmentsError { get; set; }

    // Participant's race results
    public List<ResultViewModel> MyResults { get; set; }
        = new();

    public string? MyResultsError { get; set; }

    // Participant profile
    public int ParticipantId { get; set; }
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateTime? DateOfBirth { get; set; }

    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactNumber { get; set; }
}
