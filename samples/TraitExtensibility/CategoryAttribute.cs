using Xunit;

public class CategoryAttribute : TraitAttribute
{
    public CategoryAttribute(string category)
        : base("Category", category) {}
}