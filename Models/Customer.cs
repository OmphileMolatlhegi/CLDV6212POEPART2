using Azure;
using Azure.Data.Tables;

namespace ABCRetails.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "CUSTOMER";
        public string RowKey { get; set; } = Guid.NewGuid().ToString();
        public string CustomerId { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
        public DateTime CreatedDate { get; internal set; }
        public int OrdersCount { get; internal set; }
        public string? Status { get; internal set; }
        public string? Id { get; internal set; }
        public object FirstName { get; internal set; }
        public object LastName { get; internal set; }
    }
}