using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ABCRetails.Services;
using ABCRetails.Models;
using ABCRetails.Models.ViewModels;

namespace ABCRetails.Controllers
{
    public class OrderController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IFunctionsApi functionsApi, ILogger<OrderController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                return View(orders.OrderByDescending(o => o.OrderDate));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders");
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(new List<Order>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var customers = await _functionsApi.GetCustomersAsync();
                var products = await _functionsApi.GetProductsAsync();

                var vm = new OrderCreateViewModel
                {
                    Customers = customers.Select(c => new SelectListItem($"{c.Name} {c.Surname}", c.CustomerId)),
                    Products = products.Select(p => new SelectListItem(p.ProductName, p.ProductId))
                };
                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create order form data");
                TempData["Error"] = $"Error loading form data: {ex.Message}";
                return View(new OrderCreateViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                // Reload dropdown data if validation fails
                try
                {
                    var customers = await _functionsApi.GetCustomersAsync();
                    var products = await _functionsApi.GetProductsAsync();
                    vm.Customers = customers.Select(c => new SelectListItem($"{c.Name} {c.Surname}", c.CustomerId));
                    vm.Products = products.Select(p => new SelectListItem(p.ProductName, p.ProductId));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error reloading form data");
                }
                return View(vm);
            }

            try
            {
                // Create order using Functions API
                var order = await _functionsApi.CreateOrderAsync(
                    vm.CustomerId ?? "",
                    vm.ProductId ?? "",
                    vm.Quantity
                );

                TempData["Success"] = $"Order created successfully! Order ID: {order.OrderId}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", $"Error creating order: {ex.Message}");

                // Reload dropdown data
                try
                {
                    var customers = await _functionsApi.GetCustomersAsync();
                    var products = await _functionsApi.GetProductsAsync();
                    vm.Customers = customers.Select(c => new SelectListItem($"{c.Name} {c.Surname}", c.CustomerId));
                    vm.Products = products.Select(p => new SelectListItem(p.ProductName, p.ProductId));
                }
                catch (Exception reloadEx)
                {
                    _logger.LogError(reloadEx, "Error reloading form data");
                }

                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                var order = orders.FirstOrDefault(o => o.PartitionKey == partitionKey && o.RowKey == rowKey);

                if (order == null)
                    return NotFound();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order for edit");
                TempData["Error"] = $"Error loading order: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Order model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Use the correct method to update order status
                await _functionsApi.UpdateOrderStatusAsync(model.OrderId, model.Status);

                TempData["Success"] = "Order updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order");
                ModelState.AddModelError("", $"Error updating order: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                var order = orders.FirstOrDefault(o => o.PartitionKey == partitionKey && o.RowKey == rowKey);

                if (order == null)
                    return NotFound();

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details");
                TempData["Error"] = $"Error loading order details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                TempData["Error"] = "Invalid order identifier";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                var order = orders.FirstOrDefault(o => o.PartitionKey == partitionKey && o.RowKey == rowKey);

                if (order == null)
                {
                    TempData["Error"] = "Order not found";
                    return RedirectToAction(nameof(Index));
                }

                await _functionsApi.DeleteOrderAsync(order.OrderId);
                TempData["Success"] = "Order deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order");
                TempData["Error"] = $"Error deleting order: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}