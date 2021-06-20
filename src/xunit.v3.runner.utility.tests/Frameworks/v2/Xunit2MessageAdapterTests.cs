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
using Xunit.v3;

public class Xunit2MessageAdapterTests
{
	static readonly string BeforeAfterAttributeName = "MyNamespace.MyBeforeAfterAttribute";
	static readonly ITest Test;
	static readonly ITestAssembly TestAssembly;
	static readonly string TestAssemblyUniqueID;
	static readonly ITestCase TestCase;
	static readonly string TestCaseUniqueID;
	static readonly ITestClass TestClass;
	static readonly ITypeInfo TestClassType;
	static readonly string? TestClassUniqueID;
	static readonly ITestCollection TestCollection;
	static readonly ITypeInfo TestCollectionDefinition;
	static readonly string TestCollectionUniqueID;
	static readonly ITestMethod TestMethod;
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
			TestAssembly.Assembly.Name,
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

		TestClassType = Xunit2Mocks.TypeInfo();
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
		_IErrorMetadata metadata,
		Exception ex)
	{
		var convertedMetadata = ExceptionUtility.ExtractMetadata(ex);
		Assert.Equal(convertedMetadata.ExceptionParentIndices, metadata.ExceptionParentIndices);
		Assert.Equal(convertedMetadata.ExceptionTypes, metadata.ExceptionTypes, StringComparer.Ordinal);
		Assert.Equal(convertedMetadata.Messages, metadata.Messages, StringComparer.Ordinal);
	}

	public class BeforeAfterTestAttributeTests
	{
		[Fact]
		public void AfterTestFinished()
		{
			var v2Message = Xunit2Mocks.AfterTestFinished(Test, BeforeAfterAttributeName);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_AfterTestFinished>(adapted);
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

			var v3Message = Assert.IsType<_AfterTestStarting>(adapted);
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

			var v3Message = Assert.IsType<_BeforeTestFinished>(adapted);
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

			var v3Message = Assert.IsType<_BeforeTestStarting>(adapted);
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

			var v3Message = Assert.IsType<_DiagnosticMessage>(adapted);
			Assert.Equal("Hello, world!", v3Message.Message);
		}
	}

	public class DiscoveryTests
	{
		[Fact]
		public void DiscoveryComplete()
		{
			var v2Message = Xunit2Mocks.DiscoveryCompleteMessage();
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_DiscoveryComplete>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
		}

		[Fact]
		public void TestCaseDiscoveryMessage()
		{
			var v2Message = Xunit2Mocks.TestCaseDiscoveryMessage(TestCase);
			var discoverer = Substitute.For<ITestFrameworkDiscoverer>();
			discoverer.Serialize(TestCase).Returns("abc123");
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID, discoverer);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestCaseDiscovered>(adapted);
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
					Assert.Equal(new[] { "value1a", "value1b" }, trait.Value);
				},
				trait =>
				{
					Assert.Equal("key2", trait.Key);
					Assert.Equal(new[] { "value2" }, trait.Value);
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

			var v3Message = Assert.IsType<_ErrorMessage>(adapted);
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

			var v3Message = Assert.IsType<_TestAssemblyCleanupFailure>(adapted);
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

			var v3Message = Assert.IsType<_TestAssemblyFinished>(adapted);
			Assert.NotEmpty(v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(6, v3Message.TestsSkipped);
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

			var v3Message = Assert.IsType<_TestAssemblyStarting>(adapted);
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

			var v3Message = Assert.IsType<_TestCaseCleanupFailure>(adapted);
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

			var v3Message = Assert.IsType<_TestCaseFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(404, v3Message.TestsSkipped);
		}

		[Fact]
		public void TestCaseStarting()
		{
			var v2Message = Xunit2Mocks.TestCaseStarting(TestCase);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestCaseStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal("skip-reason", v3Message.SkipReason);
			Assert.Equal("source-file", v3Message.SourceFilePath);
			Assert.Equal(2112, v3Message.SourceLineNumber);
			Assert.Equal("test-case-display-name", v3Message.TestCaseDisplayName);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(TestCaseUniqueID, v3Message.TestCaseUniqueID);
			Assert.Collection(
				v3Message.Traits.OrderBy(kvp => kvp.Key),
				trait =>
				{
					Assert.Equal("key1", trait.Key);
					Assert.Equal(new[] { "value1a", "value1b" }, trait.Value);
				},
				trait =>
				{
					Assert.Equal("key2", trait.Key);
					Assert.Equal(new[] { "value2" }, trait.Value);
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

			var v3Message = Assert.IsType<_TestClassCleanupFailure>(adapted);
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
			var v2Message = Xunit2Mocks.TestClassStarting(TestClass);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestClassStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(v2Message.TestClass.Class.Name, v3Message.TestClass);
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

			var v3Message = Assert.IsType<_TestCollectionCleanupFailure>(adapted);
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
			var v2Message = Xunit2Mocks.TestCollectionStarting(TestCollection);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestCollectionStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestCollectionDefinition.Name, v3Message.TestCollectionClass);
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

			var v3Message = Assert.IsType<_TestMethodCleanupFailure>(adapted);
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

			var v3Message = Assert.IsType<_TestMethodFinished>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(123.4567m, v3Message.ExecutionTime);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethodUniqueID, v3Message.TestMethodUniqueID);
			Assert.Equal(42, v3Message.TestsFailed);
			Assert.Equal(2112, v3Message.TestsRun);
			Assert.Equal(404, v3Message.TestsSkipped);
		}

		[Fact]
		public void TestMethodStarting()
		{
			var v2Message = Xunit2Mocks.TestMethodStarting(TestMethod);
			var v2Adapter = new Xunit2MessageAdapter(TestAssemblyUniqueID);

			var adapted = v2Adapter.Adapt(v2Message);

			var v3Message = Assert.IsType<_TestMethodStarting>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
			Assert.Equal(TestClassUniqueID, v3Message.TestClassUniqueID);
			Assert.Equal(TestCollectionUniqueID, v3Message.TestCollectionUniqueID);
			Assert.Equal(TestMethod.Method.Name, v3Message.TestMethod);
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

			var v3Message = Assert.IsType<_TestClassConstructionFinished>(adapted);
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

			var v3Message = Assert.IsType<_TestClassConstructionStarting>(adapted);
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

			var v3Message = Assert.IsType<_TestClassDisposeFinished>(adapted);
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

			var v3Message = Assert.IsType<_TestClassDisposeStarting>(adapted);
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

			var v3Message = Assert.IsType<_TestCleanupFailure>(adapted);
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

			var v3Message = Assert.IsType<_TestFinished>(adapted);
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

			var v3Message = Assert.IsType<_TestFailed>(adapted);
			Assert.Equal(TestAssemblyUniqueID, v3Message.AssemblyUniqueID);
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

			var v3Message = Assert.IsType<_TestOutput>(adapted);
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

			var v3Message = Assert.IsType<_TestPassed>(adapted);
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

			var v3Message = Assert.IsType<_TestSkipped>(adapted);
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

			var v3Message = Assert.IsType<_TestStarting>(adapted);
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
