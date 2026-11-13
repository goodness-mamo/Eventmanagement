
namespace EventManagementApi.Models.Entities;

public class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public int? Position { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public string ResultStatus { get; set; } = string.Empty;

    // Navigation property
    public Enrolments? Enrolment { get; set; }
}
