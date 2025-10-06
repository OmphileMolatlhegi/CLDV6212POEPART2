using ABCRetails.Models;

namespace ABCRetails.Controllers
{
    internal class UploadPaymentProofViewModel
    {
        public object Orders { get; set; }
        public List<Customer> Customers { get; set; }
        public object ProofFile { get; internal set; }
    }
}