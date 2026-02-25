#pragma warning disable IDE0060 // Method contracts here must match the non-AOT version

using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// This class acts as a repository for test engine configuration for xunit.v3.core.aot.
/// </summary>
public static class RegisteredEngineConfig
{
	static readonly CodeGenTestAssemblyRegistration assemblyRegistration = new();
	static readonly Dictionary<string, List<Func<ITestFrameworkDiscoveryOptions, ICodeGenTestClass, DisposalTracker, ValueTask<IReadOnlyCollection<ICodeGenTestCase>>>>> testCaseFactories = [];
	static readonly Dictionary<string, string> testClassIndexByTypeFullName = [];
	static readonly Dictionary<string, CodeGenTestClassRegistration> testClassRegistrations = [];
	static readonly Dictionary<(string TestClassIndex, string TestMethodName), CodeGenTestMethodRegistration> testMethodRegistrations = [];
	static readonly Dictionary<(string TestClassIndex, string TestMethodName), List<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>>> theoryDataRowFactories = [];

	/// <summary>
	/// Gets the test case orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCaseOrderer? GetAssemblyTestCaseOrderer(Assembly testAssembly) =>
		testAssembly == assemblyRegistration.Assembly ? assemblyRegistration.TestCaseOrdererFactory?.Invoke() : null;

	/// <summary>
	/// Gets the test class orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestClassOrderer? GetAssemblyTestClassOrderer(Assembly testAssembly) =>
		testAssembly == assemblyRegistration.Assembly ? assemblyRegistration.TestClassOrdererFactory?.Invoke() : null;

	/// <summary>
	/// Gets the test collection orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestCollectionOrderer? GetAssemblyTestCollectionOrderer(Assembly testAssembly) =>
		testAssembly == assemblyRegistration.Assembly ? assemblyRegistration.TestCollectionOrdererFactory?.Invoke() : null;

	/// <summary>
	/// Gets the test method orderer that's attached to a test assembly. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	public static ITestMethodOrderer? GetAssemblyTestMethodOrderer(Assembly testAssembly) =>
		testAssembly == assemblyRegistration.Assembly ? assemblyRegistration.TestMethodOrdererFactory?.Invoke() : null;

