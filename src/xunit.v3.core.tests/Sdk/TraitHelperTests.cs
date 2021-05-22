using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TraitHelperTests
{
	[Fact]
	public void MethodWithNoTraits_ReturnsEmptyList()
	{
		var method = typeof(ClassUnderTest).GetMethod("NoTraits");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Empty(traits);
	}

	[Fact]
	public void MethodWithTraitAttribute_ReturnsTrait()
	{
		var method = typeof(ClassUnderTest).GetMethod("Trait");

		var traits = TraitHelper.GetTraits(method!);

		var value = Assert.Single(traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase));
		Assert.Equal("foo = bar", value);
	}

	[Fact]
	public void MethodWithCustomTraitAttribute_ReturnsTraitValues()
	{
		var method = typeof(ClassUnderTest).GetMethod("CustomTrait");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Collection(
			traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
			value => Assert.Equal("Baz = 2112", value),
			value => Assert.Equal("Foo = Biff", value)
		);
	}

	[Fact]
	public void CombinesMultipleTraitSources()
	{
		var method = typeof(ClassUnderTest).GetMethod("MultipleTraits");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Collection(
			traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
			value => Assert.Equal("Baz = 2112", value),
			value => Assert.Equal("foo = bar", value),
			value => Assert.Equal("Foo = Biff", value),
			value => Assert.Equal("Hello = World", value)
		);
	}

	[Fact]
	public void MethodWithInheritedCustomTraitAttribute_ReturnsTraitValues()
	{
		var method = typeof(ClassUnderTest).GetMethod("InheritedTrait");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Collection(
			traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
			value => Assert.Equal("Baz = 2112", value),
			value => Assert.Equal("Foo = Biff", value)
		);
	}

	[Fact]
	public void MethodWithInheritedTriceCustomTraitAttribute_ReturnsTraitValues()
	{
		var method = typeof(ClassUnderTest).GetMethod("InheritedTriceTrait");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Collection(
			traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
			value => Assert.Equal("Baz = 2112", value),
			value => Assert.Equal("Foo = Biff", value)
		);
	}

	[Fact]
	public void CombinesInheritedMultipleTraitSources()
	{
		var method = typeof(ClassUnderTest).GetMethod("InheritedMultipleTraits");

		var traits = TraitHelper.GetTraits(method!);

		Assert.Collection(
			traits.Select(kvp => $"{kvp.Key} = {kvp.Value}").OrderBy(_ => _, StringComparer.OrdinalIgnoreCase),
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

		[InheritedCustomTrait]
		public void InheritedTrait() { }

		[InheritedTriceCustomTrait]
		public void InheritedTriceTrait() { }

		[Trait("foo", "bar")]
		[Trait("Hello", "World")]
		[InheritedCustomTrait]
		public void InheritedMultipleTraits() { }
	}

	class InheritedTriceCustomTrait : InheritedTwiceCustomTrait { }

	class InheritedTwiceCustomTrait : InheritedCustomTrait { }

	class InheritedCustomTrait : CustomTraitAttribute { }

	[TraitDiscoverer(typeof(CustomTraitDiscoverer))]
	class CustomTraitAttribute : Attribute, ITraitAttribute { }

	class CustomTraitDiscoverer : ITraitDiscoverer
	{
		public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits(_IAttributeInfo traitAttribute) =>
			new[]
			{
				new KeyValuePair<string, string>("Foo", "Biff"),
				new KeyValuePair<string, string>("Baz", "2112")
			};
	}
}
