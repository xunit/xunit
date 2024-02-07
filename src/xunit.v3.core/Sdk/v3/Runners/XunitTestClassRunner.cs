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
public class XunitTestClassRunner : TestClassRunner<XunitTestClassRunnerContext, IXunitTestCase>
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
	protected override async ValueTask AfterTestClassStartingAsync(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var ordererAttribute = ctxt.Class.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
		if (ordererAttribute is not null)
		{
			try
			{
				var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);
				if (testCaseOrderer is not null)
					ctxt.TestCaseOrderer = testCaseOrderer;
				else
				{
					var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);

					TestContext.Current?.SendDiagnosticMessage(
						"Could not find type '{0}' in {1} for class-level test case orderer on test class '{2}'",
						type,
						assembly,
						ctxt.TestClass.Class.Name
					);
				}
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();
				var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);

				TestContext.Current?.SendDiagnosticMessage(
					"Class-level test case orderer '{0}' for test class '{1}' threw '{2}' during construction: {3}{4}{5}",
					type,
					ctxt.TestClass.Class.Name,
					innerEx.GetType().FullName,
					innerEx.Message,
					Environment.NewLine,
					innerEx.StackTrace
				);
			}
		}

		var testClassType = ctxt.Class.Type;
		if (testClassType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
			ctxt.Aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

		var classFixtureTypes =
			testClassType
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>))
				.Select(i => i.GenericTypeArguments.Single());

		if (ctxt.TestClass.TestCollection.CollectionDefinition is _IReflectionTypeInfo collectionDefinition)
			classFixtureTypes = classFixtureTypes.Concat(
				collectionDefinition
					.Type
					.GetInterfaces()
					.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>))
					.Select(i => i.GenericTypeArguments.Single())
			);

		await ctxt.Aggregator.RunAsync(() => ctxt.ClassFixtureMappings.InitializeAsync(classFixtureTypes.ToArray()));

		await base.AfterTestClassStartingAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override async ValueTask BeforeTestClassFinishedAsync(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.ClassFixtureMappings.DisposeAsync);

		await base.BeforeTestClassFinishedAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override string FormatConstructorArgsMissingMessage(
		XunitTestClassRunnerContext ctxt,
		ConstructorInfo constructor,
		IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments) =>
			string.Format(
				CultureInfo.CurrentCulture,
				"The following constructor parameters did not have matching fixture data: {0}",
				string.Join(", ", unusedArguments.Select(arg => string.Format(CultureInfo.CurrentCulture, "{0} {1}", arg.Item2.ParameterType.Name, arg.Item2.Name)))
			);

	/// <inheritdoc/>
	protected override async ValueTask<object?> GetConstructorArgument(
		XunitTestClassRunnerContext ctxt,
		ConstructorInfo constructor,
		int index,
		ParameterInfo parameter)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(constructor);
		Guard.ArgumentNotNull(parameter);

		var result = await base.GetConstructorArgument(ctxt, constructor, index, parameter);
		if (result is not null)
			return result;

		if (parameter.ParameterType == typeof(ITestContextAccessor))
			return TestContextAccessor.Instance;

		// Logic to support passing Func<T> instead of T lives in XunitTestInvoker.CreateTestClassInstance
		// The actual TestOutputHelper instance is created in XunitTestRunner.SetTestContext when creating
		// the test context object.
		if (parameter.ParameterType == typeof(_ITestOutputHelper))
			return () => TestContext.Current?.TestOutputHelper;

		return await ctxt.ClassFixtureMappings.GetFixture(parameter.ParameterType);
	}

	/// <summary>
	/// Runs the test class.
	/// </summary>
	/// <param name="testClass">The test class to be run.</param>
	/// <param name="class">The type information about the test class.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
	/// <returns></returns>
	public async ValueTask<RunSummary> RunAsync(
		_ITestClass testClass,
		_IReflectionTypeInfo @class,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		FixtureMappingManager collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new XunitTestClassRunnerContext(testClass, @class, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, collectionFixtureMappings);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestMethodAsync(
		XunitTestClassRunnerContext ctxt,
		_ITestMethod? testMethod,
		_IReflectionMethodInfo? method,
		IReadOnlyCollection<IXunitTestCase> testCases,
		object?[] constructorArguments)
	{
		Guard.ArgumentNotNull(ctxt);

		if (testMethod is not null && method is not null)
			return XunitTestMethodRunner.Instance.RunAsync(
				ctxt.TestClass,
				testMethod,
				ctxt.Class,
				method,
				testCases,
				ctxt.ExplicitOption,
				ctxt.MessageBus,
				ctxt.Aggregator.Clone(),
				ctxt.CancellationTokenSource,
				constructorArguments
			);

		return new(XunitRunnerHelper.FailTestCases(ctxt.TestCases, ctxt.MessageBus, "Test case {0} does not have an associated method and cannot be run by XunitTestMethodRunner"));
	}

	/// <inheritdoc/>
	protected override ConstructorInfo? SelectTestClassConstructor(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var ctors =
			ctxt.Class
				.Type
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count == 1)
			return ctors[0];

		ctxt.Aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
		return null;
	}
}
