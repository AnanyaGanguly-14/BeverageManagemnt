

using System.Text.Json.Serialization;

namespace Model
{
    public class ExceptionDetails
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("message")]
        public string Message { get; set; }

        public ExceptionDetails() { }
        public ExceptionDetails(string code, string message)
        {
            this.Code = code;
            this.Message = message;
        }
    }
}
