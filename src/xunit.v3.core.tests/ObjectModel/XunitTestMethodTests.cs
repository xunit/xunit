using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestMethodTests
{
	readonly XunitTestMethod testMethod;

	public XunitTestMethodTests()
	{
		var collectionDefinitions = new Dictionary<string, (Type, CollectionDefinitionAttribute)> { ["foo"] = (typeof(BeforeAfterCollection), new CollectionDefinitionAttribute()) };
		var testAssembly = Mocks.XunitTestAssembly(beforeAfterTestAttributes: [new BeforeAfterOnAssembly()], collectionDefinitions: collectionDefinitions);
		var testCollection = TestData.XunitTestCollection(testAssembly, typeof(BeforeAfterCollection));
		var testClass = TestData.XunitTestClass<ClassUnderTest>(testCollection);
		var methodInfo = typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.Passing), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);
		Guard.NotNull($"Could not find method '{nameof(ClassUnderTest.Passing)}' on type '{typeof(ClassUnderTest).FullName}'", methodInfo);

		testMethod = TestData.XunitTestMethod(testClass, methodInfo);
	}

	[Fact]
	public void Metadata()
	{
		Assert.Equal(nameof(ClassUnderTest.Passing), testMethod.MethodName);
	}

	[Fact]
	public void BeforeAfterTestAttributes()
	{
		var result = testMethod.BeforeAfterTestAttributes;

		Assert.Collection(
			result.OrderBy(a => a.GetType().Name),
			attr => Assert.IsType<BeforeAfterOnAssembly>(attr),
			attr => Assert.IsType<BeforeAfterOnClass>(attr),
			attr => Assert.IsType<BeforeAfterOnCollection>(attr),
			attr => Assert.IsType<BeforeAfterOnMethod>(attr)
		);
	}

	[Fact]
	public void DataAttributes()
	{
		var attributes = testMethod.DataAttributes;

		var attribute = Assert.Single(attributes);
		Assert.IsType<InlineDataAttribute>(attribute);
	}

	[Fact]
	public void FactAttributes()
	{
		var attributes = testMethod.FactAttributes;

		Assert.Collection(
			attributes.OrderBy(x => x.GetType().Name),
			attribute => Assert.IsType<FactAttribute>(attribute),
			attribute => Assert.IsType<TheoryAttribute>(attribute)
		);
	}

	[Fact]
	public void Parameters()
	{
		var parameters = testMethod.Parameters;

		Assert.Collection(
			parameters,
			parameter => Assert.Equal("x", parameter.Name),
			parameter => Assert.Equal("y", parameter.Name)
		);
	}

	[Fact]
	public void Traits()
	{
		var traits = testMethod.Traits;

		var trait = Assert.Single(traits);
		Assert.Equal("Hello", trait.Key);
		var value = Assert.Single(trait.Value);
		Assert.Equal("World", value);
	}

	[Fact]
	public void Serialization()
	{
		// We can't use the XunitTestMethod backed by mocks because they don't serialize, so we'll create
		// one here that's backed by an actual XunitTestAssembly object.
		var testClass = TestData.XunitTestClass<ClassUnderTest>();
		var method = typeof(ClassUnderTest).GetMethod("Passing") ?? throw new InvalidOperationException("Could not find test method");
		var testMethod = new XunitTestMethod(testClass, method, []);

		var serialized = SerializationHelper.Instance.Serialize(testMethod);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

		Assert.IsType<XunitTestMethod>(deserialized);
		Assert.Equivalent(testMethod, deserialized);
	}

	// https://github.com/xunit/xunit/issues/2964
	[Fact]
	public void CanSerializeClosedGenericTestMethod()
	{
		var methodInfo = typeof(ClassUnderTest).GetMethod(nameof(ClassUnderTest.TheoryWithTypeArgument))!;
		var closedMethodInfo = methodInfo.MakeGenericMethod(typeof(string));
		var testClass = TestData.XunitTestClass<ClassUnderTest>();
		var testMethod = new XunitTestMethod(testClass, closedMethodInfo, ["data"]);

		var serialized = SerializationHelper.Instance.Serialize(testMethod);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

		Assert.IsType<XunitTestMethod>(deserialized);
		Assert.Equivalent(testMethod, deserialized);
	}

	[BeforeAfterOnCollection]
	class BeforeAfterCollection { }

	[BeforeAfterOnClass]
	[Collection("foo")]
	class ClassUnderTest
	{
#pragma warning disable xUnit1001 // Fact methods cannot have parameters
#pragma warning disable xUnit1002 // Test methods cannot have multiple Fact or Theory attributes
#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

		[Fact]
		[Theory]
		[BeforeAfterOnMethod]
		[InlineData(42, "Hello world")]
		[Trait("Hello", "World")]
		public void Passing(int x, string y) { }

#pragma warning restore xUnit1026 // Theory methods should use all of their parameters
#pragma warning restore xUnit1002 // Test methods cannot have multiple Fact or Theory attributes
#pragma warning restore xUnit1001 // Fact methods cannot have parameters

		[Theory]
		[InlineData("data")]
		public void TheoryWithTypeArgument<T>(T _) { }
	}

	class BeforeAfterOnAssembly : BeforeAfterTestAttribute { }
	class BeforeAfterOnClass : BeforeAfterTestAttribute { }
	class BeforeAfterOnCollection : BeforeAfterTestAttribute { }
	class BeforeAfterOnMethod : BeforeAfterTestAttribute { }
}
