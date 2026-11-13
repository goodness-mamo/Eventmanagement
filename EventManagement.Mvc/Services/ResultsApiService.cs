
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventManagement.Mvc.Models.ViewModels;

namespace EventManagement.Mvc.Services;

public class ResultsApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResultsApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient(string token)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    // GET: api/Results
    // Organiser and Admin access
    public async Task<(List<ResultViewModel>? Data, string? Error)>
        GetAllAsync(string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.GetAsync("api/Results");

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve results. HTTP {(int)response.StatusCode}.");
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>();

            return (data ?? new List<ResultViewModel>(), null);
        }
        catch (HttpRequestException ex)
        {
            return (null, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return (null, $"Request timed out: {ex.Message}");
        }
    }

    // GET: api/Results/my
    // Retrieves results belonging to the authenticated participant
    public async Task<(List<ResultViewModel>? Data, string? Error)>
        GetMyResultsAsync(string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.GetAsync("api/Results/my");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                return (null,
                    string.IsNullOrWhiteSpace(error)
                        ? $"Unable to retrieve your results. HTTP {(int)response.StatusCode}."
                        : error);
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<ResultViewModel>>();

            return (data ?? new List<ResultViewModel>(), null);
        }
        catch (HttpRequestException ex)
        {
            return (null, $"Unable to connect to the API: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return (null, $"Request timed out: {ex.Message}");
        }
    }

    // GET: api/Results/{id}
    public async Task<(ResultViewModel? Data, string? Error)>
        GetByIdAsync(int id, string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.GetAsync($"api/Results/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve result. HTTP {(int)response.StatusCode}.");
            }

            var data = await response.Content
                .ReadFromJsonAsync<ResultViewModel>();

            return (data, null);
        }
        catch (HttpRequestException ex)
        {
            return (null, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return (null, $"Request timed out: {ex.Message}");
        }
    }

    // GET: api/Enrolments
    // Organiser and Admin access
    public async Task<(List<EnrolmentViewModel>? Data, string? Error)>
        GetEnrolmentsAsync(string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.GetAsync("api/Enrolments");

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve enrolments. HTTP {(int)response.StatusCode}.");
            }

            var data = await response.Content
                .ReadFromJsonAsync<List<EnrolmentViewModel>>();

            return (data ?? new List<EnrolmentViewModel>(), null);
        }
        catch (HttpRequestException ex)
        {
            return (null, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return (null, $"Request timed out: {ex.Message}");
        }
    }

    // POST: api/Results
    public async Task<(bool Success, string? Error)>
        CreateAsync(CreateResultViewModel model, string token)
    {
        try
        {
            var client = CreateClient(token);

            var payload = new
            {
                model.EnrolmentId,
                model.Position,
                model.FinishTime,
                model.ResultStatus
            };

            var response = await client.PostAsJsonAsync(
                "api/Results", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                return (false,
                    string.IsNullOrWhiteSpace(error)
                        ? $"Unable to create result. HTTP {(int)response.StatusCode}."
                        : error);
            }

            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return (false, $"Request timed out: {ex.Message}");
        }
    }

    // PUT: api/Results/{id}
    public async Task<(bool Success, string? Error)>
        UpdateAsync(int id, UpdateResultViewModel model, string token)
    {
        try
        {
            var client = CreateClient(token);

            var payload = new
            {
                model.Position,
                model.FinishTime,
                model.ResultStatus
            };

            var response = await client.PutAsJsonAsync(
                $"api/Results/{id}", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                return (false,
                    string.IsNullOrWhiteSpace(error)
                        ? $"Unable to update result. HTTP {(int)response.StatusCode}."
                        : error);
            }

            return (true, null);
        }
        catch (HttpRequestException ex)
        {
            return (false, ex.Message);
        }
        catch (TaskCanceledException ex)
        {
            return (false, $"Request timed out: {ex.Message}");
        }
    }
}
