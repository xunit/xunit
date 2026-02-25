using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="CoreTestRunner{TContext, TTest, TBeforeAfterAttribute}"/>.
/// </summary>
/// <param name="test">The test</param>
/// <param name="messageBus">The message bus to send execution messages to</param>
/// <param name="skipReason">The skip reason for the test, if it's being skipped</param>
/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
/// <param name="aggregator">The exception aggregator</param>
/// <param name="cancellationTokenSource">The cancellation token source</param>
/// <typeparam name="TTest">The type of the test used by the test framework. Must
/// derive from <see cref="ICoreTest"/>.</typeparam>
/// <typeparam name="TBeforeAfterAttribute">The type of the before after attribute</typeparam>
/// <remarks>
/// This class is shared between reflection-based and code generation-based tests.
/// </remarks>
public abstract class CoreTestRunnerContext<TTest, TBeforeAfterAttribute>(
	TTest test,
	IMessageBus messageBus,
	string? skipReason,
	ExplicitOption explicitOption,
	ExceptionAggregator aggregator,
	CancellationTokenSource cancellationTokenSource) :
		TestRunnerContext<TTest>(test, messageBus, skipReason, explicitOption, aggregator, cancellationTokenSource)
			where TTest : class, ICoreTest
			where TBeforeAfterAttribute : notnull
{
	/// <summary>
	/// Gets or sets the collection of <typeparamref name="TBeforeAfterAttribute"/>s for this test.
	/// </summary>
	protected abstract IReadOnlyCollection<TBeforeAfterAttribute> BeforeAfterTestAttributes { get; set; }

	/// <summary>
	/// Implement this method to do runtime skip detection, which typically involves looking at
	/// <c>SkipUnless</c> and <c>SkipWhen</c> and invoking those as appropriate (or just returning
	/// the <c>SkipReason</c> when neither is set).
	/// </summary>
	/// <returns>The skip reason, if the test is skipped; <see langword="null"/>, otherwise</returns>
	protected abstract string? GetRuntimeSkipReason();

	/// <summary>
	/// Gets the runtime skip reason for the test, inspecting the provided exception to see
	/// if it contractually matches a "dynamically skipped" exception (that is, any
	/// exception message that starts with <see cref="DynamicSkipToken.Value"/> or any
	/// exception that matches a type from <see cref="ICoreTestCase.SkipExceptions"/>).
	/// If the exception does not match either of these, delegates to <see cref="GetRuntimeSkipReason"/>.
	/// </summary>
	/// <returns>The skip reason, if the test is skipped; <see langword="null"/>, otherwise</returns>
	public override string? GetSkipReason(Exception? exception)
	{
		if (exception is not null)
		{
			if (Test.TestCase.SkipExceptions?.Contains(exception.GetType()) == true)
				return
					exception.Message is not null && exception.Message.Length != 0
						? exception.Message
						: string.Format(CultureInfo.CurrentCulture, "Exception of type '{0}' was thrown", exception.GetType().SafeName());

			// We don't want a strongly typed contract here; any exception can be a "dynamically
			// skipped" exception so long as its message starts with the special token.
			if (exception.Message?.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal) == true)
				return exception.Message.Substring(DynamicSkipToken.Value.Length);
		}

		return GetRuntimeSkipReason();
	}

	/// <summary>
	/// Invokes the test and returns the amount of time spent executing.
	/// </summary>
	/// <param name="testClassInstance">The instance of the test class (may be <see langword="null"/> when
	/// running a static test method)</param>
	/// <returns>Returns the execution time (in seconds) spent running the test.</returns>
	public abstract ValueTask<TimeSpan> InvokeTest(object? testClassInstance);

	/// <summary>
	/// Runs the <typeparamref name="TBeforeAfterAttribute"/>.<c>After</c> side of the before after attributes.
	/// </summary>
	public void RunAfterAttributes()
	{
		var testUniqueID = Test.UniqueID;

		// At this point, this list has been pruned to only contain the attributes that were
		// successfully run during the call to RunBeforeAttributes, in reverse order.
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
				var afterTestStarting = new AfterTestStarting
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

				Aggregator.Run(() => RunAfter(beforeAfterAttribute));

				var afterTestFinished = new AfterTestFinished
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
	/// Runs the <typeparamref name="TBeforeAfterAttribute"/>.<c>Before</c> side of the before after attributes.
	/// </summary>
	public void RunBeforeAttributes()
	{
		var testUniqueID = Test.UniqueID;

		// At this point, this list is the full attribute list from the test method. We attempt to
		// run the Before half of the attributes, and then keep track of which ones we successfully ran,
		// so we can put only those back into the list for later retrieval during DisposeAsync.
		if (BeforeAfterTestAttributes.Count > 0)
		{
			var testAssemblyUniqueID = Test.TestCase.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = Test.TestCase.TestCollection.UniqueID;
			var testClassUniqueID = Test.TestCase.TestClass?.UniqueID;
			var testMethodUniqueID = Test.TestCase.TestMethod?.UniqueID;
			var testCaseUniqueID = Test.TestCase.UniqueID;

			// Since we want them cleaned in reverse order from how they're run, we'll push a stack back
			// into the container rather than a list.
			var beforeAfterAttributesRun = new Stack<TBeforeAfterAttribute>();

			foreach (var beforeAfterAttribute in BeforeAfterTestAttributes)
			{
				var attributeName = beforeAfterAttribute.GetType().Name;
				var beforeTestStarting = new BeforeTestStarting
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
						RunBefore(beforeAfterAttribute);
						beforeAfterAttributesRun.Push(beforeAfterAttribute);
					}
					catch (Exception ex)
					{
						Aggregator.Add(ex);
						break;
					}
					finally
					{
						var beforeTestFinished = new BeforeTestFinished
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

	/// <summary>
	/// Runs the <typeparamref name="TBeforeAfterAttribute"/>.<c>After</c> side of a single before after attribute.
	/// </summary>
	public abstract void RunAfter(TBeforeAfterAttribute attribute);

	/// <summary>
	/// Runs the <typeparamref name="TBeforeAfterAttribute"/>.<c>Before</c> side of a single before after attribute.
	/// </summary>
	public abstract void RunBefore(TBeforeAfterAttribute attribute);
}
