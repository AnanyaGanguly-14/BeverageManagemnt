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
                    return "Beverage Category not found.";

                case "Err_002":
                    return "Beverage Category already exists.";

                case "Err_004":
                    return "Valid Beverage Category is required.";

                case "Err_005":
                    return "Beverage Category not found.";

                default:
                    return "An unexpected error occurred.";
            }


        }

        private static string ErrorMessages(string? message, System.Exception innerException)
        {
            if (message == "Err_003" && innerException.Message.Contains("Connection", StringComparison.OrdinalIgnoreCase))
            {
                return "Error in adding/modifying Beverage type";
            }
            return "An error occurred while accessing the database.";
        }
    }
}
