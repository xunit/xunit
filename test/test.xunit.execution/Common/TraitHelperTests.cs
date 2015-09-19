using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class TraitHelperTests
{
    [Fact]
    public void MethodWithNoTraits_ReturnsEmptyList()
    {
        var method = typeof(ClassUnderTest).GetMethod("NoTraits");

        var traits = TraitHelper.GetTraits(method);

        Assert.Empty(traits);
    }

    [Fact]
    public void MethodWithTraitAttribute_ReturnsTrait()
    {
        var method = typeof(ClassUnderTest).GetMethod("Trait");

        var traits = TraitHelper.GetTraits(method);

        Assert.Collection(traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
            value => Assert.Equal("foo = bar", value)
        );
    }

    [Fact]
    public void MethodWithCustomTraitAttribute_ReturnsTraitValues()
    {
        var method = typeof(ClassUnderTest).GetMethod("CustomTrait");

        var traits = TraitHelper.GetTraits(method);

        Assert.Collection(traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
            value => Assert.Equal("Baz = 2112", value),
            value => Assert.Equal("Foo = Biff", value)
        );
    }

    [Fact]
    public void CombinesMultipleTraitSources()
    {
        var method = typeof(ClassUnderTest).GetMethod("MultipleTraits");

        var traits = TraitHelper.GetTraits(method);

        Assert.Collection(traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
            value => Assert.Equal("Baz = 2112", value),
            value => Assert.Equal("foo = bar", value),
            value => Assert.Equal("Foo = Biff", value),
            value => Assert.Equal("Hello = World", value)
        );
    }

    class ClassUnderTest
    {
        [Obsolete]    // Irrelevant attribute
        public void NoTraits() { }

        [Trait("foo", "bar")]
        public void Trait() { }

        [CustomTrait]
        public void CustomTrait() { }

        [Trait("foo", "bar")]
        [Trait("Hello", "World")]
        [CustomTrait]
        public void MultipleTraits() { }
    }

    [TraitDiscoverer("TraitHelperTests+CustomTraitDiscoverer", "test.xunit.execution")]
    class CustomTraitAttribute : Attribute, ITraitAttribute { }

    class CustomTraitDiscoverer : ITraitDiscoverer
    {
        public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
        {
            yield return new KeyValuePair<string, string>("Foo", "Biff");
            yield return new KeyValuePair<string, string>("Baz", "2112");
        }
    }
}
