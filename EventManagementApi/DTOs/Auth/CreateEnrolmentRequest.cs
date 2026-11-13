
using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.DTOs.Auth;

public class CreateEnrolmentRequest
{
    [Required]
    public int EventId { get; set; }

    [Required]
    public int CategoryId { get; set; }
}

