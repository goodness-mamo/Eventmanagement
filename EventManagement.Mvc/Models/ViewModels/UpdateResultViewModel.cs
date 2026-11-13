
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Mvc.Models.ViewModels;

public class UpdateResultViewModel
{
    public int ResultId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
    public int? Position { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Required(ErrorMessage = "Result status is required.")]
    [RegularExpression(
        "^(Finished|DidNotFinish|Disqualified)$",
        ErrorMessage = "Select a valid result status.")]
    public string ResultStatus { get; set; } = "Finished";
}