	/// <summary>
	/// Gets the test case orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestCaseOrderer? GetClassTestCaseOrderer(Type testClass)
	{
		if (!testClassIndexByTypeFullName.TryGetValue(testClass.SafeName(), out var testClassIndex))
			return null;
		if (!testClassRegistrations.TryGetValue(testClassIndex, out var registration))
			return null;

		return registration.TestCaseOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test method orderer that's attached to a test class. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	public static ITestMethodOrderer? GetClassTestMethodOrderer(Type testClass)
	{
		if (!testClassIndexByTypeFullName.TryGetValue(testClass.SafeName(), out var testClassIndex))
			return null;
		if (!testClassRegistrations.TryGetValue(testClassIndex, out var registration))
			return null;

		return registration.TestMethodOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestCaseOrderer? GetCollectionTestCaseOrderer(Type? collectionDefinition)
	{
		if (collectionDefinition is null)
			return null;
		if (!assemblyRegistration.CollectionDefinitionsByType.TryGetValue(collectionDefinition.SafeName(), out var registration))
			return null;

		return registration.TestCaseOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test class orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestClassOrderer? GetCollectionTestClassOrderer(Type? collectionDefinition)
	{
		if (collectionDefinition is null)
			return null;
		if (!assemblyRegistration.CollectionDefinitionsByType.TryGetValue(collectionDefinition.SafeName(), out var registration))
			return null;

		return registration.TestClassOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test method orderer that's attached to a test collection. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="collectionDefinition">The test collection definition</param>
	public static ITestMethodOrderer? GetCollectionTestMethodOrderer(Type? collectionDefinition)
	{
		if (collectionDefinition is null)
			return null;
		if (!assemblyRegistration.CollectionDefinitionsByType.TryGetValue(collectionDefinition.SafeName(), out var registration))
			return null;

		return registration.TestMethodOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test method. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testClass">The test class</param>
	/// <param name="methodName">The test method name</param>
	public static ITestCaseOrderer? GetMethodTestCaseOrderer(
		Type testClass,
		string methodName)
	{
		if (!testClassIndexByTypeFullName.TryGetValue(testClass.SafeName(), out var testClassIndex))
			return null;
		if (!testMethodRegistrations.TryGetValue((testClassIndex, methodName), out var registration))
			return null;

		return registration.TestCaseOrdererFactory?.Invoke();
	}

	/// <summary>
	/// Gets the test assembly that was registered for the given assembly.
	/// </summary>
	/// <param name="assembly">The assembly under test</param>
	/// <param name="configFileName">The optional configuration file</param>
	public static ICodeGenTestAssembly GetTestAssembly(
		Assembly assembly,
		string? configFileName) =>
			assemblyRegistration.GetTestAssembly(Guard.ArgumentNotNull(assembly), configFileName);

	internal static async ValueTask<IReadOnlyList<ICodeGenTestCase>> GetTestCases(
		ITestFrameworkDiscoveryOptions discoveryOptions,
		ICodeGenTestClass testClass,
		DisposalTracker disposalTracker)
	{
		var result = new List<ICodeGenTestCase>();

		if (testClassIndexByTypeFullName.TryGetValue(testClass.TestClassName, out var testClassIndex))
			foreach (var testCaseFactory in testCaseFactories[testClassIndex])
				result.AddRange(await testCaseFactory(discoveryOptions, testClass, disposalTracker));

		return result;
	}

	internal static ICodeGenTestClass? GetTestClass(
		ICodeGenTestAssembly testAssembly,
		Type @class)
	{
		if (!testClassIndexByTypeFullName.TryGetValue(@class.SafeName(), out var testClassIndex))
			return null;
		if (!testClassRegistrations.TryGetValue(testClassIndex, out var registration))
			return null;

		return registration.GetTestClass(testAssembly);
	}

	internal static Type[] GetTestClassTypes() =>
		testClassRegistrations.Select(kvp => kvp.Value.Class).ToArray();

	/// <summary>
	/// Gets an xUnit.net v3 test collection factory.
	/// </summary>
	/// <param name="testAssembly">The test assembly under test</param>
	public static ICodeGenTestCollectionFactory GetTestCollectionFactory(ICodeGenTestAssembly testAssembly) =>
		assemblyRegistration.TestCollectionFactoryFactory(testAssembly);

	/// <summary>
	/// Gets the test framework object for the given test assembly. It is important that callers to this function have
	/// called <see cref="TestContext.SetForInitialization"/> before calling this, so that the test framework and
	/// any ancillary helper classes have access to the diagnostic and internal diagnostic message sinks.
	/// </summary>
	/// <param name="testAssembly">The test assembly to get the test framework for</param>
	/// <param name="configFileName">The optional configuration file</param>
	public static ITestFramework GetTestFramework(
		Assembly testAssembly,
		string? configFileName) =>
			assemblyRegistration.TestFrameworkFactory(configFileName);

	/// <summary>
	/// Gets the registered test pipeline startup object.
	/// </summary>
	/// <param name="testAssembly">The test assembly</param>
	/// <param name="warnings">Warnings that results from discovering the test pipeline startup</param>
	public static ITestPipelineStartup? GetTestPipelineStartup(
		Assembly testAssembly,
		List<string>? warnings = null)
	{
		try
		{
			return assemblyRegistration.TestPipelineStartupFactory?.Invoke();
		}
		catch (Exception ex)
		{
			throw new TestPipelineException("Test pipeline startup threw during construction", ex);
		}
	}

	/// <summary>
	/// Gets the theory data row factories associated with the given test method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <remarks>
	/// It is expected that data attributes will have registered factories via <see cref="RegisterTheoryDataRowFactory"/>.
	/// This will return an empty array when no factories have been registered for the given test method.
	/// </remarks>
	public static IReadOnlyCollection<Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>>> GetTheoryDataRowFactories(ICodeGenTestMethod testMethod)
	{
		Guard.ArgumentNotNull(testMethod);

		var declaredTypeIndex = testMethod.DeclaredTypeIndex;
		if (declaredTypeIndex is null)
			if (!testClassIndexByTypeFullName.TryGetValue(testMethod.TestClass.TestClassName, out declaredTypeIndex))
				return [];
		if (!theoryDataRowFactories.TryGetValue((declaredTypeIndex, testMethod.MethodName), out var factories))
			return [];

		return factories;
	}

	/// <summary>
	/// Registers the factory that creates the assembly-level <see cref="ITestCaseOrderer"/> instance.
	/// </summary>
	/// <param name="factory">The orderer factory</param>
	/// <remarks>
	/// This is typically called when <see cref="TestCaseOrdererAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterAssemblyTestCaseOrdererFactory(Func<ITestCaseOrderer> factory) =>
		assemblyRegistration.TestCaseOrdererFactory = factory;

	/// <summary>
	/// Registers the factory that creates the assembly-level <see cref="ITestClassOrderer"/> instance.
	/// </summary>
	/// <param name="factory">The orderer factory</param>
	/// <remarks>
	/// This is typically called when <see cref="TestClassOrdererAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterAssemblyTestClassOrdererFactory(Func<ITestClassOrderer> factory) =>
		assemblyRegistration.TestClassOrdererFactory = factory;

	/// <summary>
	/// Registers the factory that creates the assembly-level <see cref="ITestCollectionOrderer"/> instance.
	/// </summary>
	/// <param name="factory">The orderer factory</param>
	/// <remarks>
	/// This is typically called when <see cref="TestCollectionOrdererAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterAssemblyTestCollectionOrdererFactory(Func<ITestCollectionOrderer> factory) =>
		assemblyRegistration.TestCollectionOrdererFactory = factory;

	/// <summary>
	/// Registers a factory for an assembly-level test fixture.
	/// </summary>
	/// <param name="type">The type of the test fixture</param>
	/// <param name="factory">The factory that creates the fixture instance</param>
	/// <remarks>
	/// This is typically called when <see cref="AssemblyFixtureAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterAssemblyFixtureFactory(
		Type type,
		Func<ValueTask<object>> factory) =>
			assemblyRegistration.AssemblyFixtureFactories[type] = _ => factory();

	/// <summary>
	/// Registers the factory that creates the assembly-level <see cref="ITestMethodOrderer"/> instance.
	/// </summary>
	/// <param name="factory">The orderer factory</param>
	/// <remarks>
	/// This is typically called when <see cref="TestMethodOrdererAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterAssemblyTestMethodOrdererFactory(Func<ITestMethodOrderer> factory) =>
		assemblyRegistration.TestMethodOrdererFactory = factory;

	/// <summary>
	/// Registers the presence of an xUnit.net v3 test case factory via code generation.
	/// </summary>
	/// <param name="testClassIndex">The dictionary index of the test class. This is the C# compilation name for
	/// the test class (typically in a form of <c>global::Namespace.TypeName</c>).</param>
	/// <param name="methodName">The test method name</param>
	/// <param name="factory">The test case factory</param>
	/// <remarks>
	/// This is typically called when test methods are found during code generation. The provided factory is
	/// responsible for creating instances of <see cref="ICodeGenTestCase"/> given an input of <see cref="ICodeGenTestMethod"/>
	/// and <see cref="ITestFrameworkDiscoveryOptions"/>.
	/// </remarks>
	public static void RegisterCodeGenTestCaseFactory(
		string testClassIndex,
		string methodName,
		ICodeGenTestCaseFactory factory)
	{
		Guard.ArgumentNotNull(testClassIndex);
		Guard.ArgumentNotNull(methodName);
		Guard.ArgumentNotNull(factory);

		testCaseFactories
			.AddOrGet(testClassIndex)
			.Add(async (discoveryOptions, testClass, disposalTracker) =>
			{
				if (!testMethodRegistrations.TryGetValue((testClassIndex, methodName), out var testMethodRegistration))
					throw new InvalidOperationException(
						string.Format(
							CultureInfo.CurrentCulture,
							"Tried to get test case for unregistered test method: {0}.{1}",
							testClass.TestClassName,
							methodName
						)
					);

				var testMethod = testMethodRegistration.GetTestMethod(testClass, methodName);
				return await factory.Generate(discoveryOptions, testMethod, disposalTracker);
			});
	}

	/// <summary>
	/// Registers the presence of an xUnit.net v3 test class via code generation.
	/// </summary>
	/// <param name="testClassIndex">The dictionary index of the test class. This is the C# compilation name for
	/// the test class (typically in a form of <c>global::Namespace.TypeName</c>).</param>
	/// <param name="registration">The test class registration.</param>
	/// <remarks>
	/// This is typically called when test classes are found during code generation.
	/// </remarks>
	public static void RegisterCodeGenTestClass(
		string testClassIndex,
		CodeGenTestClassRegistration registration)
	{
		testClassRegistrations[Guard.ArgumentNotNull(testClassIndex)] = Guard.ArgumentNotNull(registration);
		testClassIndexByTypeFullName[registration.Class.SafeName()] = testClassIndex;
	}

	/// <summary>
	/// Registers the presence of an xUnit.net v3 test class via code generation.
	/// </summary>
	/// <param name="testClassIndex">The dictionary index of the test class. This is the C# compilation name for
	/// the test class (typically in a form of <c>global::Namespace.TypeName</c>).</param>
	/// <param name="methodName">The test method name</param>
	/// <param name="registration">The test method registration</param>
	/// <remarks>
	/// This is typically called when test methods are found during code generation.
	/// </remarks>
	public static void RegisterCodeGenTestMethod(
		string testClassIndex,
		string methodName,
		CodeGenTestMethodRegistration registration) =>
			testMethodRegistrations[(Guard.ArgumentNotNull(testClassIndex), Guard.ArgumentNotNullOrEmpty(methodName))] = Guard.ArgumentNotNull(registration);

	/// <summary>
	/// Registers a collection definition.
	/// </summary>
	/// <param name="name">The name of the collection definition, if customized</param>
	/// <param name="registration">The collection definition</param>
	/// <remarks>
	/// This is typically called when <see cref="CollectionDefinitionAttribute"/> is seen on a class.
	/// </remarks>
	public static void RegisterCollectionDefinition(
		string? name,
		CodeGenTestCollectionRegistration registration)
	{
		Guard.ArgumentNotNull(registration);

		if (name is null)
		{
			Guard.ArgumentNotNull("Collection definitions must include either a name or a type", registration.Type);

			name = CollectionAttribute.GetCollectionNameForType(registration.Type);
		}

		assemblyRegistration.CollectionDefinitionsByName[name] = registration;

		if (registration.Type is not null)
			assemblyRegistration.CollectionDefinitionsByType[registration.Type.SafeName()] = registration;
	}

	/// <summary>
	/// Registers a factory for <see cref="ICodeGenTestCollectionFactory"/>.
	/// </summary>
	/// <param name="factory">The test collection factory</param>
	/// <remarks>
	/// This is typically called when <see cref="CollectionBehaviorAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterTestCollectionFactoryFactory(Func<ICodeGenTestAssembly, ICodeGenTestCollectionFactory> factory) =>
		assemblyRegistration.TestCollectionFactoryFactory = Guard.ArgumentNotNull(factory);

	/// <summary>
	/// Registers a test framework factory
	/// </summary>
	/// <remarks>
	/// This is typically called when <see cref="TestFrameworkAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterTestFrameworkFactory(Func<string?, ITestFramework> factory) =>
		assemblyRegistration.TestFrameworkFactory = Guard.ArgumentNotNull(factory);

	/// <summary>
	/// Registers a test pipeline startup factory.
	/// </summary>
	/// <remarks>
	/// This is typically called when <see cref="TestPipelineStartupAttribute"/> is seen at the test assembly level.
	/// </remarks>
	public static void RegisterTestPipelineStartupFactory(Func<ITestPipelineStartup> factory) =>
		assemblyRegistration.TestPipelineStartupFactory = Guard.ArgumentNotNull(factory);

	/// <summary>
	/// Registers a source of theory data rows for a given test method.
	/// </summary>
	/// <param name="testClassIndex">The dictionary index of the test class. This is the C# compilation name for
	/// the test class (typically in a form of <c>global::Namespace.TypeName</c>).</param>
	/// <param name="methodName">The test method name</param>
	/// <param name="factory">The factory providing theory data rows</param>
	/// <remarks>
	/// This is typically called when <see cref="DataAttribute"/>-derived classes are seen at the test method level.
	/// </remarks>
	public static void RegisterTheoryDataRowFactory(
		string testClassIndex,
		string methodName,
		Func<DisposalTracker, ValueTask<IReadOnlyCollection<ITheoryDataRow>>> factory) =>
			theoryDataRowFactories.AddOrGet((Guard.ArgumentNotNull(testClassIndex), Guard.ArgumentNotNull(methodName))).Add(factory);
}
