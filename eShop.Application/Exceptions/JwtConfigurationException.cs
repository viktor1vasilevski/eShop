namespace eShop.Application.Exceptions;

public class JwtConfigurationException : Exception
{
    public JwtConfigurationException(string message) : base(message) { }
}
