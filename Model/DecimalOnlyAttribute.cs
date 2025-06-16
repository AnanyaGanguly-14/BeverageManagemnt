
using System.ComponentModel.DataAnnotations;

namespace Model
{
    public class DecimalOnlyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string str)
            {
                return false;
            }
            else if (value is decimal) return true;
            return false;
        }
    }
}