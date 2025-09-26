namespace eShop.Application.Constants.Admin;

public static class AdminCategoryConstants
{
    public const string ErrorRetrievingCategories = "An error occurred while retrieving the categories.";
    public const string ErrorCreatingCategory = "An error occurred while creating the category.";
    public const string ErrorEditingCategory = "An error occurred while editing the category.";
    public const string ErrorDeletingCategory = "An error occurred while deleting category.";
    public const string ErrorGetCategory = "An error occurred while getting category.";

    public const string CategorySuccessfullyCreated = "Category was successfully created.";
    public const string CategorySuccessfullyUpdated = "Category was successfully updated.";
    public const string CategorySuccessfullyDeleted = "Category successfully deleted.";

    public const string CategoryExist = "Category with that name already exists.";
    public const string CategoryDoesNotExist = "Category doesn't exist.";
    public const string CategoryHasRelatedEntities = "Category cannot be deleted because it has related subcategories or products.";

    public const string CategoryHasProducts = "Cannot delete. {0} product(s) are assigned to this category or its subcategories. Move or remove them first.";
    public const string CategoriesDeletedMessage = "Deleted {0} categor{1}.";
    public const string CategoryCannotBeOwnParent = "A category cannot be its own parent.";
    public const string CategoryCannotBeMovedUnderDescendant = "A category cannot be moved under one of its own descendants.";
}
