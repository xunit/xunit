using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Xunit.v3;

public class ExtensibilityPointFactoryTests
{
	readonly List<IMessageSinkMessage> messages = new List<IMessageSinkMessage>();
	protected _IMessageSink spy;

	public ExtensibilityPointFactoryTests()
	{
		spy = SpyMessageSink.Create(messages: messages);
	}

	public IEnumerable<string> DiagnosticMessages =>
		messages.OfType<_DiagnosticMessage>().Select(m => m.Message);

	public class GetTestFramework : ExtensibilityPointFactoryTests
	{
		[Fact]
		public void NoAttribute()
		{
			var assembly = Mocks.AssemblyInfo();

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly);

			Assert.IsType<XunitTestFramework>(framework);
			Assert.Empty(messages);
		}

		[Fact]
		public void Attribute_NoDiscoverer()
		{
			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithoutDiscoverer));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage("Assembly-level test framework attribute was not decorated with [TestFrameworkDiscoverer]");
		}

		class AttributeWithoutDiscoverer : Attribute, ITestFrameworkAttribute { }

		[Fact]
		public void Attribute_ThrowingDiscovererCtor()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingDiscovererCtor));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var factory = ExtensibilityPointFactory.GetTestFramework(spy, assembly);

			Assert.IsType<XunitTestFramework>(factory);
			AssertSingleDiagnosticMessage("Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(ThrowingDiscovererCtor))]
		class AttributeWithThrowingDiscovererCtor : Attribute, ITestFrameworkAttribute
		{ }

		public class ThrowingDiscovererCtor : ITestFrameworkTypeDiscoverer
		{
			public ThrowingDiscovererCtor() =>
				throw new DivideByZeroException();

			public Type GetTestFrameworkType(IAttributeInfo attribute) =>
				throw new NotImplementedException();
		}

		[Fact]
		public void Attribute_ThrowingDiscovererMethod()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingDiscovererMethod));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage("Exception thrown during test framework discoverer construction: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(ThrowingDiscoverer))]
		class AttributeWithThrowingDiscovererMethod : Attribute, ITestFrameworkAttribute
		{ }

		public class ThrowingDiscoverer : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(IAttributeInfo attribute) =>
				throw new DivideByZeroException();
		}

		[Fact]
		public void Attribute_ThrowingTestFrameworkCtor()
		{
			CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithThrowingTestFrameworkCtor));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly);

			Assert.IsType<XunitTestFramework>(framework);
			AssertSingleDiagnosticMessage("Exception thrown during test framework construction: System.DivideByZeroException: Attempted to divide by zero.");
		}

		[TestFrameworkDiscoverer(typeof(DiscovererForThrowingTestFrameworkCtor))]
		class AttributeWithThrowingTestFrameworkCtor : Attribute, ITestFrameworkAttribute
		{ }

		public class DiscovererForThrowingTestFrameworkCtor : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(IAttributeInfo attribute) =>
				typeof(ThrowingTestFrameworkCtor);
		}

		public class ThrowingTestFrameworkCtor : _ITestFramework
		{
			public ThrowingTestFrameworkCtor() =>
				throw new DivideByZeroException();

			public _ISourceInformationProvider SourceInformationProvider { get; set; }

			public _ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly) =>
				throw new NotImplementedException();

			public _ITestFrameworkExecutor GetExecutor(IReflectionAssemblyInfo assembly) =>
				throw new NotImplementedException();
		}

		[Fact]
		public void Attribute_WithDiscoverer_NoMessageSink()
		{
			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithDiscoverer));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });
			var sourceProvider = Substitute.For<_ISourceInformationProvider>();

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly, sourceProvider);

			var testFramework = Assert.IsType<MyTestFramework>(framework);
			Assert.Same(sourceProvider, testFramework.SourceInformationProvider);
			Assert.Empty(messages);
		}

		[TestFrameworkDiscoverer(typeof(MyDiscoverer))]
		public class AttributeWithDiscoverer : Attribute, ITestFrameworkAttribute
		{ }

		public class MyDiscoverer : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(IAttributeInfo attribute) =>
				typeof(MyTestFramework);
		}

		public class MyTestFramework : _ITestFramework
		{
			public _ISourceInformationProvider? SourceInformationProvider { get; set; }

			public _ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly) =>
				throw new NotImplementedException();

			public _ITestFrameworkExecutor GetExecutor(IReflectionAssemblyInfo assembly) =>
				throw new NotImplementedException();
		}

		[Fact]
		public void Attribute_WithDiscoverer_WithMessageSink()
		{
			var attribute = Mocks.TestFrameworkAttribute(typeof(AttributeWithDiscovererWithMessageSink));
			var assembly = Mocks.AssemblyInfo(attributes: new[] { attribute });
			var sourceProvider = Substitute.For<_ISourceInformationProvider>();

			var framework = ExtensibilityPointFactory.GetTestFramework(spy, assembly, sourceProvider);

			var testFramework = Assert.IsType<MyTestFrameworkWithMessageSink>(framework);
			Assert.Same(spy, testFramework.MessageSink);
			Assert.Same(sourceProvider, testFramework.SourceInformationProvider);
			Assert.Empty(messages);
		}

		[TestFrameworkDiscoverer(typeof(MyDiscovererWithMessageSink))]
		public class AttributeWithDiscovererWithMessageSink : Attribute, ITestFrameworkAttribute
		{ }

		public class MyDiscovererWithMessageSink : ITestFrameworkTypeDiscoverer
		{
			public Type GetTestFrameworkType(IAttributeInfo attribute) =>
				typeof(MyTestFrameworkWithMessageSink);
		}

		public class MyTestFrameworkWithMessageSink : _ITestFramework
		{
			public readonly _IMessageSink MessageSink;

			public MyTestFrameworkWithMessageSink(_IMessageSink messageSink)
			{
				MessageSink = messageSink;
			}

			public _ISourceInformationProvider? SourceInformationProvider { get; set; }

			public _ITestFrameworkDiscoverer GetDiscoverer(IAssemblyInfo assembly) =>
				throw new NotImplementedException();

			public _ITestFrameworkExecutor GetExecutor(IReflectionAssemblyInfo assembly) =>
				throw new NotImplementedException();
		}

		void AssertSingleDiagnosticMessage(string expectedMessage)
		{
			var message = Assert.Single(messages);
			var diagnosticMessage = Assert.IsAssignableFrom<_DiagnosticMessage>(message);
			Assert.StartsWith(expectedMessage, diagnosticMessage.Message);
		}
	}

	public class GetXunitTestCollectionFactory : ExtensibilityPointFactoryTests
	{
		[Fact]
		public void DefaultTestCollectionFactoryIsCollectionPerClass()
		{
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, (IAttributeInfo?)null, assembly);

			Assert.IsType<CollectionPerClassTestCollectionFactory>(result);
		}

		[Theory]
		[InlineData(CollectionBehavior.CollectionPerAssembly, typeof(CollectionPerAssemblyTestCollectionFactory))]
		[InlineData(CollectionBehavior.CollectionPerClass, typeof(CollectionPerClassTestCollectionFactory))]
		public void UserCanChooseFromBuiltInCollectionFactories_NonParallel(CollectionBehavior behavior, Type expectedType)
		{
			var attr = Mocks.CollectionBehaviorAttribute(behavior);
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

			Assert.IsType(expectedType, result);
		}

		[Fact]
		public void UserCanChooseCustomCollectionFactoryWithType()
		{
			var factoryType = typeof(MyTestCollectionFactory);
			var attr = Mocks.CollectionBehaviorAttribute(factoryType);
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

			var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
			Assert.Same(assembly, myFactory.Assembly);
		}

		[Fact]
		public void UserCanChooseCustomCollectionFactoryWithTypeAndAssemblyName()
		{
			var factoryType = typeof(MyTestCollectionFactory);
			var attr = Mocks.CollectionBehaviorAttribute(factoryType.FullName!, factoryType.Assembly.FullName!);
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

			var myFactory = Assert.IsType<MyTestCollectionFactory>(result);
			Assert.Same(assembly, myFactory.Assembly);
		}

		class MyTestCollectionFactory : IXunitTestCollectionFactory
		{
			public MyTestCollectionFactory(ITestAssembly assembly)
			{
				Assembly = assembly;
			}

			public readonly ITestAssembly Assembly;

			public string DisplayName =>
				"My Factory";

			public ITestCollection Get(ITypeInfo testClass) =>
				throw new NotImplementedException();
		}

		[Theory]
		[InlineData(
			"ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor",
			"Could not find constructor for 'ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_NoCompatibleConstructor' with arguments type(s): Xunit.Sdk.TestAssembly")]
		[InlineData(
			"ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface",
			"Test collection factory type 'xunit.v3.core.tests, ExtensibilityPointFactoryTests+GetXunitTestCollectionFactory+TestCollectionFactory_DoesNotImplementInterface' does not implement IXunitTestCollectionFactory")]
		[InlineData(
			"ThisIsNotARealType",
			"Unable to create test collection factory type 'xunit.v3.core.tests, ThisIsNotARealType'")]
		public void IncompatibleOrInvalidTypesGetDefaultBehavior(string factoryTypeName, string expectedMessage)
		{
			var attr = Mocks.CollectionBehaviorAttribute(factoryTypeName, "xunit.v3.core.tests");
			var assembly = Mocks.TestAssembly();

			var result = ExtensibilityPointFactory.GetXunitTestCollectionFactory(spy, attr, assembly);

			Assert.Collection(
				DiagnosticMessages,
				msg => Assert.Equal(expectedMessage, msg)
			);
		}

		class TestCollectionFactory_NoCompatibleConstructor : IXunitTestCollectionFactory
		{
			public string DisplayName =>
				throw new NotImplementedException();

			public ITestCollection Get(ITypeInfo testClass) =>
				throw new NotImplementedException();
		}

		class TestCollectionFactory_DoesNotImplementInterface
		{
			public TestCollectionFactory_DoesNotImplementInterface(IAssemblyInfo _)
			{ }
		}
	}
}
