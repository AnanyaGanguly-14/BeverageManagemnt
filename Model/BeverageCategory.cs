using BeverageManagemnt.Interface;
using Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Model
{
    public class BeverageCategory : IHasExceptionDetails
    {
        private string? _beverageType;

        [JsonPropertyName("beverage_category_id")]
        public int BEVERAGE_CATEGORY_ID { get; set; }

        [JsonPropertyName("beverage_type")]
        [Required(ErrorMessage = "Beverage type is required.")]

        public string BEVERAGE_TYPE
        {
            get => _beverageType;
            set => _beverageType = value?.ToUpper();
        }
        [NotMapped]
        public ExceptionDetails? ExceptionDetails { get; set; }
        [JsonIgnore]
        public List<BeverageDetails>? BeverageDetails { get; set; }// Navigation property many to one relationship
    }
}
