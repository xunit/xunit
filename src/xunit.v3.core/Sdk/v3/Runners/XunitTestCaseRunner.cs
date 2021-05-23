using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// The test case runner for xUnit.net v3 tests.
	/// </summary>
	public class XunitTestCaseRunner : TestCaseRunner<IXunitTestCase>
	{
		List<BeforeAfterTestAttribute>? beforeAfterAttributes;
		object?[] constructorArguments;
		string displayName;
		Type testClass;
		MethodInfo testMethod;

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestCaseRunner"/> class.
		/// </summary>
		/// <param name="testCase">The test case to be run.</param>
		/// <param name="displayName">The display name of the test case.</param>
		/// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
		/// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
		/// <param name="testMethodArguments">The arguments to be passed to the test method.</param>
		/// <param name="messageBus">The message bus to report run status to.</param>
		/// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
		/// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
		public XunitTestCaseRunner(
			IXunitTestCase testCase,
			string displayName,
			string? skipReason,
			object?[] constructorArguments,
			object?[]? testMethodArguments,
			IMessageBus messageBus,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
				: base(testCase, messageBus, aggregator, cancellationTokenSource)
		{
			this.displayName = Guard.ArgumentNotNull(nameof(displayName), displayName);
			this.constructorArguments = Guard.ArgumentNotNull(nameof(constructorArguments), constructorArguments);

			SkipReason = skipReason;

			testClass = TestCase.TestMethod.TestClass.Class.ToRuntimeType() ?? throw new ArgumentException("testCase.TestMethod.TestClass.Class does not map to a Type object", nameof(testCase));
			testMethod = TestCase.Method.ToRuntimeMethod() ?? throw new ArgumentException("testCase.TestMethod does not map to a MethodInfo object", nameof(testCase));

			var parameters = TestMethod.GetParameters();
			var parameterTypes = new Type[parameters.Length];
			for (var i = 0; i < parameters.Length; i++)
				parameterTypes[i] = parameters[i].ParameterType;

			TestMethodArguments = Reflector.ConvertArguments(testMethodArguments, parameterTypes);
		}

		/// <summary>
		/// Gets the list of <see cref="BeforeAfterTestAttribute"/>s that will be used for this test case.
		/// </summary>
		public IReadOnlyList<BeforeAfterTestAttribute> BeforeAfterAttributes
		{
			get
			{
				if (beforeAfterAttributes == null)
					beforeAfterAttributes = GetBeforeAfterTestAttributes();

				return beforeAfterAttributes;
			}
		}

		/// <summary>
		/// Gets or sets the arguments passed to the test class constructor
		/// </summary>
		protected object?[] ConstructorArguments
		{
			get => constructorArguments;
			set => constructorArguments = Guard.ArgumentNotNull(nameof(ConstructorArguments), value);
		}

		/// <summary>
		/// Gets or sets the display name of the test case
		/// </summary>
		protected string DisplayName
		{
			get => displayName;
			set => displayName = Guard.ArgumentNotNull(nameof(DisplayName), value);
		}

		/// <summary>
		/// Gets or sets the skip reason for the test, if set.
		/// </summary>
		protected string? SkipReason { get; set; }

		/// <summary>
		/// Gets or sets the runtime type for the test class that the test method belongs to.
		/// </summary>
		protected Type TestClass
		{
			get => testClass;
			set => testClass = Guard.ArgumentNotNull(nameof(TestClass), value);
		}

		/// <summary>
		/// Gets of sets the runtime method for the test method that the test case belongs to.
		/// </summary>
		protected MethodInfo TestMethod
		{
			get => testMethod;
			set => testMethod = Guard.ArgumentNotNull(nameof(TestMethod), value);
		}

		/// <summary>
		/// Gets or sets the arguments to pass to the test method when it's being invoked.
		/// Maybe be <c>null</c> to indicate there are no arguments.
		/// </summary>
		protected object?[]? TestMethodArguments { get; set; }

		/// <summary>
		/// Creates the <see cref="_ITest"/> instance for the given test case.
		/// </summary>
		protected virtual _ITest CreateTest(
			IXunitTestCase testCase,
			string displayName,
			int testIndex) =>
				new XunitTest(testCase, displayName, testIndex);

		/// <summary>
		/// Creates the test runner used to run the given test.
		/// </summary>
		protected virtual XunitTestRunner CreateTestRunner(
			_ITest test,
			IMessageBus messageBus,
			Type testClass,
			object?[] constructorArguments,
			MethodInfo testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
			IReadOnlyList<BeforeAfterTestAttribute> beforeAfterAttributes,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource) =>
				new XunitTestRunner(
					test,
					messageBus,
					testClass,
					constructorArguments,
					testMethod,
					testMethodArguments,
					skipReason,
					beforeAfterAttributes,
					new ExceptionAggregator(aggregator),
					cancellationTokenSource
				);

		/// <summary>
		/// Gets the list of <see cref="BeforeAfterTestAttribute"/> attributes that apply to this test case.
		/// </summary>
		protected virtual List<BeforeAfterTestAttribute> GetBeforeAfterTestAttributes()
		{
			IEnumerable<Attribute> beforeAfterTestCollectionAttributes;

			if (TestCase.TestMethod.TestClass.TestCollection.CollectionDefinition is _IReflectionTypeInfo collectionDefinition)
				beforeAfterTestCollectionAttributes = collectionDefinition.Type.GetCustomAttributes(typeof(BeforeAfterTestAttribute));
			else
				beforeAfterTestCollectionAttributes = Enumerable.Empty<Attribute>();

			return
				beforeAfterTestCollectionAttributes
					.Concat(TestClass.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
					.Concat(TestMethod.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
					.Concat(TestClass.Assembly.GetCustomAttributes(typeof(BeforeAfterTestAttribute)))
					.Cast<BeforeAfterTestAttribute>()
					.CastOrToList();
		}

		/// <inheritdoc/>
		protected override Task<RunSummary> RunTestAsync() =>
			CreateTestRunner(
				CreateTest(TestCase, DisplayName, testIndex: 0),
				MessageBus,
				TestClass,
				ConstructorArguments,
				TestMethod,
				TestMethodArguments,
				SkipReason,
				BeforeAfterAttributes,
				Aggregator,
				CancellationTokenSource
			).RunAsync();
	}
}
