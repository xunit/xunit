using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

public class TestAssemblyRunnerContextTests
{
	public class CreateMessageBus
	{
		[Fact]
		public static async ValueTask GuardClause()
		{
			await using var ctxt = TestableTestAssemblyRunnerContext.Create();

			var ex = Record.Exception(() => ctxt.MessageBus);

			var upex = Assert.IsType<UnsetPropertyException>(ex);
			Assert.Equal(nameof(TestableTestAssemblyRunnerContext.MessageBus), upex.PropertyName);
			Assert.Equal(typeof(TestableTestAssemblyRunnerContext).FullName, upex.TypeName);
		}

		[Fact]
		public static async ValueTask DefaultMessageBus()
		{
			await using var ctxt = TestableTestAssemblyRunnerContext.Create();
			await ctxt.InitializeAsync();

			var result = ctxt.MessageBus;

			Assert.IsType<MessageBus>(result);
		}

		[Fact]
		public static async ValueTask SyncMessageBusOption()
		{
			var executionOptions = TestData.TestFrameworkExecutionOptions();
			executionOptions.SetSynchronousMessageReporting(true);
			await using var ctxt = TestableTestAssemblyRunnerContext.Create(executionOptions);
			await ctxt.InitializeAsync();

			var result = ctxt.MessageBus;

			Assert.IsType<SynchronousMessageBus>(result);
		}
	}

	class TestableTestAssemblyRunnerContext : TestAssemblyRunnerContext<ITestAssembly, ITestCase>
	{
		TestableTestAssemblyRunnerContext(
			ITestAssembly testAssembly,
			IReadOnlyCollection<ITestCase> testCases,
			IMessageSink executionMessageSink,
			ITestFrameworkExecutionOptions executionOptions) :
				base(testAssembly, testCases, executionMessageSink, executionOptions, default)
		{ }

		public static TestableTestAssemblyRunnerContext Create(ITestFrameworkExecutionOptions? executionOptions = null) =>
			new(Mocks.TestAssembly(), [], SpyMessageSink.Create(), executionOptions ?? TestData.TestFrameworkExecutionOptions());
	}
}
