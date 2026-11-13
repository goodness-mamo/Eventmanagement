namespace EventManagement.Mvc.Models;

public class OrganiserDashboardViewModel
{
    public string OrganisationName { get; set; } = string.Empty;
    public int TotalEvents { get; set; }
    public int TotalEnrolments { get; set; }

    public List<UpcomingEventViewModel> UpcomingEvents { get; set; } = new();

    public List<RecentEnrolmentViewModel> RecentEnrolments { get; set; } = new();
}

public class UpcomingEventViewModel
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
}

public class RecentEnrolmentViewModel
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
}
