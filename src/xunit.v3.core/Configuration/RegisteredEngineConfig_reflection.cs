using System.Collections.Concurrent;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class acts as a repository for test engine configuration for xunit.v3.core.
/// </summary>
public static class RegisteredEngineConfig
{
	static readonly ConcurrentDictionary<IXunitTestAssembly, IXunitTestCollectionFactory> testCollectionFactories = [];

	static object? CreateInstance(
		Type type,
		object?[]? ctorArgs)
	{
		ctorArgs ??= [];

		try
		{
			return Activator.CreateInstance(type, ctorArgs);
		}
		catch (MissingMemberException)
		{
			if (ctorArgs.Length == 0)
				TestContext.Current.SendDiagnosticMessage("Could not find empty constructor for '{0}'", type.SafeName());
			else
				TestContext.Current.SendDiagnosticMessage(
					"Could not find constructor for '{0}' with arguments type(s): {1}",
					type.SafeName(),
					ctorArgs.Select(a => a?.GetType()).ToCommaSeparatedList()
				);

			throw;
		}
	}

	static TInterface? Get<TInterface>(
		Type? type,
		object?[]? ctorArgs = null)
			where TInterface : class =>
				type is not null
					? CreateInstance(type, ctorArgs) as TInterface
					: default;

	/// <summary>
	/// Gets the test case orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCaseOrderer? GetAssemblyTestCaseOrderer(Assembly testAssembly) =>
		GetAssemblyTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(testAssembly, "case");

	/// <summary>
	/// Gets the test class orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestClassOrderer? GetAssemblyTestClassOrderer(Assembly testAssembly) =>
		GetAssemblyTestOrderer<ITestClassOrderer, ITestClassOrdererAttribute>(testAssembly, "class");

	/// <summary>
	/// Gets the test collection orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCollectionOrderer? GetAssemblyTestCollectionOrderer(Assembly testAssembly) =>
		GetAssemblyTestOrderer<ITestCollectionOrderer, ITestCollectionOrdererAttribute>(testAssembly, "collection");

	/// <summary>
	/// Gets the test method orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestMethodOrderer? GetAssemblyTestMethodOrderer(Assembly testAssembly) =>
		GetAssemblyTestOrderer<ITestMethodOrderer, ITestMethodOrdererAttribute>(testAssembly, "method");

