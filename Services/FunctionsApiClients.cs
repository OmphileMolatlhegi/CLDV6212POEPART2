using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ABCRetails.Models;
using Microsoft.AspNetCore.Http;


namespace ABCRetails.Services
{
    public class FunctionsApiClient : IFunctionsApi
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
        private readonly ILogger<FunctionsApiClient> _logger;

        // Centralize your Function routes here
        private const string CustomersRoute = "customers";
        private const string ProductsRoute = "products";
        private const string OrdersRoute = "orders";
        private const string UploadsRoute = "uploads/proof-of-payment";


        public FunctionsApiClient(IHttpClientFactory factory, ILogger<FunctionsApiClient> logger)
        {
            _http = factory.CreateClient("Functions");
            
            _logger = logger;
        }

        // ---------- Helpers ----------
        private static HttpContent JsonBody(object obj)
            => new StringContent(JsonSerializer.Serialize(obj, _json), Encoding.UTF8, "application/json");

        private static async Task<T> ReadJsonAsync<T>(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var errorContent = await resp.Content.ReadAsStringAsync();
                var statusCode = (int)resp.StatusCode;

                // Try to extract error message from response
                string errorMessage = $"Request failed with status {statusCode}";

                if (!string.IsNullOrWhiteSpace(errorContent))
                {
                    try
                    {
                        // Try to parse as JSON error response
                        var errorObj = JsonSerializer.Deserialize<Dictionary<string, object>>(errorContent, _json);
                        if (errorObj != null)
                        {
                            if (errorObj.TryGetValue("error", out var error))
                                errorMessage = error.ToString() ?? errorMessage;
                            else if (errorObj.TryGetValue("message", out var message))
                                errorMessage = message.ToString() ?? errorMessage;
                            else if (errorObj.TryGetValue("title", out var title))
                                errorMessage = title.ToString() ?? errorMessage;
                        }
                    }
                    catch
                    {
                        // If not JSON, use the raw content
                        if (errorContent.Length < 500)
                            errorMessage = errorContent;
                    }
                }

                throw new HttpRequestException($"{errorMessage} (Status: {statusCode})");
            }

