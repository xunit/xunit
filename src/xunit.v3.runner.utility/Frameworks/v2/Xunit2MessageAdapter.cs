using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Versioning;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2;

/// <summary>
/// A class which adapts xUnit.net v2 messages to xUnit.net v3 messages.
/// </summary>
public class Xunit2MessageAdapter
{
	readonly string assemblyUniqueID;
	readonly ITestFrameworkDiscoverer? discoverer;
	readonly Dictionary<ITestCase, Dictionary<ITest, string>> testUniqueIDsByTestCase = new();

	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit2MessageAdapter"/> class.
	/// </summary>
	/// <param name="assemblyUniqueID">The unique ID of the assembly these message belong to</param>
	/// <param name="discoverer">The discoverer used to serialize test cases</param>
	public Xunit2MessageAdapter(
		string? assemblyUniqueID = null,
		ITestFrameworkDiscoverer? discoverer = null)
	{
		this.assemblyUniqueID = assemblyUniqueID ?? "<no assembly>";
		this.discoverer = discoverer;
	}

	/// <summary>
	/// Adapts a v2 message to a v3 message.
	/// </summary>
	/// <exception cref="ArgumentException">Thrown if the message is not of a known type.</exception>
	public _MessageSinkMessage Adapt(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes = null)
	{
		return
			TryConvert<IDiagnosticMessage>(message, messageTypes, AdaptDiagnosticMessage) ??

			TryConvert<IDiscoveryCompleteMessage>(message, messageTypes, AdaptDiscoveryCompleteMessage) ??
			TryConvert<ITestCaseDiscoveryMessage>(message, messageTypes, AdaptTestCaseDiscoveryMessage) ??

			TryConvert<IErrorMessage>(message, messageTypes, AdaptErrorMessage) ??

			TryConvert<ITestAssemblyCleanupFailure>(message, messageTypes, AdaptTestAssemblyCleanupFailure) ??
			TryConvert<ITestAssemblyFinished>(message, messageTypes, AdaptTestAssemblyFinished) ??
			TryConvert<ITestAssemblyStarting>(message, messageTypes, AdaptTestAssemblyStarting) ??

			TryConvert<ITestCaseCleanupFailure>(message, messageTypes, AdaptTestCaseCleanupFailure) ??
			TryConvert<ITestCaseFinished>(message, messageTypes, AdaptTestCaseFinished) ??
			TryConvert<ITestCaseStarting>(message, messageTypes, AdaptTestCaseStarting) ??

			TryConvert<ITestClassCleanupFailure>(message, messageTypes, AdaptTestClassCleanupFailure) ??
			TryConvert<ITestClassFinished>(message, messageTypes, AdaptTestClassFinished) ??
			TryConvert<ITestClassStarting>(message, messageTypes, AdaptTestClassStarting) ??

			TryConvert<ITestCollectionCleanupFailure>(message, messageTypes, AdaptTestCollectionCleanupFailure) ??
			TryConvert<ITestCollectionFinished>(message, messageTypes, AdaptTestCollectionFinished) ??
			TryConvert<ITestCollectionStarting>(message, messageTypes, AdaptTestCollectionStarting) ??

			TryConvert<ITestMethodCleanupFailure>(message, messageTypes, AdaptTestMethodCleanupFailure) ??
			TryConvert<ITestMethodFinished>(message, messageTypes, AdaptTestMethodFinished) ??
			TryConvert<ITestMethodStarting>(message, messageTypes, AdaptTestMethodStarting) ??

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

			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Unknown message type '{0}'", message.GetType().FullName), nameof(message));
	}

