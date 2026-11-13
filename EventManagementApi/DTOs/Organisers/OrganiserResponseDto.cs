
namespace EventManagementApi.Models.DTOs;

public class OrganiserResponse
{
    public int OrganiserId { get; set; }

    public int UserId { get; set; }

    public string OrganisationName { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

