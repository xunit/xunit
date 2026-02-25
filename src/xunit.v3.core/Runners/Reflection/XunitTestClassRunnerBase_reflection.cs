using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Test class runner for xUnit.net v3 tests (with overridable context).
/// </summary>
/// <remarks>
/// This class is used for reflection-based tests.
/// </remarks>
public abstract class XunitTestClassRunnerBase<TContext, TTestClass, TTestMethod, TTestCase> :
	CoreTestClassRunner<TContext, TTestClass, TTestMethod, TTestCase>
		where TContext : XunitTestClassRunnerBaseContext<TTestClass, TTestMethod, TTestCase>
		where TTestClass : class, IXunitTestClass
		where TTestMethod : class, IXunitTestMethod
		where TTestCase : class, IXunitTestCase
{
	/// <summary>
	/// Creates the arguments for the test class constructor. By default just returns an empty
	/// set of arguments. Override to find the arguments for the constructor. Arguments without
	/// matching values may be returned as <see cref="Missing.Value"/>, as the <see cref="ITypeActivator"/>
	/// is responsible for determining how to resolve unknown arguments.
	/// </summary>
	/// <remarks>
	/// This method runs during <see cref="TestEngineStatus.Initializing"/> and any exceptions thrown will
	/// contribute to test class failure (and will prevent the test class from running)
	/// </remarks>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <returns>The test class constructor arguments.</returns>
	protected override async ValueTask<object?[]> CreateTestClassConstructorArguments(TContext ctxt)
	{
		Guard.ArgumentNotNull(ctxt);

		if (ctxt.TestCases.All(tc => tc.IsStaticallySkipped()))
			return [];

		if (!ctxt.Aggregator.HasExceptions)
		{
			var ctor = SelectTestClassConstructor(ctxt);
			if (ctor is not null)
			{
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
					else if (parameter.GetCustomAttribute<ParamArrayAttribute>() is not null && parameter.ParameterType.GetElementType() is Type elementType)
						constructorArguments[idx] = Array.CreateInstance(elementType, 0);
					else
						constructorArguments[idx] = Missing.Value;
				}

				return constructorArguments;
			}
		}

		return [];
	}

	/// <summary>
	/// Tries to supply a test class constructor argument.
	/// </summary>
	/// <param name="ctxt">The context that describes the current test class</param>
	/// <param name="constructor">The constructor that will be used to create the test class.</param>
	/// <param name="index">The parameter index.</param>
	/// <param name="parameter">The parameter information.</param>
	/// <returns>Returns the constructor argument if available, <see langword="null"/> otherwise.</returns>
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
					: ctxt.ClassFixtureMappings.InitializeAsync(
						ctxt.TestClass.ClassFixtureTypes,
						createInstances: ctxt.TestCases.Any(tc => !tc.IsStaticallySkipped())
					)
		);
		return result;
	}

	/// <inheritdoc/>
	protected override async ValueTask<RunSummary> RunTestMethods(
		TContext ctxt,
		Exception? exception)
	{
		Guard.ArgumentNotNull(ctxt);

		if (exception is null)
		{
			ctxt.ConstructorArguments = await CreateTestClassConstructorArguments(ctxt);
			exception = ctxt.Aggregator.ToException();
			ctxt.Aggregator.Clear();
		}
		else
		{
			ctxt.ConstructorArguments = [];
		}

		return await base.RunTestMethods(ctxt, exception);
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
