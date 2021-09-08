﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test class runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestClassRunner : TestClassRunner<IXunitTestCase>
	{
		readonly IDictionary<Type, object> collectionFixtureMappings;

		Dictionary<Type, object> classFixtureMappings = new();
		HashSet<IAsyncLifetime> initializedAsyncFixtures = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestClassRunner"/> class.
		/// </summary>
		/// <param name="testClass">The test class to be run. May be <c>null</c> for test cases that do not support classes
		/// and methods.</param>
		/// <param name="class">The test class that contains the tests to be run. May be <c>null</c> for test cases that do not
		/// support classes and methods.</param>
		/// <param name="testCases">The test cases to be run. Cannot be empty.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		/// <param name="collectionFixtureMappings">The mapping of collection fixture types to fixtures.</param>
		public XunitTestClassRunner(
			_ITestClass? testClass,
			_IReflectionTypeInfo? @class,
			IReadOnlyCollection<IXunitTestCase> testCases,
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource,
			IDictionary<Type, object> collectionFixtureMappings)
				: base(testClass, @class, testCases, diagnosticMessageSink, messageBus, testCaseOrderer, aggregator, cancellationTokenSource)
		{
			this.collectionFixtureMappings = Guard.ArgumentNotNull(nameof(collectionFixtureMappings), collectionFixtureMappings);
		}

		/// <summary>
		/// Gets the fixture mappings that were created during <see cref="AfterTestClassStartingAsync"/>.
		/// </summary>
		protected Dictionary<Type, object> ClassFixtureMappings
		{
			get => classFixtureMappings;
			set => classFixtureMappings = Guard.ArgumentNotNull(nameof(ClassFixtureMappings), value);
		}

		/// <summary>
		/// Gets the already initialized async fixtures <see cref="CreateClassFixtureAsync"/>.
		/// </summary>
		protected HashSet<IAsyncLifetime> InitializedAsyncFixtures
		{
			get => initializedAsyncFixtures;
			set => initializedAsyncFixtures = Guard.ArgumentNotNull(nameof(InitializedAsyncFixtures), value);
		}

		/// <summary>
		/// Creates the instance of a class fixture type to be used by the test class. If the fixture can be created,
		/// it should be placed into the <see cref="ClassFixtureMappings"/> dictionary; if it cannot, then the method
		/// should record the error by calling <code>Aggregator.Add</code>.
		/// </summary>
		/// <param name="fixtureType">The type of the fixture to be created</param>
		protected virtual void CreateClassFixture(Type fixtureType)
		{
			var ctors =
				fixtureType
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.CastOrToReadOnlyList();

			if (ctors.Count != 1)
			{
				Aggregator.Add(new TestClassException($"Class fixture type '{fixtureType.FullName}' may only define a single public constructor."));
				return;
			}

			var ctor = ctors[0];
			var missingParameters = new List<ParameterInfo>();
			var ctorArgs = ctor.GetParameters().Select(p =>
			{
				object? arg;
				if (p.ParameterType == typeof(_IMessageSink))
					arg = DiagnosticMessageSink;
				else
				if (!collectionFixtureMappings.TryGetValue(p.ParameterType, out arg))
					missingParameters.Add(p);
				return arg;
			}).ToArray();

			if (missingParameters.Count > 0)
				Aggregator.Add(new TestClassException(
					$"Class fixture type '{fixtureType.FullName}' had one or more unresolved constructor arguments: {string.Join(", ", missingParameters.Select(p => $"{p.ParameterType.Name} {p.Name}"))}"
				));
			else
				Aggregator.Run(() => ClassFixtureMappings[fixtureType] = ctor.Invoke(ctorArgs));
		}

		Task CreateClassFixtureAsync(Type fixtureType)
		{
			CreateClassFixture(fixtureType);

			var uninitializedFixtures =
				ClassFixtureMappings
					.Values
					.OfType<IAsyncLifetime>()
					.Where(fixture => !InitializedAsyncFixtures.Contains(fixture))
					.ToList();

			InitializedAsyncFixtures.UnionWith(uninitializedFixtures);

			return Task.WhenAll(uninitializedFixtures.Select(fixture => Aggregator.RunAsync(fixture.InitializeAsync).AsTask()));
		}

		/// <inheritdoc/>
		protected override string FormatConstructorArgsMissingMessage(
			ConstructorInfo constructor,
			IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments) =>
				$"The following constructor parameters did not have matching fixture data: {string.Join(", ", unusedArguments.Select(arg => $"{arg.Item2.ParameterType.Name} {arg.Item2.Name}"))}";

		/// <inheritdoc/>
		protected override async Task AfterTestClassStartingAsync()
		{
			if (TestClass == null || Class == null)
				return;

			var ordererAttribute = Class.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
			if (ordererAttribute != null)
			{
				try
				{
					var testCaseOrderer = ExtensibilityPointFactory.GetTestCaseOrderer(DiagnosticMessageSink, ordererAttribute);
					if (testCaseOrderer != null)
						TestCaseOrderer = testCaseOrderer;
					else
					{
						var (type, assembly) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);
						DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Could not find type '{type}' in {assembly} for class-level test case orderer on test class '{TestClass.Class.Name}'" });
					}
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();
					var (type, _) = ExtensibilityPointFactory.TypeStringsFromAttributeConstructor(ordererAttribute);
					DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Class-level test case orderer '{type}' for test class '{TestClass.Class.Name}' threw '{innerEx.GetType().FullName}' during construction: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}" });
				}
			}

			var testClassType = Class.Type;
			if (testClassType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
				Aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

			var createClassFixtureAsyncTasks = new List<Task>();
			foreach (var interfaceType in testClassType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
				createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(interfaceType.GenericTypeArguments.Single()));

			if (TestClass.TestCollection.CollectionDefinition != null)
			{
				var declarationType = ((_IReflectionTypeInfo)TestClass.TestCollection.CollectionDefinition).Type;
				foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
					createClassFixtureAsyncTasks.Add(CreateClassFixtureAsync(interfaceType.GenericTypeArguments.Single()));
			}

			await Task.WhenAll(createClassFixtureAsyncTasks);
		}

		/// <inheritdoc/>
		protected override async Task BeforeTestClassFinishedAsync()
		{
			var disposeAsyncTasks =
				ClassFixtureMappings
					.Values
					.OfType<IAsyncDisposable>()
					.Select(fixture => Aggregator.RunAsync(fixture.DisposeAsync).AsTask())
					.ToList();

			await Task.WhenAll(disposeAsyncTasks);

			foreach (var fixture in ClassFixtureMappings.Values.OfType<IDisposable>())
				Aggregator.Run(fixture.Dispose);
		}

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestMethodAsync(
			_ITestMethod? testMethod,
			_IReflectionMethodInfo? method,
			IReadOnlyCollection<IXunitTestCase> testCases,
			object?[] constructorArguments) =>
				new XunitTestMethodRunner(
					testMethod,
					Class,
					method,
					testCases,
					DiagnosticMessageSink,
					MessageBus,
					new ExceptionAggregator(Aggregator),
					CancellationTokenSource,
					constructorArguments
				).RunAsync();

		/// <inheritdoc/>
		protected override ConstructorInfo? SelectTestClassConstructor(_IReflectionTypeInfo @class)
		{
			var ctors =
				@class
					.Type
					.GetConstructors()
					.Where(ci => !ci.IsStatic && ci.IsPublic)
					.ToList();

			if (ctors.Count == 1)
				return ctors[0];

			Aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
			return null;
		}

		/// <inheritdoc/>
		protected override bool TryGetConstructorArgument(
			ConstructorInfo constructor,
			int index,
			ParameterInfo parameter,
			[MaybeNullWhen(false)] out object argumentValue)
		{
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
				argumentValue = (Func<_ITestOutputHelper?>)(() => TestContext.Current?.TestOutputHelper);
				return true;
			}

			return ClassFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue)
				|| collectionFixtureMappings.TryGetValue(parameter.ParameterType, out argumentValue);
		}
	}
}
