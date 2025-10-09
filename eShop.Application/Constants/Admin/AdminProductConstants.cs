namespace eShop.Application.Constants.Admin;

public static class AdminProductConstants
{
    public const string ErrorRetrievingProducts = "An error occurred while retrieving the products.";
    public const string ErrorCreatingProduct = "An error occurred while creating the product.";
    public const string ErrorEditingProduct = "An error occurred while editing the product.";
    public const string ErrorDeletingProduct = "An error occurred while deleting the product.";
    public const string ErrorGetProduct = "An error occurred while getting the product.";

    public const string ProductSuccessfullyCreated = "Product was successfully created.";
    public const string ProductSuccessfullyUpdated = "Product was successfully updated.";
    public const string ProductSuccessfullyDeleted = "Product was successfully deleted.";

    public const string ProductExist = "Product with that name already exists.";
    public const string ProductDoesNotExist = "Product doesn't exist.";
    public const string ProductHasRelatedEntities = "Product cannot be deleted because it has related orders or dependencies.";
    public const string ProductsAllowedOnlyOnLeafCategories = "Products are allowed only on leaf categories.";

}
