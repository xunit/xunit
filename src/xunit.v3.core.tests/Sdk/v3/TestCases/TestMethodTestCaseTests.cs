using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class TestMethodTestCaseTests
{
	public class DisposeAsync
	{
		[Fact]
		public static async ValueTask DisposesArguments()
		{
			var disposable = new SerializableDisposable();
			var asyncDisposable = new SerializableAsyncDisposable();
			var testMethod = Mocks.TestMethod();
			var testCase = new TestableTestMethodTestCase(testMethod, testMethodArguments: new object[] { disposable, asyncDisposable });

			await testCase.DisposeAsync();

			Assert.True(disposable.DisposeCalled);
			Assert.True(asyncDisposable.DisposeAsyncCalled);
			Assert.False(asyncDisposable.DisposeCalled);  // Don't double-dispose
		}

		class SerializableDisposable : IXunitSerializable, IDisposable
		{
			public bool DisposeCalled = false;

			public void Dispose() => DisposeCalled = true;

			void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
			{ }

			void IXunitSerializable.Serialize(IXunitSerializationInfo info)
			{ }
		}

		class SerializableAsyncDisposable : IXunitSerializable, IAsyncDisposable, IDisposable
		{
			public bool DisposeAsyncCalled = false;
			public bool DisposeCalled = false;

			public void Dispose()
			{
				DisposeAsyncCalled = true;
			}

			public ValueTask DisposeAsync()
			{
				DisposeAsyncCalled = true;
				return default;
			}

			void IXunitSerializable.Deserialize(IXunitSerializationInfo info)
			{ }

			void IXunitSerializable.Serialize(IXunitSerializationInfo info)
			{ }
		}
	}

	public class Serialization
	{
		[Fact]
		public static void CanRoundTrip_PublicClass_PublicTestMethod()
		{
			var testCase = TestableTestMethodTestCase.Create<Serialization>(nameof(CanRoundTrip_PublicClass_PublicTestMethod));

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<TestableTestMethodTestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PublicClass_PrivateTestMethod()
		{
			var testCase = TestableTestMethodTestCase.Create<Serialization>(nameof(PrivateTestMethod));

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<_ITestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.IsType<TestableTestMethodTestCase>(deserialized);
		}

		[Fact]
		public static void CanRoundTrip_PrivateClass()
		{
			var testCase = TestableTestMethodTestCase.Create<PrivateClass>(nameof(PrivateClass.TestMethod));

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

	class TestableTestMethodTestCase : TestMethodTestCase
	{
		[Obsolete("For deserialization purposes only")]
		public TestableTestMethodTestCase()
		{ }

		public TestableTestMethodTestCase(
			_ITestMethod testMethod,
			string? uniqueID = null,
			string? skipReason = null,
			Dictionary<string, List<string>>? traits = null,
			object?[]? testMethodArguments = null)
				: base(testMethod, "test-case-display-name", uniqueID ?? "unique-id", skipReason, traits, testMethodArguments)
		{ }

		public static TestableTestMethodTestCase Create<TClass>(
			string methodName,
			object?[]? testMethodArguments = null)
		{
			var testMethod = TestData.TestMethod<TClass>(methodName);
			return new TestableTestMethodTestCase(testMethod, testMethodArguments: testMethodArguments);
		}
	}
}
