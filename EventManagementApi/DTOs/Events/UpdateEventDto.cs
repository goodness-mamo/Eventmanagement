using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.DTOs.Events;

public class UpdateEventDto
{
    [Required, StringLength(150, MinimumLength = 2)]
    public string EventName { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime? EventDate { get; set; }

    [Required, StringLength(250)]
    public string Location { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int OrganiserId { get; set; }
}
