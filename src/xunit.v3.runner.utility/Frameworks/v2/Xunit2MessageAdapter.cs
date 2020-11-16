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
		/// <summary>
		/// Adapts <see cref="IMessageSinkMessage"/> to <see cref="_MessageSinkMessage"/>.
		/// </summary>
		// TODO: This has the wrong return type so that we can do fall-through, until
		// we're finished moving from v2 to v3 messages in Runner Utility.
		public static IMessageSinkMessage Adapt(
			IMessageSinkMessage message,
			HashSet<string>? messageTypes = null)
		{
			return
				Convert<IDiagnosticMessage>(message, messageTypes, AdaptDiagnosticMessage) ??

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

				message;
		}

		static _MessageSinkMessage AdaptDiagnosticMessage(IDiagnosticMessage message) =>
			new _DiagnosticMessage { Message = message.Message };

		static _MessageSinkMessage AdaptTestAssemblyCleanupFailure(ITestAssemblyCleanupFailure message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);

			return new _TestAssemblyCleanupFailure
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExceptionParentIndices = message.ExceptionParentIndices,
				ExceptionTypes = message.ExceptionTypes,
				Messages = message.Messages,
				StackTraces = message.StackTraces
			};
		}

		static _MessageSinkMessage AdaptTestAssemblyFinished(ITestAssemblyFinished message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);

			return new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = message.ExecutionTime,
				TestsFailed = message.TestsFailed,
				TestsRun = message.TestsRun,
				TestsSkipped = message.TestsSkipped
			};
		}

		static _MessageSinkMessage AdaptTestAssemblyStarting(ITestAssemblyStarting message)
		{
			var targetFrameworkAttribute = message.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
			var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);

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

		static _MessageSinkMessage AdaptTestCaseCleanupFailure(ITestCaseCleanupFailure message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);

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

		static _MessageSinkMessage AdaptTestCaseFinished(ITestCaseFinished message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);
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

		static _MessageSinkMessage AdaptTestCaseStarting(ITestCaseStarting message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);

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

		static _MessageSinkMessage AdaptTestClassCleanupFailure(ITestClassCleanupFailure message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);

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

		static _MessageSinkMessage AdaptTestClassFinished(ITestClassFinished message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);

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

		static _MessageSinkMessage AdaptTestClassStarting(ITestClassStarting message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);

			return new _TestClassStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClass = message.TestClass.Class.Name,
				TestClassUniqueID = testClassUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _MessageSinkMessage AdaptTestCollectionCleanupFailure(ITestCollectionCleanupFailure message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);

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

		static _MessageSinkMessage AdaptTestCollectionFinished(ITestCollectionFinished message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);

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

		static _MessageSinkMessage AdaptTestCollectionStarting(ITestCollectionStarting message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);

			return new _TestCollectionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClass = message.TestCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = message.TestCollection.DisplayName,
				TestCollectionUniqueID = testCollectionUniqueID
			};
		}

		static _MessageSinkMessage AdaptTestMethodCleanupFailure(ITestMethodCleanupFailure message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);

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

		static _MessageSinkMessage AdaptTestMethodFinished(ITestMethodFinished message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);

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

		static _MessageSinkMessage AdaptTestMethodStarting(ITestMethodStarting message)
		{
			var assemblyUniqueID = ComputeUniqueID(message.TestAssembly);
			var testCollectionUniqueID = ComputeUniqueID(assemblyUniqueID, message.TestCollection);
			var testClassUniqueID = ComputeUniqueID(testCollectionUniqueID, message.TestClass);
			var testMethodUniqueID = ComputeUniqueID(testClassUniqueID, message.TestMethod);

			return new _TestMethodStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestClassUniqueID = testClassUniqueID,
				TestMethod = message.TestMethod.Method.Name,
				TestMethodUniqueID = testMethodUniqueID,
			};
		}

		static string ComputeUniqueID(ITestAssembly testAssembly) =>
			UniqueIDGenerator.ForAssembly(testAssembly.Assembly.Name, testAssembly.Assembly.AssemblyPath, testAssembly.ConfigFileName);

		static string? ComputeUniqueID(
			string testCollectionUniqueID,
			ITestClass? testClass) =>
				UniqueIDGenerator.ForTestClass(testCollectionUniqueID, testClass?.Class?.Name);

		static string ComputeUniqueID(
			string assemblyUniqueID,
			ITestCollection testCollection) =>
				UniqueIDGenerator.ForTestCollection(assemblyUniqueID, testCollection.DisplayName, testCollection.CollectionDefinition?.Name);

		static string? ComputeUniqueID(
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
	}
}
