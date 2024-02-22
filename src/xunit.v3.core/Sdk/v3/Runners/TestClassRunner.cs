using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A base class that provides default behavior when running tests in a test class. It groups the tests
/// by test method, and then runs the individual test methods.
/// </summary>
/// <typeparam name="TContext">The context type used by the runner</typeparam>
/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
/// derive from <see cref="_ITestCase"/>.</typeparam>
public abstract class TestClassRunner<TContext, TTestCase>
	where TContext : TestClassRunnerContext<TTestCase>
	where TTestCase : class, _ITestCase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="TestClassRunner{TContext, TTestCase}"/> class.
	/// </summary>
	protected TestClassRunner()
	{ }

	/// <summary>
	/// Creates the arguments for the test class constructor. Attempts to resolve each parameter
	/// individually, and adds an error when the constructor arguments cannot all be provided.
	/// If the class is static, does not look for constructor, since one will not be needed.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>The test class constructor arguments.</returns>
	protected virtual object?[] CreateTestClassConstructorArguments(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var isStaticClass = ctxt.Class.Type.IsAbstract && ctxt.Class.Type.IsSealed;
		if (!isStaticClass && !ctxt.Aggregator.HasExceptions)
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

					if (TryGetConstructorArgument(ctxt, ctor, idx, parameter, out var argumentValue))
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
					ctxt.Aggregator.Add(new TestClassException(FormatConstructorArgsMissingMessage(ctxt, ctor, unusedArguments)));

				return constructorArguments;
			}
		}

		return Array.Empty<object>();
	}

	/// <summary>
	/// Gets the message to be used when the constructor is missing arguments.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="constructor">The constructor that was selected</param>
	/// <param name="unusedArguments">The arguments that had no matching parameter values</param>
	protected virtual string FormatConstructorArgsMissingMessage(
		TContext ctxt,
		ConstructorInfo constructor,
		IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments) =>
			string.Format(
				CultureInfo.CurrentCulture,
				"The following constructor parameters did not have matching arguments: {0}",
				string.Join(", ", unusedArguments.Select(arg => string.Format(CultureInfo.CurrentCulture, "{0} {1}", arg.Item2.ParameterType.Name, arg.Item2.Name)))
			);

	/// <summary>
	/// This method is called just after <see cref="_TestClassStarting"/> is sent, but before any test methods are run.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	protected virtual ValueTask AfterTestClassStartingAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// This method is called after all test methods are run, but just before <see cref="_TestClassFinished"/> is sent.
	/// This method should NEVER throw; any exceptions should be placed into the aggregator in <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	protected virtual ValueTask BeforeTestClassFinishedAsync(TContext ctxt) =>
		default;

	/// <summary>
	/// Runs the tests in the test class.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected async ValueTask<RunSummary> RunAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		SetTestContext(ctxt, TestEngineStatus.Initializing);

		var classSummary = new RunSummary();
		var testCollection = ctxt.TestCases.First().TestCollection;
		var testAssemblyUniqueID = testCollection.TestAssembly.UniqueID;
		var testCollectionUniqueID = testCollection.UniqueID;
		var testClassUniqueID = ctxt.TestClass.UniqueID;

		var classStarting = new _TestClassStarting
		{
			AssemblyUniqueID = testAssemblyUniqueID,
			TestClass = ctxt.TestClass.Class.Name,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID
		};

		if (!ctxt.MessageBus.QueueMessage(classStarting))
		{
			ctxt.CancellationTokenSource.Cancel();
			return classSummary;
		}

		try
		{
			await AfterTestClassStartingAsync(ctxt);

			SetTestContext(ctxt, TestEngineStatus.Running);

			classSummary = await RunTestMethodsAsync(ctxt);

			SetTestContext(ctxt, TestEngineStatus.CleaningUp);

			ctxt.Aggregator.Clear();
			await BeforeTestClassFinishedAsync(ctxt);

			if (ctxt.Aggregator.HasExceptions)
			{
				var classCleanupFailure = _TestClassCleanupFailure.FromException(ctxt.Aggregator.ToException()!, testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID);
				if (!ctxt.MessageBus.QueueMessage(classCleanupFailure))
					ctxt.CancellationTokenSource.Cancel();
			}

			return classSummary;
		}
		finally
		{
			var classFinished = new _TestClassFinished
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				ExecutionTime = classSummary.Time,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = classSummary.Failed,
				TestsNotRun = classSummary.NotRun,
				TestsTotal = classSummary.Total,
				TestsSkipped = classSummary.Skipped
			};

			if (!ctxt.MessageBus.QueueMessage(classFinished))
				ctxt.CancellationTokenSource.Cancel();
		}
	}

	/// <summary>
	/// Runs the list of test methods. By default, orders the tests, groups them by method and runs them synchronously.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected virtual async ValueTask<RunSummary> RunTestMethodsAsync(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var summary = new RunSummary();
		IReadOnlyCollection<TTestCase> orderedTestCases;

		try
		{
			orderedTestCases = ctxt.TestCaseOrderer.OrderTestCases(ctxt.TestCases);
		}
		catch (Exception ex)
		{
			var innerEx = ex.Unwrap();

			TestContext.Current?.SendDiagnosticMessage(
				"Test case orderer '{0}' threw '{1}' during ordering: {2}{3}{4}",
				ctxt.TestCaseOrderer.GetType().FullName,
				innerEx.GetType().FullName,
				innerEx.Message,
				Environment.NewLine,
				innerEx.StackTrace
			);

			orderedTestCases = ctxt.TestCases.CastOrToReadOnlyCollection();
		}

		var constructorArguments = CreateTestClassConstructorArguments(ctxt);

		foreach (var method in orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance))
		{
			summary.Aggregate(
				await RunTestMethodAsync(
					ctxt,
					method.Key,
					method.Key?.Method as _IReflectionMethodInfo,
					method.CastOrToReadOnlyCollection(),
					constructorArguments
				)
			);

			if (ctxt.CancellationTokenSource.IsCancellationRequested)
				break;
		}

		return summary;
	}

	/// <summary>
	/// Override this method to run the tests in an individual test method.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testMethod">The test method that contains the test cases. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="method">The CLR method that contains the tests to be run. May be <c>null</c> for test cases that do not
	/// support classes and methods.</param>
	/// <param name="testCases">The test cases to be run.</param>
	/// <param name="constructorArguments">The constructor arguments that will be used to create the test class.</param>
	/// <returns>Returns summary information about the tests that were run.</returns>
	protected abstract ValueTask<RunSummary> RunTestMethodAsync(
		TContext ctxt,
		_ITestMethod? testMethod,
		_IReflectionMethodInfo? method,
		IReadOnlyCollection<TTestCase> testCases,
		object?[] constructorArguments
	);

	/// <summary>
	/// Selects the constructor to be used for the test class. By default, chooses the parameterless
	/// constructor. Override to change the constructor selection logic.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>The constructor to be used for creating the test class.</returns>
	protected virtual ConstructorInfo? SelectTestClassConstructor(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = ctxt.Class.Type.GetConstructors().FirstOrDefault(ci => !ci.IsStatic && ci.GetParameters().Length == 0);
		if (result is null)
			ctxt.Aggregator.Add(new TestClassException("A test class must have a parameterless constructor."));

		return result;
	}

	/// <summary>
	/// Sets the current <see cref="TestContext"/> for the current test class and the given test class status.
	/// Does nothing when <see cref="TestClass"/> is <c>null</c>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="testClassStatus">The current test class status.</param>
	protected virtual void SetTestContext(
		TContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token);
	}

	/// <summary>
	/// Tries to supply a test class constructor argument. By default, always fails. Override to
	/// change the argument lookup logic.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="constructor">The constructor that will be used to create the test class.</param>
	/// <param name="index">The parameter index.</param>
	/// <param name="parameter">The parameter information.</param>
	/// <param name="argumentValue">The argument value that should be used for the parameter.</param>
	/// <returns>Returns <c>true</c> if the argument was supplied; <c>false</c>, otherwise.</returns>
	protected virtual bool TryGetConstructorArgument(
		TContext ctxt,
		ConstructorInfo constructor,
		int index,
		ParameterInfo parameter,
		[MaybeNullWhen(false)] out object argumentValue)
	{
		argumentValue = null;
		return false;
	}
}
