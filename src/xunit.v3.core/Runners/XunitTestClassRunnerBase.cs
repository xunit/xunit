using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// The test class runner for xUnit.net v3 tests (with overridable context).
/// </summary>
public abstract class XunitTestClassRunnerBase<TContext, TTestClass, TTestMethod, TTestCase> :
	TestClassRunner<TContext, TTestClass, TTestMethod, TTestCase>
		where TContext : XunitTestClassRunnerBaseContext<TTestClass, TTestCase>
		where TTestClass : class, IXunitTestClass
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{
	/// <inheritdoc/>
	protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(TContext ctxt)
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
		TContext ctxt,
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
		TContext ctxt,
		ConstructorInfo constructor,
		int index,
		ParameterInfo parameter)
	{
		Guard.ArgumentNotNull(ctxt);
		Guard.ArgumentNotNull(constructor);
		Guard.ArgumentNotNull(parameter);

		if (parameter.ParameterType == typeof(ITestContextAccessor))
			return TestContextAccessor.Instance;

		// Logic to support passing Func<T> instead of T lives in XunitTestRunner.CreateTestClassInstance.
		// The actual TestOutputHelper instance is created in XunitTestRunner.SetTestContext when creating
		// the test context object.
		return
			parameter.ParameterType == typeof(ITestOutputHelper)
				? (() => TestContext.Current.TestOutputHelper)
				: await ctxt.ClassFixtureMappings.GetFixture(parameter.ParameterType);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassFinished(
		TContext ctxt,
		RunSummary summary)
	{
		Guard.ArgumentNotNull(ctxt);

		await ctxt.Aggregator.RunAsync(ctxt.ClassFixtureMappings.DisposeAsync);
		return await base.OnTestClassFinished(ctxt, summary);
	}

	/// <inheritdoc/>
	protected override async ValueTask<bool> OnTestClassStarting(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		var result = await base.OnTestClassStarting(ctxt);
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
	protected override IReadOnlyCollection<TTestCase> OrderTestCases(TContext ctxt)
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
	/// Selects the test constructor.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	protected virtual ConstructorInfo? SelectTestClassConstructor(TContext ctxt)
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
		TContext ctxt,
		TestEngineStatus testClassStatus)
	{
		Guard.ArgumentNotNull(ctxt);

		TestContext.SetForTestClass(ctxt.TestClass, testClassStatus, ctxt.CancellationTokenSource.Token, ctxt.ClassFixtureMappings);
	}
}
