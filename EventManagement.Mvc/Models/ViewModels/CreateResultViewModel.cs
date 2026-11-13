
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Mvc.Models.ViewModels;

public class CreateResultViewModel
{
    [Required(ErrorMessage = "Please select an enrolment.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid enrolment.")]
    public int EnrolmentId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
    public int? Position { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Required(ErrorMessage = "Result status is required.")]
    [RegularExpression(
        "^(Finished|DidNotFinish|Disqualified)$",
        ErrorMessage = "Select a valid result status.")]
    public string ResultStatus { get; set; } = "Finished";

    public List<EnrolmentViewModel> AvailableEnrolments { get; set; } = new();
}
