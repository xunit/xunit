using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// The test invoker for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestInvoker : TestInvoker<IXunitTestCase>
	{
		readonly Stack<BeforeAfterTestAttribute> beforeAfterAttributesRun = new Stack<BeforeAfterTestAttribute>();

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestInvoker"/> class.
		/// </summary>
		/// <param name="testAssemblyUniqueID">The test assembly unique ID.</param>
		/// <param name="testCollectionUniqueID">The test collection unique ID.</param>
		/// <param name="testClassUniqueID">The test class unique ID.</param>
		/// <param name="testMethodUniqueID">The test method unique ID.</param>
		/// <param name="testCaseUniqueID">The test case unique ID.</param>
		/// <param name="testUniqueID">The test unique ID.</param>
		/// <param name="test">The test that this invocation belongs to.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testClass">The test class that the test method belongs to.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethod">The test method that will be invoked.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="beforeAfterAttributes">The list of <see cref="BeforeAfterTestAttribute"/>s for this test invocation.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public XunitTestInvoker(
			string testAssemblyUniqueID,
			string testCollectionUniqueID,
			string? testClassUniqueID,
			string? testMethodUniqueID,
			string testCaseUniqueID,
			string testUniqueID,
			ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				base(
					testAssemblyUniqueID,
					testCollectionUniqueID,
					testClassUniqueID,
					testMethodUniqueID,
					testCaseUniqueID,
					testUniqueID,
					test,
					messageBus,
					testClass,
					constructorArguments,
					testMethod,
					testMethodArguments,
					aggregator,
					cancellationTokenSource
				)
		{
			BeforeAfterAttributes = Guard.ArgumentNotNull(nameof(beforeAfterAttributes), beforeAfterAttributes);
		}

		/// <summary>
		/// Gets the list of <see cref="BeforeAfterTestAttribute"/>s for this test invocation.
		/// </summary>
		protected IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes { get; }

		/// <inheritdoc/>
		protected override Task BeforeTestMethodInvokedAsync()
		{
			foreach (var beforeAfterAttribute in BeforeAfterAttributes)
			{
				var attributeName = beforeAfterAttribute.GetType().Name;
				var beforeTestStarting = new _BeforeTestStarting
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = TestCaseUniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = TestUniqueID
				};
				if (!MessageBus.QueueMessage(beforeTestStarting))
					CancellationTokenSource.Cancel();
				else
				{
					try
					{
						Timer.Aggregate(() => beforeAfterAttribute.Before(TestMethod, Test));
						beforeAfterAttributesRun.Push(beforeAfterAttribute);
					}
					catch (Exception ex)
					{
						Aggregator.Add(ex);
						break;
					}
					finally
					{
						var beforeTestFinished = new _BeforeTestFinished
						{
							AssemblyUniqueID = TestAssemblyUniqueID,
							AttributeName = attributeName,
							TestCaseUniqueID = TestCaseUniqueID,
							TestClassUniqueID = TestClassUniqueID,
							TestCollectionUniqueID = TestCollectionUniqueID,
							TestMethodUniqueID = TestMethodUniqueID,
							TestUniqueID = TestUniqueID
						};
						if (!MessageBus.QueueMessage(beforeTestFinished))
							CancellationTokenSource.Cancel();
					}
				}

				if (CancellationTokenSource.IsCancellationRequested)
					break;
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task AfterTestMethodInvokedAsync()
		{
			foreach (var beforeAfterAttribute in beforeAfterAttributesRun)
			{
				var attributeName = beforeAfterAttribute.GetType().Name;
				var afterTestStarting = new _AfterTestStarting
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = TestCaseUniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = TestUniqueID
				};
				if (!MessageBus.QueueMessage(afterTestStarting))
					CancellationTokenSource.Cancel();

				Aggregator.Run(() => Timer.Aggregate(() => beforeAfterAttribute.After(TestMethod, Test)));

				var afterTestFinished = new _AfterTestFinished
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = TestCaseUniqueID,
					TestClassUniqueID = TestClassUniqueID,
					TestCollectionUniqueID = TestCollectionUniqueID,
					TestMethodUniqueID = TestMethodUniqueID,
					TestUniqueID = TestUniqueID
				};
				if (!MessageBus.QueueMessage(afterTestFinished))
					CancellationTokenSource.Cancel();
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc/>
		protected override Task<decimal> InvokeTestMethodAsync(object? testClassInstance)
		{
			if (TestCase.InitializationException != null)
			{
				var tcs = new TaskCompletionSource<decimal>();
				tcs.SetException(TestCase.InitializationException);
				return tcs.Task;
			}

			return TestCase.Timeout > 0
				? InvokeTimeoutTestMethodAsync(testClassInstance)
				: base.InvokeTestMethodAsync(testClassInstance);
		}

		async Task<decimal> InvokeTimeoutTestMethodAsync(object? testClassInstance)
		{
			var baseTask = base.InvokeTestMethodAsync(testClassInstance);
			var resultTask = await Task.WhenAny(baseTask, Task.Delay(TestCase.Timeout));

			if (resultTask != baseTask)
				throw new TestTimeoutException(TestCase.Timeout);

			return baseTask.Result;
		}
	}
}
