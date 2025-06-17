using System.Drawing;

namespace BeverageManagemnt.Exception
{
    public class BeverageServiceException : System.Exception
    {
        public string? ErrorCode { get; }
        public BeverageServiceException(string? message) : base(ErrorMessages(message))
        {

            ErrorCode = message;
        }

        public BeverageServiceException(string? message, System.Exception innerException) : base(ErrorMessages(message), innerException)
        {
            ErrorCode = message;
        }

        private static string ErrorMessages(string? message)
        {
         
            switch (message)
            {
                case "Err_001":
                    return "Contact number must contain only digits.";

                case "Err_002":
                    return "Beverage Category already exists.";

                case "Err_004":
                    return "Valid Beverage Details is required.";

                case "Err_005":
                    return "Beverage Category not found.";

                case "Err_006":
                    return "Mobile numner cannot be more than 10 digits.";

                case "Err_007":
                    return "Mobile number must not contain spaces.";

                case "Err_DUPLICATE":
                    return "Duplicate Entry";

                default:
                    return "An unexpected error occurred.";
            }


        }

        private static string ErrorMessages(string? message, System.Exception innerException)
        {
            if (message == "Err_003" && innerException.Message.Contains("Connection", StringComparison.OrdinalIgnoreCase))
            {
                return "An error occurred while accessing the database.";
            }
            return "An error occurred from database.";
        }
    }
}
