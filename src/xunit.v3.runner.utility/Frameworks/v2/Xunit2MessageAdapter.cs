using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Runner.v2;

/// <summary>
/// A class which adapts xUnit.net v2 messages to xUnit.net v3 messages.
/// </summary>
/// <param name="assemblyUniqueID">The unique ID of the assembly these message belong to</param>
/// <param name="discoverer">The discoverer used to serialize test cases</param>
public class Xunit2MessageAdapter(
	string? assemblyUniqueID = null,
	ITestFrameworkDiscoverer? discoverer = null)
{
	readonly string assemblyUniqueID = assemblyUniqueID ?? "<no assembly>";
	readonly ITestFrameworkDiscoverer? discoverer = discoverer;
	readonly Dictionary<ITestCase, Dictionary<ITest, string>> testUniqueIDsByTestCase = [];

	/// <summary>
	/// Adapts a v2 message to a v3 message.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown if the message is not of a known type.</exception>
	public Sdk.IMessageSinkMessage Adapt(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes = null) =>
			// Discovery
			TryConvert<IDiagnosticMessage>(message, messageTypes, AdaptDiagnosticMessage) ??
			TryConvert<IDiscoveryCompleteMessage>(message, messageTypes, AdaptDiscoveryCompleteMessage) ??
			TryConvert<ITestCaseDiscoveryMessage>(message, messageTypes, AdaptTestCaseDiscoveryMessage) ??

			// Fatal error
			TryConvert<IErrorMessage>(message, messageTypes, AdaptErrorMessage) ??

			// Test assembly
			TryConvert<ITestAssemblyCleanupFailure>(message, messageTypes, AdaptTestAssemblyCleanupFailure) ??
			TryConvert<ITestAssemblyFinished>(message, messageTypes, AdaptTestAssemblyFinished) ??
			TryConvert<ITestAssemblyStarting>(message, messageTypes, AdaptTestAssemblyStarting) ??

			// Test case
			TryConvert<ITestCaseCleanupFailure>(message, messageTypes, AdaptTestCaseCleanupFailure) ??
			TryConvert<ITestCaseFinished>(message, messageTypes, AdaptTestCaseFinished) ??
			TryConvert<ITestCaseStarting>(message, messageTypes, AdaptTestCaseStarting) ??

			// Test class
			TryConvert<ITestClassCleanupFailure>(message, messageTypes, AdaptTestClassCleanupFailure) ??
			TryConvert<ITestClassFinished>(message, messageTypes, AdaptTestClassFinished) ??
			TryConvert<ITestClassStarting>(message, messageTypes, AdaptTestClassStarting) ??

			// Test collection
			TryConvert<ITestCollectionCleanupFailure>(message, messageTypes, AdaptTestCollectionCleanupFailure) ??
			TryConvert<ITestCollectionFinished>(message, messageTypes, AdaptTestCollectionFinished) ??
			TryConvert<ITestCollectionStarting>(message, messageTypes, AdaptTestCollectionStarting) ??

			// Test method
			TryConvert<ITestMethodCleanupFailure>(message, messageTypes, AdaptTestMethodCleanupFailure) ??
			TryConvert<ITestMethodFinished>(message, messageTypes, AdaptTestMethodFinished) ??
			TryConvert<ITestMethodStarting>(message, messageTypes, AdaptTestMethodStarting) ??

			// Test
			TryConvert<IAfterTestFinished>(message, messageTypes, AdaptAfterTestFinished) ??
			TryConvert<IAfterTestStarting>(message, messageTypes, AdaptAfterTestStarting) ??
			TryConvert<IBeforeTestFinished>(message, messageTypes, AdaptBeforeTestFinished) ??
			TryConvert<IBeforeTestStarting>(message, messageTypes, AdaptBeforeTestStarting) ??
			TryConvert<ITestClassConstructionFinished>(message, messageTypes, AdaptTestClassConstructionFinished) ??
			TryConvert<ITestClassConstructionStarting>(message, messageTypes, AdaptTestClassConstructionStarting) ??
			TryConvert<ITestClassDisposeFinished>(message, messageTypes, AdaptTestClassDisposeFinished) ??
			TryConvert<ITestClassDisposeStarting>(message, messageTypes, AdaptTestClassDisposeStarting) ??
			TryConvert<ITestCleanupFailure>(message, messageTypes, AdaptTestCleanupFailure) ??
			TryConvert<ITestFailed>(message, messageTypes, AdaptTestFailed) ??
			TryConvert<ITestFinished>(message, messageTypes, AdaptTestFinished) ??
			TryConvert<ITestOutput>(message, messageTypes, AdaptTestOutput) ??
			TryConvert<ITestPassed>(message, messageTypes, AdaptTestPassed) ??
			TryConvert<ITestSkipped>(message, messageTypes, AdaptTestSkipped) ??
			TryConvert<ITestStarting>(message, messageTypes, AdaptTestStarting) ??

			// Unknown
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown message type '{0}'", message.GetType().FullName), nameof(message));

	Sdk.IAfterTestFinished AdaptAfterTestFinished(IAfterTestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new AfterTestFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			AttributeName = message.AttributeName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.IAfterTestStarting AdaptAfterTestStarting(IAfterTestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new AfterTestStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			AttributeName = message.AttributeName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.IBeforeTestFinished AdaptBeforeTestFinished(IBeforeTestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new BeforeTestFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			AttributeName = message.AttributeName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.IBeforeTestStarting AdaptBeforeTestStarting(IBeforeTestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new BeforeTestStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			AttributeName = message.AttributeName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.IDiagnosticMessage AdaptDiagnosticMessage(IDiagnosticMessage message) =>
		new DiagnosticMessage(message.Message);

	Sdk.IDiscoveryComplete AdaptDiscoveryCompleteMessage(IDiscoveryCompleteMessage message) =>
		new DiscoveryComplete()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCasesToRun = 0,  // TODO: Do we know this number?
		};

	Sdk.IErrorMessage AdaptErrorMessage(IErrorMessage message) =>
		new ErrorMessage()
		{
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
		};

	Sdk.ITestAssemblyCleanupFailure AdaptTestAssemblyCleanupFailure(ITestAssemblyCleanupFailure message) =>
		new TestAssemblyCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
		};

	Sdk.ITestAssemblyFinished AdaptTestAssemblyFinished(ITestAssemblyFinished message) =>
		new TestAssemblyFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.Now,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};

	Sdk.ITestAssemblyStarting AdaptTestAssemblyStarting(ITestAssemblyStarting message)
	{
		var targetFrameworkAttribute = message.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
		var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();

		return new TestAssemblyStarting()
		{
			AssemblyName = message.TestAssembly.Assembly.Name,
			AssemblyPath = message.TestAssembly.Assembly.AssemblyPath,
			AssemblyUniqueID = assemblyUniqueID,
			ConfigFilePath = message.TestAssembly.ConfigFileName,
			Seed = null,
			StartTime = message.StartTime,
			TargetFramework = targetFramework,
			TestEnvironment = message.TestEnvironment,
			TestFrameworkDisplayName = message.TestFrameworkDisplayName,
			Traits = Xunit2.EmptyV3Traits,
		};
	}

	Sdk.ITestCaseCleanupFailure AdaptTestCaseCleanupFailure(ITestCaseCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new TestCaseCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCaseUniqueID = message.TestCase.UniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
		};
	}

	Sdk.ITestCaseDiscovered AdaptTestCaseDiscoveryMessage(ITestCaseDiscoveryMessage message)
	{
		var testCase = message.TestCase;

		// Clean up the cache
		lock (testUniqueIDsByTestCase)
			testUniqueIDsByTestCase.Remove(testCase);

		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = testCase.UniqueID;

		var typeName = testCase.TestMethod?.TestClass.Class.Name;
		var lastDotIdx = typeName?.LastIndexOf('.') ?? -1;
		var @namespace = typeName is not null && lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
		var simpleName = typeName is not null && lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;

		if (typeName is not null)
		{
			var namespaceIdx = typeName.LastIndexOf('.');
			if (namespaceIdx > -1)
				@namespace = typeName.Substring(0, namespaceIdx);
		}

		return new TestCaseDiscovered
		{
			AssemblyUniqueID = assemblyUniqueID,
			Explicit = false,
			Serialization = discoverer?.Serialize(testCase) ?? string.Empty,
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceInformation?.FileName,
			SourceLineNumber = testCase.SourceInformation?.LineNumber,
			TestCaseDisplayName = testCase.DisplayName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassMetadataToken = null,
			TestClassName = typeName,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = testClassUniqueID,
			TestMethodMetadataToken = null,
			TestMethodName = testCase.TestMethod?.Method.Name,
			TestMethodParameterTypesVSTest = null,
			TestMethodReturnTypeVSTest = null,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = testCase.Traits.ToReadOnly(),
		};
	}

	Sdk.ITestCaseFinished AdaptTestCaseFinished(ITestCaseFinished message)
	{
		// Clean up the cache
		lock (testUniqueIDsByTestCase)
			testUniqueIDsByTestCase.Remove(message.TestCase);

		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;

		return new TestCaseFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};
	}

	Sdk.ITestCaseStarting AdaptTestCaseStarting(ITestCaseStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		var typeName = message.TestCase.TestMethod?.TestClass.Class.Name;
		var lastDotIdx = typeName?.LastIndexOf('.') ?? -1;
		var @namespace = typeName is not null && lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
		var simpleName = typeName is not null && lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;

		return new TestCaseStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Explicit = false,
			SkipReason = message.TestCase.SkipReason,
			SourceFilePath = message.TestCase.SourceInformation?.FileName,
			SourceLineNumber = message.TestCase.SourceInformation?.LineNumber,
			TestCaseDisplayName = message.TestCase.DisplayName,
			TestCaseUniqueID = message.TestCase.UniqueID,
			TestClassMetadataToken = null,
			TestClassName = typeName,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestMethodMetadataToken = null,
			TestMethodName = message.TestCase.TestMethod?.Method.Name,
			TestMethodParameterTypesVSTest = null,
			TestMethodReturnTypeVSTest = null,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = message.TestCase.Traits.ToReadOnly(),
		};
	}

	Sdk.ITestClassCleanupFailure AdaptTestClassCleanupFailure(ITestClassCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		return new TestClassCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
		};
	}

	Sdk.ITestClassConstructionFinished AdaptTestClassConstructionFinished(ITestClassConstructionFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestClassConstructionFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestClassConstructionStarting AdaptTestClassConstructionStarting(ITestClassConstructionStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestClassConstructionStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestClassDisposeFinished AdaptTestClassDisposeFinished(ITestClassDisposeFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestClassDisposeFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestClassDisposeStarting AdaptTestClassDisposeStarting(ITestClassDisposeStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestClassDisposeStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestClassFinished AdaptTestClassFinished(ITestClassFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		return new TestClassFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};
	}

	Sdk.ITestClassStarting AdaptTestClassStarting(ITestClassStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		var typeName = message.TestClass.Class.Name;
		var lastDotIdx = typeName.LastIndexOf('.');
		var @namespace = lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
		var simpleName = lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;

		return new TestClassStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestClassName = message.TestClass.Class.Name,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			Traits = Xunit2.EmptyV3Traits,
		};
	}

	Sdk.ITestCleanupFailure AdaptTestCleanupFailure(ITestCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestCollectionCleanupFailure AdaptTestCollectionCleanupFailure(ITestCollectionCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new TestCollectionCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
		};
	}

	Sdk.ITestCollectionFinished AdaptTestCollectionFinished(ITestCollectionFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new TestCollectionFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};
	}

	Sdk.ITestCollectionStarting AdaptTestCollectionStarting(ITestCollectionStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new TestCollectionStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCollectionClassName = message.TestCollection.CollectionDefinition?.Name,
			TestCollectionDisplayName = message.TestCollection.DisplayName,
			TestCollectionUniqueID = testCollectionUniqueID,
			Traits = Xunit2.EmptyV3Traits,
		};
	}

	Sdk.ITestFailed AdaptTestFailed(ITestFailed message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestFailed()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Cause = Sdk.FailureCause.Assertion,  // We don't know in v2, so we just assume it's an assertion failure
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Messages = message.Messages,
			Output = message.Output,
			StackTraces = message.StackTraces,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = null,
		};
	}

	Sdk.ITestFinished AdaptTestFinished(ITestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Attachments = Xunit2.EmptyAttachments,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = message.Output,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = null,
		};
	}

	Sdk.ITestMethodCleanupFailure AdaptTestMethodCleanupFailure(ITestMethodCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new TestMethodCleanupFailure()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
		};
	}

	Sdk.ITestMethodFinished AdaptTestMethodFinished(ITestMethodFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new TestMethodFinished()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};
	}

	Sdk.ITestMethodStarting AdaptTestMethodStarting(ITestMethodStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new TestMethodStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			MethodName = message.TestMethod.Method.Name,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = Xunit2.EmptyV3Traits,
		};
	}

	Sdk.ITestOutput AdaptTestOutput(ITestOutput message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestOutput()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Output = message.Output,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	Sdk.ITestPassed AdaptTestPassed(ITestPassed message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestPassed()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = message.Output,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = null,
		};
	}

	Sdk.ITestSkipped AdaptTestSkipped(ITestSkipped message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestSkipped()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.UtcNow,
			Output = message.Output,
			Reason = message.Reason,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Warnings = null,
		};
	}

	Sdk.ITestStarting AdaptTestStarting(ITestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new TestStarting()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Explicit = false,
			StartTime = DateTimeOffset.UtcNow,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestDisplayName = message.Test.DisplayName,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
			Timeout = 0,
			Traits = message.TestCase.Traits.ToReadOnly(),
		};
	}

	static Sdk.IMessageSinkMessage? TryConvert<TMessage>(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes,
		Func<TMessage, Sdk.IMessageSinkMessage> converter)
			where TMessage : class, IMessageSinkMessage
	{
		Guard.ArgumentNotNull(message);

		var castMessage = messageTypes is null || messageTypes.Contains(typeof(TMessage).FullName!) ? message as TMessage : null;

		return
			castMessage is not null
				? converter(castMessage)
				: null;
	}

	string UniqueIDForTest(
		string testCaseUniqueID,
		ITest test)
	{
		lock (testUniqueIDsByTestCase)
		{
			var uniqueIDLookup = testUniqueIDsByTestCase.AddOrGet(test.TestCase, () => []);
			if (!uniqueIDLookup.TryGetValue(test, out var result))
			{
				var testIndex = uniqueIDLookup.Count;
				result = Sdk.UniqueIDGenerator.ForTest(testCaseUniqueID, testIndex);
				uniqueIDLookup[test] = result;
			}

			return result;
		}
	}

	static string? UniqueIDForTestClass(
		string testCollectionUniqueID,
		ITestClass? testClass) =>
			Sdk.UniqueIDGenerator.ForTestClass(testCollectionUniqueID, testClass?.Class?.Name);

	static string UniqueIDForTestCollection(
		string assemblyUniqueID,
		ITestCollection testCollection) =>
			Sdk.UniqueIDGenerator.ForTestCollection(assemblyUniqueID, testCollection.DisplayName, testCollection.CollectionDefinition?.Name);

	static string? UniqueIDForTestMethod(
		string? classUniqueID,
		ITestMethod testMethod) =>
			Sdk.UniqueIDGenerator.ForTestMethod(classUniqueID, testMethod.Method.Name);
}
