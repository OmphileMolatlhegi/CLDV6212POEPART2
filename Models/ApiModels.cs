using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCRetailOnlineFunctions.Models
{
    internal class ApiModels
    {

        public record CustomerDto(string Id, string Name, string Surname, string Username, string Email, string ShippingAddress);
        public record ProductDto(string Id, string ProductName, string Description, decimal Price, int StockAvailable, string ImageUrl);
        public record OrderDto(
            string Id, string CustomerId, string ProductId, string ProductName,
            int Quantity, decimal UnitPrice, decimal TotalAmount, DateTimeOffset OrderDateUtc, string Status);

    }
    public class CustomerApiModel
    {
        public string? Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ProductApiModel
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class OrderApiModel
    {
        public string? Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class OrderQueueMessage
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
