#if NETCOREAPP  // Default interface methods are not supported on .NET Framework or Native AOT

using Xunit;
using Xunit.Sdk;

public class DefaultInterfaceMethodAcceptanceTests : AcceptanceTestV3
{
	[Fact]
	public static async ValueTask AcceptanceTest()
	{
		var results = await RunForResultsAsync(typeof(ClassUnderTest));

		Assert.Collection(
			results.OrderBy(t => t.Test.TestDisplayName),
			result =>
			{
				Assert.Equal(nameof(AbstractBaseClass.AbstractTestMethod), result.TestMethod?.MethodName);
				Assert.IsType<TestPassedWithMetadata>(result);
			},
			result =>
			{
				Assert.Equal(nameof(IHaveDefaultMethods.Failing), result.TestMethod?.MethodName);
				var failed = Assert.IsType<TestFailedWithMetadata>(result);
				Assert.Equal(typeof(TrueException).FullName, failed.ExceptionTypes.Single());
			},
			result =>
			{
				Assert.Equal(nameof(IHaveDefaultMethods.NonDefaultInterfaceMethodsSupported), result.TestMethod?.MethodName);
				Assert.IsType<TestPassedWithMetadata>(result);
			},
			result =>
			{
				Assert.Equal(nameof(IHaveDefaultMethods.OverloadOfDefaultNotSupported), result.TestMethod?.MethodName);
				var failed = Assert.IsType<TestFailedWithMetadata>(result);
				Assert.Equal(typeof(TestPipelineException).FullName, failed.ExceptionTypes.Single());
				Assert.Equal($"Test method {typeof(IHaveDefaultMethods).FullName}.{nameof(IHaveDefaultMethods.OverloadOfDefaultNotSupported)} has overloads, which is not supported. See xUnit1024 for more information. https://xunit.net/xunit.analyzers/rules/xUnit1024", failed.Messages.Single());
			},
			result =>
			{
				Assert.Equal(nameof(IHaveDefaultMethods.Passing), result.TestMethod?.MethodName);
				Assert.IsType<TestPassedWithMetadata>(result);
			},
			result =>
			{
				Assert.Equal(nameof(IHaveDefaultMethods.Skipping), result.TestMethod?.MethodName);
				var skipped = Assert.IsType<TestSkippedWithMetadata>(result);
				Assert.Equal("Statically skipped", skipped.Reason);
			}
		);
	}

	interface IHaveDefaultMethods
	{
		[Fact]
		public void Passing() => Assert.IsType<ClassUnderTest>(this);

		[Fact]
		public void Failing() => Assert.True(false);

		[Fact(Skip = "Statically skipped")]
		public void Skipping() => Assert.True(false);

		[Fact]
		public void OverloadOfDefaultNotSupported() { }

		void NonDefaultInterfaceMethodsSupported();
	}

	abstract class AbstractBaseClass
	{
		[Fact]
		public abstract void AbstractTestMethod();
	}

	class ClassUnderTest : AbstractBaseClass, IHaveDefaultMethods
	{
		public override void AbstractTestMethod() { }

		public void OverloadOfDefaultNotSupported() { }

		[Fact]
		public void NonDefaultInterfaceMethodsSupported() { }
	}
}

#endif
