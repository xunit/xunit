using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SomeNamespace;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestClassTests
{
	readonly XunitTestClass testClass;

	public XunitTestClassTests()
	{
		var collectionDefinitions = new Dictionary<string, (Type, CollectionDefinitionAttribute)> { ["foo"] = (typeof(BeforeAfterCollection), new CollectionDefinitionAttribute()) };
		var testAssembly = Mocks.XunitTestAssembly(beforeAfterTestAttributes: [new BeforeAfterOnAssembly()], collectionDefinitions: collectionDefinitions);
		var testCollection = TestData.XunitTestCollection(testAssembly, typeof(BeforeAfterCollection));
		testClass = new XunitTestClass(typeof(ClassUnderTest), testCollection);
	}

	[Fact]
	public void Metadata()
	{
		Assert.Equal(typeof(ClassUnderTest), testClass.Class);
		Assert.Equal("SomeNamespace.ClassUnderTest", testClass.TestClassName);
		Assert.Equal("SomeNamespace", testClass.TestClassNamespace);
		Assert.Equal("ClassUnderTest", testClass.TestClassSimpleName);
	}

	[Fact]
	public void BeforeAfterTestAttributes()
	{
		var result = testClass.BeforeAfterTestAttributes;

		Assert.Collection(
			result.OrderBy(a => a.GetType().Name),
			attr => Assert.IsType<BeforeAfterOnAssembly>(attr),
			attr => Assert.IsType<BeforeAfterOnClass>(attr),
			attr => Assert.IsType<BeforeAfterOnCollection>(attr)
		);
	}

	[Fact]
	public void ClassFixtureTypes()
	{
		var fixtures = testClass.ClassFixtureTypes;

		var fixture = Assert.Single(fixtures);
		Assert.Equal(typeof(MyClassFixture), fixture);
	}

	[Fact]
	public void Constructors()
	{
		var constructors = testClass.Constructors;

		Assert.NotNull(constructors);
		Assert.Collection(
			constructors.OrderBy(c => c.GetParameters().Length),
			constructor => Assert.Empty(constructor.GetParameters()),
			constructor => Assert.Single(constructor.GetParameters())
		);
	}

	[Fact]
	public void Methods()
	{
		var methods = testClass.Methods;

		static string displayName(MethodInfo method)
		{
			var parameterTexts =
				method
					.GetParameters()
					.Select(p => $"{p.ParameterType.FullName} {p.Name}");

			return $"{method.DeclaringType?.FullName}.{method.Name}({string.Join(", ", parameterTexts)})";
		}

		Assert.Collection(
			methods.Where(m => m.DeclaringType != typeof(object)).Select(displayName).OrderBy(x => x),
			method => Assert.Equal("SomeNamespace.BaseClass.BaseMethod()", method),
			method => Assert.Equal("SomeNamespace.BaseClass.BaseStaticMethod()", method),
			method => Assert.Equal("SomeNamespace.ClassUnderTest.InternalMethod()", method),
			method => Assert.Equal("SomeNamespace.ClassUnderTest.PrivateMethod()", method),
			method => Assert.Equal("SomeNamespace.ClassUnderTest.ProtectedMethod()", method),
			method => Assert.Equal("SomeNamespace.ClassUnderTest.PublicMethod()", method)
		);
	}

	[Fact]
	public void TestCaseOrderer()
	{
		var orderer = testClass.TestCaseOrderer;

		Assert.IsType<SomeNamespace.MyTestCaseOrderer>(orderer);
	}

	[Fact]
	public void Traits()
	{
		var traits = testClass.Traits;

		var trait = Assert.Single(traits);
		Assert.Equal("Hello", trait.Key);
		var value = Assert.Single(trait.Value);
		Assert.Equal("World", value);
	}

	[Fact]
	public void Serialization()
	{
		// We can't use the XunitTestClass backed by mocks because they don't serialize, so we'll create
		// one here that's backed by an actual XunitTestAssembly object.
		var testCollection = TestData.XunitTestCollection<ClassUnderTest>();
		var testClass = new XunitTestClass(typeof(ClassUnderTest), testCollection);

		var serialized = SerializationHelper.Instance.Serialize(testClass);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

		Assert.IsType<XunitTestClass>(deserialized);
		Assert.Equivalent(testClass, deserialized);
	}
}

namespace SomeNamespace
{
	class BeforeAfterOnAssembly : BeforeAfterTestAttribute { }
	class BeforeAfterOnClass : BeforeAfterTestAttribute { }
	class BeforeAfterOnCollection : BeforeAfterTestAttribute { }

	class MyClassFixture { }

	class MyTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : notnull, ITestCase =>
				throw new NotImplementedException();
	}

	abstract class BaseClass
	{
		public void BaseMethod() { }

		public static void BaseStaticMethod() { }
	}

	[BeforeAfterOnCollection]
	class BeforeAfterCollection { }

	[BeforeAfterOnClass]
	[Collection("foo")]
	[TestCaseOrderer(typeof(MyTestCaseOrderer))]
	[Trait("Hello", "World")]
	class ClassUnderTest : BaseClass, IClassFixture<MyClassFixture>
	{
		public ClassUnderTest() { }

		public ClassUnderTest(int _) { }

		public void PublicMethod() { }

		protected void ProtectedMethod() { }

		internal void InternalMethod() { }

		void PrivateMethod() { }
	}
}
