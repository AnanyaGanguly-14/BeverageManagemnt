namespace BeverageManagemnt.Exception
{
    public class BeverageException : System.Exception
    {
        public BeverageException(string? message) : base(message)

        {
            ErrorMessages(message);
        }

        private static void ErrorMessages(string? message)
        {
            if (message == "Err_002")
            {
                throw new BeverageException("Beverage Category not found.");
            }
            else if (message == "Err_001")
            {
                throw new BeverageException("Beverage Category already exists.");
            }
            else if(string.IsNullOrEmpty(message))
            {
                throw new BeverageException("An unexpected error occurred.");
            }
        }
    }
}
