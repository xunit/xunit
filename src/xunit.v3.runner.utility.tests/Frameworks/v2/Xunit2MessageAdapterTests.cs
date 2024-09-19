using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.v2;
using Xunit.Sdk;

public class Xunit2MessageAdapterTests
{
	static readonly string BeforeAfterAttributeName = "MyNamespace.MyBeforeAfterAttribute";
	static readonly Xunit.Abstractions.ITest Test;
	static readonly Xunit.Abstractions.ITestAssembly TestAssembly;
	static readonly string TestAssemblyUniqueID;
	static readonly Xunit.Abstractions.ITestCase TestCase;
	static readonly string TestCaseUniqueID;
	static readonly Xunit.Abstractions.ITestClass TestClass;
	static readonly ITypeInfo TestClassType;
	static readonly string? TestClassUniqueID;
	static readonly Xunit.Abstractions.ITestCollection TestCollection;
	static readonly ITypeInfo TestCollectionDefinition;
	static readonly string TestCollectionUniqueID;
	static readonly Xunit.Abstractions.ITestMethod TestMethod;
	static readonly string? TestMethodUniqueID;
	static readonly string TestUniqueID;
	static readonly Exception ThrownException;
	static readonly Dictionary<string, List<string>> Traits;

	static Xunit2MessageAdapterTests()
	{
		try
		{
			throw new DivideByZeroException();
		}
		catch (Exception ex)
		{
			ThrownException = ex;
		}

		Traits = new Dictionary<string, List<string>>
		{
			{ "key1", new List<string> { "value1a", "value1b" } },
			{ "key2", new List<string> { "value2" } },
			{ "key3", new List<string>() }
		};

		TestAssembly = Xunit2Mocks.TestAssembly("testAssembly.dll", "xunit.runner.json");
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(
			TestAssembly.Assembly.AssemblyPath,
			TestAssembly.ConfigFileName
		);

		TestCollectionDefinition = Xunit2Mocks.TypeInfo();
		TestCollection = Xunit2Mocks.TestCollection(TestAssembly, TestCollectionDefinition, "test-collection-display-name");
		TestCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
			TestAssemblyUniqueID,
			TestCollection.DisplayName,
			TestCollectionDefinition.Name
		);

		TestClassType = Xunit2Mocks.TypeInfo("TestNamespace.TestClass+EmbeddedClass");
		TestClass = Xunit2Mocks.TestClass(TestCollection, TestClassType);
		TestClassUniqueID = UniqueIDGenerator.ForTestClass(
			TestCollectionUniqueID,
			TestClass.Class.Name
		);

		TestMethod = Xunit2Mocks.TestMethod(TestClass, "MyTestMethod");
		TestMethodUniqueID = UniqueIDGenerator.ForTestMethod(
			TestClassUniqueID,
			TestMethod.Method.Name
		);

		TestCase = Xunit2Mocks.TestCase(TestMethod, "test-case-display-name", "skip-reason", "source-file", 2112, Traits, "test-case-id");
		TestCaseUniqueID = TestCase.UniqueID;

