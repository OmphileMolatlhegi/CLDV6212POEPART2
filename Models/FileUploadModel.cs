using Microsoft.AspNetCore.Http;

namespace ABCRetails.Models
{
    public class FileUploadModel
    {
        public IFormFile? ProofOfPayment { get; set; }
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
    }
}