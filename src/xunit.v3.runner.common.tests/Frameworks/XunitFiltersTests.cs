using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;
using ClassWithTraits = Namespace2.ClassWithTraits;
using InnerClass1 = Namespace1.ClassInNamespace1.InnerClass1;
using InnerClass2 = Namespace1.ClassInNamespace1.InnerClass2;

public class XunitFiltersTests
{
	static _TestCaseDiscovered InnerClass1Name1 = TestData.TestCaseDiscovered<InnerClass1>(nameof(InnerClass1.Name1));
	static _TestCaseDiscovered InnerClass1Name2 = TestData.TestCaseDiscovered<InnerClass1>(nameof(InnerClass1.Name2));
	static _TestCaseDiscovered InnerClass1Name3 = TestData.TestCaseDiscovered<InnerClass1>(nameof(InnerClass1.Name3));
	static _TestCaseDiscovered InnerClass2Name3 = TestData.TestCaseDiscovered<InnerClass2>(nameof(InnerClass2.Name3));
	static _TestCaseDiscovered MethodWithFooBarTrait = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.FooBar));
	static _TestCaseDiscovered MethodWithBazBiffTrait = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.BazBiff));
	static _TestCaseDiscovered MethodWithNoTraits = TestData.TestCaseDiscovered<ClassWithTraits>(nameof(ClassWithTraits.NoTraits));
	static _TestCaseDiscovered NonClassTest = TestData.TestCaseDiscovered();
	static _TestCaseDiscovered NonMethodTest = TestData.TestCaseDiscovered(
		testClass: typeof(ClassWithTraits).Name,
		testClassWithNamespace: typeof(ClassWithTraits).FullName,
		testNamespace: typeof(ClassWithTraits).Namespace
	);

	public static class NoFilters
	{
		[Fact]
		public static void AlwaysPass()
		{
			var filters = new XunitFilters();

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
			Assert.True(filters.Filter(MethodWithNoTraits));
			Assert.True(filters.Filter(MethodWithFooBarTrait));
			Assert.True(filters.Filter(MethodWithBazBiffTrait));
		}
	}

	public static class ExcludedClasses
	{
		[Fact]
		public static void SingleFilter_ExcludesMatchingClass()
		{
			var filters = new XunitFilters();
			filters.ExcludedClasses.Add(typeof(InnerClass1).FullName!);

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAnOrOperation()
		{
			var filters = new XunitFilters();
			filters.ExcludedClasses.Add(typeof(InnerClass1).FullName!);
			filters.ExcludedClasses.Add(typeof(InnerClass2).FullName!.ToUpperInvariant());

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
		}
	}

	public static class ExcludedMethods
	{
		[Fact]
		public static void ExactMatch_ExcludesMatchingMethod()
		{
			var filters = new XunitFilters();
			filters.ExcludedMethods.Add($"{typeof(InnerClass1).FullName}.{nameof(InnerClass1.Name1)}");

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void WildcardMatch_ExcludesMatchingMethod()
		{
			var filters = new XunitFilters();
			filters.ExcludedMethods.Add($"*.nAmE1");

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void MultipleFilters_ActsAsOrOperation()
		{
			var filters = new XunitFilters();
			filters.ExcludedMethods.Add($"{typeof(InnerClass1).FullName}.{nameof(InnerClass1.Name1)}");
			filters.ExcludedMethods.Add($"*.nAmE2");

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
		}
	}

	public static class ExcludedNamespaces
	{
		[Fact]
		public static void SingleFilter_ExcludesMatchingClass()
		{
			var filters = new XunitFilters();
			filters.ExcludedNamespaces.Add(typeof(InnerClass1).Namespace!);

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
			Assert.True(filters.Filter(MethodWithNoTraits));
			Assert.True(filters.Filter(MethodWithFooBarTrait));
			Assert.True(filters.Filter(MethodWithBazBiffTrait));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAnOrOperation()
		{
			var filters = new XunitFilters();
			filters.ExcludedNamespaces.Add(typeof(InnerClass1).Namespace!);
			filters.ExcludedNamespaces.Add(typeof(ClassWithTraits).Namespace!.ToUpperInvariant());

			Assert.True(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
			Assert.False(filters.Filter(MethodWithNoTraits));
			Assert.False(filters.Filter(MethodWithFooBarTrait));
			Assert.False(filters.Filter(MethodWithBazBiffTrait));
		}
	}

	public static class ExcludedTraits
	{
		[Fact]
		public static void SingleFilter_ExcludesMatchingTrait()
		{
			var filters = new XunitFilters();
			filters.ExcludedTraits.Add("foo", "bar");

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(MethodWithNoTraits));
			Assert.False(filters.Filter(MethodWithFooBarTrait));
			Assert.True(filters.Filter(MethodWithBazBiffTrait));
		}

		[Fact]
		public static void MultipleFilters_ActsAsOrOperation()
		{
			var filters = new XunitFilters();
			filters.ExcludedTraits.Add("fOo", "bAr");
			filters.ExcludedTraits.Add("bAz", "bIff");

			Assert.True(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(MethodWithNoTraits));
			Assert.False(filters.Filter(MethodWithFooBarTrait));
			Assert.False(filters.Filter(MethodWithBazBiffTrait));
		}
	}

	public static class IncludedClasses
	{
		[Fact]
		public static void SingleFilter_IncludesMatchingClass()
		{
			var filters = new XunitFilters();
			filters.IncludedClasses.Add(typeof(InnerClass1).FullName!);

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAnAndOperation()
		{
			var filters = new XunitFilters();
			filters.IncludedClasses.Add(typeof(InnerClass1).FullName!);
			filters.IncludedClasses.Add(typeof(InnerClass2).FullName!.ToUpperInvariant());

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
		}
	}

	public static class IncludedMethods
	{
		[Fact]
		public static void ExactMatch_IncludesMatchingMethod()
		{
			var filters = new XunitFilters();
			filters.IncludedMethods.Add($"{typeof(InnerClass1).FullName}.{nameof(InnerClass1.Name1)}");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void WildcardMatch_IncludesMatchingMethod()
		{
			var filters = new XunitFilters();
			filters.IncludedMethods.Add($"*.{nameof(InnerClass1.Name1)}");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAndOperation()
		{
			var filters = new XunitFilters();
			filters.IncludedMethods.Add($"{typeof(InnerClass1).FullName}.{nameof(InnerClass1.Name1)}");
			filters.IncludedMethods.Add($"*.{nameof(InnerClass1.Name2).ToUpperInvariant()}");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.False(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
		}
	}

	public static class IncludedNamespaces
	{
		[Fact]
		public static void SingleFilter_ExcludesMatchingClass()
		{
			var filters = new XunitFilters();
			filters.IncludedNamespaces.Add(typeof(InnerClass1).Namespace!);

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
			Assert.False(filters.Filter(MethodWithNoTraits));
			Assert.False(filters.Filter(MethodWithFooBarTrait));
			Assert.False(filters.Filter(MethodWithBazBiffTrait));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAnOrOperation()
		{
			var filters = new XunitFilters();
			filters.IncludedNamespaces.Add(typeof(InnerClass1).Namespace!);
			filters.IncludedNamespaces.Add(typeof(ClassWithTraits).Namespace!.ToUpperInvariant());

			Assert.False(filters.Filter(NonClassTest));
			Assert.True(filters.Filter(NonMethodTest));
			Assert.True(filters.Filter(InnerClass1Name1));
			Assert.True(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.True(filters.Filter(InnerClass2Name3));
			Assert.True(filters.Filter(MethodWithNoTraits));
			Assert.True(filters.Filter(MethodWithFooBarTrait));
			Assert.True(filters.Filter(MethodWithBazBiffTrait));
		}
	}

	public static class IncludedTraits
	{
		[Fact]
		public static void SingleFilter_IncludesMatchingTrait()
		{
			var filters = new XunitFilters();
			filters.IncludedTraits.Add("foo", "bar");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(MethodWithNoTraits));
			Assert.True(filters.Filter(MethodWithFooBarTrait));
			Assert.False(filters.Filter(MethodWithBazBiffTrait));
		}

		[Fact]
		public static void MultipleFilters_ActsAsAndOperation()
		{
			var filters = new XunitFilters();
			filters.IncludedTraits.Add("fOo", "bAr");
			filters.IncludedTraits.Add("bAz", "bIff");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(MethodWithNoTraits));
			Assert.True(filters.Filter(MethodWithFooBarTrait));
			Assert.True(filters.Filter(MethodWithBazBiffTrait));
		}
	}

	public static class MixedFilters
	{
		[Fact]
		public static void ActsAsAnAndOperation()
		{
			var filters = new XunitFilters();
			filters.IncludedClasses.Add(typeof(InnerClass1).FullName!);
			filters.IncludedMethods.Add("*.nAmE3");

			Assert.False(filters.Filter(NonClassTest));
			Assert.False(filters.Filter(NonMethodTest));
			Assert.False(filters.Filter(InnerClass1Name1));
			Assert.False(filters.Filter(InnerClass1Name2));
			Assert.True(filters.Filter(InnerClass1Name3));
			Assert.False(filters.Filter(InnerClass2Name3));
			Assert.False(filters.Filter(MethodWithNoTraits));
			Assert.False(filters.Filter(MethodWithFooBarTrait));
			Assert.False(filters.Filter(MethodWithBazBiffTrait));
		}
	}
}

namespace Namespace1
{
	class ClassInNamespace1
	{
		internal class InnerClass1
		{
			[Fact]
			public static void Name1() { }

			[Fact]
			public static void Name2() { }

			[Fact]
			public static void Name3() { }
		}

		internal class InnerClass2
		{
			[Fact]
			public static void Name3() { }
		}
	}
}

namespace Namespace2
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
