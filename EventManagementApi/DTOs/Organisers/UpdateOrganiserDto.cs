
using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.Models.DTOs;

public class UpdateOrganiserRequest
{
    [Required(ErrorMessage = "Organisation name is required.")]
    [StringLength(150)]
    public string OrganisationName { get; set; } = string.Empty;
}

