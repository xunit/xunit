using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test invoker for xUnit.net v3 tests.
/// </summary>
public class XunitTestInvoker : TestInvoker<XunitTestInvokerContext>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestInvoker"/> class.
	/// </summary>
	protected XunitTestInvoker()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestInvoker"/>.
	/// </summary>
	public static XunitTestInvoker Instance = new();

	/// <inheritdoc/>
	protected override ValueTask AfterTestMethodInvokedAsync(XunitTestInvokerContext ctxt)
	{
		var testUniqueID = ctxt.Test.UniqueID;

		// At this point, this list has been pruned to only the attributes that were successfully run
		// during the call to BeforeTestMethodInvokedAsync.
		if (ctxt.BeforeAfterTestAttributes.Count > 0)
		{
			var testAssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = ctxt.Test.TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = ctxt.Test.TestCase.UniqueID;

			foreach (var beforeAfterAttribute in ctxt.BeforeAfterTestAttributes)
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

				if (!ctxt.MessageBus.QueueMessage(afterTestStarting))
					ctxt.CancellationTokenSource.Cancel();

				ctxt.Aggregator.Run(() => beforeAfterAttribute.After(ctxt.TestMethod, ctxt.Test));

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

				if (!ctxt.MessageBus.QueueMessage(afterTestFinished))
					ctxt.CancellationTokenSource.Cancel();
			}
		}

		return default;
	}

	/// <inheritdoc/>
	protected override ValueTask BeforeTestMethodInvokedAsync(XunitTestInvokerContext ctxt)
	{
		var testUniqueID = ctxt.Test.UniqueID;

		// At this point, this list is the full attribute list from the call to RunAsync. We attempt to
		// run the Before half of the attributes, and then keep track of which ones we successfully ran,
		// so we can put only those back into the dictionary for later retrieval by AfterTestMethodInvokedAsync.
		if (ctxt.BeforeAfterTestAttributes.Count > 0)
		{
			var testAssemblyUniqueID = ctxt.Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = ctxt.Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = ctxt.Test.TestCase.TestMethod?.TestClass.UniqueID;
			var testMethodUniqueID = ctxt.Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = ctxt.Test.TestCase.UniqueID;

			// Since we want them cleaned in reverse order from how they're run, we'll push a stack back
			// into the container rather than a list.
			var beforeAfterAttributesRun = new Stack<BeforeAfterTestAttribute>();

			foreach (var beforeAfterAttribute in ctxt.BeforeAfterTestAttributes)
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

				if (!ctxt.MessageBus.QueueMessage(beforeTestStarting))
					ctxt.CancellationTokenSource.Cancel();
				else
				{
					try
					{
						beforeAfterAttribute.Before(ctxt.TestMethod, ctxt.Test);
						beforeAfterAttributesRun.Push(beforeAfterAttribute);
					}
					catch (Exception ex)
					{
						ctxt.Aggregator.Add(ex);
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

						if (!ctxt.MessageBus.QueueMessage(beforeTestFinished))
							ctxt.CancellationTokenSource.Cancel();
					}
				}

				if (ctxt.CancellationTokenSource.IsCancellationRequested)
					break;
			}

			ctxt.BeforeAfterTestAttributes = beforeAfterAttributesRun;
		}

		return default;
	}

	/// <inheritdoc/>
	protected override object? CreateTestClassInstance(XunitTestInvokerContext ctxt)
	{
		// We allow for Func<T> when the argument is T, such that we should be able to get the value just before
		// invoking the test. So we need to do a transform on the arguments.
		object?[]? actualCtorArguments = null;

		if (ctxt.ConstructorArguments != null)
		{
			var ctorParams =
				ctxt
					.TestClass
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.Single()
					.GetParameters();

			actualCtorArguments = new object?[ctxt.ConstructorArguments.Length];

			for (var idx = 0; idx < ctxt.ConstructorArguments.Length; ++idx)
			{
				actualCtorArguments[idx] = ctxt.ConstructorArguments[idx];

				var ctorArgumentValueType = ctxt.ConstructorArguments[idx]?.GetType();
				if (ctorArgumentValueType != null)
				{
					var ctorArgumentParamType = ctorParams[idx].ParameterType;
					if (ctorArgumentParamType != ctorArgumentValueType &&
						ctorArgumentValueType == typeof(Func<>).MakeGenericType(ctorArgumentParamType))
					{
						var invokeMethod = ctorArgumentValueType.GetMethod("Invoke", new Type[0]);
						if (invokeMethod != null)
							actualCtorArguments[idx] = invokeMethod.Invoke(ctxt.ConstructorArguments[idx], new object?[0]);
					}
				}
			}
		}

		return Activator.CreateInstance(ctxt.TestClass, actualCtorArguments);
	}

	/// <inheritdoc/>
	protected override ValueTask<decimal> InvokeTestMethodAsync(
		XunitTestInvokerContext ctxt,
		object? testClassInstance)
	{
		var testCase = (IXunitTestCase)ctxt.Test.TestCase;

		if (testCase.InitializationException != null)
		{
			var tcs = new TaskCompletionSource<decimal>();
			tcs.SetException(testCase.InitializationException);
			return new(tcs.Task);
		}

		return
			testCase.Timeout > 0
				? InvokeTimeoutTestMethodAsync(ctxt, testClassInstance, testCase.Timeout)
				: base.InvokeTestMethodAsync(ctxt, testClassInstance);
	}

	async ValueTask<decimal> InvokeTimeoutTestMethodAsync(
		XunitTestInvokerContext ctxt,
		object? testClassInstance,
		int timeout)
	{
		if (!ctxt.TestMethod.IsAsync())
			throw TestTimeoutException.ForIncompatibleTest();

		var baseTask = base.InvokeTestMethodAsync(ctxt, testClassInstance).AsTask();
		var resultTask = await Task.WhenAny(baseTask, Task.Delay(timeout));

		if (resultTask != baseTask)
			throw TestTimeoutException.ForTimedOutTest(timeout);

		return await baseTask;
	}

	/// <summary>
	/// Creates the test class (if necessary), and invokes the test method.
	/// </summary>
	/// <param name="test">The test that should be run</param>
	/// <param name="testClass">The type that the test belongs to</param>
	/// <param name="constructorArguments">The constructor arguments used to create the class instance</param>
	/// <param name="testMethod">The method that the test belongs to</param>
	/// <param name="testMethodArguments">The arguments for the test method</param>
	/// <param name="beforeAfterTestAttributes">The <see cref="BeforeAfterTestAttribute"/> instances attached to this test</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="aggregator">The aggregator used to </param>
	/// <param name="cancellationTokenSource">The cancellation token source used to cancel test execution</param>
	/// <returns>Returns the time (in seconds) spent creating the test class, running
	/// the test, and disposing of the test class.</returns>
	public ValueTask<decimal> RunAsync(
		_ITest test,
		Type testClass,
		object?[] constructorArguments,
		MethodInfo testMethod,
		object?[]? testMethodArguments,
		IReadOnlyCollection<BeforeAfterTestAttribute> beforeAfterTestAttributes,
		IMessageBus messageBus,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource) =>
			RunAsync(new(test, testClass, constructorArguments, testMethod, testMethodArguments, messageBus, aggregator, cancellationTokenSource, beforeAfterTestAttributes));
}
