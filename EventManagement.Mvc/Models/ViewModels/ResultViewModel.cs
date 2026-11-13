
namespace EventManagement.Mvc.Models.ViewModels;

public class ResultViewModel
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }

    public int? Position { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public string ResultStatus { get; set; } = string.Empty;

    // MVC display properties
    public string EventName { get; set; } = string.Empty;
    public string ParticipantName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string RaceNumber { get; set; } = string.Empty;
}


