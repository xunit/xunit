using System;
using Xunit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CategoryAttribute : TraitAttribute
{
    public CategoryAttribute(string category)
        : base("Category", category) {}
}