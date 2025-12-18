namespace eShop.Application.Exceptions;

public class ExternalDependencyException : ApplicationException
{
    public ExternalDependencyException(string message, Exception? inner = null)
        : base(message, inner) { }
}
