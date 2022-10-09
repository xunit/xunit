using System;
using System.Globalization;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

public class ExtensibilityPointFactoryTests
{
	public class GetTestFramework : ExtensibilityPointFactoryTests
	{
		[Fact]
		public void NoAttribute()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var assembly = Mocks.AssemblyInfo();

			var framework = ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.IsType<XunitTestFramework>(framework);
			Assert.Empty(spy.Messages);
		}

		[Fact]
		public void Attribute_NoDiscoverer()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var attribute = Mocks.TestFrameworkAttribute<AttributeWithoutDiscoverer>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage(spy, "Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]");
		}

		class AttributeWithoutDiscoverer : Attribute, ITestFrameworkAttribute { }

		[Fact]
		public void Attribute_ThrowingDiscovererCtor()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var attribute = Mocks.TestFrameworkAttribute<AttributeWithThrowingDiscovererCtor>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var factory = ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.IsType<XunitTestFramework>(factory);
			AssertSingleDiagnosticMessage(spy, "Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(ThrowingDiscovererCtor))]
		class AttributeWithThrowingDiscovererCtor : Attribute, ITestFrameworkAttribute
		{ }

		public class ThrowingDiscovererCtor : ITestFrameworkTypeDiscoverer
		{
			public ThrowingDiscovererCtor() =>
				throw new DivideByZeroException();

			public Type GetTestFrameworkType(_IAttributeInfo attribute) =>
				throw new NotImplementedException();
		}

		[Fact]
		public void Attribute_ThrowingDiscovererMethod()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var attribute = Mocks.TestFrameworkAttribute<AttributeWithThrowingDiscovererMethod>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage(spy, "Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(ThrowingDiscoverer))]
		class AttributeWithThrowingDiscovererMethod : Attribute, ITestFrameworkAttribute
		{ }

		public class ThrowingDiscoverer : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(_IAttributeInfo attribute) =>
				throw new DivideByZeroException();
		}

		[Fact]
		public void Attribute_ThrowingTestFrameworkCtor()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var attribute = Mocks.TestFrameworkAttribute<AttributeWithThrowingTestFrameworkCtor>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage(spy, "Exception thrown during test framework construction; falling back to default test framework: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(DiscovererForThrowingTestFrameworkCtor))]
		class AttributeWithThrowingTestFrameworkCtor : Attribute, ITestFrameworkAttribute
		{ }

		public class DiscovererForThrowingTestFrameworkCtor : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(_IAttributeInfo attribute) =>
				typeof(ThrowingTestFrameworkCtor);
		}

		public class ThrowingTestFrameworkCtor : _ITestFramework
		{
			public ThrowingTestFrameworkCtor() =>
				throw new DivideByZeroException();

			public _ISourceInformationProvider SourceInformationProvider { get; set; }

			public _ITestFrameworkDiscoverer GetDiscoverer(_IAssemblyInfo assembly) =>
				throw new NotImplementedException();

			public _ITestFrameworkExecutor GetExecutor(_IReflectionAssemblyInfo assembly) =>
				throw new NotImplementedException();
		}

		[Fact]
		public void Attribute_WithDiscoverer_NoMessageSink()
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;
			var attribute = Mocks.TestFrameworkAttribute<AttributeWithDiscoverer>();
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			ExtensibilityPointFactory.GetTestFramework(assembly);

			Assert.Empty(spy.Messages);
		}

		[TestFrameworkDiscoverer(typeof(MyDiscoverer))]
		public class AttributeWithDiscoverer : Attribute, ITestFrameworkAttribute
		{ }

		public class MyDiscoverer : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(_IAttributeInfo attribute) =>
				typeof(MyTestFramework);
		}

		public class MyTestFramework : _ITestFramework
		{
			public _ISourceInformationProvider? SourceInformationProvider { get; set; }

			public _ITestFrameworkDiscoverer GetDiscoverer(_IAssemblyInfo assembly) =>
				throw new NotImplementedException();

			public _ITestFrameworkExecutor GetExecutor(_IReflectionAssemblyInfo assembly) =>
				throw new NotImplementedException();
		}
	}

	public class GetXunitTestCollectionFactory : ExtensibilityPointFactoryTests
	{
		[Fact]
		public void DefaultTestCollectionFactoryIsCollectionPerClass()
		{
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory((_IAttributeInfo?)null, assembly);

			Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
		}

		[Theory]
		[InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory))]
		[InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory))]
		public void UserCanChooseFromBuiltInCollectionFactories_NonParallel(CollectionBehavior behavior, Type expectedType)
		{
			var attr = Mocks.CollectionBehaviorAttribute(behavior);
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

			Assert.IsType(expectedType, result);
		}

		[Fact]
		public void UserCanChooseCustomCollectionFactoryWithType()
		{
			var attr = Mocks.CollectionBehaviorAttribute<MyTestCollectionFactory>();
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

			var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
			Assert.Same(assembly, myFactory.Assembly);
		}

		[Fact]
		public void UserCanChooseCustomCollectionFactoryWithTypeAndAssemblyName()
		{
			var factoryType = typeof(MyTestCollectionFactory);
			var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName!, factoryType.Assembly.FullName!);
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

			var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
			Assert.Same(assembly, myFactory.Assembly);
		}

		class MyTestCollectionFactory : IXunitTestCollectionFactory
		{
			public MyTestCollectionFactory(_ITestAssembly assembly)
			{
				Assembly = assembly;
			}

			public readonly _ITestAssembly Assembly;

			public string DisplayName =>
				"My Factory";

			public _ITestCollection Get(_ITypeInfo testClass) =>
				throw new NotImplementedException();
		}

		[Theory]
		[InlineData(
			"ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor",
			"Could not find constructor for 'ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor' with arguments type(s): Castle.Proxies.InterfaceProxy")]
		[InlineData(
			"ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface",
			"Test collection factory type 'xunit.v3.core.tests, ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface' does not implement IXunitTestCollectionFactory")]
		[InlineData(
			"ThisIsNotARealType",
			"Unable to create test collection factory type 'xunit.v3.core.tests, ThisIsNotARealType'")]
		public void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName, string expectedMessage)
		{
			var spy = SpyMessageSink.Capture();
			TestContext.Current!.DiagnosticMessageSink = spy;

#if BUILD_X86
			expectedMessage = expectedMessage.Replace("xunit.v3.core.tests", "xunit.v3.core.tests.x86");
			var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "xunit.v3.core.tests.x86");
#else
			var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "xunit.v3.core.tests");
#endif
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(attr, assembly);

			AssertSingleDiagnosticMessage(spy, expectedMessage);
		}

		class TestCollectionFactory_NoCompatibleConstructor : IXunitTestCollectionFactory
		{
			public string DisplayName =>
				throw new NotImplementedException();

			public _ITestCollection Get(_ITypeInfo _) =>
				throw new NotImplementedException();
		}

		class TestCollectionFactory_DoesNotImplementInterface
		{
			public TestCollectionFactory_DoesNotImplementInterface(_IAssemblyInfo _)
			{ }
		}
	}
	void AssertSingleDiagnosticMessage(SpyMessageSink spy, string expectedMessage)
	{
		var message = Assert.Single(spy.Messages);
		var diagnosticMessage = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
		Assert.StartsWith(expectedMessage, diagnosticMessage.Message);
	}
}
