namespace eShop.Application.Constants;

public static class CategoryConstants
{
    public const string ERROR_RETRIEVING_CATEGORIES = "An error occurred while retrieving the categories.";
    public const string ERROR_CREATING_CATEGORY = "An error occurred while creating the category.";
    public const string ERROR_EDITING_CATEGORY = "An error occurred while editing the category.";
    public const string ERROR_DELETING_CATEGORY = "An error occurred while deleting category.";
    public const string ERROR_GET_CATEGORY = "An error occurred while geting category.";

    public const string CategorySuccessfullyCreated = "Category was successfully created.";
    public const string CategorySuccessfullyUpdated = "Category was successfully updated.";
    public const string CATEGORY_SUCCESSFULLY_DELETED = "Category successfully deleted.";

    public const string CategoryExist = "Category with that name already exists.";
    public const string CategoryDoesNotExist = "Category doesn't exist.";
    public const string CATEGORY_HAS_RELATED_ENTITIES = "Category cannot be deleted because it has related subcategories or products";


    public const string CategoryHasProducts = "Cannot delete. {0} product(s) are assigned to this category or its subcategories. Move or remove them first.";
    public const string CategoriesDeletedMessage = "Deleted {0} categor{1}.";
    public const string CategoryCannotBeOwnParent = "A category cannot be its own parent.";




}
