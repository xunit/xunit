using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestMethodTestCaseTests
{
	public class Ctor
	{
		[Fact]
		public static void NonSerializableArgumentsThrows()
		{
			var testMethod = Mocks.TestMethod("MockType", "MockMethod");

			var ex = Record.Exception(
				() => new TestableTestMethodTestCase(testMethod, new object[] { new XunitTestCaseTests() })
			);

			Assert.IsType<SerializationException>(ex);
			Assert.Equal($"Type '{typeof(XunitTestCaseTests).FullName}' in Assembly '{typeof(XunitTestCaseTests).Assembly.FullName}' is not marked as serializable.", ex.Message);
		}

		[Fact]
		public static void DefaultBehavior()
		{
			var testMethod = Mocks.TestMethod("MockType", "MockMethod");

			var testCase = new TestableTestMethodTestCase(testMethod);

			Assert.Equal("MockType.MockMethod", testCase.DisplayName);
			Assert.Null(testCase.InitializationException);
			Assert.Same(testMethod.Method, testCase.Method);
			Assert.Null(testCase.SkipReason);
			Assert.Null(testCase.SourceInformation);
			Assert.Same(testMethod, testCase.TestMethod);
			Assert.Null(testCase.TestMethodArguments);
			Assert.Empty(testCase.Traits);
			Assert.Equal("4428bc4e444a8f5294832dc06425f20fc994bdc44788f03219b7237f892bffe0", testCase.UniqueID);
		}

		[Fact]
		public static void Overrides()
		{
			var testMethod = Mocks.TestMethod("MockType", "Mock_Method");
			var arguments = new object[] { 42, 21.12, "Hello world!" };
			var traits = new Dictionary<string, List<string>> { { "FOO", new List<string> { "BAR" } } };

			var testCase = new TestableTestMethodTestCase(
				testMethod,
				arguments,
				TestMethodDisplay.Method,
				TestMethodDisplayOptions.ReplaceUnderscoreWithSpace,
				"Skip me!",
				traits,
				"test-case-custom-id"
			);

			Assert.Equal($"Mock Method(???: 42, ???: {21.12:G17}, ???: \"Hello world!\")", testCase.DisplayName);
			Assert.Equal("Skip me!", testCase.SkipReason);
			Assert.Same(arguments, testCase.TestMethodArguments);
			Assert.Collection(
				testCase.Traits,
				kvp =>
				{
					Assert.Equal("FOO", kvp.Key);
					Assert.Equal("BAR", Assert.Single(kvp.Value));
				}
			);
			Assert.Equal("test-case-custom-id", testCase.UniqueID);
		}
	}

	public class DisplayName
	{
		[Fact]
		public static void CorrectNumberOfTestArguments()
		{
			var param1 = Mocks.ParameterInfo("p1");
			var param2 = Mocks.ParameterInfo("p2");
			var param3 = Mocks.ParameterInfo("p3");
			var testMethod = Mocks.TestMethod(parameters: new[] { param1, param2, param3 });
			var arguments = new object[] { 42, "Hello, world!", 'A' };

			var testCase = new TestableTestMethodTestCase(testMethod, arguments);

			Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
		}

		[Fact]
		public static void NotEnoughTestArguments()
		{
			var param = Mocks.ParameterInfo("p1");
			var testMethod = Mocks.TestMethod(parameters: new[] { param });

			var testCase = new TestableTestMethodTestCase(testMethod, new object[0]);

			Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: ???)", testCase.DisplayName);
		}

		[CulturedFact]
		public static void TooManyTestArguments()
		{
			var param = Mocks.ParameterInfo("p1");
			var testMethod = Mocks.TestMethod(parameters: new[] { param });
			var arguments = new object[] { 42, 21.12M };

			var testCase = new TestableTestMethodTestCase(testMethod, arguments);

			Assert.Equal($"{testMethod.TestClass.Class.Name}.{testMethod.Method.Name}(p1: 42, ???: {21.12})", testCase.DisplayName);
		}

		[Theory]
		[InlineData(TestMethodDisplay.ClassAndMethod, "TestMethodTestCaseTests+DisplayName.OverrideDefaultMethodDisplay")]
		[InlineData(TestMethodDisplay.Method, "OverrideDefaultMethodDisplay")]
		public static void OverrideDefaultMethodDisplay(
			TestMethodDisplay methodDisplay,
			string expectedDisplayName)
		{
			var testMethod = Mocks.TestMethod<DisplayName>("OverrideDefaultMethodDisplay");

			var testCase = new TestableTestMethodTestCase(testMethod, defaultMethodDisplay: methodDisplay);

			Assert.Equal(expectedDisplayName, testCase.DisplayName);
		}
	}

	public class DisposeAsync
	{
		[Fact]
		public static async ValueTask DisposesArguments()
		{
			var disposable = new SerializableDisposable();
			var asyncDisposable = new SerializableAsyncDisposable();
			var testMethod = Mocks.TestMethod();
			var testCase = new TestableTestMethodTestCase(testMethod, new object[] { disposable, asyncDisposable });

			await testCase.DisposeAsync();

			Assert.True(disposable.DisposeCalled);
			Assert.True(asyncDisposable.DisposeAsyncCalled);
		}

		[Serializable]
		class SerializableDisposable : IDisposable
		{
			public bool DisposeCalled = false;

			public void Dispose() => DisposeCalled = true;
		}

		[Serializable]
		class SerializableAsyncDisposable : IAsyncDisposable
		{
			public bool DisposeAsyncCalled = false;

			public ValueTask DisposeAsync()
			{
				DisposeAsyncCalled = true;
				return default;
			}
		}
	}

	public class Method : AcceptanceTestV3
	{
		[Theory]
		[InlineData(42, typeof(int))]
		[InlineData("Hello world", typeof(string))]
		[InlineData(null, typeof(object))]
		public void OpenGenericIsClosedByArguments(
			object? testArg,
			Type expectedGenericType)
		{
			var method = TestData.TestMethod<ClassUnderTest>("OpenGeneric");
			var testCase = new TestableTestMethodTestCase(method, new[] { testArg });

			var closedMethod = testCase.Method;

			var methodInfo = Assert.IsAssignableFrom<_IReflectionMethodInfo>(method.Method).MethodInfo;
			Assert.True(methodInfo.IsGenericMethodDefinition);
			var closedMethodInfo = Assert.IsAssignableFrom<_IReflectionMethodInfo>(closedMethod).MethodInfo;
			Assert.False(closedMethodInfo.IsGenericMethodDefinition);
			var genericType = Assert.Single(closedMethodInfo.GetGenericArguments());
			Assert.Same(expectedGenericType, genericType);
		}

		[Fact]
		public void IncompatibleArgumentsSetsInitializationException()
		{
			var method = TestData.TestMethod<ClassUnderTest>("NonGeneric");

			var testCase = new TestableTestMethodTestCase(method, new[] { new ClassUnderTest() });

			Assert.NotNull(testCase.InitializationException);
			Assert.IsType<InvalidOperationException>(testCase.InitializationException);
			Assert.Equal("The arguments for this test method did not match the parameters: [ClassUnderTest { }]", testCase.InitializationException.Message);
		}

		[Serializable]
		class ClassUnderTest
		{
			[Theory]
			public void OpenGeneric<T>(T value) { }

			[Theory]
			public void NonGeneric(params int[] values) { }
		}
	}

	public class Serialization
	{
		[Fact]
		public static void CanRoundTrip_PublicClass_PublicTestMethod()
		{
			var testCase = TestableTestMethodTestCase.Create<Serialization>("CanRoundTrip_PublicClass_PublicTestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<TestableTestMethodTestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PublicClass_PrivateTestMethod()
		{
			var testCase = TestableTestMethodTestCase.Create<Serialization>("PrivateTestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<TestableTestMethodTestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PrivateClass()
		{
			var testCase = TestableTestMethodTestCase.Create<PrivateClass>("TestMethod");

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<TestableTestMethodTestCase>(deserialized);
		}

		[Fact]
		void PrivateTestMethod() { }

		class PrivateClass
		{
			[Fact]
			public static void TestMethod()
			{
				Assert.True(false);
			}
		}
	}

	public class Traits
	{
		[Fact]
		public void TraitNamesAreCaseInsensitive_AddedAfter()
		{
			var testMethod = Mocks.TestMethod();
			var testCase = new TestableTestMethodTestCase(testMethod);
			testCase.Traits.Add("FOO", new List<string> { "BAR" });

			var fooTraitValues = testCase.Traits["foo"];

			var fooTraitValue = Assert.Single(fooTraitValues);
			Assert.Equal("BAR", fooTraitValue);
		}

		[Fact]
		public void TraitNamesAreCaseInsensitive_PreSeeded()
		{
			var traits = new Dictionary<string, List<string>> { { "FOO", new List<string> { "BAR" } } };
			var testMethod = Mocks.TestMethod();
			var testCase = new TestableTestMethodTestCase(testMethod, traits: traits);

			var fooTraitValues = testCase.Traits["foo"];

			var fooTraitValue = Assert.Single(fooTraitValues);
			Assert.Equal("BAR", fooTraitValue);
		}
	}

	public class UniqueID
	{
		[Fact]
		public static void UniqueID_NoArguments()
		{
			var uniqueID = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod").UniqueID;

			Assert.Equal("4428bc4e444a8f5294832dc06425f20fc994bdc44788f03219b7237f892bffe0", uniqueID);
		}

		[Fact]
		public static void UniqueID_Arguments()
		{
			var uniqueID42 = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object?[] { 42 }).UniqueID;
			var uniqueIDHelloWorld = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object?[] { "Hello, world!" }).UniqueID;
			var uniqueIDNull = TestableTestMethodTestCase.Create<ClassUnderTest>("TestMethod", new object?[] { null }).UniqueID;

			Assert.Equal("738d958f58f29698b62aa50479dcbb465fc18a500073f46947e60842e79e3e3b", uniqueID42);
			Assert.Equal("7ed69c84a3b325b79c2fd6a8a808033ac0c3f7bdda1a7575c882e69a5dc7ff9a", uniqueIDHelloWorld);
			Assert.Equal("e104382e5370a800728ffb748e92f65ffa3925eb888c4f48238d24c180b8bd48", uniqueIDNull);
		}

		class ClassUnderTest
		{
			[Fact]
			public static void TestMethod() { }
		}
	}

	[Serializable]
	class TestableTestMethodTestCase : TestMethodTestCase
	{
		protected TestableTestMethodTestCase(
			SerializationInfo info,
			StreamingContext context) :
				base(info, context)
		{ }

		public TestableTestMethodTestCase(
			_ITestMethod testMethod,
			object?[]? testMethodArguments = null,
			TestMethodDisplay defaultMethodDisplay = TestMethodDisplay.ClassAndMethod,
			TestMethodDisplayOptions defaultMethodDisplayOptions = TestMethodDisplayOptions.None,
			string? skipReason = null,
			Dictionary<string, List<string>>? traits = null,
			string? uniqueID = null)
				: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments, skipReason, traits, uniqueID)
		{ }

		public static TestableTestMethodTestCase Create<TClass>(
			string methodName,
			object?[]? testMethodArguments = null)
		{
			var testMethod = TestData.TestMethod<TClass>(methodName);
			return new TestableTestMethodTestCase(testMethod, testMethodArguments);
		}
	}
}
