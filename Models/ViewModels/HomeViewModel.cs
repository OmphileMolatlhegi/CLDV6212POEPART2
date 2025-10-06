using System.Collections.Generic;

namespace ABCRetails.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<ABCRetails.Models.Product>? FeaturedProducts { get; set; }
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}