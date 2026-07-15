#if NETFRAMEWORK

using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class XunitTestAssemblyTests(XunitTestAssemblyTests.XunitTestAssemblyFixture fixture) :
	IClassFixture<XunitTestAssemblyTests.XunitTestAssemblyFixture>
{
	readonly XunitTestAssembly testAssembly = fixture.TestAssembly;

	[Fact]
	public void AssemblyFixtureTypes()
	{
		var fixtures = testAssembly.AssemblyFixtureTypes;

		var fixture = Assert.Single(fixtures);
		Assert.Equal("SomeNamespace.SomeFixtureClass", fixture.FullName);
	}

	[Fact]
	public void BeforeAfterTestAttributes()
	{
		var attributes = testAssembly.BeforeAfterTestAttributes;

		var attribute = Assert.Single(attributes);
		Assert.Equal("SomeNamespace.BeforeAfterTest1Attribute", attribute.GetType().FullName);
	}

	[Fact]
	public void CollectionDefinitions()
	{
		var definitions = testAssembly.CollectionDefinitions;

		Assert.Collection(
			definitions.OrderBy(d => d.Key),
			definition =>
			{
				Assert.Equal("Foo", definition.Key);
				Assert.Equal("SomeNamespace.NamedCollectionDefinition", definition.Value.Type.FullName);
			},
			definition =>
			{
				// The ID is based on the assembly ID, which is based on the assembly name. Since we rebuild every time
				// and get a random assembly name every time, this ID will change every time. This is the same reason we
				// don't check the unique ID of the test assembly itself.
				Assert.Matches("Test collection for SomeNamespace.UnnamedCollectionDefinition \\(id: [0-9a-f]{64}\\)", definition.Key);
				Assert.Equal("SomeNamespace.UnnamedCollectionDefinition", definition.Value.Type.FullName);
			}
		);
	}

	[Fact]
	public void Parallelization()
	{
		Assert.Equal(42, testAssembly.MaxParallelThreads);
		Assert.Equal(ParallelAlgorithm.Aggressive, testAssembly.ParallelAlgorithm);
		Assert.Equal(ParallelMode.All, testAssembly.ParallelMode);
	}

	[Fact]
	public void TargetFramework()
	{
		Assert.Equal(".NETFramework,Version=v4.7.2", testAssembly.TargetFramework);
	}

	[Fact]
	public void TestCaseOrderer()
	{
		var orderer = testAssembly.TestCaseOrderer;

		Assert.NotNull(orderer);
		Assert.Equal("SomeNamespace.MyTestCaseOrderer", orderer.GetType().FullName);
	}

	[Fact]
	public void TestClassOrderer()
	{
		var orderer = testAssembly.TestClassOrderer;

		Assert.NotNull(orderer);
		Assert.Equal("SomeNamespace.MyTestClassOrderer", orderer.GetType().FullName);
	}

	[Fact]
	public void TestCollectionOrderer()
	{
		var orderer = testAssembly.TestCollectionOrderer;

		Assert.NotNull(orderer);
		Assert.Equal("SomeNamespace.MyTestCollectionOrderer", orderer.GetType().FullName);
	}

	[Fact]
	public void TestMethodOrderer()
	{
		var orderer = testAssembly.TestMethodOrderer;

		Assert.NotNull(orderer);
		Assert.Equal("SomeNamespace.MyTestMethodOrderer", orderer.GetType().FullName);
	}

	[Fact]
	public void Traits()
	{
		var traits = testAssembly.Traits;

		var trait = Assert.Single(traits);
		Assert.Equal("Hello", trait.Key);
		var value = Assert.Single(trait.Value);
		Assert.Equal("World", value);
	}

	[Fact]
	public void Version()
	{
		var version = testAssembly.Version;

		Assert.Equal(new Version(1, 2, 3, 4), version);
	}

	[Fact]
	public void Serialization()
	{
		var serialized = SerializationHelper.Instance.Serialize(testAssembly);
		var deserialized = SerializationHelper.Instance.Deserialize(serialized);

		Assert.IsType<XunitTestAssembly>(deserialized);
		Assert.Equivalent(testAssembly, deserialized);
	}

	public class XunitTestAssemblyFixture : IAsyncLifetime
	{
		CSharpAcceptanceTestV3Assembly? assemblyOnDisk;
		IDisposable? resolver;
		XunitTestAssembly? testAssembly;

		public XunitTestAssembly TestAssembly =>
			testAssembly ?? throw new InvalidOperationException("InitializeAsync must be called first");

		public ValueTask DisposeAsync()
		{
			assemblyOnDisk?.Dispose();
			resolver?.Dispose();
			return default;
		}

		public async ValueTask InitializeAsync()
		{
			assemblyOnDisk = await CSharpAcceptanceTestV3Assembly.CreateIn(Path.GetTempPath(), @"
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: AssemblyVersion(""1.2.3.4"")]
[assembly: AssemblyFixture(typeof(SomeNamespace.SomeFixtureClass))]
[assembly: SomeNamespace.BeforeAfterTest1]
[assembly: Parallelization(Algorithm = ParallelAlgorithm.Aggressive, Mode = ParallelMode.All, MaxThreads = 42)]
[assembly: TestCaseOrderer(typeof(SomeNamespace.MyTestCaseOrderer))]
[assembly: TestClassOrderer(typeof(SomeNamespace.MyTestClassOrderer))]
[assembly: TestCollectionOrderer(typeof(SomeNamespace.MyTestCollectionOrderer))]
[assembly: TestMethodOrderer(typeof(SomeNamespace.MyTestMethodOrderer))]
[assembly: Trait(""Hello"", ""World"")]

namespace SomeNamespace
{
	public class BeforeAfterTest1Attribute : BeforeAfterTestAttribute { }
	public class SomeFixtureClass { }

	[CollectionDefinition]
	public class UnnamedCollectionDefinition { }

	[CollectionDefinition(""Foo"")]
	public class NamedCollectionDefinition { }

	public class MyTestCaseOrderer : ITestCaseOrderer
	{
		public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
			where TTestCase : ITestCase
		{
			throw new NotImplementedException();
		}
	}

	public class MyTestClassOrderer : ITestClassOrderer
	{
		public IReadOnlyCollection<TTestClass> OrderTestClasses<TTestClass>(IReadOnlyCollection<TTestClass> testClasses)
			where TTestClass : ITestClass
		{
			throw new NotImplementedException();
		}
	}

	public class MyTestCollectionOrderer : ITestCollectionOrderer
	{
		public IReadOnlyCollection<TTestCollection> OrderTestCollections<TTestCollection>(IReadOnlyCollection<TTestCollection> testCollections)
			where TTestCollection : ITestCollection
		{
			throw new NotImplementedException();
		}
	}

	public class MyTestMethodOrderer : ITestMethodOrderer
	{
		public IReadOnlyCollection<TTestMethod> OrderTestMethods<TTestMethod>(IReadOnlyCollection<TTestMethod> testMethods)
			where TTestMethod : ITestMethod
		{
			throw new NotImplementedException();
		}
	}
}");

			resolver = AssemblyHelper.SubscribeResolveForAssembly(assemblyOnDisk.FileName);

			var assemblyPath = assemblyOnDisk.FileName;
			var assemblyName =
				assemblyPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) || assemblyPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
					? Path.GetFileNameWithoutExtension(assemblyPath)
					: Path.GetFileName(assemblyPath);
			var assembly = Assembly.Load(assemblyName);
			testAssembly = new XunitTestAssembly(assembly, configFilePath: null);
		}
	}
}

#endif