	static TTestOrderer? GetAssemblyTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Assembly testAssembly,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		Guard.ArgumentNotNull(testAssembly);

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = testAssembly.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one assembly-level test {0} orderer: {1}",
					ordererType,
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Assembly-level test {0} orderer '{1}' threw during construction",
							ordererType,
							ordererAttribute.OrdererType
						),
						innerEx
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestCaseOrderer? GetClassTestCaseOrderer(Type testClass) =>
		GetClassTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(testClass, "case");

	/// <summary>
	/// Gets the test method orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestMethodOrderer? GetClassTestMethodOrderer(Type testClass) =>
		GetClassTestOrderer<ITestMethodOrderer, ITestMethodOrdererAttribute>(testClass, "method");

	static TTestOrderer? GetClassTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Type testClass,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		Guard.ArgumentNotNull(testClass);

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = testClass.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
			{
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one class-level test {0} orderer for test class '{1}': {2}",
					ordererType,
					testClass.SafeName(),
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

				return null;
			}

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Class-level test {0} orderer '{1}' for test class '{2}' threw during construction",
							ordererType,
							ordererAttribute.OrdererType,
							testClass.SafeName()
						),
						innerEx
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestCaseOrderer? GetCollectionTestCaseOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(collectionDefinition, "case");

	/// <summary>
	/// Gets the test class orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestClassOrderer? GetCollectionTestClassOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestClassOrderer, ITestClassOrdererAttribute>(collectionDefinition, "class");

	/// <summary>
	/// Gets the test method orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestMethodOrderer? GetCollectionTestMethodOrderer(Type? collectionDefinition) =>
		GetCollectionTestOrderer<ITestMethodOrderer, ITestMethodOrdererAttribute>(collectionDefinition, "method");

	static TTestOrderer? GetCollectionTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Type? collectionDefinition,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		if (collectionDefinition is null)
			return null;

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = collectionDefinition.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one collection-level test {0} orderer for test collection '{1}': {2}",
					ordererType,
					collectionDefinition.SafeName(),
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Collection-level test {0} orderer '{1}' for test collection '{2}' threw during construction",
							ordererType,
							ordererAttribute.OrdererType,
							collectionDefinition.SafeName()
						),
						innerEx
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test method. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="methodName">The test method name</param>
	public static ITestCaseOrderer? GetMethodTestCaseOrderer(
		Type testClass,
		string methodName) =>
			GetMethodTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(testClass, methodName, "case");

	static TTestOrderer? GetMethodTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		Type testClass,
		string methodName,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		Guard.ArgumentNotNull(testClass);
		Guard.ArgumentNotNull(methodName);

		var testMethod = Guard.NotNull(() => $"Cannot locate test method '{testClass.SafeName()}.{methodName}'", testClass.GetMethod(methodName, XunitTestClass.MethodBindingFlags));
		var warnings = new List<string>();

		try
		{
			var ordererAttributes = testMethod.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one method-level test {0} orderer for test method '{1}.{2}': {3}",
					ordererType,
					testClass.SafeName(),
					testClass.Name,
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					throw new TestPipelineException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Method-level test {0} orderer '{1}' for test method '{2}.{3}' threw during construction",
							ordererType,
							ordererAttribute.OrdererType,
							testClass.SafeName(),
							testMethod.Name
						),
						innerEx
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets an xUnit.net v3 test collection factory.
	/// </summary>
	/// <param name="testAssembly">The test assembly under test</param>
	public static IXunitTestCollectionFactory GetTestCollectionFactory(IXunitTestAssembly testAssembly) =>
		testCollectionFactories.GetOrAdd(Guard.ArgumentNotNull(testAssembly), assembly =>
		{
			var testCollectionFactoryType = assembly.CollectionBehavior?.CollectionFactoryType;
			if (testCollectionFactoryType is not null && !typeof(IXunitTestCollectionFactory).IsAssignableFrom(testCollectionFactoryType))
			{
				TestContext.Current.SendDiagnosticMessage(
					"Test collection factory type '{0}' does not implement IXunitTestCollectionFactory",
					testCollectionFactoryType.SafeName()
				);
				testCollectionFactoryType = null;
			}

			var result = Get<IXunitTestCollectionFactory>(testCollectionFactoryType, [assembly]);
			if (result is null && testCollectionFactoryType is not null)
				TestContext.Current.SendDiagnosticMessage(
					"Test collection factory type '{0}' does not implement IXunitTestCollectionFactory",
					testCollectionFactoryType.SafeName()
				);

			return result ?? new CollectionPerClassTestCollectionFactory(assembly);
		});

	/// <summary>
	/// Gets the test framework object for the given test assembly. It is important that callers to this function have
	/// called <see cref="TestContext.SetForInitialization"/> before calling this, so that the test framework and
	/// any ancillary helper classes have access to the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testAssembly">The test assembly to get the test framework for</param>
	/// <param name="configFileName">The optional configuration file</param>
	public static ITestFramework GetTestFramework(
		Assembly testAssembly,
		string? configFileName)
	{
		Guard.ArgumentNotNull(testAssembly);

		var warnings = new List<string>();

		try
		{
			var testFrameworkAttribute = testAssembly.GetMatchingCustomAttributes<ITestFrameworkAttribute>(warnings).FirstOrDefault();
			var testFrameworkType = testFrameworkAttribute?.FrameworkType ?? typeof(XunitTestFramework);
			if (!typeof(ITestFramework).IsAssignableFrom(testFrameworkType))
				throw new ArgumentException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Test framework type '{0}' does not implement '{1}'",
						testFrameworkType.SafeName(),
						typeof(ITestFramework).SafeName()
					)
				);

			var obj = default(object);

			// Try to create with the config file first
			try
			{
				obj = Activator.CreateInstance(testFrameworkType, [configFileName]);
			}
			catch (MissingMemberException)
			{ }

			// Fall back to a parameterless constructor
			obj ??=
				Activator.CreateInstance(testFrameworkType)
					?? throw new ArgumentException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Failed to create instance of test framework type '{0}'",
							testFrameworkType.SafeName()
						)
					);

			if (obj is ITestFramework result)
				return result;

			throw new ArgumentException(
				string.Format(
					CultureInfo.CurrentCulture,
					"Test framework type '{0}' could not be cast to '{1}'",
					testFrameworkType.SafeName(),
					typeof(ITestFramework).SafeName()
				)
			);
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the registered test pipeline startup object.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	/// <param name="warnings">Warnings that results from discovering the test pipeline startup</param>
	public static ITestPipelineStartup? GetTestPipelineStartup(
		Assembly testAssembly,
		List<string>? warnings = null)
	{
		var result = default(ITestPipelineStartup);

		var pipelineStartupAttributes = testAssembly.GetMatchingCustomAttributes<ITestPipelineStartupAttribute>(warnings ?? []);
		if (pipelineStartupAttributes.Count > 1)
			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"More than one pipeline startup attribute was specified: {0}",
					pipelineStartupAttributes.Select(a => a.GetType()).ToCommaSeparatedList()
				)
			);

		if (pipelineStartupAttributes.FirstOrDefault() is ITestPipelineStartupAttribute pipelineStartupAttribute)
		{
			var pipelineStartupType = pipelineStartupAttribute.TestPipelineStartupType;
			if (!typeof(ITestPipelineStartup).IsAssignableFrom(pipelineStartupType))
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Pipeline startup type '{0}' does not implement '{1}'",
						pipelineStartupType.SafeName(),
						typeof(ITestPipelineStartup).SafeName()
					)
				);

			try
			{
				result = Activator.CreateInstance(pipelineStartupType) as ITestPipelineStartup;
			}
			catch (Exception ex)
			{
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Pipeline startup type '{0}' threw during construction",
						pipelineStartupType.SafeName()
					),
					ex
				);
			}

			if (result is null)
				throw new TestPipelineException(
					string.Format(
						CultureInfo.CurrentCulture,
						"Pipeline startup type '{0}' does not implement '{1}'",
						pipelineStartupType.SafeName(),
						typeof(ITestPipelineStartup).SafeName()
					)
				);
		}

		return result;
	}
}
