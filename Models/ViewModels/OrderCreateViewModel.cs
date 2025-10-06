using Microsoft.AspNetCore.Mvc.Rendering;

namespace ABCRetails.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        public string? CustomerId { get; set; }
        public string? ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public string Status { get; set; } = "Pending";

        public IEnumerable<SelectListItem> Customers { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Products { get; set; } = new List<SelectListItem>();
    }
}