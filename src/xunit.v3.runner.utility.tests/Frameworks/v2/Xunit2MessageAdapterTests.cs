using System;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.v2;
using Xunit.v3;
using v2Mocks = Xunit.Runner.v2.Mocks;

public class Xunit2MessageAdapterTests
{
	static readonly string osSpecificAssemblyPath;

	static Xunit2MessageAdapterTests()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			osSpecificAssemblyPath = @"C:\Users\bradwilson\assembly.dll";
		else
			osSpecificAssemblyPath = "/home/bradwilson/assembly.dll";
	}

	public class TestAssemblyTests
	{
		protected ITestAssembly TestAssembly;
		protected string TestAssemblyUniqueID;

		public TestAssemblyTests()
		{
			TestAssembly = v2Mocks.TestAssembly("testAssembly.dll", "xunit.runner.json");
			TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(
				TestAssembly.Assembly.Name,
				TestAssembly.Assembly.AssemblyPath,
				TestAssembly.ConfigFileName
			);
		}

		[Fact]
		public void TestAssemblyFinished()
		{
			var v2Message = v2Mocks.TestAssemblyFinished(
				TestAssembly,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 6,
				executionTime: 123.4567m
			);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestAssemblyFinished>(adapted);
			Assert.NotEmpty(v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(6, v3Message.TestsSkipped);
		}

		public static TheoryData<string, string?, string> TestAssemblyStartingData()
		{
			var osSpecificConfigPath = osSpecificAssemblyPath + ".json";
			var osSpecificUniqueID = UniqueIDGenerator.ForAssembly("assembly", osSpecificAssemblyPath, osSpecificConfigPath);

			return new TheoryData<string, string?, string>
			{
				{ "asm-path", null, "dded4d854ffcc191c6cd019b8c26cd68190c43122fef8a8d812e9e46ab6d640d" },
				{ "asm-path", "config-path", "7c90ea022d32916680dfa4fb9546e2690a9716c6e8a8b867d62b3c4da5833a91" },
				{ osSpecificAssemblyPath, osSpecificConfigPath, osSpecificUniqueID }
			};
		}

		[Theory]
		[MemberData(nameof(TestAssemblyStartingData))]
		public void TestAssemblyStarting(
			string assemblyPath,
			string? configFilePath,
			string expectedUniqueID)
		{
			var testAssembly = v2Mocks.TestAssembly(assemblyPath, configFilePath, "target-framework");
			var v2Message = v2Mocks.TestAssemblyStarting(
				testAssembly,
				new DateTime(2020, 11, 3, 17, 55, 0, DateTimeKind.Utc),
				"test-environment",
				"test-framework"
			);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestAssemblyStarting>(adapted);
			Assert.Equal(Path.GetFileNameWithoutExtension(assemblyPath), v3Message.AssemblyName);
			Assert.Equal(assemblyPath, v3Message.AssemblyPath);
			Assert.Equal(expectedUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(configFilePath, v3Message.ConfigFilePath);
			Assert.Equal(new DateTimeOffset(2020, 11, 3, 17, 55, 0, TimeSpan.Zero), v3Message.StartTime);
			Assert.Equal("target-framework", v3Message.TargetFramework);
			Assert.Equal("test-environment", v3Message.TestEnvironment);
			Assert.Equal("test-framework", v3Message.TestFrameworkDisplayName);
		}
	}

	public class TestClassTests : TestCollectionTests
	{
		protected ITestClass TestClass;
		protected string TestClassUniqueID;

		public TestClassTests()
		{
			TestClass = v2Mocks.TestClass(TestCollection);
			TestClassUniqueID = UniqueIDGenerator.ForTestClass(
				TestCollectionUniqueID,
				TestClass.Class.Name
			);
		}

		[Fact]
		public void TestClassFinished()
		{
			var v2Message = v2Mocks.TestClassFinished(
				TestClass,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestClassFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(404, v3Message.TestsSkipped);
		}

		[Fact]
		public void TestClassStarting()
		{
			var v2Message = v2Mocks.TestClassStarting(TestClass);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestClassStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(v2Message.TestClass.Class.Name, v3Message.TestClass);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
		}
	}

	public class TestCollectionTests : TestAssemblyTests
	{
		protected ITestCollection TestCollection;
		protected ITypeInfo TestCollectionDefinition;
		protected string TestCollectionUniqueID;

		public TestCollectionTests()
		{
			TestCollectionDefinition = v2Mocks.TypeInfo();
			TestCollection = v2Mocks.TestCollection(TestAssembly, TestCollectionDefinition, "test-collection-display-name");
			TestCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				TestAssemblyUniqueID,
				TestCollection.DisplayName,
				TestCollectionDefinition.Name
			);
		}

		[Fact]
		public void TestCollectionFinished()
		{
			var v2Message = v2Mocks.TestCollectionFinished(
				TestCollection,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestCollectionFinished>(adapted);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(404, v3Message.TestsSkipped);
		}

		[Fact]
		public void TestCollectionStarting()
		{
			var v2Message = v2Mocks.TestCollectionStarting(TestCollection);

			var adapted = Xunit2MessageAdapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestCollectionStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionDefinition.Name, v3Message.TestCollectionClass);
			Assert.Equal(TestCollection.DisplayName, v3Message.TestCollectionDisplayName);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
		}
	}
}
