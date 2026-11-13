using Microsoft.AspNetCore.Mvc;
using EventManagement.Mvc.Services;

namespace EventManagement.Mvc.Controllers
{
    public class ApiStatusController : Controller
    {
        private readonly ApiConnectionService _apiService;

        public ApiStatusController(ApiConnectionService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var result = await _apiService.CheckConnectionAsync();

            ViewBag.ApiConnected = result.IsConnected;
            ViewBag.ApiMessage = result.Message;

            return View();
        }
    }
}
