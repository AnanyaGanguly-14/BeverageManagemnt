using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Model
{
    public class BeverageCategory
    {
        [JsonPropertyName("beverage_category_id")]
        public int BEVERAGE_CATEGORY_ID { get; set; }

        [JsonPropertyName("beverage_type")]
        [Required(ErrorMessage = "Beverage type is required.")]
        public required string BEVERAGE_TYPE { get; set; }
        [NotMapped]
        public ExceptionDetails? ExceptionDetails { get; set; }
    }
}
