
using EventManagement.Mvc.Services;
using EventManagement.Mvc.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EventManagement.Mvc.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantDashboardController : Controller
{
    private readonly ParticipantApiService _participantService;
    private readonly ResultsApiService _resultsService;

    public ParticipantDashboardController(
        ParticipantApiService participantService,
        ResultsApiService resultsService)
    {
        _participantService = participantService;
        _resultsService = resultsService;
    }

    public async Task<IActionResult> Index()
    {
        // Get JWT token from authenticated user
        var token = User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        // Step 1: Retrieve participant profile
        var (profile, profileError) =
            await _participantService.GetMyProfileAsync(token);

        if (profile == null)
        {
            ViewBag.Error = profileError;
            return View();
        }

        // Step 2: Retrieve available events
        var (events, eventsError) =
            await _participantService.GetAvailableEventsAsync(token);

        profile.AvailableEvents =
            events ?? new List<EventViewModel>();

        profile.AvailableEventsError = eventsError;

        // Step 3: Retrieve this participant's enrolments
        var (enrolments, enrolmentsError) =
            await _participantService.GetMyEnrolmentsAsync(token);

        profile.MyEnrolments =
            enrolments ?? new List<EnrolmentViewModel>();

        profile.MyEnrolmentsError = enrolmentsError;

        // Step 4: Retrieve this participant's race results
        var (results, resultsError) =
            await _resultsService.GetMyResultsAsync(token);

        profile.MyResults =
            results ?? new List<ResultViewModel>();

        profile.MyResultsError = resultsError;

        // Step 5: Display dashboard
        return View(profile);
    }

    // GET: /ParticipantDashboard/ProfilePicture
    [HttpGet]
    public async Task<IActionResult> ProfilePicture()
    {
        var token = User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        try
        {
            using var response =
                await _participantService.GetMyProfilePictureAsync(token);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode((int)response.StatusCode);
            }

            var imageBytes =
                await response.Content.ReadAsByteArrayAsync();

            var contentType =
                response.Content.Headers.ContentType?.MediaType
                ?? "application/octet-stream";

            return File(imageBytes, contentType);
        }
        catch (HttpRequestException)
        {
            return StatusCode(502,
                "Unable to retrieve the profile picture.");
        }
    }

    // POST: /ParticipantDashboard/UploadProfilePicture
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfilePicture(
        IFormFile file)
    {
        var token = User.FindFirst("AccessToken")?.Value;

        if (string.IsNullOrWhiteSpace(token))
        {
            return Challenge();
        }

        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] =
                "Please select a profile picture.";

            return RedirectToAction(nameof(Index));
        }

        var (success, error) =
            await _participantService.UploadProfilePictureAsync(
                file, token);

        if (success)
        {
            TempData["SuccessMessage"] =
                "Your profile picture has been uploaded successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                $"Profile picture upload failed: {error}";
        }

        return RedirectToAction(nameof(Index));
    }
}
