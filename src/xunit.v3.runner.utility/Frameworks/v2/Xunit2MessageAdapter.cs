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
				Convert<ITestAssemblyFinished>(message, messageTypes, AdaptTestAssemblyFinished) ??
				Convert<ITestAssemblyStarting>(message, messageTypes, AdaptTestAssemblyStarting) ??
				Convert<ITestClassStarting>(message, messageTypes, AdaptTestClassStarting) ??
				Convert<ITestCollectionFinished>(message, messageTypes, AdaptTestCollectionFinished) ??
				Convert<ITestCollectionStarting>(message, messageTypes, AdaptTestCollectionStarting) ??
				message;
		}

		static _MessageSinkMessage AdaptTestAssemblyFinished(ITestAssemblyFinished testAssemblyFinished)
		{
			var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
				testAssemblyFinished.TestAssembly.Assembly.Name,
				testAssemblyFinished.TestAssembly.Assembly.AssemblyPath,
				testAssemblyFinished.TestAssembly.ConfigFileName
			);

			return new _TestAssemblyFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = testAssemblyFinished.ExecutionTime,
				TestsFailed = testAssemblyFinished.TestsFailed,
				TestsRun = testAssemblyFinished.TestsRun,
				TestsSkipped = testAssemblyFinished.TestsSkipped
			};
		}

		static _MessageSinkMessage AdaptTestAssemblyStarting(ITestAssemblyStarting testAssemblyStarting)
		{
			var targetFrameworkAttribute = testAssemblyStarting.TestAssembly.Assembly.GetCustomAttributes(typeof(TargetFrameworkAttribute).FullName).FirstOrDefault();
			var targetFramework = targetFrameworkAttribute?.GetConstructorArguments().Cast<string>().Single();

			var result = new _TestAssemblyStarting
			{
				AssemblyName = testAssemblyStarting.TestAssembly.Assembly.Name,
				AssemblyPath = testAssemblyStarting.TestAssembly.Assembly.AssemblyPath,
				ConfigFilePath = testAssemblyStarting.TestAssembly.ConfigFileName,
				StartTime = testAssemblyStarting.StartTime,
				TargetFramework = targetFramework,
				TestEnvironment = testAssemblyStarting.TestEnvironment,
				TestFrameworkDisplayName = testAssemblyStarting.TestFrameworkDisplayName
			};

			result.AssemblyUniqueID = UniqueIDGenerator.ForAssembly(
				result.AssemblyName,
				result.AssemblyPath,
				result.ConfigFilePath
			);

			return result;
		}

		static _MessageSinkMessage AdaptTestClassStarting(ITestClassStarting testClassStarting)
		{
			var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
				testClassStarting.TestAssembly.Assembly.Name,
				testClassStarting.TestAssembly.Assembly.AssemblyPath,
				testClassStarting.TestAssembly.ConfigFileName
			);
			var testCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				assemblyUniqueID,
				testClassStarting.TestCollection.DisplayName,
				testClassStarting.TestCollection.CollectionDefinition?.Name
			);

			var result = new _TestClassStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClass = testClassStarting.TestClass.Class.Name,
				TestCollectionUniqueID = testCollectionUniqueID
			};

			result.TestClassUniqueID = UniqueIDGenerator.ForTestClass(testCollectionUniqueID, result.TestClass);

			return result;
		}

		static _MessageSinkMessage AdaptTestCollectionFinished(ITestCollectionFinished testCollectionFinished)
		{
			var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
				testCollectionFinished.TestAssembly.Assembly.Name,
				testCollectionFinished.TestAssembly.Assembly.AssemblyPath,
				testCollectionFinished.TestAssembly.ConfigFileName
			);
			var testCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				assemblyUniqueID,
				testCollectionFinished.TestCollection.DisplayName,
				testCollectionFinished.TestCollection.CollectionDefinition?.Name
			);

			return new _TestCollectionFinished
			{
				AssemblyUniqueID = assemblyUniqueID,
				ExecutionTime = testCollectionFinished.ExecutionTime,
				TestCollectionUniqueID = testCollectionUniqueID,
				TestsFailed = testCollectionFinished.TestsFailed,
				TestsRun = testCollectionFinished.TestsRun,
				TestsSkipped = testCollectionFinished.TestsSkipped
			};
		}

		static _MessageSinkMessage AdaptTestCollectionStarting(ITestCollectionStarting testCollectionStarting)
		{
			var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
				testCollectionStarting.TestAssembly.Assembly.Name,
				testCollectionStarting.TestAssembly.Assembly.AssemblyPath,
				testCollectionStarting.TestAssembly.ConfigFileName
			);

			var result = new _TestCollectionStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestCollectionClass = testCollectionStarting.TestCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = testCollectionStarting.TestCollection.DisplayName
			};

			result.TestCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				assemblyUniqueID,
				result.TestCollectionDisplayName,
				result.TestCollectionClass
			);

			return result;
		}

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
