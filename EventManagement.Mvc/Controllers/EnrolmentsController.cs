
using EventManagement.Mvc.Models.ViewModels;
using EventManagement.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Mvc.Controllers;

[Authorize(Roles = "Participant")]
public class EnrolmentsController : Controller
{
    private readonly ParticipantApiService _participantService;

    public EnrolmentsController(
        ParticipantApiService participantService)
    {
        _participantService = participantService;
    }

    // GET: /Enrolments/EnrolNow?eventId=1
    [HttpGet]
    public async Task<IActionResult> EnrolNow(int eventId)
    {
        var token = User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        if (eventId <= 0)
        {
            return BadRequest("Invalid event selected.");
        }

        // Retrieve available events to validate the selected event
        var (events, eventsError) =
            await _participantService.GetAvailableEventsAsync(token);

        if (events == null)
        {
            return View(new EnrolmentCreateViewModel
            {
                EventId = eventId,
                PageError = eventsError
            });
        }

        var selectedEvent = events.FirstOrDefault(
            e => e.EventId == eventId);

        if (selectedEvent == null)
        {
            return NotFound("The selected event could not be found.");
        }

        // Retrieve categories belonging to the selected event
        var (categories, categoriesError) =
            await _participantService.GetEventCategoriesAsync(
                eventId, token);

        var model = new EnrolmentCreateViewModel
        {
            EventId = eventId,
            EventName = selectedEvent.EventName,
            Categories = categories ?? new List<EventCategoryViewModel>(),
            PageError = categoriesError
        };

        return View(model);
    }

    // POST: /Enrolments/EnrolNow
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnrolNow(
        EnrolmentCreateViewModel model)
    {
        var token = User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        // Retrieve the event and its current available categories
        var (events, eventsError) =
            await _participantService.GetAvailableEventsAsync(token);

        if (events == null)
        {
            model.PageError = eventsError;
            return View(model);
        }

        var selectedEvent = events.FirstOrDefault(
            e => e.EventId == model.EventId);

        if (selectedEvent == null)
        {
            return NotFound("The selected event could not be found.");
        }

        model.EventName = selectedEvent.EventName;

        var (categories, categoriesError) =
            await _participantService.GetEventCategoriesAsync(
                model.EventId, token);

        model.Categories = categories ?? new List<EventCategoryViewModel>();

        if (categories == null)
        {
            model.PageError = categoriesError;
            return View(model);
        }

        // Validate the selected category against the event
        var selectedCategory = model.Categories.FirstOrDefault(
            c => c.CategoryId == model.CategoryId);

        if (!ModelState.IsValid || selectedCategory == null)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Please select a valid category with available spaces.");

            return View(model);
        }

        // Submit the enrolment to the API
        var (success, error) =
            await _participantService.CreateEnrolmentAsync(
                model.EventId,
                model.CategoryId,
                token);

        if (success)
        {
            TempData["SuccessMessage"] =
                $"You have successfully enrolled in {model.EventName} - {selectedCategory.CategoryName}.";

            return RedirectToAction(
                "Index",
                "ParticipantDashboard");
        }

        // Preserve the form and display the API error
        model.PageError = error;

        return View(model);
    }
}

