
using System.Text.Json.Serialization;

namespace Model
{
    public class Orders
    {
        [JsonPropertyName("customer_name")]
        public string? CUSTOMER_NAME {  get; set; }
        [JsonPropertyName("customer_contact")]
        public string? CUSTOMER_CONTACT {  get; set; }
        public List<OrderItems>? OrderItems { get; set; }
        
    }
}
