using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	protected override ValueTask AfterTestClassStartingAsync(XunitTestClassRunnerContext ctxt)
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

		var createClassFixtureAsyncTasks = new List<Task>();
		foreach (var interfaceType in testClassType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
			createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(ctxt, interfaceType.GenericTypeArguments.Single()).AsTask());

		if (ctxt.TestClass.TestCollection.CollectionDefinition is _IReflectionTypeInfo collectionDefinition)
		{
			var declarationType = collectionDefinition.Type;
			foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
				createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(ctxt, interfaceType.GenericTypeArguments.Single()).AsTask());
		}

		return new(Task.WhenAll(createClassFixtureAsyncTasks));
	}

	/// <inheritdoc/>
	protected override async ValueTask BeforeTestClassFinishedAsync(XunitTestClassRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var disposeAsyncTasks =
			ctxt.ClassFixtureMappings
				.Values
				.OfType<IAsyncDisposable>()
				.Select(fixture => ctxt.Aggregator.RunAsync(async () =>
				{
					try
					{
						await fixture.DisposeAsync();
					}
					catch (Exception ex)
					{
						throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' threw in DisposeAsync", fixture.GetType().FullName), ex.Unwrap());
					}
				}).AsTask())
				.ToList();

		await Task.WhenAll(disposeAsyncTasks);

		foreach (var fixture in ctxt.ClassFixtureMappings.Values.OfType<IDisposable>())
			ctxt.Aggregator.Run(() =>
			{
				try
				{
					fixture.Dispose();
				}
				catch (Exception ex)
				{
					throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' threw in Dispose", fixture.GetType().FullName), ex.Unwrap());
				}
			});
	}

	/// <summary>
	/// Creates the instance of a class fixture type to be used by the test class. If the fixture can be created,
	/// it should be placed into the ClassFixtureMappings dictionary in <paramref name="ctxt"/>; if it cannot, then
	/// the method should record the error by calling <code>Aggregator.Add</code> on <paramref name="ctxt"/>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="fixtureType">The type of the fixture to be created</param>
	protected virtual void CreateClassFixture(
		XunitTestClassRunnerContext ctxt,
		Type fixtureType)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(fixtureType);

		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.CastOrToReadOnlyList();

		if (ctors.Count != 1)
		{
			ctxt.Aggregator.Add(new TestClassException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' may only define a single public constructor.", fixtureType.FullName)));
			return;
		}

		var ctor = ctors[0];
		var missingParameters = new List<ParameterInfo>();
		var ctorArgs = ctor.GetParameters().Select(p =>
		{
			object? arg;
			if (p.ParameterType == typeof(_IMessageSink))
				arg = TestContext.Current?.DiagnosticMessageSink;
			else if (p.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else if (!ctxt.CollectionFixtureMappings.TryGetValue(p.ParameterType, out arg) && !ctxt.AssemblyFixtureMappings.TryGetValue(p.ParameterType, out arg))
				missingParameters.Add(p);
			return arg;
		}).ToArray();

		if (missingParameters.Count > 0)
			ctxt.Aggregator.Add(
				new TestClassException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Class fixture type '{0}' had one or more unresolved constructor arguments: {1}",
						fixtureType.FullName,
						string.Join(", ", missingParameters.Select(p => string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))
					)
				)
			);
		else
			ctxt.Aggregator.Run(() =>
			{
				try
				{
					ctxt.ClassFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs);
				}
				catch (Exception ex)
				{
					throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' threw in its constructor", fixtureType.FullName), ex.Unwrap());
				}
			});
	}

	ValueTask CreateClassFixtureAsync(
		XunitTestClassRunnerContext ctxt,
		Type fixtureType)
	{
		CreateClassFixture(ctxt, fixtureType);

		var uninitializedFixtures =
			ctxt.ClassFixtureMappings
				.Values
				.OfType<IAsyncLifetime>()
				.Where(fixture => !ctxt.InitializedAsyncClassFixtures.Contains(fixture))
				.ToList();

		ctxt.InitializedAsyncClassFixtures.UnionWith(uninitializedFixtures);

		var initializeAsyncTasks =
			uninitializedFixtures
				.Select(
					fixture => ctxt.Aggregator.RunAsync(async () =>
					{
						try
						{
							await fixture.InitializeAsync();
						}
						catch (Exception ex)
						{
							throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Class fixture type '{0}' threw in InitializeAsync", fixture.GetType().FullName), ex.Unwrap());
						}
					}).AsTask()
				)
				.ToList();

		return new(Task.WhenAll(initializeAsyncTasks));
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
	/// <param name="assemblyFixtureMappings">The mapping of assembly fixture types to fixtures.</param>
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
		IReadOnlyDictionary<Type, object> assemblyFixtureMappings,
		IReadOnlyDictionary<Type, object> collectionFixtureMappings)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(assemblyFixtureMappings);
		Guard.ArgumentNotNull(collectionFixtureMappings);

		await using var ctxt = new XunitTestClassRunnerContext(testClass, @class, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings, collectionFixtureMappings);
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

	/// <inheritdoc/>
	protected override bool TryGetConstructorArgument(
		XunitTestClassRunnerContext ctxt,
		ConstructorInfo constructor,
		int index,
		ParameterInfo parameter,
		[MaybeNullWhen(false)] out object argumentValue)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(constructor);
		Guard.ArgumentNotNull(parameter);

		if (parameter.ParameterType == typeof(ITestContextAccessor))
		{
			argumentValue = TestContextAccessor.Instance;
			return true;
		}

		if (parameter.ParameterType == typeof(_ITestOutputHelper))
		{
			// Logic to support passing Func<T> instead of T lives in XunitTestInvoker.CreateTestClassInstance
			// The actual TestOutputHelper instance is created in XunitTestRunner.SetTestContext when creating
			// test test context object.
			argumentValue = () => TestContext.Current?.TestOutputHelper;
			return true;
		}

		return ctxt.ClassFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
			|| ctxt.CollectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
			|| ctxt.AssemblyFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
	}
}
