using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test invoker for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestInvoker : TestInvoker<IXunitTestCase>
	{
		readonly Stack<BeforeAfterTestAttribute> beforeAfterAttributesRun = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestInvoker"/> class.
		/// </summary>
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
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) :
				base(
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
		protected override ValueTask BeforeTestMethodInvokedAsync()
		{
			var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
			var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = TestCase.UniqueID;
			var testUniqueID = Test.UniqueID;

			foreach (var beforeAfterAttribute in BeforeAfterAttributes)
			{
				var attributeName = beforeAfterAttribute.GetType().Name;
				var beforeTestStarting = new _BeforeTestStarting
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
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
							AssemblyUniqueID = testAssemblyUniqueID,
							AttributeName = attributeName,
							TestCaseUniqueID = testCaseUniqueID,
							TestClassUniqueID = testClassUniqueID,
							TestCollectionUniqueID = testCollectionUniqueID,
							TestMethodUniqueID = testMethodUniqueID,
							TestUniqueID = testUniqueID
						};
						if (!MessageBus.QueueMessage(beforeTestFinished))
							CancellationTokenSource.Cancel();
					}
				}

				if (CancellationTokenSource.IsCancellationRequested)
					break;
			}

			return default;
		}

		/// <inheritdoc/>
		protected override ValueTask AfterTestMethodInvokedAsync()
		{
			var testAssemblyUniqueID = TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = TestCase.TestCollection.UniqueID;
			var testClassUniqueID = TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = TestCase.UniqueID;
			var testUniqueID = Test.UniqueID;

			foreach (var beforeAfterAttribute in beforeAfterAttributesRun)
			{
				var attributeName = beforeAfterAttribute.GetType().Name;
				var afterTestStarting = new _AfterTestStarting
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};
				if (!MessageBus.QueueMessage(afterTestStarting))
					CancellationTokenSource.Cancel();

				Aggregator.Run(() => Timer.Aggregate(() => beforeAfterAttribute.After(TestMethod, Test)));

				var afterTestFinished = new _AfterTestFinished
				{
					AssemblyUniqueID = testAssemblyUniqueID,
					AttributeName = attributeName,
					TestCaseUniqueID = testCaseUniqueID,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestMethodUniqueID = testMethodUniqueID,
					TestUniqueID = testUniqueID
				};
				if (!MessageBus.QueueMessage(afterTestFinished))
					CancellationTokenSource.Cancel();
			}

			return default;
		}

		/// <inheritdoc/>
		protected override object? CreateTestClassInstance()
		{
			// We allow for Func<T> when the argument is T, such that we should be able to get the value just before
			// invoking the test. So we need to do a transform on the arguments.
			object?[]? actualCtorArguments = null;

			if (ConstructorArguments != null)
			{
				// TODO: Don't like redoing the logic from XunitTestClassRunner here
				var ctorParams =
					TestClass
						.GetConstructors()
						.Where(ci => !ci.IsStatic && ci.IsPublic)
						.Single()
						.GetParameters();

				actualCtorArguments = new object?[ConstructorArguments.Length];

				for (var idx = 0; idx < ConstructorArguments.Length; ++idx)
				{
					actualCtorArguments[idx] = ConstructorArguments[idx];

					var ctorArgumentValueType = ConstructorArguments[idx]?.GetType();
					if (ctorArgumentValueType != null)
					{
						var ctorArgumentParamType = ctorParams[idx].ParameterType;
						if (ctorArgumentParamType != ctorArgumentValueType &&
							ctorArgumentValueType == typeof(Func<>).MakeGenericType(ctorArgumentParamType))
						{
							var invokeMethod = ctorArgumentValueType.GetMethod("Invoke", new Type[0]);
							if (invokeMethod != null)
								actualCtorArguments[idx] = invokeMethod.Invoke(ConstructorArguments[idx], new object?[0]);
						}
					}
				}
			}

			return Activator.CreateInstance(TestClass, actualCtorArguments);
		}

		/// <inheritdoc/>
		protected override ValueTask InvokeTestMethodAsync(object? testClassInstance)
		{
			if (TestCase.InitializationException != null)
			{
				var tcs = new TaskCompletionSource<decimal>();
				tcs.SetException(TestCase.InitializationException);
				return new(tcs.Task);
			}

			return TestCase.Timeout > 0
				? InvokeTimeoutTestMethodAsync(testClassInstance)
				: base.InvokeTestMethodAsync(testClassInstance);
		}

		async ValueTask InvokeTimeoutTestMethodAsync(object? testClassInstance)
		{
			var baseTask = base.InvokeTestMethodAsync(testClassInstance).AsTask();
			var resultTask = await Task.WhenAny(baseTask, Task.Delay(TestCase.Timeout));

			if (resultTask != baseTask)
				throw new TestTimeoutException(TestCase.Timeout);
		}
	}
}
