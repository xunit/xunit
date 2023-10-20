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
/// The test collection runner for xUnit.net v3 tests.
/// </summary>
public class XunitTestCollectionRunner : TestCollectionRunner<XunitTestCollectionRunnerContext, IXunitTestCase>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestCollectionRunner"/> class.
	/// </summary>
	protected XunitTestCollectionRunner()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="XunitTestCollectionRunner"/>.
	/// </summary>
	public static XunitTestCollectionRunner Instance { get; } = new();

	/// <inheritdoc/>
	protected override async ValueTask AfterTestCollectionStartingAsync(XunitTestCollectionRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		await CreateCollectionFixturesAsync(ctxt);

		ctxt.TestCaseOrderer = GetTestCaseOrderer(ctxt) ?? ctxt.TestCaseOrderer;
	}

	/// <inheritdoc/>
	protected override async ValueTask BeforeTestCollectionFinishedAsync(XunitTestCollectionRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var disposeAsyncTasks =
			ctxt.CollectionFixtureMappings
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
						throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in DisposeAsync", fixture.GetType().FullName), ex.Unwrap());
					}
				}).AsTask())
				.ToList();

		await Task.WhenAll(disposeAsyncTasks);

		foreach (var fixture in ctxt.CollectionFixtureMappings.Values.OfType<IDisposable>())
			ctxt.Aggregator.Run(() =>
			{
				try
				{
					fixture.Dispose();
				}
				catch (Exception ex)
				{
					throw new TestFixtureCleanupException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in Dispose", fixture.GetType().FullName), ex.Unwrap());
				}
			});
	}

	/// <summary>
	/// Creates the instance of a collection fixture type to be used by the test collection. If the fixture can be created,
	/// it should be placed into the CollectionFixtureMappings dictionary of <paramref name="ctxt"/>; if it cannot, then the
	/// method should record the error by calling <code>Aggregator.Add</code>.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	/// <param name="fixtureType">The type of the fixture to be created</param>
	protected virtual void CreateCollectionFixture(
		XunitTestCollectionRunnerContext ctxt,
		Type fixtureType)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(fixtureType);

		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
		{
			ctxt.Aggregator.Add(new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' may only define a single public constructor.", fixtureType.FullName)));
			return;
		}

		var ctor = ctors[0];
		var missingParameters = new List<ParameterInfo>();
		var ctorArgs = ctor.GetParameters().Select(p =>
		{
			object? arg = null;
			if (p.ParameterType == typeof(_IMessageSink))
				arg = TestContext.Current?.DiagnosticMessageSink;
			else if (p.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else if (!ctxt.AssemblyFixtureMappings.TryGetValue(p.ParameterType, out arg))
				missingParameters.Add(p);
			return arg;
		}).ToArray();

		if (missingParameters.Count > 0)
			ctxt.Aggregator.Add(
				new TestClassException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Collection fixture type '{0}' had one or more unresolved constructor arguments: {1}",
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
					ctxt.CollectionFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs);
				}
				catch (Exception ex)
				{
					throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in its constructor", fixtureType.FullName), ex.Unwrap());
				}
			});
	}

	ValueTask CreateCollectionFixturesAsync(XunitTestCollectionRunnerContext ctxt)
	{
		if (ctxt.TestCollection.CollectionDefinition is null || ctxt.TestCollection.CollectionDefinition is not _IReflectionTypeInfo collectionDefinition)
			return default;

		var declarationType = collectionDefinition.Type;
		foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
		{
			var fixtureType = interfaceType.GenericTypeArguments.Single();
			CreateCollectionFixture(ctxt, fixtureType);
		}

		var initializeAsyncTasks =
			ctxt.CollectionFixtureMappings
				.Values
				.OfType<IAsyncLifetime>()
				.Select(
					fixture => ctxt.Aggregator.RunAsync(async () =>
					{
						try
						{
							await fixture.InitializeAsync();
						}
						catch (Exception ex)
						{
							throw new TestClassException(string.Format(CultureInfo.CurrentCulture, "Collection fixture type '{0}' threw in InitializeAsync", fixture.GetType().FullName), ex.Unwrap());
						}
					}).AsTask()
				)
				.ToList();

		return new(Task.WhenAll(initializeAsyncTasks));
	}

	/// <summary>
	/// Gives an opportunity to override test case orderer. By default, this method gets the
	/// orderer from the collection definition. If this function returns <c>null</c>, the
	/// test case orderer passed into the constructor will be used.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test collection</param>
	protected virtual ITestCaseOrderer? GetTestCaseOrderer(XunitTestCollectionRunnerContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.TestCollection.CollectionDefinition is not null)
		{
			var ordererAttribute = ctxt.TestCollection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
			if (ordererAttribute is not null)
			{
				try
				{
					var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(ordererAttribute);
					if (testCaseOrderer is not null)
						return testCaseOrderer;

					var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);

					TestContext.Current?.SendDiagnosticMessage(
						"Could not find type '{0}' in {1} for collection-level test case orderer on test collection '{2}'",
						type,
						assembly,
						ctxt.TestCollection.DisplayName
					);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();
					var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);

					TestContext.Current?.SendDiagnosticMessage(
						"Collection-level test case orderer '{0}' for test collection '{1}' threw '{2}' during construction: {3}{4}{5}",
						type,
						ctxt.TestCollection.DisplayName,
						innerEx.GetType().FullName,
						innerEx.Message,
						Environment.NewLine,
						innerEx.StackTrace
					);
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Runs the test collection.
	/// </summary>
	/// <param name="testCollection">The test collection to be run.</param>
	/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
	/// <param name="explicitOption">A flag to indicate how explicit tests should be treated.</param>
	/// <param name="messageBus">The message bus to report run status to.</param>
	/// <param name="testCaseOrderer">The test case orderer that was applied at the assembly level.</param>
	/// <param name="aggregator">The exception aggregator used to run code and collection exceptions.</param>
	/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
	/// <param name="assemblyFixtureMappings">The mapping of assembly fixture types to fixtures.</param>
	public async ValueTask<RunSummary> RunAsync(
		_ITestCollection testCollection,
		IReadOnlyCollection<IXunitTestCase> testCases,
		ExplicitOption explicitOption,
		IMessageBus messageBus,
		ITestCaseOrderer testCaseOrderer,
		ExceptionAggregator aggregator,
		CancellationTokenSource cancellationTokenSource,
		IReadOnlyDictionary<Type, object> assemblyFixtureMappings)
	{
		Guard.ArgumentNotNull(testCollection);
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageBus);
		Guard.ArgumentNotNull(testCaseOrderer);
		Guard.ArgumentNotNull(cancellationTokenSource);
		Guard.ArgumentNotNull(assemblyFixtureMappings);

		await using var ctxt = new XunitTestCollectionRunnerContext(testCollection, testCases, explicitOption, messageBus, testCaseOrderer, aggregator, cancellationTokenSource, assemblyFixtureMappings);
		await ctxt.InitializeAsync();

		return await RunAsync(ctxt);
	}

	/// <inheritdoc/>
	protected override ValueTask<RunSummary> RunTestClassAsync(
		XunitTestCollectionRunnerContext ctxt,
		_ITestClass? testClass,
		_IReflectionTypeInfo? @class,
		IReadOnlyCollection<IXunitTestCase> testCases)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(testCases);

		if (testClass is not null && @class is not null)
			return XunitTestClassRunner.Instance.RunAsync(
				testClass,
				@class,
				testCases,
				ctxt.ExplicitOption,
				ctxt.MessageBus,
				ctxt.TestCaseOrderer,
				ctxt.Aggregator.Clone(),
				ctxt.CancellationTokenSource,
				ctxt.AssemblyFixtureMappings,
				ctxt.CollectionFixtureMappings
			);

		return new(XunitRunnerHelper.FailTestCases(ctxt.TestCases, ctxt.MessageBus, "Test case {0} does not have an associated class and cannot be run by XunitTestClassRunner"));
	}
}
