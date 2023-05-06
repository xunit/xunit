using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestCaseTests
{
	public class Serialization
	{
		[Theory]
		[InlineData(false, 0)]
		[InlineData(true, 42)]
		public void SerializesExplicitAndTimeout(
			bool @explicit,
			int timeout)
		{
			var testMethod = TestData.TestMethod<Serialization>(nameof(SerializesExplicitAndTimeout));
			var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", @explicit, timeout: timeout);

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<IXunitTestCase>(serialized);

			Assert.NotNull(deserialized);
			Assert.Equal(@explicit, deserialized.Explicit);
			Assert.Equal(timeout, deserialized.Timeout);
		}

		[Fact]
		public void SerializesBaseStaticMethodDecoratedWithFact()
		{
			var testMethod = TestData.TestMethod<ClassWithInheritedStaticMethodUnderTest>(nameof(ClassWithInheritedStaticMethodUnderTest.Passing));
			var testCase = new XunitTestCase(testMethod, "display-name", "unique-id", true, timeout: 42);

			var serialized = SerializationHelper.Serialize(testCase);
			var deserialized = SerializationHelper.Deserialize<IXunitTestCase>(serialized);

			Assert.NotNull(deserialized);
		}

		public abstract class BaseClassWithStaticMethodUnderTest
		{
			[Fact]
			public static void Passing() { }
		}

		public class ClassWithInheritedStaticMethodUnderTest : BaseClassWithStaticMethodUnderTest
		{ }
	}
}
