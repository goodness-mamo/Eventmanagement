namespace EventManagement.Mvc.Services
{
    public class ApiConnectionService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ApiConnectionService(
            IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<(bool IsConnected, string Message)>
            CheckConnectionAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("RaceDayApi");

                var response = await client.GetAsync("swagger/index.html");

                return response.IsSuccessStatusCode
                    ? (true, "EventManagementApi is connected.")
                    : (false, $"API returned: {(int)response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection failed: {ex.Message}");
            }
        }
    }
}