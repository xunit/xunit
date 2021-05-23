using System;
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
	/// A base class that provides default behavior when running tests in a test class. It groups the tests
	/// by test method, and then runs the individual test methods.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestClassRunner<TTestCase>
		where TTestCase : class, _ITestCase
	{
		ExceptionAggregator aggregator;
		CancellationTokenSource cancellationTokenSource;
		_IReflectionTypeInfo @class;
		_IMessageSink diagnosticMessageSink;
		IMessageBus messageBus;
		ITestCaseOrderer testCaseOrderer;
		IReadOnlyCollection<TTestCase> testCases;
		_ITestClass testClass;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestClassRunner{TTestCase}"/> class.
		/// </summary>
		/// <param name="testClass">The test class to be run.</param>
		/// <param name="class">The test class that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="testCaseOrderer">The test case orderer that will be used to decide how to order the test.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		protected TestClassRunner(
			_ITestClass testClass,
			_IReflectionTypeInfo @class,
			IReadOnlyCollection<TTestCase> testCases,
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			ITestCaseOrderer testCaseOrderer,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			this.testClass = Guard.ArgumentNotNull(nameof(testClass), testClass);
			this.@class = Guard.ArgumentNotNull(nameof(@class), @class);
			this.testCases = Guard.ArgumentNotNull(nameof(testCases), testCases);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
			this.messageBus = Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			this.testCaseOrderer = Guard.ArgumentNotNull(nameof(testCaseOrderer), testCaseOrderer);
			this.aggregator = Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			this.cancellationTokenSource = Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);
		}

		/// <summary>
		/// Gets or sets the exception aggregator used to run code and collect exceptions.
		/// </summary>
		protected ExceptionAggregator Aggregator
		{
			get => aggregator;
			set => aggregator = Guard.ArgumentNotNull(nameof(Aggregator), value);
		}

		/// <summary>
		/// Gets or sets the task cancellation token source, used to cancel the test run.
		/// </summary>
		protected CancellationTokenSource CancellationTokenSource
		{
			get => cancellationTokenSource;
			set => cancellationTokenSource = Guard.ArgumentNotNull(nameof(CancellationTokenSource), value);
		}

		/// <summary>
		/// Gets or sets the CLR class that contains the tests to be run.
		/// </summary>
		protected _IReflectionTypeInfo Class
		{
			get => @class;
			set => @class = Guard.ArgumentNotNull(nameof(Class), value);
		}

		/// <summary>
		/// Gets the message sink used to send diagnostic messages.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink
		{
			get => diagnosticMessageSink;
			set => diagnosticMessageSink = Guard.ArgumentNotNull(nameof(DiagnosticMessageSink), value);
		}

		/// <summary>
		/// Gets or sets the message bus to report run status to.
		/// </summary>
		protected IMessageBus MessageBus
		{
			get => messageBus;
			set => messageBus = Guard.ArgumentNotNull(nameof(MessageBus), value);
		}

		/// <summary>
		/// Gets or sets the test case orderer that will be used to decide how to order the test.
		/// </summary>
		protected ITestCaseOrderer TestCaseOrderer
		{
			get => testCaseOrderer;
			set => testCaseOrderer = Guard.ArgumentNotNull(nameof(TestCaseOrderer), value);
		}

		/// <summary>
		/// Gets or sets the test cases to be run.
		/// </summary>
		protected IReadOnlyCollection<TTestCase> TestCases
		{
			get => testCases;
			set => testCases = Guard.ArgumentNotNull(nameof(TestCaseOrderer), value);
		}

		/// <summary>
		/// Gets or sets the test class to be run.
		/// </summary>
		protected _ITestClass TestClass
		{
			get => testClass;
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <summary>
		/// Creates the arguments for the test class constructor. Attempts to resolve each parameter
		/// individually, and adds an error when the constructor arguments cannot all be provided.
		/// If the class is static, does not look for constructor, since one will not be needed.
		/// </summary>
		/// <returns>The test class constructor arguments.</returns>
		protected virtual object?[] CreateTestClassConstructorArguments()
		{
			var isStaticClass = Class.Type.IsAbstract && Class.Type.IsSealed;
			if (!isStaticClass)
			{
				var ctor = SelectTestClassConstructor();
				if (ctor != null)
				{
					var unusedArguments = new List<Tuple<int, ParameterInfo>>();
					var parameters = ctor.GetParameters();

					var constructorArguments = new object?[parameters.Length];
					for (var idx = 0; idx < parameters.Length; ++idx)
					{
						var parameter = parameters[idx];

						if (TryGetConstructorArgument(ctor, idx, parameter, out var argumentValue))
							constructorArguments[idx] = argumentValue;
						else if (parameter.HasDefaultValue)
							constructorArguments[idx] = parameter.DefaultValue;
						else if (parameter.IsOptional)
							constructorArguments[idx] = parameter.ParameterType.GetDefaultValue();
						else if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
							constructorArguments[idx] = Array.CreateInstance(parameter.ParameterType, 0);
						else
							unusedArguments.Add(Tuple.Create(idx, parameter));
					}

					if (unusedArguments.Count > 0)
						Aggregator.Add(new TestClassException(FormatConstructorArgsMissingMessage(ctor, unusedArguments)));

					return constructorArguments;
				}
			}

			return new object[0];
		}

		/// <summary>
		/// Gets the message to be used when the constructor is missing arguments.
		/// </summary>
		protected virtual string FormatConstructorArgsMissingMessage(
			ConstructorInfo constructor,
			IReadOnlyList<Tuple<int, ParameterInfo>> unusedArguments) =>
				$"The following constructor parameters did not have matching arguments: {string.Join(", ", unusedArguments.Select(arg => $"{arg.Item2.ParameterType.Name} {arg.Item2.Name}"))}";

		/// <summary>
		/// This method is called just after <see cref="_TestClassStarting"/> is sent, but before any test methods are run.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task AfterTestClassStartingAsync() => Task.CompletedTask;

		/// <summary>
		/// This method is called just before <see cref="_TestClassFinished"/> is sent.
		/// This method should NEVER throw; any exceptions should be placed into the <see cref="Aggregator"/>.
		/// </summary>
		protected virtual Task BeforeTestClassFinishedAsync() => Task.CompletedTask;

		/// <summary>
		/// Runs the tests in the test class.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		public async Task<RunSummary> RunAsync()
		{
			var classSummary = new RunSummary();

			var testAssemblyUniqueID = TestClass.TestCollection.TestAssembly.UniqueID;
			var testCollectionUniqueID = TestClass.TestCollection.UniqueID;
			var testClassUniqueID = TestClass.UniqueID;

			var classStarting = new _TestClassStarting
			{
				AssemblyUniqueID = testAssemblyUniqueID,
				TestClass = TestClass.Class.Name,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID
			};

			if (!MessageBus.QueueMessage(classStarting))
				CancellationTokenSource.Cancel();
			else
			{
				try
				{
					await AfterTestClassStartingAsync();
					classSummary = await RunTestMethodsAsync();

					Aggregator.Clear();
					await BeforeTestClassFinishedAsync();

					if (Aggregator.HasExceptions)
					{
						var classCleanupFailure = _TestClassCleanupFailure.FromException(Aggregator.ToException()!, testAssemblyUniqueID, testCollectionUniqueID, testClassUniqueID);
						if (!MessageBus.QueueMessage(classCleanupFailure))
							CancellationTokenSource.Cancel();
					}
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
						TestsRun = classSummary.Total,
						TestsSkipped = classSummary.Skipped
					};

					if (!MessageBus.QueueMessage(classFinished))
						CancellationTokenSource.Cancel();
				}
			}

			return classSummary;
		}

		/// <summary>
		/// Runs the list of test methods. By default, orders the tests, groups them by method and runs them synchronously.
		/// </summary>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected virtual async Task<RunSummary> RunTestMethodsAsync()
		{
			var summary = new RunSummary();
			IReadOnlyCollection<TTestCase> orderedTestCases;

			try
			{
				orderedTestCases = TestCaseOrderer.OrderTestCases(TestCases);
			}
			catch (Exception ex)
			{
				var innerEx = ex.Unwrap();
				DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Test case orderer '{TestCaseOrderer.GetType().FullName}' threw '{innerEx.GetType().FullName}' during ordering: {innerEx.Message}{Environment.NewLine}{innerEx.StackTrace}" });
				orderedTestCases = TestCases.CastOrToReadOnlyCollection();
			}

			var constructorArguments = CreateTestClassConstructorArguments();

			foreach (var method in orderedTestCases.GroupBy(tc => tc.TestMethod, TestMethodComparer.Instance))
			{
				summary.Aggregate(await RunTestMethodAsync(method.Key, (_IReflectionMethodInfo)method.Key.Method, method.CastOrToReadOnlyCollection(), constructorArguments));
				if (CancellationTokenSource.IsCancellationRequested)
					break;
			}

			return summary;
		}

		/// <summary>
		/// Override this method to run the tests in an individual test method.
		/// </summary>
		/// <param name="testMethod">The test method that contains the test cases.</param>
		/// <param name="method">The CLR method that contains the tests to be run.</param>
		/// <param name="testCases">The test cases to be run.</param>
		/// <param name="constructorArguments">The constructor arguments that will be used to create the test class.</param>
		/// <returns>Returns summary information about the tests that were run.</returns>
		protected abstract Task<RunSummary> RunTestMethodAsync(
			_ITestMethod testMethod,
			_IReflectionMethodInfo method,
			IReadOnlyCollection<TTestCase> testCases,
			object?[] constructorArguments
		);

		/// <summary>
		/// Selects the constructor to be used for the test class. By default, chooses the parameterless
		/// constructor. Override to change the constructor selection logic.
		/// </summary>
		/// <returns>The constructor to be used for creating the test class.</returns>
		protected virtual ConstructorInfo? SelectTestClassConstructor()
		{
			var result = Class.Type.GetConstructors().FirstOrDefault(ci => !ci.IsStatic && ci.GetParameters().Length == 0);
			if (result == null)
				Aggregator.Add(new TestClassException("A test class must have a parameterless constructor."));

			return result;
		}

		/// <summary>
		/// Tries to supply a test class constructor argument. By default, always fails. Override to
		/// change the argument lookup logic.
		/// </summary>
		/// <param name="constructor">The constructor that will be used to create the test class.</param>
		/// <param name="index">The parameter index.</param>
		/// <param name="parameter">The parameter information.</param>
		/// <param name="argumentValue">The argument value that should be used for the parameter.</param>
		/// <returns>Returns <c>true</c> if the argument was supplied; <c>false</c>, otherwise.</returns>
		protected virtual bool TryGetConstructorArgument(
			ConstructorInfo constructor,
			int index,
			ParameterInfo parameter,
			[MaybeNullWhen(false)] out object argumentValue)
		{
			argumentValue = null;
			return false;
		}
	}
}
