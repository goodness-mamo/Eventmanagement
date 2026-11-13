
using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.Models.DTOs;

public class CreateOrganiserRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "A valid UserId is required.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Organisation name is required.")]
    [StringLength(150)]
    public string OrganisationName { get; set; } = string.Empty;
}

