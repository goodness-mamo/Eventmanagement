
using System.ComponentModel.DataAnnotations;

namespace EventManagementApi.Models.Entities;

public class Categories
{
    [Key]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int? MinimumAge { get; set; }

    public int? MaximumAge { get; set; }

    public ICollection<EventCategory> EventCategories { get; set; }
        = new List<EventCategory>();
}
