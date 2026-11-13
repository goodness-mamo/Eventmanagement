
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventManagement.Mvc.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace EventManagement.Mvc.Services;

public class ParticipantApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ParticipantApiService(
        IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // =====================================================
    // 1. RETRIEVE LOGGED-IN PARTICIPANT PROFILE
    // GET: api/Participants/me
    // =====================================================

    public async Task<(
        ParticipantDashboardViewModel? Profile,
        string? Error)> GetMyProfileAsync(string token)
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Participants/me");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"API request failed: {(int)response.StatusCode}");
            }

            var profile = await response.Content
                .ReadFromJsonAsync<ParticipantDashboardViewModel>();

            return profile == null
                ? (null, "The API returned an empty profile.")
                : (profile, null);
        }
        catch (HttpRequestException)
        {
            return (null,
                "Unable to connect to the Event Management API.");
        }
    }


    // =====================================================
    // 2. RETRIEVE AVAILABLE EVENTS
    // GET: api/Events
    // =====================================================

    public async Task<(
        List<EventViewModel>? Events,
        string? Error)> GetAvailableEventsAsync(string token)
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Events");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve events. API status: {(int)response.StatusCode}");
            }

            var events = await response.Content
                .ReadFromJsonAsync<List<EventViewModel>>();

            return (events ?? new List<EventViewModel>(), null);
        }
        catch (HttpRequestException)
        {
            return (null,
                "Unable to connect to the Event Management API.");
        }
    }


    // =====================================================
    // 3. RETRIEVE LOGGED-IN PARTICIPANT ENROLMENTS
    // GET: api/Enrolments/my
    // =====================================================

    public async Task<(
        List<EnrolmentViewModel>? Enrolments,
        string? Error)> GetMyEnrolmentsAsync(string token)
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                "api/Enrolments/my");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve enrolments. API status: {(int)response.StatusCode}");
            }

            var enrolments = await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            return (enrolments ?? new List<EnrolmentViewModel>(), null);
        }
        catch (HttpRequestException)
        {
            return (null,
                "Unable to connect to the Event Management API.");
        }
    }


    // =====================================================
    // 4. RETRIEVE CATEGORIES FOR A SELECTED EVENT
    // GET: api/EventCategories/event/{eventId}
    // =====================================================

    public async Task<(
        List<EventCategoryViewModel>? Categories,
        string? Error)> GetEventCategoriesAsync(
            int eventId,
            string token)
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"api/EventCategories/event/{eventId}");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve categories. API status: {(int)response.StatusCode}");
            }

            var categories = await response.Content
                .ReadFromJsonAsync<List<EventCategoryViewModel>>();

            return (
                categories ?? new List<EventCategoryViewModel>(),
                null
            );
        }
        catch (HttpRequestException)
        {
            return (null,
                "Unable to connect to the Event Management API.");
        }
    }


    // =====================================================
    // 5. CREATE A NEW PARTICIPANT ENROLMENT
    // POST: api/Enrolments
    // =====================================================

    public async Task<(bool Success, string? Error)>
        CreateEnrolmentAsync(
            int eventId,
            int categoryId,
            string token)
    {
        try
        {
            var client = _httpClientFactory
                .CreateClient("RaceDayApi");

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Enrolments");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(new
            {
                EventId = eventId,
                CategoryId = categoryId
            });

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content
                .ReadAsStringAsync();

            return (
                false,
                string.IsNullOrWhiteSpace(error)
                    ? $"Enrolment failed. API status: {(int)response.StatusCode}"
                    : error
            );
        }
        catch (HttpRequestException)
        {
            return (false,
                "Unable to connect to the Event Management API.");
        }
    }

    // =====================================================
    // 6. UPLOAD PARTICIPANT PROFILE PICTURE
    // POST: api/Participants/me/profile-picture
    // =====================================================

    public async Task<(bool Success, string? Error)>
        UploadProfilePictureAsync(IFormFile file, string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);

            content.Add(fileContent, "file", file.FileName);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "api/Participants/me/profile-picture");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Content = content;

            using var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var error = await response.Content.ReadAsStringAsync();

            return (false,
                string.IsNullOrWhiteSpace(error)
                    ? $"Profile picture upload failed. API status: {(int)response.StatusCode}"
                    : error);
        }
        catch (HttpRequestException)
        {
            return (false,
                "Unable to connect to the Event Management API.");
        }
    }


    // =====================================================
    // 7. RETRIEVE PARTICIPANT PROFILE PICTURE
    // GET: api/Participants/me/profile-picture
    // =====================================================

    public async Task<HttpResponseMessage> GetMyProfilePictureAsync(
        string token)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            "api/Participants/me/profile-picture");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead);
    }

}
