
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Mvc.Models.ViewModels;

public class EnrolmentCreateViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Invalid event selected.")]
    public int EventId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select an event category.")]
    public int CategoryId { get; set; }

    public string EventName { get; set; } = string.Empty;

    public List<EventCategoryViewModel> Categories { get; set; } = new();

    public string? PageError { get; set; }
}

