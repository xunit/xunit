using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// This class adapts xUnit.net v2 messages to xUnit.net v3 messages.
	/// </summary>
	public static class Xunit2MessageAdapter
	{
		static readonly Dictionary<ITestCase, Dictionary<ITest, string>> testUniqueIDsByTestCase = new Dictionary<ITestCase, Dictionary<ITest, string>>();

		/// <summary>
		/// Adapts <see cref="IMessageSinkMessage"/> to <see cref="_MessageSinkMessage"/>.
		/// </summary>
		// TODO: This has the wrong return type so that we can do fall-through, until
		// we're finished moving from v2 to v3 messages in Runner Utility.
		public static IMessageSinkMessage Adapt(
			string assemblyUniqueID,
			IMessageSinkMessage message,
			HashSet<string>? messageTypes = null)
		{
			return
				Convert<IDiagnosticMessage>(message, messageTypes, AdaptDiagnosticMessage) ??

				Convert<IDiscoveryCompleteMessage>(assemblyUniqueID, message, messageTypes, AdaptDiscoveryCompleteMessage) ??

				Convert<IErrorMessage>(message, messageTypes, AdaptErrorMessage) ??

				Convert<ITestAssemblyCleanupFailure>(message, messageTypes, AdaptTestAssemblyCleanupFailure) ??
				Convert<ITestAssemblyFinished>(message, messageTypes, AdaptTestAssemblyFinished) ??
				Convert<ITestAssemblyStarting>(message, messageTypes, AdaptTestAssemblyStarting) ??

				Convert<ITestCaseCleanupFailure>(message, messageTypes, AdaptTestCaseCleanupFailure) ??
				Convert<ITestCaseFinished>(message, messageTypes, AdaptTestCaseFinished) ??
				Convert<ITestCaseStarting>(message, messageTypes, AdaptTestCaseStarting) ??

				Convert<ITestClassCleanupFailure>(message, messageTypes, AdaptTestClassCleanupFailure) ??
				Convert<ITestClassFinished>(message, messageTypes, AdaptTestClassFinished) ??
				Convert<ITestClassStarting>(message, messageTypes, AdaptTestClassStarting) ??

				Convert<ITestCollectionCleanupFailure>(message, messageTypes, AdaptTestCollectionCleanupFailure) ??
				Convert<ITestCollectionFinished>(message, messageTypes, AdaptTestCollectionFinished) ??
				Convert<ITestCollectionStarting>(message, messageTypes, AdaptTestCollectionStarting) ??

				Convert<ITestMethodCleanupFailure>(message, messageTypes, AdaptTestMethodCleanupFailure) ??
				Convert<ITestMethodFinished>(message, messageTypes, AdaptTestMethodFinished) ??
				Convert<ITestMethodStarting>(message, messageTypes, AdaptTestMethodStarting) ??

				Convert<IAfterTestFinished>(message, messageTypes, AdaptAfterTestFinished) ??
				Convert<IAfterTestStarting>(message, messageTypes, AdaptAfterTestStarting) ??
				Convert<IBeforeTestFinished>(message, messageTypes, AdaptBeforeTestFinished) ??
				Convert<IBeforeTestStarting>(message, messageTypes, AdaptBeforeTestStarting) ??
				Convert<ITestClassConstructionFinished>(message, messageTypes, AdaptTestClassConstructionFinished) ??
				Convert<ITestClassConstructionStarting>(message, messageTypes, AdaptTestClassConstructionStarting) ??
				Convert<ITestCleanupFailure>(message, messageTypes, AdaptTestCleanupFailure) ??
				Convert<ITestFailed>(message, messageTypes, AdaptTestFailed) ??
				Convert<ITestFinished>(message, messageTypes, AdaptTestFinished) ??
				Convert<ITestOutput>(message, messageTypes, AdaptTestOutput) ??
				Convert<ITestPassed>(message, messageTypes, AdaptTestPassed) ??
				Convert<ITestSkipped>(message, messageTypes, AdaptTestSkipped) ??
				Convert<ITestStarting>(message, messageTypes, AdaptTestStarting) ??

				message;
		}

		static _AfterTestFinished AdaptAfterTestFinished(IAfterTestFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _AfterTestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = message.AttributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _AfterTestStarting AdaptAfterTestStarting(IAfterTestStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _AfterTestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = message.AttributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _BeforeTestFinished AdaptBeforeTestFinished(IBeforeTestFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _BeforeTestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = message.AttributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _BeforeTestStarting AdaptBeforeTestStarting(IBeforeTestStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _BeforeTestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				AttributeName = message.AttributeName,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _DiagnosticMessage AdaptDiagnosticMessage(IDiagnosticMessage message) =>
			new _DiagnosticMessage { Message = message.Message };

		static _DiscoveryComplete AdaptDiscoveryCompleteMessage(string assemblyUniqueID, IDiscoveryCompleteMessage message) =>
			new _DiscoveryComplete { AssemblyUniqueID = assemblyUniqueID };

		static _ErrorMessage AdaptErrorMessage(IErrorMessage message) =>
			new _ErrorMessage
			{
				ExceptionParentIndices = message.ExceptionParentIndices,
				ExceptionTypes = message.ExceptionTypes,
				Messages = message.Messages,
				StackTraces = message.StackTraces
			};

		static _TestAssemblyCleanupFailure AdaptTestAssemblyCleanupFailure(ITestAssemblyCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);

			return new _TestAssemblyCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = message.ExceptionParentIndices,
				ExceptionTypes = message.ExceptionTypes,
				Messages = message.Messages,
				StackTraces = message.StackTraces
			};
		}

		static _TestAssemblyFinished AdaptTestAssemblyFinished(ITestAssemblyFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);

			return new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _TestAssemblyStarting AdaptTestAssemblyStarting(ITestAssemblyStarting message)
		{
			var targetFrameworkAttribute = message.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
			var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);

			return new _TestAssemblyStarting
			{
				AssemblyName = message.TestAssembly.Assembly.Name,
				AssemblyPath = message.TestAssembly.Assembly.AssemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
				ConfigFilePath = message.TestAssembly.ConfigFileName,
				StartTime = message.StartTime,
				TargetFramework = targetFramework,
				TestEnvironment = message.TestEnvironment,
				TestFrameworkDisplayName = message.TestFrameworkDisplayName
			};
		}

		static _TestCaseCleanupFailure AdaptTestCaseCleanupFailure(ITestCaseCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

			return new _TestCaseCleanupFailure
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

		static _TestCaseFinished AdaptTestCaseFinished(ITestCaseFinished message)
		{
			// Clean up the cache
			lock (testUniqueIDsByTestCase)
				testUniqueIDsByTestCase.Remove(message.TestCase);

			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;

			return new _TestCaseFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _TestCaseStarting AdaptTestCaseStarting(ITestCaseStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

			return new _TestCaseStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				SkipReason = message.TestCase.SkipReason,
				SourceFilePath = message.TestCase.SourceInformation?.FileName,
				SourceLineNumber = message.TestCase.SourceInformation?.LineNumber,
				TestCaseDisplayName = message.TestCase.DisplayName,
				TestCaseUniqueID = message.TestCase.UniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = message.TestCase.Traits
			};
		}

		static _TestClassCleanupFailure AdaptTestClassCleanupFailure(ITestClassCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

			return new _TestClassCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = message.ExceptionParentIndices,
				ExceptionTypes = message.ExceptionTypes,
				Messages = message.Messages,
				StackTraces = message.StackTraces,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID
			};
		}

		static _TestClassConstructionFinished AdaptTestClassConstructionFinished(ITestClassConstructionFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestClassConstructionFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestClassConstructionStarting AdaptTestClassConstructionStarting(ITestClassConstructionStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestClassConstructionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestClassFinished AdaptTestClassFinished(ITestClassFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

			return new _TestClassFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _TestClassStarting AdaptTestClassStarting(ITestClassStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);

			return new _TestClassStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClass = message.TestClass.Class.Name,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _TestCleanupFailure AdaptTestCleanupFailure(ITestCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestCleanupFailure
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
				TestUniqueID = testUniqueID
			};
		}

		static _TestCollectionCleanupFailure AdaptTestCollectionCleanupFailure(ITestCollectionCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

			return new _TestCollectionCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = message.ExceptionParentIndices,
				ExceptionTypes = message.ExceptionTypes,
				Messages = message.Messages,
				StackTraces = message.StackTraces,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _TestCollectionFinished AdaptTestCollectionFinished(ITestCollectionFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

			return new _TestCollectionFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _TestCollectionStarting AdaptTestCollectionStarting(ITestCollectionStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

			return new _TestCollectionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClass = message.TestCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = message.TestCollection.DisplayName,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _TestFailed AdaptTestFailed(ITestFailed message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestFailed
			{
				AssemblyUniqueID = assemblyUniqueID,
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
				TestUniqueID = testUniqueID
			};
		}

		static _TestFinished AdaptTestFinished(ITestFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				Output = message.Output,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestMethodCleanupFailure AdaptTestMethodCleanupFailure(ITestMethodCleanupFailure message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

			return new _TestMethodCleanupFailure
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

		static _TestMethodFinished AdaptTestMethodFinished(ITestMethodFinished message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

			return new _TestMethodFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _TestMethodStarting AdaptTestMethodStarting(ITestMethodStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);

			return new _TestMethodStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethod = message.TestMethod.Method.Name,
				TestMethodUniqueID = testMethodUniqueID,
			};
		}

		static _TestOutput AdaptTestOutput(ITestOutput message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestOutput
			{
				AssemblyUniqueID = assemblyUniqueID,
				Output = message.Output,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestPassed AdaptTestPassed(ITestPassed message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestPassed
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				Output = message.Output,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestSkipped AdaptTestSkipped(ITestSkipped message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestSkipped
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				Output = message.Output,
				Reason = message.Reason,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestStarting AdaptTestStarting(ITestStarting message)
		{
			var assemblyUniqueID = UniqueIDForAssembly(message.TestAssembly);
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestDisplayName = message.Test.DisplayName,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static string UniqueIDForAssembly(ITestAssembly testAssembly) =>
			UniqueIDGenerator.ForAssembly(testAssembly.Assembly.Name, testAssembly.Assembly.AssemblyPath, testAssembly.ConfigFileName);

		static string UniqueIDForTest(
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

		static _MessageSinkMessage? Convert<TMessage>(
			IMessageSinkMessage message,
			HashSet<string>? messageTypes,
			Func<TMessage, _MessageSinkMessage> converter)
				where TMessage : class, IMessageSinkMessage
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var castMessage = message.Cast<TMessage>(messageTypes);
			if (castMessage != null)
				return converter(castMessage);

			return null;
		}

		static _MessageSinkMessage? Convert<TMessage>(
			string assemblyUniqueID,
			IMessageSinkMessage message,
			HashSet<string>? messageTypes,
			Func<string, TMessage, _MessageSinkMessage> converter)
				where TMessage : class, IMessageSinkMessage
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var castMessage = message.Cast<TMessage>(messageTypes);
			if (castMessage != null)
				return converter(assemblyUniqueID, castMessage);

			return null;
		}
	}
}
