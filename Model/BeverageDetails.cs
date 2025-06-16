using BeverageManagemnt.Interface;
using Common;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Model
{
    public class BeverageDetails : IHasExceptionDetails
    {
        [JsonPropertyName("beverage_deatils_id")]
        public int BEVERAGE_DETAILS_ID { get; set; }
        [JsonPropertyName("beverage_size")]
        public string? BEVERAGE_SIZE { get; set; }

        [JsonPropertyName("beverage_price")]
        [DecimalOnly(ErrorMessage = "Price must be a valid decimal number.")]
        public decimal BEVERAGE_PRICE { get; set; }
        public BeverageCategory? beverageCategory { get; set; } // Navigation property one to many relationship

        [ForeignKey("beverageCategory")]
        [JsonPropertyName("beverage_category_id")]
        public int BEVERAGE_CATEGORY_ID { get; set; }

        [NotMapped]
        public ExceptionDetails? ExceptionDetails { get; set; }


    }

}
