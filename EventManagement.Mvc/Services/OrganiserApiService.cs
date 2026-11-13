
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EventManagement.Mvc.Models.ViewModels;

namespace EventManagement.Mvc.Services;

public class OrganiserApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public OrganiserApiService(IHttpClientFactory httpClientFactory)
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

    // GET: api/Organisers
    public async Task<(List<OrganiserViewModel>? Data, string? Error)>
        GetAllAsync(string token)
    {
        try
        {
            var client = CreateClient(token);

            var data = await client.GetFromJsonAsync<List<OrganiserViewModel>>(
                "api/Organisers");

            return (data ?? new List<OrganiserViewModel>(), null);
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

    // GET: api/Organisers/{id}
    public async Task<(OrganiserViewModel? Data, string? Error)>
        GetByIdAsync(int id, string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.GetAsync($"api/Organisers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return (null,
                    $"Unable to retrieve organiser. HTTP {(int)response.StatusCode}.");
            }

            var data = await response.Content
                .ReadFromJsonAsync<OrganiserViewModel>();

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

    // POST: api/Organisers
    public async Task<(bool Success, string? Error)>
        CreateAsync(OrganiserViewModel model, string token)
    {
        try
        {
            var client = CreateClient(token);

            var payload = new
            {
                model.UserId,
                model.OrganisationName
            };

            var response = await client.PostAsJsonAsync(
                "api/Organisers", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
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

    // PUT: api/Organisers/{id}
    public async Task<(bool Success, string? Error)>
        UpdateAsync(int id, OrganiserViewModel model, string token)
    {
        try
        {
            var client = CreateClient(token);

            var payload = new
            {
                model.OrganisationName
            };

            var response = await client.PutAsJsonAsync(
                $"api/Organisers/{id}", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
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

    // DELETE: api/Organisers/{id}
    public async Task<(bool Success, string? Error)>
        DeleteAsync(int id, string token)
    {
        try
        {
            var client = CreateClient(token);

            var response = await client.DeleteAsync(
                $"api/Organisers/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return (false, error);
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
