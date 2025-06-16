using Model;

namespace BeverageManagemnt.Interface
{
    public interface IHasExceptionDetails
    {
        ExceptionDetails? ExceptionDetails { get; set; }
    }
}
