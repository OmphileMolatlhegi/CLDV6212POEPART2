using Microsoft.AspNetCore.Mvc;
using ABCRetails.Services;
using ABCRetails.Models;

namespace ABCRetails.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public CustomerController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _functionsApi.GetCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customers: {ex.Message}";
                return View(new List<Customer>());
            }
        }

        [HttpGet]
        public IActionResult Create() => View(new Customer());

        [HttpPost]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Generate IDs if not provided
                if (string.IsNullOrEmpty(model.CustomerId))
                    model.CustomerId = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(model.RowKey))
                    model.RowKey = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(model.PartitionKey))
                    model.PartitionKey = "CUSTOMER";

                var createdCustomer = await _functionsApi.CreateCustomerAsync(model);
                TempData["Success"] = "Customer created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            try
            {
                // Since Functions API uses CustomerId, we need to get all and filter
                var customers = await _functionsApi.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);

                if (customer == null)
                    return NotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // Ensure required fields are set
                if (string.IsNullOrEmpty(model.PartitionKey))
                    model.PartitionKey = "CUSTOMER";

                var updatedCustomer = await _functionsApi.UpdateCustomerAsync(model.CustomerId, model);
                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
            {
                TempData["Error"] = "Invalid customer identifier";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                // Get the customer first to find the CustomerId
                var customers = await _functionsApi.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found";
                    return RedirectToAction(nameof(Index));
                }

                await _functionsApi.DeleteCustomerAsync(customer.CustomerId);
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(string partitionKey, string rowKey)
        {
            if (string.IsNullOrEmpty(partitionKey) || string.IsNullOrEmpty(rowKey))
                return NotFound();

            try
            {
                var customers = await _functionsApi.GetCustomersAsync();
                var customer = customers.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);

                if (customer == null)
                    return NotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error loading customer details: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}