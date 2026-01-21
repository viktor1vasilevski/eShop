namespace eShop.Application.Exceptions;

public class ExternalDependencyException : Exception
{
    public ExternalDependencyException(string message, Exception? inner = null) : base(message, inner) { }
}
