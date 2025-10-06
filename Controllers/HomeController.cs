using Microsoft.AspNetCore.Mvc;
using ABCRetails.Services;
using ABCRetails.Models.ViewModels;
using ABCRetails.Models;

namespace ABCRetails.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFunctionsApi functionsApi, ILogger<HomeController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetProductsAsync();
                var customers = await _functionsApi.GetCustomersAsync();
                var orders = await _functionsApi.GetOrdersAsync();

                var vm = new HomeViewModel
                {
                    FeaturedProducts = products.Take(5),
                    CustomerCount = customers.Count,
                    ProductCount = products.Count,
                    OrderCount = orders.Count
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page data");

                // Return view with empty data in case of error
                var vm = new HomeViewModel
                {
                    FeaturedProducts = new List<Product>(),
                    CustomerCount = 0,
                    ProductCount = 0,
                    OrderCount = 0
                };

                TempData["Error"] = "Unable to load dashboard data. Please try again later.";
                return View(vm);
            }
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}