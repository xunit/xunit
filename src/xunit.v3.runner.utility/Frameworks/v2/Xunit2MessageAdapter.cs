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
