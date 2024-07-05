using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
/// <param name="test">The test</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="skipReason">The skip reason for the test, if it's being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <param name="beforeAfterTestAttributes">The <see cref="IBeforeAfterTestAttribute"/>s that are applied to the test</param>
/// <param name="constructorArguments">The constructor arguments for the test class</param>
/// <param name="testMethodArguments">The method arguments for the test method</param>
public class XunitTestRunnerContext(
	IXunitTest test,
	IMessageBus messageBus,
	string? skipReason,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource,
	IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
	object?[] constructorArguments,
	object?[] testMethodArguments) :
		TestRunnerContext<IXunitTest>(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
{
	/// <summary>
	/// Gets the collection of <see cref="IBeforeAfterTestAttribute"/> used for this test.
	/// </summary>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; private set; } = Guard.ArgumentNotNull(beforeAfterTestAttributes);

	/// <summary>
	/// Gets the arguments that should be passed to the test class when it's constructed.
	/// </summary>
	public object?[] ConstructorArguments { get; } = Guard.ArgumentNotNull(constructorArguments);

	/// <summary>
	/// Gets the arguments to be passed to the test method during invocation.
	/// </summary>
	public object?[] TestMethodArguments { get; } = Guard.ArgumentNotNull(testMethodArguments);

	/// <summary>
	/// Runs the <see cref="IBeforeAfterTestAttribute.After"/> side of the before after attributes.
	/// </summary>
	public async ValueTask RunAfterAttributes()
	{
		var testUniqueID = Test.UniqueID;

		// At this point, this list has been pruned to only contain the attributes that were
		// successfully run during the call to BeforeTestMethodInvoked.
		if (BeforeAfterTestAttributes.Count > 0)
		{
			var testAssemblyUniqueID = Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = Test.TestCase.TestClass?.UniqueID;
			var testMethodUniqueID = Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = Test.TestCase.UniqueID;

			foreach (var beforeAfterAttribute in BeforeAfterTestAttributes)
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

				await Aggregator.RunAsync(() => beforeAfterAttribute.After(Test.TestMethod.Method, Test));

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
		}
	}

	/// <summary>
	/// Runs the <see cref="IBeforeAfterTestAttribute.Before"/> side of the before after attributes.
	/// </summary>
	public async ValueTask RunBeforeAttributes()
	{
		var testUniqueID = Test.UniqueID;

		// At this point, this list is the full attribute list from the call to RunAsync. We attempt to
		// run the Before half of the attributes, and then keep track of which ones we successfully ran,
		// so we can put only those back into the dictionary for later retrieval during DisposeAsync.
		if (BeforeAfterTestAttributes.Count > 0)
		{
			var testAssemblyUniqueID = Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = Test.TestCase.TestClass?.UniqueID;
			var testMethodUniqueID = Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = Test.TestCase.UniqueID;

			// Since we want them cleaned in reverse order from how they're run, we'll push a stack back
			// into the container rather than a list.
			var beforeAfterAttributesRun = new Stack<IBeforeAfterTestAttribute>();

			foreach (var beforeAfterAttribute in BeforeAfterTestAttributes)
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
						await beforeAfterAttribute.Before(Test.TestMethod.Method, Test);
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

			BeforeAfterTestAttributes = beforeAfterAttributesRun;
		}
	}
}
