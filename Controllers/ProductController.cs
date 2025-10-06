using Microsoft.AspNetCore.Mvc;
using ABCRetails.Services;
using ABCRetails.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace ABCRetails.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _api;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi api, ILogger<ProductController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // GET: Product
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _api.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products from API");
                TempData["Error"] = "An error occurred while retrieving products.";
                return View(new List<Product>());
            }
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var product = await _api.GetProductAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product details for ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Product/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            try
            {
                // Set required fields
                if (string.IsNullOrEmpty(product.PartitionKey))
                    product.PartitionKey = "PRODUCT";

                if (string.IsNullOrEmpty(product.RowKey))
                    product.RowKey = Guid.NewGuid().ToString();

                if (string.IsNullOrEmpty(product.ProductId))
                    product.ProductId = Guid.NewGuid().ToString();

                var saved = await _api.CreateProductAsync(product, imageFile);
                TempData["Success"] = $"Product '{saved.ProductName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                return View(product);
            }
        }

        // GET: Product/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            try
            {
                var product = await _api.GetProductAsync(id);
                return product is null ? NotFound() : View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product for edit with ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving the product for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            try
            {
                // Ensure IDs are preserved
                if (string.IsNullOrEmpty(product.ProductId) && !string.IsNullOrEmpty(product.Id))
                {
                    product.ProductId = product.Id;
                }

                var updated = await _api.UpdateProductAsync(product.ProductId, product, imageFile);
                TempData["Success"] = $"Product '{updated.ProductName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", product.ProductId);
                ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                return View(product);
            }
        }

        // GET: Product/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            try
            {
                var product = await _api.GetProductAsync(id);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product for deletion with ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while retrieving the product for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                await _api.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Validate Product ID (if needed for custom IDs)
        public async Task<IActionResult> ValidateProductId(string productId)
        {
            if (string.IsNullOrEmpty(productId))
            {
                return Json(true);
            }

            try
            {
                // Note: This assumes you want to check if a product with a specific custom ID exists
                // The Functions API generates its own IDs, so this might not be needed
                var existingProduct = await _api.GetProductAsync(productId);
                return Json(existingProduct == null);
            }
            catch
            {
                // If there's an error, assume it's valid to avoid blocking the user
                return Json(true);
            }
        }

        // GET: Product/LowStock
        public async Task<IActionResult> LowStock()
        {
            try
            {
                var products = await _api.GetProductsAsync();
                var lowStockProducts = products.Where(p => p.StockAvailable < (p.LowStockThreshold > 0 ? p.LowStockThreshold : 10)).ToList();
                return View(lowStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving low stock products");
                TempData["Error"] = "An error occurred while retrieving low stock products.";
                return View(new List<Product>());
            }
        }

        // GET: Product/OutOfStock
        public async Task<IActionResult> OutOfStock()
        {
            try
            {
                var products = await _api.GetProductsAsync();
                var outOfStockProducts = products.Where(p => p.StockAvailable <= 0).ToList();
                return View(outOfStockProducts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving out of stock products");
                TempData["Error"] = "An error occurred while retrieving out of stock products.";
                return View(new List<Product>());
            }
        }
    }
}
