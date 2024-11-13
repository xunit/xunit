using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestClassRunner :
	TestClassRunner<XunitTestClassRunnerContext, IXunitTestClass, IXunitTestMethod, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	protected XunitTestClassRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of the <see cref="XunitTestClassRunner"/> class.
	/// </summary>
	public static XunitTestClassRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (!ctxt.Aggregator.HasExceptions)
		{
			var ctor = SelectTestClassConstructor(ctxt);
			if (ctor is not null)
			{
				var unusedArguments = new List<Tuple<int, ParameterInfo>>();
				var parameters = ctor.GetParameters();

				var constructorArguments = new object?[parameters.Length];
				for (var idx = 0; idx < parameters.Length; ++idx)
				{
					var parameter = parameters[idx];

					var argumentValue = await GetConstructorArgument(ctxt, ctor, idx, parameter);
					if (argumentValue is not null)
						constructorArguments[idx] = argumentValue;
					else if (parameter.HasDefaultValue)
						constructorArguments[idx] = parameter.DefaultValue;
					else if (parameter.IsOptional)
						constructorArguments[idx] = parameter.ParameterType.GetDefaultValue();
					else if (parameter.GetCustomAttribute<ParamArrayAttribute>() is not null)
						constructorArguments[idx] = Array.CreateInstance(parameter.ParameterType, 0);
					else
						unusedArguments.Add(Tuple.Create(idx, parameter));
				}

				if (unusedArguments.Count > 0)
					ctxt.Aggregator.Add(new TestPipelineException(FormatConstructorArgsMissingMessage(ctxt, ctor, unusedArguments)));

				return constructorArguments;
			}
		}

		return [];
	}

	/// <summary>
	/// Gets the message to be used when the constructor is missing arguments.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="constructor">The constructor that was selected</param>
	/// <param name="unusedArguments">The arguments that had no matching parameter values</param>
	protected virtual string FormatConstructorArgsMissingMessage(
		XunitTestClassRunnerContext ctxt,
		ConstructorInfo constructor,
		IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments) =>
			string.Format(
				CultureInfo.CurrentCulture,
				"The following constructor parameters did not have matching fixture data: {0}",
				string.Join(", ", unusedArguments.Select(arg => string.Format(CultureInfo.CurrentCulture, "{0} {1}", arg.Item2.ParameterType.Name, arg.Item2.Name)))
			);


	/// <summary>
	/// Tries to supply a test class constructor argument.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="constructor">The constructor that will be used to create the test class.</param>
	/// <param name="index">The parameter index.</param>
	/// <param name="parameter">The parameter information.</param>
	/// <returns>Returns the constructor argument if available, <c>null</c> otherwise.</returns>
	protected virtual async ValueTask<object?> GetConstructorArgument(
		XunitTestClassRunnerContext ctxt,
		ConstructorInfo constructor,
		int index,
		ParameterInfo parameter)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(constructor);
		Guard.ArgumentNotNull(parameter);

		if (parameter.ParameterType == typeof(ITestContextAccessor))
			return TestContextAccessor.Instance;

		// Logic to support passing Func<T> instead of T lives in XunitTestInvoker.CreateTestClassInstance
		// The actual TestOutputHelper instance is created in XunitTestRunner.SetTestContext when creating
		// the test context object.
		return
			parameter.ParameterType == typeof(ITestOutputHelper)
				? (() => TestContext.Current.TestOutputHelper)
				: await ctxt.ClassFixtureMappings.GetFixture(parameter.ParameterType);
	}

	/// <inheritdoc/>
	protected override ValueTask<bool> OnTestClassCleanupFailure(
		XunitTestClassRunnerContext ctxt,
		Exception exception)
	{
		Guard.ArgumentNotNull(ctxt);

		var (types, messages, stackTraces, indices, _) = ExceptionUtility.ExtractMetadata(exception);

		return new(ctxt.MessageBus.QueueMessage(new TestClassCleanupFailure
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			ExceptionParentIndices = indices,
			ExceptionTypes = types,
			Messages = messages,
			StackTraces = stackTraces,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
		}));
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassFinished(
		XunitTestClassRunnerContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.ClassFixtureMappings.DisposeAsync);

		return ctxt.MessageBus.QueueMessage(new TestClassFinished
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			ExecutionTime = summary.Time,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
			TestsFailed = summary.Failed,
			TestsNotRun = summary.NotRun,
			TestsSkipped = summary.Skipped,
			TestsTotal = summary.Total,
		});
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassStarting(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = ctxt.MessageBus.QueueMessage(new TestClassStarting
		{
			AssemblyUniqueID = ctxt.TestClass.TestCollection.TestAssembly.UniqueID,
			TestClassName = Guard.ArgumentNotNull(ctxt).TestClass.TestClassName,
			TestClassNamespace = ctxt.TestClass.TestClassNamespace,
			TestClassSimpleName = ctxt.TestClass.TestClassSimpleName,
			TestClassUniqueID = ctxt.TestClass.UniqueID,
			TestCollectionUniqueID = ctxt.TestClass.TestCollection.UniqueID,
			Traits = ctxt.TestClass.Traits,
		});

		await ctxt.Aggregator.RunAsync(() =>
			ctxt.TestClass.Class.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>))
				? throw new TestPipelineException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead).")
				: ctxt.TestClass.Constructors?.Count > 1
					? throw new TestPipelineException("A test class may only define a single public constructor.")
					: ctxt.ClassFixtureMappings.InitializeAsync(ctxt.TestClass.ClassFixtureTypes)
		);

		return result;
	}

	/// <inheritdoc/>
	protected override IReadOnlyCollection<IXunitTestCase> OrderTestCases(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		try
		{
			return ctxt.TestCaseOrderer.OrderTestCases(ctxt.TestCases);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			ctxt.MessageBus.QueueMessage(new ErrorMessage
			{
				ExceptionParentIndices = [-1],
				ExceptionTypes = [typeof(TestPipelineException).SafeName()],
				Messages = [
					string.Format(
						CultureInfo.CurrentCulture,
						"Test case orderer '{0}' threw '{1}' during ordering: {2}",
						ctxt.TestCaseOrderer.GetType().SafeName(),
						innerEx.GetType().SafeName(),
						innerEx.Message
					)
				],
				StackTraces = [innerEx.StackTrace],
			});

			return [];
		}
	}

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	/// <returns></returns>
	public async ValueTask<RunSummary> RunAsync(
		IXunitTestClass testClass,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new XunitTestClassRunnerContext(testClass, @testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings);
		await ctxt.InitializeAsync();

		return await ctxt.Aggregator.RunAsync(() => RunAsync(ctxt), default);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestMethodAsync(
		XunitTestClassRunnerContext ctxt,
		IXunitTestMethod? testMethod,
		IReadOnlyCollection<IXunitTestCase> testCases,
		object?[] constructorArguments,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is not null)
			return new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases, exception, sendTestMethodMessages: true));

		// Technically not possible because of the design of IXunitTestClass, but this signature is imposed
		// by the base class, which allows method-less tests
		return
			testMethod is null
				? new(XunitRunnerHelper.FailTestCases(ctxt.MessageBus, ctxt.CancellationTokenSource, testCases, "Test case '{0}' does not have an associated method and cannot be run by XunitTestMethodRunner", sendTestMethodMessages: true))
				: XunitTestMethodRunner.Instance.RunAsync(
					testMethod,
					testCases,
					ctxt.ExplicitOption,
					ctxt.MessageBus,
					ctxt.Aggregator.Clone(),
					ctxt.CancellationTokenSource,
					constructorArguments
				);
	}

	/// <summary>
	/// Selects the test constructor.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	protected virtual ConstructorInfo? SelectTestClassConstructor(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var ctors = ctxt.TestClass.Constructors;

		return ctors switch
		{
			null => null,
			{ Count: 1 } => ctors.First(),
			_ => throw new InvalidOperationException("Multiple constructors found; expected the context to have caught this earlier")
		};
	}

	/// <inheritdoc/>
	protected override void SetTestContext(
		XunitTestClassRunnerContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token, ctxt.ClassFixtureMappings);
	}
}
