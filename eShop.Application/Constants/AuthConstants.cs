namespace eShop.Application.Constants;

public static class AuthConstants
{
    // Generic Errors
    public const string ERROR_LOGIN = "An error occurred during the login process.";

    // Success Messages
    public const string ADMIN_LOGIN_SUCCESS = "Admin logged in successfully.";
    public const string CUSTOMER_LOGIN_SUCCESS = "User logged in successfully.";
    public const string CUSTOMER_REGISTER_SUCCESS = "User registered successfully.";

    // Validation Errors
    public const string USER_NOT_FOUND = "User not found.";
    public const string INVALID_CREDENTIAL = "Invalid email or password.";
    public const string ACCOUNT_ALREADY_EXISTS = "An account with the provided credentials already exists.";

}
