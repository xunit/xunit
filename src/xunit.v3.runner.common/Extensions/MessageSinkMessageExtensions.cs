using Xunit.Internal;
using Xunit.Runner.Common;

namespace Xunit.Sdk;

/// <summary>
/// Extension methods for <see cref="IMessageSinkMessage"/>.
/// </summary>
public static partial class MessageSinkMessageExtensions
{
	/// <summary>
	/// Handles a message of a specific type by testing it for the type, as well as verifying that there
	/// is a registered callback.
	/// </summary>
	/// <param name="message">The message to dispatch.</param>
	/// <param name="callback">The callback to dispatch the message to.</param>
	/// <returns>Returns <c>true</c> if processing should continue; <c>false</c> otherwise.</returns>
	public static bool DispatchWhen<TMessage>(
		this IMessageSinkMessage message,
		MessageHandler<TMessage>? callback)
			where TMessage : IMessageSinkMessage
	{
		Guard.ArgumentNotNull(message);

		if (callback is not null && message is TMessage castMessage)
		{
			var args = new MessageHandlerArgs<TMessage>(castMessage);
			callback(args);
			return !args.IsStopped;
		}

		return true;
	}

	/// <summary>
	/// Converts an instance of <see cref="ITestCase"/> into <see cref="ITestCaseDiscovered"/> for reporting
	/// back to a remote meta-runner.
	/// </summary>
	public static ITestCaseDiscovered ToTestCaseDiscovered(this ITestCase testCase)
	{
		Guard.ArgumentNotNull(testCase);

		return new TestCaseDiscovered()
		{
			AssemblyUniqueID = testCase.TestCollection.TestAssembly.UniqueID,
			Serialization = SerializationHelper.Serialize(testCase),
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceFilePath,
			SourceLineNumber = testCase.SourceLineNumber,
			TestCaseDisplayName = testCase.TestCaseDisplayName,
			TestCaseUniqueID = testCase.UniqueID,
			TestClassMetadataToken = testCase.TestClassMetadataToken,
			TestClassName = testCase.TestClassName,
			TestClassNamespace = testCase.TestClassNamespace,
			TestClassUniqueID = testCase.TestClass?.UniqueID,
			TestCollectionUniqueID = testCase.TestCollection.UniqueID,
			TestMethodMetadataToken = testCase.TestMethodMetadataToken,
			TestMethodName = testCase.TestMethodName,
			TestMethodUniqueID = testCase.TestMethod?.UniqueID,
			Traits = testCase.Traits,
		};
	}

	/// <summary>
	/// Creates a new <see cref="ITestCaseDiscovered"/>, replacing the source file and line number information
	/// with the provided values.
	/// </summary>
	/// <param name="discovered"/>
	/// <param name="sourceFilePath">The source file</param>
	/// <param name="sourceLineNumber">The line number</param>
	public static ITestCaseDiscovered WithSourceInfo(
		this ITestCaseDiscovered discovered,
		string? sourceFilePath,
		int? sourceLineNumber) =>
			new TestCaseDiscovered
			{
				AssemblyUniqueID = Guard.ArgumentNotNull(discovered).AssemblyUniqueID,
				Serialization = discovered.Serialization,
				SkipReason = discovered.SkipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = discovered.TestCaseDisplayName,
				TestCaseUniqueID = discovered.TestCaseUniqueID,
				TestClassMetadataToken = discovered.TestClassMetadataToken,
				TestClassName = discovered.TestClassName,
				TestClassNamespace = discovered.TestClassNamespace,
				TestClassUniqueID = discovered.TestClassUniqueID,
				TestCollectionUniqueID = discovered.TestCollectionUniqueID,
				TestMethodMetadataToken = discovered.TestMethodMetadataToken,
				TestMethodName = discovered.TestMethodName,
				TestMethodUniqueID = discovered.TestMethodUniqueID,
				Traits = discovered.Traits,
			};
}