	_AfterTestFinished AdaptAfterTestFinished(IAfterTestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_AfterTestStarting AdaptAfterTestStarting(IAfterTestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_BeforeTestFinished AdaptBeforeTestFinished(IBeforeTestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_BeforeTestStarting AdaptBeforeTestStarting(IBeforeTestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_DiagnosticMessage AdaptDiagnosticMessage(IDiagnosticMessage message) =>
		new() { Message = message.Message };

	_DiscoveryComplete AdaptDiscoveryCompleteMessage(IDiscoveryCompleteMessage message) =>
		new() { AssemblyUniqueID = assemblyUniqueID };

	_ErrorMessage AdaptErrorMessage(IErrorMessage message) =>
		new()
		{
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
		};

	_TestAssemblyCleanupFailure AdaptTestAssemblyCleanupFailure(ITestAssemblyCleanupFailure message) =>
		new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
		};

	_TestAssemblyFinished AdaptTestAssemblyFinished(ITestAssemblyFinished message) =>
		new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			FinishTime = DateTimeOffset.Now,
			TestsFailed = message.TestsFailed,
			TestsNotRun = 0,
			TestsSkipped = message.TestsSkipped,
			TestsTotal = message.TestsRun,
		};

	_TestAssemblyStarting AdaptTestAssemblyStarting(ITestAssemblyStarting message)
	{
		var targetFrameworkAttribute = message.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
		var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();

		return new()
		{
			AssemblyName = message.TestAssembly.Assembly.Name,
			AssemblyPath = message.TestAssembly.Assembly.AssemblyPath,
			AssemblyUniqueID = assemblyUniqueID,
			ConfigFilePath = message.TestAssembly.ConfigFileName,
			StartTime = message.StartTime,
			TargetFramework = targetFramework,
			TestEnvironment = message.TestEnvironment,
			TestFrameworkDisplayName = message.TestFrameworkDisplayName,
		};
	}

	_TestCaseCleanupFailure AdaptTestCaseCleanupFailure(ITestCaseCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new()
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

	_TestCaseDiscovered AdaptTestCaseDiscoveryMessage(ITestCaseDiscoveryMessage message)
	{
		var testCase = message.TestCase;

		// Clean up the cache
		lock (testUniqueIDsByTestCase)
			testUniqueIDsByTestCase.Remove(testCase);

		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = testCase.UniqueID;

		string? @namespace = null;
		string? @class = null;

		var typeName = testCase.TestMethod?.TestClass.Class.Name;
		if (typeName is not null)
		{
			var namespaceIdx = typeName.LastIndexOf('.');
			if (namespaceIdx < 0)
				@class = typeName;
			else
			{
				@namespace = typeName.Substring(0, namespaceIdx);
				@class = typeName.Substring(namespaceIdx + 1);

				var innerClassIdx = @class.LastIndexOf('+');
				if (innerClassIdx >= 0)
					@class = @class.Substring(innerClassIdx + 1);
			}
		}

		var result = new _TestCaseDiscovered
		{
			AssemblyUniqueID = assemblyUniqueID,
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceInformation?.FileName,
			SourceLineNumber = testCase.SourceInformation?.LineNumber,
			TestCaseDisplayName = testCase.DisplayName,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassName = @class,
			TestClassNamespace = @namespace,
			TestClassNameWithNamespace = typeName,
			TestClassUniqueID = testClassUniqueID,
			TestMethodName = testCase.TestMethod?.Method.Name,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = testCase.Traits.ToReadOnly(),
		};

		if (discoverer is not null)
			result.Serialization = discoverer.Serialize(testCase);

		return result;
	}

	_TestCaseFinished AdaptTestCaseFinished(ITestCaseFinished message)
	{
		// Clean up the cache
		lock (testUniqueIDsByTestCase)
			testUniqueIDsByTestCase.Remove(message.TestCase);

		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;

		return new()
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

	_TestCaseStarting AdaptTestCaseStarting(ITestCaseStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		var testClassNameWithNamespace = message.TestCase.TestMethod?.TestClass.Class.Name;
		var lastDotIdx = testClassNameWithNamespace?.LastIndexOf('.') ?? -1;
		var testClassNamespace = lastDotIdx > -1 ? testClassNameWithNamespace!.Substring(0, lastDotIdx) : null;
		var testClassName = lastDotIdx > -1 ? testClassNameWithNamespace!.Substring(lastDotIdx + 1) : testClassNameWithNamespace;

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			SkipReason = message.TestCase.SkipReason,
			SourceFilePath = message.TestCase.SourceInformation?.FileName,
			SourceLineNumber = message.TestCase.SourceInformation?.LineNumber,
			TestCaseDisplayName = message.TestCase.DisplayName,
			TestCaseUniqueID = message.TestCase.UniqueID,
			TestClassName = testClassName,
			TestClassNamespace = testClassNamespace,
			TestClassNameWithNamespace = testClassNameWithNamespace,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestMethodName = message.TestCase.TestMethod?.Method.Name,
			TestMethodUniqueID = testMethodUniqueID,
			Traits = message.TestCase.Traits.ToReadOnly(),
		};
	}

	_TestClassCleanupFailure AdaptTestClassCleanupFailure(ITestClassCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		return new()
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

	_TestClassConstructionFinished AdaptTestClassConstructionFinished(ITestClassConstructionFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestClassConstructionStarting AdaptTestClassConstructionStarting(ITestClassConstructionStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestClassDisposeFinished AdaptTestClassDisposeFinished(ITestClassDisposeFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestClassDisposeStarting AdaptTestClassDisposeStarting(ITestClassDisposeStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestClassFinished AdaptTestClassFinished(ITestClassFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		return new()
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

	_TestClassStarting AdaptTestClassStarting(ITestClassStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestClass = message.TestClass.Class.Name,
			TestClassUniqueID = testClassUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
		};
	}

	_TestCleanupFailure AdaptTestCleanupFailure(ITestCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_TestCollectionCleanupFailure AdaptTestCollectionCleanupFailure(ITestCollectionCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			Messages = message.Messages,
			StackTraces = message.StackTraces,
			TestCollectionUniqueID = testCollectionUniqueID,
		};
	}

	_TestCollectionFinished AdaptTestCollectionFinished(ITestCollectionFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new()
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

	_TestCollectionStarting AdaptTestCollectionStarting(ITestCollectionStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCollectionClass = message.TestCollection.CollectionDefinition?.Name,
			TestCollectionDisplayName = message.TestCollection.DisplayName,
			TestCollectionUniqueID = testCollectionUniqueID,
		};
	}

	_TestFailed AdaptTestFailed(ITestFailed message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Cause = FailureCause.Assertion,  // We don't know in v2, so we just assume it's an assertion failure
			ExceptionParentIndices = message.ExceptionParentIndices,
			ExceptionTypes = message.ExceptionTypes,
			ExecutionTime = message.ExecutionTime,
			Messages = message.Messages,
			Output = message.Output,
			StackTraces = message.StackTraces,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestFinished AdaptTestFinished(ITestFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			Output = message.Output,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestMethodCleanupFailure AdaptTestMethodCleanupFailure(ITestMethodCleanupFailure message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new()
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

	_TestMethodFinished AdaptTestMethodFinished(ITestMethodFinished message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new()
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

	_TestMethodStarting AdaptTestMethodStarting(ITestMethodStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethod = message.TestMethod.Method.Name,
			TestMethodUniqueID = testMethodUniqueID,
		};
	}

	_TestOutput AdaptTestOutput(ITestOutput message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
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

	_TestPassed AdaptTestPassed(ITestPassed message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			Output = message.Output,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestSkipped AdaptTestSkipped(ITestSkipped message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			ExecutionTime = message.ExecutionTime,
			Output = message.Output,
			Reason = message.Reason,
			TestCaseUniqueID = testCaseUniqueID,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestClassUniqueID = testClassUniqueID,
			TestMethodUniqueID = testMethodUniqueID,
			TestUniqueID = testUniqueID,
		};
	}

	_TestStarting AdaptTestStarting(ITestStarting message)
	{
		var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
		var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
		var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
		var testCaseUniqueID = message.TestCase.UniqueID;
		var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

		return new()
		{
			AssemblyUniqueID = assemblyUniqueID,
			Explicit = false,
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

	static _MessageSinkMessage? TryConvert<TMessage>(
		IMessageSinkMessage message,
		HashSet<string>? messageTypes,
		Func<TMessage, _MessageSinkMessage> converter)
			where TMessage : class, IMessageSinkMessage
	{
		Guard.ArgumentNotNull(message);

		var castMessage = messageTypes is null || messageTypes.Contains(typeof(TMessage).FullName!) ? message as TMessage : null;
		if (castMessage is not null)
			return converter(castMessage);

		return null;
	}

	string UniqueIDForTest(
		string testCaseUniqueID,
		ITest test)
	{
		lock (testUniqueIDsByTestCase)
		{
			var uniqueIDLookup = testUniqueIDsByTestCase.GetOrAdd(test.TestCase, () => new Dictionary<ITest, string>());
			if (!uniqueIDLookup.TryGetValue(test, out var result))
			{
				var testIndex = uniqueIDLookup.Count;
				result = UniqueIDGenerator.ForTest(testCaseUniqueID, testIndex);
				uniqueIDLookup[test] = result;
			}

			return result;
		}
	}

	static string? UniqueIDForTestClass(
		string testCollectionUniqueID,
		ITestClass? testClass) =>
			UniqueIDGenerator.ForTestClass(testCollectionUniqueID, testClass?.Class?.Name);

	static string UniqueIDForTestCollection(
		string assemblyUniqueID,
		ITestCollection testCollection) =>
			UniqueIDGenerator.ForTestCollection(assemblyUniqueID, testCollection.DisplayName, testCollection.CollectionDefinition?.Name);

	static string? UniqueIDForTestMethod(
		string? classUniqueID,
		ITestMethod testMethod) =>
			UniqueIDGenerator.ForTestMethod(classUniqueID, testMethod.Method.Name);
}
