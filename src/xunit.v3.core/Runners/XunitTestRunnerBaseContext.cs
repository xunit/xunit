using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Context class for <see cref="XunitTestRunner"/>.
/// </summary>
public class XunitTestRunnerBaseContext<TTest> : TestRunnerContext<TTest>
	where TTest : class, IXunitTest
{
	// We want to cache the results of this, since it will potentially be called more than once,
	// and it involves reflection and dynamic invocation.
	readonly Lazy<string?> getRuntimeSkipReason;

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestRunnerBaseContext{TTest}"/> class.
	/// </summary>
	/// <param name="test">The test</param>
	/// <param name="messageBus">The message bus to send execution messages to</param>
	/// <param name="explicitOption">The user's choice on how to treat explicit tests</param>
	/// <param name="aggregator">The exception aggregator</param>
	/// <param name="cancellationTokenSource">The cancellation token source</param>
	/// <param name="beforeAfterTestAttributes">The <see cref="IBeforeAfterTestAttribute"/>s that are applied to the test</param>
	/// <param name="constructorArguments">The constructor arguments for the test class</param>
	public XunitTestRunnerBaseContext(
		TTest test,
		IMessageBus messageBus,
		ExplicitOption explicitOption,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyCollection<IBeforeAfterTestAttribute> beforeAfterTestAttributes,
		object?[] constructorArguments) :
			base(
				Guard.ArgumentNotNull(test),
				messageBus,
				test.SkipReason,
				explicitOption,
				aggregator,
				cancellationTokenSource,
				Guard.ArgumentNotNull(test).TestMethod.Method,
				test.TestMethodArguments
			)
	{
		BeforeAfterTestAttributes = Guard.ArgumentNotNull(beforeAfterTestAttributes);
		ConstructorArguments = Guard.ArgumentNotNull(constructorArguments);

		getRuntimeSkipReason = new(GetRuntimeSkipReason);
	}

	/// <summary>
	/// Gets the collection of <see cref="IBeforeAfterTestAttribute"/> used for this test.
	/// </summary>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes { get; private set; }

	/// <summary>
	/// Gets the arguments that should be passed to the test class when it's constructed.
	/// </summary>
	public object?[] ConstructorArguments { get; }

	string? GetRuntimeSkipReason() =>
		// We want to record any issues as exceptions in the aggregator so that the test
		// fails rather than run. We know the first time we're call it'll be before test
		// invocation, so recording the exception will result in a test failure.
		Aggregator.Run(() =>
		{
			// TODO: Tests should have SkipUnless, SkipWhen, and SkipType as well, so that we can allow
			// data rows to not just temporarily skip, but conditionally skip, just like whole tests.
			var skipReason = Test.SkipReason;
			var skipUnless = Test.TestCase.SkipUnless;
			var skipWhen = Test.TestCase.SkipWhen;

			if (skipUnless is null && skipWhen is null)
				return skipReason;
			if (skipUnless is not null && skipWhen is not null)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Both 'SkipUnless' and 'SkipWhen' are set on test method '{0}.{1}'; they are mutually exclusive",
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);
			if (skipReason is null)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"You must set 'Skip' when you set 'SkipUnless' or 'SkipWhen' on test method '{0}.{1}' to set the message for conditional skips",
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);

			var propertyType = Test.TestCase.SkipType ?? Test.TestCase.TestClass.Class;
			var propertyName = (skipUnless ?? skipWhen)!;
			var property =
				propertyType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)
					?? throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Cannot find public static property '{0}' on type '{1}' for dynamic skip on test method '{2}.{3}'",
							propertyName,
							propertyType,
							Test.TestCase.TestClassName,
							Test.TestCase.TestMethodName
						)
					);
			var getMethod =
				property.GetGetMethod()
					?? throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Public static property '{0}' on type '{1}' must be readable for dynamic skip on test method '{2}.{3}'",
							propertyName,
							propertyType,
							Test.TestCase.TestClassName,
							Test.TestCase.TestMethodName
						)
					);
			if (getMethod.ReturnType != typeof(bool) || getMethod.Invoke(null, []) is not bool result)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Public static property '{0}' on type '{1}' must return bool for dynamic skip on test method '{2}.{3}'",
						propertyName,
						propertyType,
						Test.TestCase.TestClassName,
						Test.TestCase.TestMethodName
					)
				);

			var shouldSkip = (skipUnless, skipWhen, result) switch
			{
				(not null, _, false) => true,
				(_, not null, true) => true,
				_ => false,
			};

			return shouldSkip ? skipReason : null;
		}, null);

	/// <summary>
	/// Gets the runtime skip reason for the test, inspecting the provided exception to see
	/// if it contractually matches a "dynamically skipped" exception (that is, any
	/// exception message that starts with <see cref="DynamicSkipToken.Value"/>).
	/// If the exception does not match the pattern, consults the base skip reason
	/// (from <see cref="IFactAttribute.Skip"/>), as well as <see cref="IFactAttribute.SkipUnless"/>
	/// and <see cref="IFactAttribute.SkipWhen"/> to determine if the test should be
	/// dynamically skipped.
	/// </summary>
	/// <param name="exception">The exception to inspect</param>
	/// <returns>The skip reason, if the test is skipped; <c>null</c>, otherwise</returns>
	public override string? GetSkipReason(Exception? exception)
	{
		if (exception is not null)
		{
			if (Test.TestCase.SkipExceptions?.Contains(exception.GetType()) == true)
				return exception.Message.Length != 0 ? exception.Message : string.Format(CultureInfo.CurrentCulture, "Exception of type '{0}' was thrown", exception.GetType().SafeName());

			// We don't want a strongly typed contract here; any exception can be a "dynamically
			// skipped" exception so long as its message starts with the special token.
			if (exception.Message.StartsWith(DynamicSkipToken.Value, StringComparison.Ordinal))
				return exception.Message.Substring(DynamicSkipToken.Value.Length);
		}

		return getRuntimeSkipReason.Value;
	}

	/// <summary>
	/// Runs the <see cref="IBeforeAfterTestAttribute.After"/> side of the before after attributes.
	/// </summary>
	public void RunAfterAttributes()
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

				Aggregator.Run(() => beforeAfterAttribute.After(Test.TestMethod.Method, Test));

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
	/// Runs the <see cref="IBeforeAfterTestAttribute.Before"/> side of the before after attributes.
	/// </summary>
	public void RunBeforeAttributes()
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
						beforeAfterAttribute.Before(Test.TestMethod.Method, Test);
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
}
