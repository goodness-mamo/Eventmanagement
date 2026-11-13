
using System.Net.Http.Json;
using EventManagement.Mvc.Models.ViewModels;

namespace EventManagement.Mvc.Services;

public class AuthenticationApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthenticationApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<(ApiAuthResponse? Response, string? Error)>
        RegisterAsync(RegisterViewModel model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/register", model);

            if (!response.IsSuccessStatusCode)
                return (null, await response.Content.ReadAsStringAsync());

            var result = await response.Content
                .ReadFromJsonAsync<ApiAuthResponse>();

            return (result, result == null
                ? "The API returned an empty response."
                : null);
        }
        catch (HttpRequestException)
        {
            return (null, "Unable to connect to the API.");
        }
    }

    public async Task<(ApiAuthResponse? Response, string? Error)>
        LoginAsync(LoginViewModel model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("RaceDayApi");

            var response = await client.PostAsJsonAsync(
                "api/Auth/login", model);

            if (!response.IsSuccessStatusCode)
                return (null, await response.Content.ReadAsStringAsync());

            var result = await response.Content
                .ReadFromJsonAsync<ApiAuthResponse>();

            return (result, result == null
                ? "The API returned an empty response."
                : null);
        }
        catch (HttpRequestException)
        {
            return (null, "Unable to connect to the API.");
        }
    }
}

