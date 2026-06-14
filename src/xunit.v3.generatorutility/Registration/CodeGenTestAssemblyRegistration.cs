#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0090 // Use 'new(...)'

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to perform test-assembly-level registration.
	/// </summary>
	public class CodeGenTestAssemblyRegistration : IEquatable<CodeGenTestAssemblyRegistration>
	{
		static readonly HashSet<string> StringTypes = new HashSet<string>() { "string", "string?" };

		readonly Dictionary<string, string> assemblyFixtureFactories = new Dictionary<string, string>();
		string? parallelAlgorithm;
		string? parallelMaxThreads;
		string? parallelMode;
		string? testCaseOrdererFactory;
		string? testClassOrdererFactory;
		string? testCollectionFactoryFactory;
		string? testCollectionOrdererFactory;
		string? testFrameworkFactory;
		string? testMethodOrdererFactory;
		string? testPipelineStartupFactory;

		/// <summary>
		/// Add an assembly fixture to the registration.
		/// </summary>
		/// <param name="assemblyFixtureAttribute">The <c>[AssemblyFixture]</c> attribute</param>
		public void AddAssemblyFixture(AttributeData assemblyFixtureAttribute)
		{
			if (assemblyFixtureAttribute is null)
				throw new ArgumentNullException(nameof(assemblyFixtureAttribute));

			if (GetTypeFromAttribute(assemblyFixtureAttribute) is { } fixtureType)
				AddAssemblyFixture(fixtureType);
		}

		/// <summary>
		/// Add an assembly fixture to the registration.
		/// </summary>
		/// <param name="fixtureType">The fixture type</param>
		public void AddAssemblyFixture(INamedTypeSymbol fixtureType)
		{
			if (fixtureType is null)
				throw new ArgumentNullException(nameof(fixtureType));

			var factory = fixtureType.ToAssemblyFixtureFactory();
			if (factory != null)
				assemblyFixtureFactories[fixtureType.ToCSharp()] = factory;
		}

		/// <summary>
		/// Add an assembly fixture to the registration.
		/// </summary>
		/// <param name="fixtureTypeName">The global-qualified fixture type name (e.g., <c>"global::Namespace.Type"</c>)</param>
		public void AddAssemblyFixture(string fixtureTypeName)
		{
			if (fixtureTypeName is null)
				throw new ArgumentNullException(nameof(fixtureTypeName));

			assemblyFixtureFactories[fixtureTypeName] = $"async () => new {fixtureTypeName}()";
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as CodeGenTestAssemblyRegistration);

		/// <inheritdoc/>
		public bool Equals(CodeGenTestAssemblyRegistration? other) =>
			other != null &&
			ComparerHelper.Equal(assemblyFixtureFactories, other.assemblyFixtureFactories) &&
			ComparerHelper.Equal(testCaseOrdererFactory, other.testCaseOrdererFactory) &&
			ComparerHelper.Equal(testClassOrdererFactory, other.testClassOrdererFactory) &&
			ComparerHelper.Equal(testCollectionFactoryFactory, other.testCollectionFactoryFactory) &&
			ComparerHelper.Equal(testCollectionOrdererFactory, other.testCollectionOrdererFactory) &&
			ComparerHelper.Equal(testFrameworkFactory, other.testFrameworkFactory) &&
			ComparerHelper.Equal(testMethodOrdererFactory, other.testMethodOrdererFactory) &&
			ComparerHelper.Equal(testPipelineStartupFactory, other.testPipelineStartupFactory);

		/// <summary>
		/// Generates the assembly registration source.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to generate the source into</param>
		public void GenerateSource(StringBuilder builder)
		{
			if (builder is null)
				throw new ArgumentNullException(nameof(builder));

			foreach (var kvp in assemblyFixtureFactories)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyFixtureFactory(typeof({kvp.Key}), {kvp.Value});
");

			if (parallelAlgorithm is not null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterParallelAlgorithm({parallelAlgorithm});
");

			if (parallelMaxThreads is not null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterParallelMaxThreads({parallelMaxThreads});
");

			if (parallelMode is not null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterParallelMode({parallelMode});
");

			if (testCaseOrdererFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyTestCaseOrdererFactory({testCaseOrdererFactory});
");

			if (testClassOrdererFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyTestClassOrdererFactory({testClassOrdererFactory});
");

			if (testCollectionFactoryFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterTestCollectionFactoryFactory({testCollectionFactoryFactory});
");

			if (testCollectionOrdererFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyTestCollectionOrdererFactory({testCollectionOrdererFactory});
");

			if (testFrameworkFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterTestFrameworkFactory({testFrameworkFactory});
");

			if (testMethodOrdererFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterAssemblyTestMethodOrdererFactory({testMethodOrdererFactory});
");

			if (testPipelineStartupFactory != null)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterTestPipelineStartupFactory({testPipelineStartupFactory});
");
		}

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(assemblyFixtureFactories)
				.With(testCaseOrdererFactory)
				.With(testClassOrdererFactory)
				.With(testCollectionFactoryFactory)
				.With(testCollectionOrdererFactory)
				.With(testFrameworkFactory)
				.With(testMethodOrdererFactory)
				.With(testPipelineStartupFactory);

		static INamedTypeSymbol? GetTypeFromAttribute(AttributeData attribute)
		{
			var type = default(INamedTypeSymbol);

			if (attribute.AttributeClass?.TypeArguments.Length == 1)
				type = attribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
			else if (attribute.ConstructorArguments.Length == 1)
				type = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

			return type;
		}

		/// <summary>
		/// Sets parallelization overrides for the registration.
		/// </summary>
		/// <param name="parallelizationAttribute">The <c>[Parallelization]</c> attribute</param>
		public void SetParallelization(AttributeData parallelizationAttribute)
		{
			if (parallelizationAttribute is null)
				throw new ArgumentNullException(nameof(parallelizationAttribute));

			foreach (var namedArgument in parallelizationAttribute.NamedArguments)
				switch (namedArgument.Key)
				{
					case Names.ParallelizationAttribute.Algorithm:
						parallelAlgorithm = namedArgument.Value.ToCSharp();
						break;

					case Names.ParallelizationAttribute.MaxThreads:
						parallelMaxThreads = namedArgument.Value.ToCSharp();
						break;

					case Names.ParallelizationAttribute.Mode:
						parallelMode = namedArgument.Value.ToCSharp();
						break;
				}
		}

		/// <summary>
		/// Sets the test case orderer for the registration.
		/// </summary>
		/// <param name="ordererAttribute">The <c>[TestCaseOrderer]</c> attribute</param>
		public void SetTestCaseOrderer(AttributeData ordererAttribute)
		{
			if (ordererAttribute is null)
				throw new ArgumentNullException(nameof(ordererAttribute));

			var factory = ordererAttribute.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
			if (factory != null)
				testCaseOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test case orderer for the registration.
		/// </summary>
		/// <param name="ordererType">The test case orderer type</param>
		public void SetTestCaseOrderer(INamedTypeSymbol ordererType)
		{
			if (ordererType is null)
				throw new ArgumentNullException(nameof(ordererType));

			var factory = ordererType.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
			if (factory != null)
				testCaseOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test class orderer for the registration.
		/// </summary>
		/// <param name="ordererAttribute">The <c>[TestClassOrderer]</c> attribute</param>
		public void SetTestClassOrderer(AttributeData ordererAttribute)
		{
			if (ordererAttribute is null)
				throw new ArgumentNullException(nameof(ordererAttribute));

			var factory = ordererAttribute.ToOrdererFactory(Types.Xunit.v3.ITestClassOrderer);
			if (factory != null)
				testClassOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test class orderer for the registration.
		/// </summary>
		/// <param name="ordererType">The test class orderer type</param>
		public void SetTestClassOrderer(INamedTypeSymbol ordererType)
		{
			if (ordererType is null)
				throw new ArgumentNullException(nameof(ordererType));

			var factory = ordererType.ToOrdererFactory(Types.Xunit.v3.ITestClassOrderer);
			if (factory != null)
				testClassOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test collection factory for the registration.
		/// </summary>
		/// <param name="collectionBehaviorAttribute">The <c>[CollectionBehavior]</c> attribute</param>
		public void SetTestCollectionFactory(AttributeData collectionBehaviorAttribute)
		{
			if (collectionBehaviorAttribute is null)
				throw new ArgumentNullException(nameof(collectionBehaviorAttribute));

			if (GetTypeFromAttribute(collectionBehaviorAttribute) is { } testCollectionFactoryType)
				SetTestCollectionFactory(testCollectionFactoryType);
		}

		/// <summary>
		/// Sets the test collection factory for the registration.
		/// </summary>
		/// <param name="testCollectionFactoryType">The test collection factory type</param>
		public void SetTestCollectionFactory(INamedTypeSymbol testCollectionFactoryType)
		{
			if (testCollectionFactoryType is null)
				throw new ArgumentNullException(nameof(testCollectionFactoryType));

			if (testCollectionFactoryType.ImplementsInterface(Types.Xunit.v3.ICodeGenTestCollectionFactory)
					&& testCollectionFactoryType.HasConstructorParameters(Types.Xunit.v3.ICodeGenTestAssembly))
				testCollectionFactoryFactory = $"(assembly) => new {testCollectionFactoryType.ToCSharp()}(assembly)";
		}

		/// <summary>
		/// Sets the test collection orderer for the registration.
		/// </summary>
		/// <param name="ordererAttribute">The <c>[TestCollectionOrderer]</c> attribute</param>
		public void SetTestCollectionOrderer(AttributeData ordererAttribute)
		{
			if (ordererAttribute is null)
				throw new ArgumentNullException(nameof(ordererAttribute));

			var factory = ordererAttribute.ToOrdererFactory(Types.Xunit.v3.ITestCollectionOrderer);
			if (factory != null)
				testCollectionOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test collection orderer for the registration.
		/// </summary>
		/// <param name="ordererType">The test collection orderer type</param>
		public void SetTestCollectionOrderer(INamedTypeSymbol ordererType)
		{
			if (ordererType is null)
				throw new ArgumentNullException(nameof(ordererType));

			var factory = ordererType.ToOrdererFactory(Types.Xunit.v3.ITestCollectionOrderer);
			if (factory != null)
				testCollectionOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test framework for the registration.
		/// </summary>
		/// <param name="testFrameworkAttribute">The <c>[TestFramework]</c> attribute</param>
		public void SetTestFramework(AttributeData testFrameworkAttribute)
		{
			if (testFrameworkAttribute is null)
				throw new ArgumentNullException(nameof(testFrameworkAttribute));

			if (GetTypeFromAttribute(testFrameworkAttribute) is { } testFrameworkType)
				SetTestFramework(testFrameworkType);
		}

		/// <summary>
		/// Sets the test framework for the registration.
		/// </summary>
		/// <param name="testFrameworkType">The test framework type</param>
		public void SetTestFramework(INamedTypeSymbol testFrameworkType)
		{
			if (testFrameworkType is null)
				throw new ArgumentNullException(nameof(testFrameworkType));

			if (!testFrameworkType.ImplementsInterface(Types.Xunit.v3.ITestFramework) || !testFrameworkType.IsSafeToReference())
				return;

			// First check for a ctor that takes a string/string?
			var ctor = testFrameworkType.Constructors.FirstOrDefault(c =>
				!c.IsStatic
					&& c.DeclaredAccessibility == Accessibility.Public
					&& c.Parameters.Length == 1
					&& StringTypes.Contains(c.Parameters[0].Type.ToCSharp())
			);
			if (ctor != null)
				testFrameworkFactory = $"configFileName => new {testFrameworkType.ToCSharp()}(configFileName)";
			else
			{
				// Fall back to a parameterless ctor
				ctor = testFrameworkType.Constructors.FirstOrDefault(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0);
				if (ctor != null)
					testFrameworkFactory = $"configFileName => new {testFrameworkType.ToCSharp()}()";
			}
		}

		/// <summary>
		/// Sets the test method orderer for the registration.
		/// </summary>
		/// <param name="ordererAttribute">The <c>[TestMethodOrderer]</c> attribute</param>
		public void SetTestMethodOrderer(AttributeData ordererAttribute)
		{
			if (ordererAttribute is null)
				throw new ArgumentNullException(nameof(ordererAttribute));

			var factory = ordererAttribute.ToOrdererFactory(Types.Xunit.v3.ITestMethodOrderer);
			if (factory != null)
				testMethodOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test method orderer for the registration.
		/// </summary>
		/// <param name="ordererType">The test method orderer type</param>
		public void SetTestMethodOrderer(INamedTypeSymbol ordererType)
		{
			if (ordererType is null)
				throw new ArgumentNullException(nameof(ordererType));

			var factory = ordererType.ToOrdererFactory(Types.Xunit.v3.ITestMethodOrderer);
			if (factory != null)
				testMethodOrdererFactory = "() => " + factory;
		}

		/// <summary>
		/// Sets the test pipeline startup for the registration.
		/// </summary>
		/// <param name="testPipelineStartupAttribute">The <c>[TestPipelineStartup]</c> attribute</param>
		public void SetTestPipelineStartup(AttributeData testPipelineStartupAttribute)
		{
			if (testPipelineStartupAttribute is null)
				throw new ArgumentNullException(nameof(testPipelineStartupAttribute));

			if (GetTypeFromAttribute(testPipelineStartupAttribute) is { } testPipelineStartupType)
				SetTestPipelineStartup(testPipelineStartupType);
		}

		/// <summary>
		/// Sets the test pipeline startup for the registration.
		/// </summary>
		/// <param name="testPipelineStartupType">The test pipeline startup type</param>
		public void SetTestPipelineStartup(INamedTypeSymbol testPipelineStartupType)
		{
			if (testPipelineStartupType is null)
				throw new ArgumentNullException(nameof(testPipelineStartupType));

			if (!testPipelineStartupType.ImplementsInterface(Types.Xunit.v3.ITestPipelineStartup) || !testPipelineStartupType.IsSafeToReference())
				return;

			var ctor = testPipelineStartupType.Constructors.FirstOrDefault(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == 0);
			if (ctor != null)
				testPipelineStartupFactory = $"() => new {testPipelineStartupType.ToCSharp()}()";
		}
	}
}
