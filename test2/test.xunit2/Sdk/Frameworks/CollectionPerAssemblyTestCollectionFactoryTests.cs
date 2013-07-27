using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class CollectionPerAssemblyTestCollectionFactoryTests
{
    [Fact]
    public void ReturnsDefaultTestCollectionForUndecoratedTestClass()
    {
        var type1 = Mocks.TypeInfo("type1");
        var type2 = Mocks.TypeInfo("type2");
        var assembly = Mocks.AssemblyInfo();
        assembly.AssemblyPath.Returns(@"C:\Foo\bar.dll");
        var factory = new CollectionPerAssemblyTestCollectionFactory(assembly);

        var result1 = factory.Get(type1);
        var result2 = factory.Get(type2);

        Assert.Same(result1, result2);
        Assert.Equal("Test collection for bar.dll", result1.DisplayName);
    }

    [Fact]
    public void ClassesDecoratedWithSameCollectionNameAreInSameTestCollection()
    {
        var attr = Mocks.CollectionAttribute("My Collection");
        var type1 = Mocks.TypeInfo("type1", attributes: new[] { attr });
        var type2 = Mocks.TypeInfo("type2", attributes: new[] { attr });
        var assembly = Mocks.AssemblyInfo();
        assembly.AssemblyPath.Returns(@"C:\Foo\bar.dll");
        var factory = new CollectionPerAssemblyTestCollectionFactory(assembly);

        var result1 = factory.Get(type1);
        var result2 = factory.Get(type2);

        Assert.Same(result1, result2);
        Assert.Equal("My Collection", result1.DisplayName);
    }

    [Fact]
    public void ClassesWithDifferentCollectionNamesHaveDifferentCollectionObjects()
    {
        var type1 = Mocks.TypeInfo("type1", attributes: new[] { Mocks.CollectionAttribute("Collection 1") });
        var type2 = Mocks.TypeInfo("type2", attributes: new[] { Mocks.CollectionAttribute("Collection 2") });
        var assembly = Mocks.AssemblyInfo();
        assembly.AssemblyPath.Returns(@"C:\Foo\bar.dll");
        var factory = new CollectionPerAssemblyTestCollectionFactory(assembly);

        var result1 = factory.Get(type1);
        var result2 = factory.Get(type2);

        Assert.NotSame(result1, result2);
        Assert.Equal("Collection 1", result1.DisplayName);
        Assert.Equal("Collection 2", result2.DisplayName);
    }
}