
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Mvc.Models.ViewModels;

public class OrganiserViewModel
{
    public int OrganiserId { get; set; }

    [Required(ErrorMessage = "Please select a user.")]
    [Range(1, int.MaxValue, ErrorMessage = "Invalid user selected.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Organisation name is required.")]
    [StringLength(150)]
    public string OrganisationName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

