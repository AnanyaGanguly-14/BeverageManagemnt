

using System.Text.Json.Serialization;

namespace Model
{
    public class OrderItems : BeverageDetails
    {
        [JsonPropertyName("quantity")]
        public int QUANTITY { get; set; }
        [JsonPropertyName("total_price")]
        public decimal TOTAL_PRICE { get; set; }
    }
}