		Test = Xunit2Mocks.Test(TestCase, "test-display-name");
		TestUniqueID = UniqueIDGenerator.ForTest(TestCaseUniqueID, 0);
	}

	static void AssertErrorMetadata(
		IErrorMetadata metadata,
		Exception ex)
	{
		var (exceptionTypes, messages, _, exceptionParentIndices, _) = ExceptionUtility.ExtractMetadata(ex);

		Assert.Equal(exceptionParentIndices, metadata.ExceptionParentIndices);
		Assert.Equal(exceptionTypes, metadata.ExceptionTypes, StringComparer.Ordinal);
		Assert.Equal(messages, metadata.Messages, StringComparer.Ordinal);
	}

	public class BeforeAfterTestAttributeTests
	{
		[Fact]
		public void AfterTestFinished()
		{
			var v2Message = Xunit2Mocks.AfterTestFinished(Test, BeforeAfterAttributeName);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IAfterTestFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(BeforeAfterAttributeName, v3Message.AttributeName);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void AfterTestStarting()
		{
			var v2Message = Xunit2Mocks.AfterTestStarting(Test, BeforeAfterAttributeName);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IAfterTestStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(BeforeAfterAttributeName, v3Message.AttributeName);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void BeforeTestFinished()
		{
			var v2Message = Xunit2Mocks.BeforeTestFinished(Test, BeforeAfterAttributeName);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IBeforeTestFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(BeforeAfterAttributeName, v3Message.AttributeName);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void BeforeTestStarting()
		{
			var v2Message = Xunit2Mocks.BeforeTestStarting(Test, BeforeAfterAttributeName);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IBeforeTestStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(BeforeAfterAttributeName, v3Message.AttributeName);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}
	}

	public class DiagnosticMessageTests
	{
		[Fact]
		public void DiagnosticMessage()
		{
			var v2Message = Xunit2Mocks.DiagnosticMessage("Hello, world!");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IDiagnosticMessage>(adapted);
			Assert.Equal("Hello, world!", v3Message.Message);
		}
	}

	public class DiscoveryTests
	{
		[Fact]
		public void TestCaseDiscoveryMessage()
		{
			var v2Message = Xunit2Mocks.TestCaseDiscoveryMessage(TestCase);
			var discoverer = Substitute.For<ITestFrameworkDiscoverer>();
			discoverer.Serialize(TestCase).Returns("abc123");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID, discoverer);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCaseDiscovered>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal("abc123", v3Message.Serialization);
			Assert.Equal("skip-reason", v3Message.SkipReason);
			Assert.Equal("source-file", v3Message.SourceFilePath);
			Assert.Equal(2112, v3Message.SourceLineNumber);
			Assert.Equal("test-case-display-name", v3Message.TestCaseDisplayName);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Collection(
				v3Message.Traits.OrderBy(kvp => kvp.Key),
				trait =>
				{
					Assert.Equal("key1", trait.Key);
					Assert.Equal(["value1a", "value1b"], trait.Value);
				},
				trait =>
				{
					Assert.Equal("key2", trait.Key);
					Assert.Equal(["value2"], trait.Value);
				},
				trait =>
				{
					Assert.Equal("key3", trait.Key);
					Assert.Empty(trait.Value);
				}
			);
		}
	}

	public class FatalErrorTests
	{
		[Fact]
		public void ErrorMessage()
		{
			var v2Message = Xunit2Mocks.ErrorMessage(ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.IErrorMessage>(adapted);
			AssertErrorMetadata(v3Message, ThrownException);
		}
	}

	public class TestAssemblyTests
	{
		[Fact]
		public void TestAssemblyCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestAssemblyCleanupFailure(TestAssembly, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestAssemblyCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestAssemblyFinished()
		{
			var v2Message = Xunit2Mocks.TestAssemblyFinished(
				TestAssembly,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 6,
				executionTime: 123.4567m
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestAssemblyFinished>(adapted);
			Assert.NotEmpty(v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(0, v3Message.TestsNotRun);
			Assert.Equal(6, v3Message.TestsSkipped);
			Assert.Equal(2112, v3Message.TestsTotal);
		}

		[Fact]
		public void TestAssemblyStarting()
		{
			var assemblyPath =
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? @"C:\Users\bradwilson\assembly.dll"
					: "/home/bradwilson/assembly.dll";
			var configFilePath =
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
					? @"C:\Users\bradwilson\xunit.runner.json"
					: "/home/bradwilson/xunit.runner.json";

			var testAssembly = Xunit2Mocks.TestAssembly(assemblyPath, configFilePath, "target-framework");
			var v2Message = Xunit2Mocks.TestAssemblyStarting(
				testAssembly,
				new DateTime(2020, 11, 3, 17, 55, 0, DateTimeKind.Utc),
				"test-environment",
				"test-framework"
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestAssemblyStarting>(adapted);
			Assert.Equal(Path.GetFileNameWithoutExtension(assemblyPath), v3Message.AssemblyName);
			Assert.Equal(assemblyPath, v3Message.AssemblyPath);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(configFilePath, v3Message.ConfigFilePath);
			Assert.Equal(new DateTimeOffset(2020, 11, 3, 17, 55, 0, TimeSpan.Zero), v3Message.StartTime);
			Assert.Equal("target-framework", v3Message.TargetFramework);
			Assert.Equal("test-environment", v3Message.TestEnvironment);
			Assert.Equal("test-framework", v3Message.TestFrameworkDisplayName);
		}
	}

	public class TestCaseTests
	{
		[Fact]
		public void TestCaseCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestCaseCleanupFailure(TestCase, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCaseCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestCaseFinished()
		{
			var v2Message = Xunit2Mocks.TestCaseFinished(
				TestCase,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCaseFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(0, v3Message.TestsNotRun);
			Assert.Equal(404, v3Message.TestsSkipped);
			Assert.Equal(2112, v3Message.TestsTotal);
		}

		[Fact]
		public void TestCaseStarting()
		{
			var v2Message = Xunit2Mocks.TestCaseStarting(TestCase);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCaseStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal("skip-reason", v3Message.SkipReason);
			Assert.Equal("source-file", v3Message.SourceFilePath);
			Assert.Equal(2112, v3Message.SourceLineNumber);
			Assert.Equal("test-case-display-name", v3Message.TestCaseDisplayName);
			Assert.Equal("test-case-id", v3Message.TestCaseUniqueID);
			Assert.Equal("TestNamespace.TestClass+EmbeddedClass", v3Message.TestClassName);
			Assert.Equal("TestNamespace", v3Message.TestClassNamespace);
			Assert.Equal("TestClass+EmbeddedClass", v3Message.TestClassSimpleName);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal("MyTestMethod", v3Message.TestMethodName);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Collection(
				v3Message.Traits.OrderBy(kvp => kvp.Key),
				trait =>
				{
					Assert.Equal("key1", trait.Key);
					Assert.Equal(["value1a", "value1b"], trait.Value);
				},
				trait =>
				{
					Assert.Equal("key2", trait.Key);
					Assert.Equal(["value2"], trait.Value);
				},
				trait =>
				{
					Assert.Equal("key3", trait.Key);
					Assert.Empty(trait.Value);
				}
			);
		}
	}

	public class TestClassTests
	{
		[Fact]
		public void TestClassCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestClassCleanupFailure(TestClass, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestClassFinished()
		{
			var v2Message = Xunit2Mocks.TestClassFinished(
				TestClass,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(0, v3Message.TestsNotRun);
			Assert.Equal(404, v3Message.TestsSkipped);
			Assert.Equal(2112, v3Message.TestsTotal);
		}

		[Fact]
		public void TestClassStarting()
		{
			var v2Message = Xunit2Mocks.TestClassStarting(TestClass);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(v2Message.TestClass.Class.Name, v3Message.TestClassName);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
		}
	}

	public class TestCollectionTests
	{
		[Fact]
		public void TestCollectionCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestCollectionCleanupFailure(TestCollection, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCollectionCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestCollectionFinished()
		{
			var v2Message = Xunit2Mocks.TestCollectionFinished(
				TestCollection,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCollectionFinished>(adapted);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(0, v3Message.TestsNotRun);
			Assert.Equal(404, v3Message.TestsSkipped);
			Assert.Equal(2112, v3Message.TestsTotal);
		}

		[Fact]
		public void TestCollectionStarting()
		{
			var v2Message = Xunit2Mocks.TestCollectionStarting(TestCollection);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCollectionStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionDefinition.Name, v3Message.TestCollectionClassName);
			Assert.Equal(TestCollection.DisplayName, v3Message.TestCollectionDisplayName);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
		}
	}

	public class TestMethodTests
	{
		[Fact]
		public void TestMethodCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestMethodCleanupFailure(TestMethod, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestMethodCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestMethodFinished()
		{
			var v2Message = Xunit2Mocks.TestMethodFinished(
				TestMethod,
				testsRun: 2112,
				testsFailed: 42,
				testsSkipped: 404,
				executionTime: 123.4567m
			);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestMethodFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(0, v3Message.TestsNotRun);
			Assert.Equal(404, v3Message.TestsSkipped);
			Assert.Equal(2112, v3Message.TestsTotal);
		}

		[Fact]
		public void TestMethodStarting()
		{
			var v2Message = Xunit2Mocks.TestMethodStarting(TestMethod);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestMethodStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethod.Method.Name, v3Message.MethodName);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
		}
	}

	public class TestTests
	{
		[Fact]
		public void TestClassConstructionFinished()
		{
			var v2Message = Xunit2Mocks.TestClassConstructionFinished(Test);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassConstructionFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestClassConstructionStarting()
		{
			var v2Message = Xunit2Mocks.TestClassConstructionStarting(Test);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassConstructionStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestClassDisposeFinished()
		{
			var v2Message = Xunit2Mocks.TestClassDisposeFinished(Test);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassDisposeFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestClassDisposeStarting()
		{
			var v2Message = Xunit2Mocks.TestClassDisposeStarting(Test);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestClassDisposeStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestCleanupFailure()
		{
			var v2Message = Xunit2Mocks.TestCleanupFailure(Test, ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestCleanupFailure>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestFinished()
		{
			var v2Message = Xunit2Mocks.TestFinished(Test, 123.4567m, "abc123");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal("abc123", v3Message.Output);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestFailed()
		{
			var v2Message = Xunit2Mocks.TestFailed(Test, 123.4567m, "abc123", ThrownException);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestFailed>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(FailureCause.Assertion, v3Message.Cause);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal("abc123", v3Message.Output);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
			AssertErrorMetadata(v3Message, ThrownException);
		}

		[Fact]
		public void TestOutput()
		{
			var v2Message = Xunit2Mocks.TestOutput(Test, "this is my test output");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestOutput>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal("this is my test output", v3Message.Output);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestPassed()
		{
			var v2Message = Xunit2Mocks.TestPassed(Test, 123.4567m, "abc123");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestPassed>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal("abc123", v3Message.Output);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestSkipped()
		{
			var v2Message = Xunit2Mocks.TestSkipped(Test, "I am not running");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestSkipped>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(0m, v3Message.ExecutionTime);  // Statically skipped tests always take no runtime
			Assert.Empty(v3Message.Output);
			Assert.Equal("I am not running", v3Message.Reason);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}

		[Fact]
		public void TestStarting()
		{
			var v2Message = Xunit2Mocks.TestStarting(Test);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsAssignableFrom<Xunit.Sdk.ITestStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal("test-display-name", v3Message.TestDisplayName);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestUniqueID, v3Message.TestUniqueID);
		}
	}
}
