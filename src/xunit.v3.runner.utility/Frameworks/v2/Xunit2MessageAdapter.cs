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
		/// <remarks>
		/// We pass the assembly unique ID through here for two reasons: (a) there is no assembly information
		/// contained in <see cref="IDiscoveryCompleteMessage"/>, but we need it in our v3 message;
		/// and (b) the local information we have for assembly path/name/config file name change after
		/// being passed into v2, so in order for everything to match, we need to use the assembly unique ID
		/// based on what we passed into v2 (rather than what it computed), or else our unique IDs won't
		/// line up across all messages.
		/// </remarks>
		public static _MessageSinkMessage Adapt(
			string assemblyUniqueID,
			IMessageSinkMessage message,
			HashSet<string>? messageTypes = null)
		{
			return
				TryConvert<IDiagnosticMessage>(assemblyUniqueID, message, messageTypes, AdaptDiagnosticMessage) ??

				TryConvert<IDiscoveryCompleteMessage>(assemblyUniqueID, message, messageTypes, AdaptDiscoveryCompleteMessage) ??
				TryConvert<ITestCaseDiscoveryMessage>(assemblyUniqueID, message, messageTypes, AdaptTestCaseDiscoveryMessage) ??

				TryConvert<IErrorMessage>(assemblyUniqueID, message, messageTypes, AdaptErrorMessage) ??

				TryConvert<ITestAssemblyCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestAssemblyCleanupFailure) ??
				TryConvert<ITestAssemblyFinished>(assemblyUniqueID, message, messageTypes, AdaptTestAssemblyFinished) ??
				TryConvert<ITestAssemblyStarting>(assemblyUniqueID, message, messageTypes, AdaptTestAssemblyStarting) ??

				TryConvert<ITestCaseCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestCaseCleanupFailure) ??
				TryConvert<ITestCaseFinished>(assemblyUniqueID, message, messageTypes, AdaptTestCaseFinished) ??
				TryConvert<ITestCaseStarting>(assemblyUniqueID, message, messageTypes, AdaptTestCaseStarting) ??

				TryConvert<ITestClassCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestClassCleanupFailure) ??
				TryConvert<ITestClassFinished>(assemblyUniqueID, message, messageTypes, AdaptTestClassFinished) ??
				TryConvert<ITestClassStarting>(assemblyUniqueID, message, messageTypes, AdaptTestClassStarting) ??

				TryConvert<ITestCollectionCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestCollectionCleanupFailure) ??
				TryConvert<ITestCollectionFinished>(assemblyUniqueID, message, messageTypes, AdaptTestCollectionFinished) ??
				TryConvert<ITestCollectionStarting>(assemblyUniqueID, message, messageTypes, AdaptTestCollectionStarting) ??

				TryConvert<ITestMethodCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestMethodCleanupFailure) ??
				TryConvert<ITestMethodFinished>(assemblyUniqueID, message, messageTypes, AdaptTestMethodFinished) ??
				TryConvert<ITestMethodStarting>(assemblyUniqueID, message, messageTypes, AdaptTestMethodStarting) ??

				TryConvert<IAfterTestFinished>(assemblyUniqueID, message, messageTypes, AdaptAfterTestFinished) ??
				TryConvert<IAfterTestStarting>(assemblyUniqueID, message, messageTypes, AdaptAfterTestStarting) ??
				TryConvert<IBeforeTestFinished>(assemblyUniqueID, message, messageTypes, AdaptBeforeTestFinished) ??
				TryConvert<IBeforeTestStarting>(assemblyUniqueID, message, messageTypes, AdaptBeforeTestStarting) ??
				TryConvert<ITestClassConstructionFinished>(assemblyUniqueID, message, messageTypes, AdaptTestClassConstructionFinished) ??
				TryConvert<ITestClassConstructionStarting>(assemblyUniqueID, message, messageTypes, AdaptTestClassConstructionStarting) ??
				TryConvert<ITestClassDisposeFinished>(assemblyUniqueID, message, messageTypes, AdaptTestClassDisposeFinished) ??
				TryConvert<ITestClassDisposeStarting>(assemblyUniqueID, message, messageTypes, AdaptTestClassDisposeStarting) ??
				TryConvert<ITestCleanupFailure>(assemblyUniqueID, message, messageTypes, AdaptTestCleanupFailure) ??
				TryConvert<ITestFailed>(assemblyUniqueID, message, messageTypes, AdaptTestFailed) ??
				TryConvert<ITestFinished>(assemblyUniqueID, message, messageTypes, AdaptTestFinished) ??
				TryConvert<ITestOutput>(assemblyUniqueID, message, messageTypes, AdaptTestOutput) ??
				TryConvert<ITestPassed>(assemblyUniqueID, message, messageTypes, AdaptTestPassed) ??
				TryConvert<ITestSkipped>(assemblyUniqueID, message, messageTypes, AdaptTestSkipped) ??
				TryConvert<ITestStarting>(assemblyUniqueID, message, messageTypes, AdaptTestStarting) ??

				throw new ArgumentException($"Unknown message type '{message.GetType().FullName}'", nameof(message));
		}

		static _AfterTestFinished AdaptAfterTestFinished(
			string assemblyUniqueID,
			IAfterTestFinished message)
		{
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

		static _AfterTestStarting AdaptAfterTestStarting(
			string assemblyUniqueID,
			IAfterTestStarting message)
		{
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

		static _BeforeTestFinished AdaptBeforeTestFinished(
			string assemblyUniqueID,
			IBeforeTestFinished message)
		{
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

		static _BeforeTestStarting AdaptBeforeTestStarting(
			string assemblyUniqueID,
			IBeforeTestStarting message)
		{
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

		static _DiagnosticMessage AdaptDiagnosticMessage(
			string assemblyUniqueID,
			IDiagnosticMessage message) =>
				new _DiagnosticMessage { Message = message.Message };

		static _DiscoveryComplete AdaptDiscoveryCompleteMessage(
			string assemblyUniqueID,
			IDiscoveryCompleteMessage message) =>
				new _DiscoveryComplete { AssemblyUniqueID = assemblyUniqueID };

		static _ErrorMessage AdaptErrorMessage(
			string assemblyUniqueID,
			IErrorMessage message) =>
				new _ErrorMessage
				{
					ExceptionParentIndices = message.ExceptionParentIndices,
					ExceptionTypes = message.ExceptionTypes,
					Messages = message.Messages,
					StackTraces = message.StackTraces
				};

		static _TestAssemblyCleanupFailure AdaptTestAssemblyCleanupFailure(
			string assemblyUniqueID,
			ITestAssemblyCleanupFailure message) =>
				new _TestAssemblyCleanupFailure
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExceptionParentIndices = message.ExceptionParentIndices,
					ExceptionTypes = message.ExceptionTypes,
					Messages = message.Messages,
					StackTraces = message.StackTraces
				};

		static _TestAssemblyFinished AdaptTestAssemblyFinished(
			string assemblyUniqueID,
			ITestAssemblyFinished message) =>
				new _TestAssemblyFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = message.ExecutionTime,
					TestsFailed = message.TestsFailed,
					TestsRun = message.TestsRun,
					TestsSkipped = message.TestsSkipped
				};

		static _TestAssemblyStarting AdaptTestAssemblyStarting(
			string assemblyUniqueID,
			ITestAssemblyStarting message)
		{
			var targetFrameworkAttribute = message.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
			var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();

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

		static _TestCaseCleanupFailure AdaptTestCaseCleanupFailure(
			string assemblyUniqueID,
			ITestCaseCleanupFailure message)
		{
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

		static _TestCaseDiscovered AdaptTestCaseDiscoveryMessage(
			string assemblyUniqueID,
			ITestCaseDiscoveryMessage message)
		{
			var testCase = message.TestCase;

			// Clean up the cache
			lock (testUniqueIDsByTestCase)
				testUniqueIDsByTestCase.Remove(testCase);

			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = testCase.UniqueID;

			return new _TestCaseDiscovered
			{
				AssemblyUniqueID = assemblyUniqueID,
				SkipReason = testCase.SkipReason,
				SourceFilePath = testCase.SourceInformation?.FileName,
				SourceLineNumber = testCase.SourceInformation?.LineNumber,
				TestCase = testCase,
				TestCaseDisplayName = testCase.DisplayName,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				Traits = testCase.Traits
			};
		}

		static _TestCaseFinished AdaptTestCaseFinished(
			string assemblyUniqueID,
			ITestCaseFinished message)
		{
			// Clean up the cache
			lock (testUniqueIDsByTestCase)
				testUniqueIDsByTestCase.Remove(message.TestCase);

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

		static _TestCaseStarting AdaptTestCaseStarting(
			string assemblyUniqueID,
			ITestCaseStarting message)
		{
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

		static _TestClassCleanupFailure AdaptTestClassCleanupFailure(
			string assemblyUniqueID,
			ITestClassCleanupFailure message)
		{
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

		static _TestClassConstructionFinished AdaptTestClassConstructionFinished(
			string assemblyUniqueID,
			ITestClassConstructionFinished message)
		{
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

		static _TestClassConstructionStarting AdaptTestClassConstructionStarting(
			string assemblyUniqueID,
			ITestClassConstructionStarting message)
		{
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

		static _TestClassDisposeFinished AdaptTestClassDisposeFinished(
			string assemblyUniqueID,
			ITestClassDisposeFinished message)
		{
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestClassDisposeFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestClassDisposeStarting AdaptTestClassDisposeStarting(
			string assemblyUniqueID,
			ITestClassDisposeStarting message)
		{
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = UniqueIDForTestClass(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = UniqueIDForTestMethod(testClassUniqueID, message.TestMethod);
			var testCaseUniqueID = message.TestCase.UniqueID;
			var testUniqueID = UniqueIDForTest(testCaseUniqueID, message.Test);

			return new _TestClassDisposeStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCaseUniqueID = testCaseUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethodUniqueID = testMethodUniqueID,
				TestUniqueID = testUniqueID
			};
		}

		static _TestClassFinished AdaptTestClassFinished(
			string assemblyUniqueID,
			ITestClassFinished message)
		{
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

		static _TestClassStarting AdaptTestClassStarting(
			string assemblyUniqueID,
			ITestClassStarting message)
		{
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

		static _TestCleanupFailure AdaptTestCleanupFailure(
			string assemblyUniqueID,
			ITestCleanupFailure message)
		{
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

		static _TestCollectionCleanupFailure AdaptTestCollectionCleanupFailure(
			string assemblyUniqueID,
			ITestCollectionCleanupFailure message)
		{
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

		static _TestCollectionFinished AdaptTestCollectionFinished(
			string assemblyUniqueID,
			ITestCollectionFinished message)
		{
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

		static _TestCollectionStarting AdaptTestCollectionStarting(
			string assemblyUniqueID,
			ITestCollectionStarting message)
		{
			var testCollectionUniqueID = UniqueIDForTestCollection(assemblyUniqueID, message.TestCollection);

			return new _TestCollectionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClass = message.TestCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = message.TestCollection.DisplayName,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _TestFailed AdaptTestFailed(
			string assemblyUniqueID,
			ITestFailed message)
		{
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

		static _TestFinished AdaptTestFinished(
			string assemblyUniqueID,
			ITestFinished message)
		{
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

		static _TestMethodCleanupFailure AdaptTestMethodCleanupFailure(
			string assemblyUniqueID,
			ITestMethodCleanupFailure message)
		{
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

		static _TestMethodFinished AdaptTestMethodFinished(
			string assemblyUniqueID,
			ITestMethodFinished message)
		{
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

		static _TestMethodStarting AdaptTestMethodStarting(
			string assemblyUniqueID,
			ITestMethodStarting message)
		{
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

		static _TestOutput AdaptTestOutput(
			string assemblyUniqueID,
			ITestOutput message)
		{
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

		static _TestPassed AdaptTestPassed(
			string assemblyUniqueID,
			ITestPassed message)
		{
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

		static _TestSkipped AdaptTestSkipped(
			string assemblyUniqueID,
			ITestSkipped message)
		{
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

		static _TestStarting AdaptTestStarting(
			string assemblyUniqueID,
			ITestStarting message)
		{
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

		static _MessageSinkMessage? TryConvert<TMessage>(
			string assemblyUniqueID,
			IMessageSinkMessage message,
			HashSet<string>? messageTypes,
			Func<string, TMessage, _MessageSinkMessage> converter)
				where TMessage : class, IMessageSinkMessage
		{
			Guard.ArgumentNotNull(nameof(message), message);

			var castMessage = messageTypes == null || messageTypes.Contains(typeof(TMessage).FullName!) ? message as TMessage : null;
			if (castMessage != null)
				return converter(assemblyUniqueID, castMessage);

			return null;
		}

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
	}
}
