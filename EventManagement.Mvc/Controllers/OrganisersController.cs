
using EventManagement.Mvc.Models.ViewModels;
using EventManagement.Mvc.Services;
using EventManagement.Mvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EventManagement.Mvc.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganisersController : Controller
{
    private readonly OrganiserApiService _organiserService;
    private readonly IHttpClientFactory _httpClientFactory;

    public OrganisersController(
        OrganiserApiService organiserService,
        IHttpClientFactory httpClientFactory)
    {
        _organiserService = organiserService;
        _httpClientFactory = httpClientFactory;
    }

    private string? GetToken()
    {
        return User.FindFirst("AccessToken")?.Value;
    }

    // GET: /Organisers/Dashboard
    [HttpGet]
    public async Task<IActionResult> Dashboard()
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(
            "api/Organisers/dashboard");

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Challenge();

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Unable to load organiser dashboard.";

            return View(new OrganiserDashboardViewModel());
        }

        var dashboard = await response.Content
            .ReadFromJsonAsync<OrganiserDashboardViewModel>();

        return View(
            dashboard ?? new OrganiserDashboardViewModel());
    }

    // GET: /Organisers/Banner/3
    // Retrieves an event banner through the API.
    [HttpGet]
    public async Task<IActionResult> Banner(int id)
    {
        if (id <= 0)
            return BadRequest();

        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.GetAsync(
            $"api/Events/{id}/banner",
            HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
            return Challenge();

        if (response.StatusCode == HttpStatusCode.NotFound)
            return NotFound();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode);

        var contentType = response.Content.Headers.ContentType?.MediaType
                          ?? "application/octet-stream";

        var imageBytes = await response.Content.ReadAsByteArrayAsync();

        return File(imageBytes, contentType);
    }

    // GET: /Organisers
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (organisers, error) =
            await _organiserService.GetAllAsync(token);

        if (organisers == null)
        {
            ViewBag.Error = error;
            return View(new List<OrganiserViewModel>());
        }

        return View(organisers);
    }

    // GET: /Organisers/Details/1
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (organiser, error) =
            await _organiserService.GetByIdAsync(id, token);

        if (organiser == null)
            return NotFound(error);

        return View(organiser);
    }

    // GET: /Organisers/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Organisers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        OrganiserViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (success, error) =
            await _organiserService.CreateAsync(model, token);

        if (!success)
        {
            ModelState.AddModelError(
                string.Empty,
                error ?? "Unable to create organiser.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Organiser created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Organisers/Edit/1
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (organiser, error) =
            await _organiserService.GetByIdAsync(id, token);

        if (organiser == null)
            return NotFound(error);

        return View(organiser);
    }

    // POST: /Organisers/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        OrganiserViewModel model)
    {
        if (id != model.OrganiserId)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (success, error) =
            await _organiserService.UpdateAsync(
                id, model, token);

        if (!success)
        {
            ModelState.AddModelError(
                string.Empty,
                error ?? "Unable to update organiser.");

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Organiser updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Organisers/Delete/1
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (organiser, error) =
            await _organiserService.GetByIdAsync(id, token);

        if (organiser == null)
            return NotFound(error);

        return View(organiser);
    }

    // POST: /Organisers/Delete/1
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var token = GetToken();

        if (string.IsNullOrWhiteSpace(token))
            return Challenge();

        var (success, error) =
            await _organiserService.DeleteAsync(id, token);

        if (!success)
        {
            TempData["ErrorMessage"] =
                error ?? "Unable to delete organiser.";

            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] =
            "Organiser deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
