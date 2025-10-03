using MyLibrary.Models.Dtos;

namespace MyLibrary.Models.Requests
{
    public class RetrieveExpiredOrderRequest
    {
        public int PostamatId { get; set; }
        public string DeliveryDate { get; set; } = null!;  // "yyyy-MM-dd"
        public ParcelSizeDto ParcelSize { get; set; } = null!;
    }
}