using Microsoft.AspNetCore.Mvc;
using ABCRetails.Services;
using ABCRetails.Models;

namespace ABCRetails.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IFunctionsApi functionsApi, ILogger<UploadController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index() => View(new FileUploadModel());

        [HttpPost]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (model.ProofOfPayment == null)
            {
                ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                return View(model);
            }

            // Validate file size and type
            if (model.ProofOfPayment.Length == 0)
            {
                ModelState.AddModelError("ProofOfPayment", "The selected file is empty.");
                return View(model);
            }

            if (model.ProofOfPayment.Length > 10 * 1024 * 1024) // 10MB limit
            {
                ModelState.AddModelError("ProofOfPayment", "File size must be less than 10MB.");
                return View(model);
            }

            var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
            var fileExtension = Path.GetExtension(model.ProofOfPayment.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ProofOfPayment", "Please upload a valid file type (PDF, JPG, PNG, DOC, DOCX).");
                return View(model);
            }

            try
            {
                var fileName = await _functionsApi.UploadProofOfPaymentAsync(
                    model.ProofOfPayment,
                    model.OrderId,
                    model.CustomerName
                );

                TempData["Success"] = $"File '{fileName}' uploaded successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading proof of payment file");
                ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            try
            {
                var orders = await _functionsApi.GetOrdersAsync();
                return View(orders.Where(o => string.IsNullOrEmpty(o.ProofOfPaymentUrl)).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading orders for upload");
                TempData["Error"] = $"Error loading orders: {ex.Message}";
                return View(new List<Order>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> UploadForOrder(string orderId)
        {
            if (string.IsNullOrEmpty(orderId))
                return NotFound();

            try
            {
                var order = await _functionsApi.GetOrderAsync(orderId);
                if (order == null)
                    return NotFound();

                var model = new FileUploadModel
                {
                    OrderId = orderId,
                    CustomerName = $"{order.CustomerName}"
                };

                return View("Index", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order for upload");
                TempData["Error"] = $"Error loading order details: {ex.Message}";
                return RedirectToAction(nameof(Orders));
            }
        }
    }
}