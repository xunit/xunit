using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using ClassWithTraits = NamespaceTwo.ClassWithTraits;
using InnerClassOne = NamespaceOne.ClassInNamespace1.InnerClassOne;
using InnerClassTwo = NamespaceOne.ClassInNamespace1.InnerClassTwo;

public class XunitFiltersTests
{
	static readonly ITestCaseDiscovered InnerClassOne_NameOne = TestData.TestCaseDiscovered<InnerClassOne>(nameof(InnerClassOne.NameOne));
	static readonly ITestCaseDiscovered InnerClassOne_NameTwo = TestData.TestCaseDiscovered<InnerClassOne>(nameof(InnerClassOne.NameTwo));
	static readonly ITestCaseDiscovered InnerClassOne_NameThree = TestData.TestCaseDiscovered<InnerClassOne>(nameof(InnerClassOne.NameThree));
	static readonly ITestCaseDiscovered InnerClassTwo_NameThree = TestData.TestCaseDiscovered<InnerClassTwo>(nameof(InnerClassTwo.NameThree));
	static readonly ITestCaseDiscovered MethodWithFooBarTrait = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.FooBar));
	static readonly ITestCaseDiscovered MethodWithBazBiffTrait = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.BazBiff));
	static readonly ITestCaseDiscovered MethodWithNoTraits = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.NoTraits));
	static readonly ITestCaseDiscovered NonClassTest = TestData.TestCaseDiscovered(traits: TestData.EmptyTraits);
	static readonly ITestCaseDiscovered NonMethodTest = TestData.TestCaseDiscovered(
		testClassName: typeof(ClassWithTraits).FullName,
		testClassNamespace: typeof(ClassWithTraits).Namespace,
		traits: TestData.EmptyTraits
	);

	static IEnumerable<string> WithWildcards(string value)
	{
		yield return value;
		// Case insensitive
		yield return value.ToLowerInvariant();
		yield return value.ToUpperInvariant();
		// Wildcards
		yield return $"*{value.Substring(1)}";
		yield return $"{value.Substring(0, value.Length - 1)}*";
		yield return $"*{value.Substring(1, value.Length - 2)}*";
	}

	[Fact]
	public static void NoFilters_AlwaysPass()
	{
		var filters = new XunitFilters();

		Assert.True(filters.Filter("asm1", NonClassTest));
		Assert.True(filters.Filter("asm1", NonMethodTest));
		Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
		Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
		Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
		Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
		Assert.True(filters.Filter("asm1", MethodWithNoTraits));
		Assert.True(filters.Filter("asm1", MethodWithFooBarTrait));
		Assert.True(filters.Filter("asm1", MethodWithBazBiffTrait));
	}

	[Fact]
	public static void MixedFilters_NotAllowed_SimpleFirst()
	{
		var filters = new XunitFilters();
		filters.AddIncludedClassFilter("class");

		var ex = Record.Exception(() => filters.AddQueryFilter("/1/2/3/4"));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("query", argEx.ParamName);
		Assert.StartsWith("Cannot add query filter; simple filters already exist", argEx.Message);
	}

	public static IEnumerable<TheoryDataRow<Action<XunitFilters>>> SimpleFilterActions =
	[
		new(filter => filter.AddIncludedClassFilter("class")),
		new(filter => filter.AddIncludedMethodFilter("method")),
		new(filter => filter.AddIncludedNamespaceFilter("namespace")),
		new(filter => filter.AddIncludedTraitFilter("name", "value")),
		new(filter => filter.AddExcludedClassFilter("class")),
		new(filter => filter.AddExcludedMethodFilter("method")),
		new(filter => filter.AddExcludedNamespaceFilter("namespace")),
		new(filter => filter.AddExcludedTraitFilter("name", "value")),
	];

	[Theory(DisableDiscoveryEnumeration = true)]
	[MemberData(nameof(SimpleFilterActions))]
	public static void MixedFilters_NotAllowed_QueryFirst(Action<XunitFilters> simpleFilterAction)
	{
		var filters = new XunitFilters();
		filters.AddQueryFilter("/1/2/3/4");

		var ex = Record.Exception(() => simpleFilterAction(filters));

		var argEx = Assert.IsType<ArgumentException>(ex);
		Assert.Equal("query", argEx.ParamName);
		Assert.StartsWith("Cannot add simple filter; query filters already exist", argEx.Message);
	}

	public class QueryFilters
	{
		// These tests aren't meant to be comprehensive tests against the query language; those exist
		// in GraphQueryFilterParserTests.

		[Fact]
		public static void SingleFilter_MatchesQuery()
		{
			var filters = new XunitFilters();
			filters.AddQueryFilter($"/asm1/{typeof(InnerClassOne).Namespace}/{typeof(InnerClassOne).Name}/{nameof(InnerClassOne.NameOne)}");

			Assert.False(filters.Filter("asm1", NonClassTest));
			Assert.False(filters.Filter("asm1", NonMethodTest));
			Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
			Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
			Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
			Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAnOrOperation()
		{
			var filters = new XunitFilters();
			filters.AddQueryFilter($"/asm1/{typeof(InnerClassOne).Namespace}/{typeof(InnerClassOne).Name}");
			filters.AddQueryFilter($"/asm1/{typeof(InnerClassOne).Namespace}/{typeof(InnerClassTwo).Name}");

			Assert.False(filters.Filter("asm1", NonClassTest));
			Assert.False(filters.Filter("asm1", NonMethodTest));
			Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
			Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
			Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
			Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
		}
	}

	public class SimpleFilters
	{
		public static IEnumerable<TheoryDataRow<string>> ClassNameData()
		{
			foreach (var value in WithWildcards(typeof(InnerClassOne).FullName!))
				yield return value;
		}

		public static IEnumerable<TheoryDataRow<string>> MethodNameData()
		{
			foreach (var value in WithWildcards($"{typeof(InnerClassOne).FullName}.{nameof(InnerClassOne.NameOne)}"))
				yield return value;
		}

		public static IEnumerable<TheoryDataRow<string>> NamespaceData()
		{
			foreach (var value in WithWildcards(typeof(InnerClassOne).Namespace!))
				yield return value;
		}

		public static IEnumerable<TheoryDataRow<string, string>> TraitData()
		{
			foreach (var name in WithWildcards("foo"))
				foreach (var value in WithWildcards("bar"))
					yield return (name, value);
		}

		public static class ExcludedClasses
		{
			[Theory]
			[MemberData(nameof(ClassNameData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_ExcludesMatchingClass(string query)
			{
				var filters = new XunitFilters();
				filters.AddExcludedClassFilter(query);

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAnOrOperation()
			{
				var filters = new XunitFilters();
				filters.AddExcludedClassFilter(typeof(InnerClassOne).FullName!);
				filters.AddExcludedClassFilter(typeof(InnerClassTwo).FullName!.ToUpperInvariant());

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
			}
		}

		public static class ExcludedMethods
		{
			[Theory]
			[MemberData(nameof(MethodNameData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_ExcludesMatchingMethod(string query)
			{
				var filters = new XunitFilters();
				filters.AddExcludedMethodFilter(query);

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
			}

			[Fact]
			public static void MultipleFilters_ActsAsOrOperation()
			{
				var filters = new XunitFilters();
				filters.AddExcludedMethodFilter($"{typeof(InnerClassOne).FullName}.{nameof(InnerClassOne.NameOne)}");
				filters.AddExcludedMethodFilter($"*.nAmEtWo");

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
			}
		}

		public static class ExcludedNamespaces
		{
			[Theory]
			[MemberData(nameof(NamespaceData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_ExcludesMatchingClass(string query)
			{
				var filters = new XunitFilters();
				filters.AddExcludedNamespaceFilter(query);

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
				Assert.True(filters.Filter("asm1", MethodWithNoTraits));
				Assert.True(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.True(filters.Filter("asm1", MethodWithBazBiffTrait));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAnOrOperation()
			{
				var filters = new XunitFilters();
				filters.AddExcludedNamespaceFilter(typeof(InnerClassOne).Namespace!);
				filters.AddExcludedNamespaceFilter(typeof(ClassWithTraits).Namespace!.ToUpperInvariant());

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
				Assert.False(filters.Filter("asm1", MethodWithNoTraits));
				Assert.False(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.False(filters.Filter("asm1", MethodWithBazBiffTrait));
			}
		}

		public static class ExcludedTraits
		{
			[Theory]
			[MemberData(nameof(TraitData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_ExcludesMatchingTrait(
				string name,
				string value)
			{
				var filters = new XunitFilters();
				filters.AddExcludedTraitFilter(name, value);

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", MethodWithNoTraits));
				Assert.False(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.True(filters.Filter("asm1", MethodWithBazBiffTrait));
			}

			[Fact]
			public static void MultipleFilters_ActsAsOrOperation()
			{
				var filters = new XunitFilters();
				filters.AddExcludedTraitFilter("fOo", "bAr");
				filters.AddExcludedTraitFilter("bAz", "bIff");

				Assert.True(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", MethodWithNoTraits));
				Assert.False(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.False(filters.Filter("asm1", MethodWithBazBiffTrait));
			}
		}

		public static class IncludedClasses
		{
			[Theory]
			[MemberData(nameof(ClassNameData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_IncludesMatchingClass(string query)
			{
				var filters = new XunitFilters();
				filters.AddIncludedClassFilter(query);

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAnAndOperation()
			{
				var filters = new XunitFilters();
				filters.AddIncludedClassFilter(typeof(InnerClassOne).FullName!);
				filters.AddIncludedClassFilter(typeof(InnerClassTwo).FullName!.ToUpperInvariant());

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
			}
		}

		public static class IncludedMethods
		{
			[Theory]
			[MemberData(nameof(MethodNameData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_IncludesMatchingMethod(string query)
			{
				var filters = new XunitFilters();
				filters.AddIncludedMethodFilter(query);

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAndOperation()
			{
				var filters = new XunitFilters();
				filters.AddIncludedMethodFilter($"{typeof(InnerClassOne).FullName}.{nameof(InnerClassOne.NameOne)}");
				filters.AddIncludedMethodFilter($"*.{nameof(InnerClassOne.NameTwo).ToUpperInvariant()}");

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
			}
		}

		public static class IncludedNamespaces
		{
			[Theory]
			[MemberData(nameof(NamespaceData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_ExcludesMatchingClass(string query)
			{
				var filters = new XunitFilters();
				filters.AddIncludedNamespaceFilter(query);

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
				Assert.False(filters.Filter("asm1", MethodWithNoTraits));
				Assert.False(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.False(filters.Filter("asm1", MethodWithBazBiffTrait));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAnOrOperation()
			{
				var filters = new XunitFilters();
				filters.AddIncludedNamespaceFilter(typeof(InnerClassOne).Namespace!);
				filters.AddIncludedNamespaceFilter(typeof(ClassWithTraits).Namespace!.ToUpperInvariant());

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.True(filters.Filter("asm1", NonMethodTest));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.True(filters.Filter("asm1", InnerClassTwo_NameThree));
				Assert.True(filters.Filter("asm1", MethodWithNoTraits));
				Assert.True(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.True(filters.Filter("asm1", MethodWithBazBiffTrait));
			}
		}

		public static class IncludedTraits
		{
			[Theory]
			[MemberData(nameof(TraitData), MemberType = typeof(SimpleFilters))]
			public static void SingleFilter_IncludesMatchingTrait(
				string name,
				string value)
			{
				var filters = new XunitFilters();
				filters.AddIncludedTraitFilter(name, value);

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", MethodWithNoTraits));
				Assert.True(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.False(filters.Filter("asm1", MethodWithBazBiffTrait));
			}

			[Fact]
			public static void MultipleFilters_ActsAsAndOperation()
			{
				var filters = new XunitFilters();
				filters.AddIncludedTraitFilter("fOo", "bAr");
				filters.AddIncludedTraitFilter("bAz", "bIff");

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", MethodWithNoTraits));
				Assert.True(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.True(filters.Filter("asm1", MethodWithBazBiffTrait));
			}
		}

		public static class MixedFilters
		{
			[Fact]
			public static void ActsAsAnAndOperation()
			{
				var filters = new XunitFilters();
				filters.AddIncludedClassFilter(typeof(InnerClassOne).FullName!);
				filters.AddIncludedMethodFilter("*.nAmEtHrEe");

				Assert.False(filters.Filter("asm1", NonClassTest));
				Assert.False(filters.Filter("asm1", NonMethodTest));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameOne));
				Assert.False(filters.Filter("asm1", InnerClassOne_NameTwo));
				Assert.True(filters.Filter("asm1", InnerClassOne_NameThree));
				Assert.False(filters.Filter("asm1", InnerClassTwo_NameThree));
				Assert.False(filters.Filter("asm1", MethodWithNoTraits));
				Assert.False(filters.Filter("asm1", MethodWithFooBarTrait));
				Assert.False(filters.Filter("asm1", MethodWithBazBiffTrait));
			}
		}
	}
}

namespace NamespaceOne
{
	class ClassInNamespace1
	{
		internal class InnerClassOne
		{
			[Fact]
			public static void NameOne() { }

			[Fact]
			public static void NameTwo() { }

			[Fact]
			public static void NameThree() { }
		}

		internal class InnerClassTwo
		{
			[Fact]
			public static void NameThree() { }
		}
	}
}

namespace NamespaceTwo
{
	internal class ClassWithTraits
	{
		[Fact]
		public static void NoTraits() { }

		[Fact]
		[Trait("foo", "bar")]
		public static void FooBar() { }

		[Fact]
		[Trait("baz", "biff")]
		public static void BazBiff() { }
	}
}
