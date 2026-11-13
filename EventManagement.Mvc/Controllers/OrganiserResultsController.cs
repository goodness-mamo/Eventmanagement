
using EventManagement.Mvc.Models.ViewModels;
using EventManagement.Mvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Mvc.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserResultsController : Controller
{
    private readonly ResultsApiService _resultsService;

    public OrganiserResultsController(
        ResultsApiService resultsService)
    {
        _resultsService = resultsService;
    }

    private string? GetToken()
    {
        return User.FindFirst("AccessToken")?.Value;
    }

    // GET: /OrganiserResults
    public async Task<IActionResult> Index()
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (results, resultsError) =
            await _resultsService.GetAllAsync(token);

        if (results == null)
        {
            ViewBag.Error = resultsError;
            return View(new List<ResultViewModel>());
        }

        var (enrolments, enrolmentsError) =
            await _resultsService.GetEnrolmentsAsync(token);

        if (enrolments == null)
        {
            ViewBag.Error = enrolmentsError;
            return View(results);
        }

        // Add participant and event details to each result.
        foreach (var result in results)
        {
            var enrolment = enrolments.FirstOrDefault(
                e => e.EnrolmentId == result.EnrolmentId);

            if (enrolment == null)
                continue;

            result.EventName = enrolment.EventName;
            result.ParticipantName = enrolment.ParticipantName;
            result.CategoryName = enrolment.CategoryName;
            result.RaceNumber = enrolment.RaceNumber;
        }

        return View(results);
    }

    // GET: /OrganiserResults/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (enrolments, error) =
            await _resultsService.GetEnrolmentsAsync(token);

        if (enrolments == null)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToAction(nameof(Index));
        }

        var model = new CreateResultViewModel
        {
            AvailableEnrolments = enrolments,
            ResultStatus = "Completed"
        };

        return View(model);
    }

    // POST: /OrganiserResults/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateResultViewModel model)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (enrolments, enrolmentsError) =
            await _resultsService.GetEnrolmentsAsync(token);

        model.AvailableEnrolments =
            enrolments ?? new List<EnrolmentViewModel>();

        if (!ModelState.IsValid)
            return View(model);

        if (enrolments == null ||
            !enrolments.Any(e => e.EnrolmentId == model.EnrolmentId))
        {
            ModelState.AddModelError(
                nameof(model.EnrolmentId),
                "Please select a valid enrolment.");

            return View(model);
        }

        var (success, error) =
            await _resultsService.CreateAsync(model, token);

        if (!success)
        {
            ModelState.AddModelError(
                string.Empty,
                error ?? "Unable to create result.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Race result recorded successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /OrganiserResults/Edit/1
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (result, error) =
            await _resultsService.GetByIdAsync(id, token);

        if (result == null)
        {
            TempData["ErrorMessage"] =
                error ?? "Result not found.";

            return RedirectToAction(nameof(Index));
        }

        var model = new UpdateResultViewModel
        {
            ResultId = result.ResultId,
            Position = result.Position,
            FinishTime = result.FinishTime,
            ResultStatus = result.ResultStatus
        };

        return View(model);
    }

    // POST: /OrganiserResults/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        UpdateResultViewModel model)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        if (id != model.ResultId)
            return BadRequest("Result ID does not match.");

        if (!ModelState.IsValid)
            return View(model);

        var (success, error) =
            await _resultsService.UpdateAsync(id, model, token);

        if (!success)
        {
            ModelState.AddModelError(
                string.Empty,
                error ?? "Unable to update result.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Race result updated successfully.";

        return RedirectToAction(nameof(Index));
    }
}

