
using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.Models.DTOs;

public class UpdateResultRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
    public int? Position { get; set; }

    public TimeSpan? FinishTime { get; set; }

    [Required(ErrorMessage = "Result status is required.")]
    [StringLength(50)]
    public string ResultStatus { get; set; } = string.Empty;
}