            var stream = await resp.Content.ReadAsStreamAsync();
            var data = await JsonSerializer.DeserializeAsync<T>(stream, _json);
            return data!;
        }

        // ---------- Customers ----------
        public async Task<List<Customer>> GetCustomersAsync()
            => await ReadJsonAsync<List<Customer>>(await _http.GetAsync(CustomersRoute));

        public async Task<Customer?> GetCustomerAsync(string id)
        {
            var resp = await _http.GetAsync($"{CustomersRoute}/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadJsonAsync<Customer>(resp);
        }

        public async Task<Customer> CreateCustomerAsync(Customer c)
            => await ReadJsonAsync<Customer>(await _http.PostAsync(CustomersRoute, JsonBody(c)));

        public async Task<Customer> UpdateCustomerAsync(string id, Customer c)
            => await ReadJsonAsync<Customer>(await _http.PutAsync($"{CustomersRoute}/{id}", JsonBody(c)));

        public async Task DeleteCustomerAsync(string id)
            => (await _http.DeleteAsync($"{CustomersRoute}/{id}")).EnsureSuccessStatusCode();

        // ---------- Products ----------
        public async Task<List<Product>> GetProductsAsync()
            => await ReadJsonAsync<List<Product>>(await _http.GetAsync(ProductsRoute));

        public async Task<Product?> GetProductAsync(string id)
        {
            var resp = await _http.GetAsync($"{ProductsRoute}/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadJsonAsync<Product>(resp);
        }

        public async Task<Product> CreateProductAsync(Product p, IFormFile? imageFile)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(p.ProductName), "ProductName");
            form.Add(new StringContent(p.Description ?? string.Empty), "Description");
            form.Add(new StringContent(p.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
            form.Add(new StringContent(p.StockAvailable.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockAvailable");
            if (!string.IsNullOrWhiteSpace(p.ImageUrl)) form.Add(new StringContent(p.ImageUrl), "ImageUrl");
            if (imageFile is not null && imageFile.Length > 0)
            {
                var file = new StreamContent(imageFile.OpenReadStream());
                file.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType ?? "application/octet-stream");
                form.Add(file, "ImageFile", imageFile.FileName);
            }
            return await ReadJsonAsync<Product>(await _http.PostAsync(ProductsRoute, form));
        }

        public async Task<Product> UpdateProductAsync(string id, Product p, IFormFile? imageFile)
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(p.ProductName), "ProductName");
            form.Add(new StringContent(p.Description ?? string.Empty), "Description");
            form.Add(new StringContent(p.Price.ToString(System.Globalization.CultureInfo.InvariantCulture)), "Price");
            form.Add(new StringContent(p.StockAvailable.ToString(System.Globalization.CultureInfo.InvariantCulture)), "StockAvailable");
            if (!string.IsNullOrWhiteSpace(p.ImageUrl)) form.Add(new StringContent(p.ImageUrl), "ImageUrl");
            if (imageFile is not null && imageFile.Length > 0)
            {
                var file = new StreamContent(imageFile.OpenReadStream());
                file.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType ?? "application/octet-stream");
                form.Add(file, "ImageFile", imageFile.FileName);
            }
            return await ReadJsonAsync<Product>(await _http.PutAsync($"{ProductsRoute}/{id}", form));
        }

        public async Task DeleteProductAsync(string id)
            => (await _http.DeleteAsync($"{ProductsRoute}/{id}")).EnsureSuccessStatusCode();

        // ---------- Orders ----------
        public async Task<List<Order>> GetOrdersAsync()
            => await ReadJsonAsync<List<Order>>(await _http.GetAsync(OrdersRoute));

        public async Task<Order?> GetOrderAsync(string id)
        {
            var resp = await _http.GetAsync($"{OrdersRoute}/{id}");
            if (resp.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
            return await ReadJsonAsync<Order>(resp);
        }

        public async Task<Order> CreateOrderAsync(string customerId, string productId, int quantity)
        {
            var payload = new { customerId, productId, quantity };
            var response = await _http.PostAsync(OrdersRoute, JsonBody(payload));
            return await ReadJsonAsync<Order>(response);
        }

        public async Task UpdateOrderStatusAsync(string id, string newStatus)
        {
            var payload = new { status = newStatus };
            var response = await _http.PatchAsync($"{OrdersRoute}/{id}/status", JsonBody(payload));

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to update order status: {errorContent}");
            }
        }

        public async Task DeleteOrderAsync(string id)
            => (await _http.DeleteAsync($"{OrdersRoute}/{id}")).EnsureSuccessStatusCode();

        // ---------- Uploads ----------
        public async Task<string> UploadProofOfPaymentAsync(IFormFile file, string? orderId, string? customerName)
        {
            using var form = new MultipartFormDataContent();
            var sc = new StreamContent(file.OpenReadStream());
            sc.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
            form.Add(sc, "ProofOfPayment", file.FileName);
            if (!string.IsNullOrWhiteSpace(orderId)) form.Add(new StringContent(orderId), "OrderId");
            if (!string.IsNullOrWhiteSpace(customerName)) form.Add(new StringContent(customerName), "CustomerName");

            var resp = await _http.PostAsync(UploadsRoute, form);
            resp.EnsureSuccessStatusCode();

            var doc = await ReadJsonAsync<Dictionary<string, string>>(resp);
            return doc.TryGetValue("fileName", out var name) ? name : file.FileName;
        }
    }

    // HttpClient PATCH extension
    internal static class HttpClientPatchExtensions
    {
        public static Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
            => client.SendAsync(new HttpRequestMessage(HttpMethod.Patch, requestUri) { Content = content });
    }
}
