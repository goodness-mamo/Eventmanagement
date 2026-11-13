
namespace EventManagementApi.Models.DTOs;

public class ResultResponse
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public int EventId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string RaceNumber { get; set; } = string.Empty;

    public int? Position { get; set; }

    public TimeSpan? FinishTime { get; set; }

    public string ResultStatus { get; set; } = string.Empty;
}
