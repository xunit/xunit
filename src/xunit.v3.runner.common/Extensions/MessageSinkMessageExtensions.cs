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
			Explicit = testCase.Explicit,
			Serialization = SerializationHelper.Instance.Serialize(testCase),
			SkipReason = testCase.SkipReason,
			SourceFilePath = testCase.SourceFilePath,
			SourceLineNumber = testCase.SourceLineNumber,
			TestCaseDisplayName = testCase.TestCaseDisplayName,
			TestCaseUniqueID = testCase.UniqueID,
			TestClassMetadataToken = testCase.TestClassMetadataToken,
			TestClassName = testCase.TestClassName,
			TestClassNamespace = testCase.TestClassNamespace,
			TestClassSimpleName = testCase.TestClassSimpleName,
			TestClassUniqueID = testCase.TestClass?.UniqueID,
			TestCollectionUniqueID = testCase.TestCollection.UniqueID,
			TestMethodMetadataToken = testCase.TestMethodMetadataToken,
			TestMethodName = testCase.TestMethodName,
			TestMethodParameterTypesVSTest = testCase.TestMethodParameterTypesVSTest,
			TestMethodReturnTypeVSTest = testCase.TestMethodReturnTypeVSTest,
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
			Guard.ArgumentNotNull(discovered).SourceFilePath is not null || discovered.SourceLineNumber is not null
				? discovered
				: new TestCaseDiscovered
				{
					AssemblyUniqueID = discovered.AssemblyUniqueID,
					Explicit = discovered.Explicit,
					Serialization = discovered.Serialization,
					SkipReason = discovered.SkipReason,
					SourceFilePath = sourceFilePath,
					SourceLineNumber = sourceLineNumber,
					TestCaseDisplayName = discovered.TestCaseDisplayName,
					TestCaseUniqueID = discovered.TestCaseUniqueID,
					TestClassMetadataToken = discovered.TestClassMetadataToken,
					TestClassName = discovered.TestClassName,
					TestClassNamespace = discovered.TestClassNamespace,
					TestClassSimpleName = discovered.TestClassSimpleName,
					TestClassUniqueID = discovered.TestClassUniqueID,
					TestCollectionUniqueID = discovered.TestCollectionUniqueID,
					TestMethodMetadataToken = discovered.TestMethodMetadataToken,
					TestMethodName = discovered.TestMethodName,
					TestMethodParameterTypesVSTest = discovered.TestMethodParameterTypesVSTest,
					TestMethodReturnTypeVSTest = discovered.TestMethodReturnTypeVSTest,
					TestMethodUniqueID = discovered.TestMethodUniqueID,
					Traits = discovered.Traits,
				};

	/// <summary>
	/// Creates a new <see cref="ITestCaseDiscovered"/>, replacing the source file and line number information
	/// with values from the source information provider.
	/// </summary>
	/// <param name="discovered"/>
	/// <param name="sourceInformationProvider">The source information provider</param>
	public static ITestCaseDiscovered WithSourceInfo(
		this ITestCaseDiscovered discovered,
		ISourceInformationProvider sourceInformationProvider)
	{
		Guard.ArgumentNotNull(discovered);
		Guard.ArgumentNotNull(sourceInformationProvider);

		if (discovered.SourceFilePath is not null || discovered.SourceLineNumber is not null)
			return discovered;

		var sourceInfo = sourceInformationProvider.GetSourceInformation(discovered.TestClassName, discovered.TestMethodName);

		return WithSourceInfo(discovered, sourceInfo.SourceFile, sourceInfo.SourceLine);
	}

	/// <summary>
	/// Creates a new <see cref="ITestCaseStarting"/>, replacing the source file and line number information
	/// with the provided values.
	/// </summary>
	/// <param name="starting"/>
	/// <param name="sourceFilePath">The source file</param>
	/// <param name="sourceLineNumber">The line number</param>
	public static ITestCaseStarting WithSourceInfo(
		this ITestCaseStarting starting,
		string? sourceFilePath,
		int? sourceLineNumber) =>
			Guard.ArgumentNotNull(starting).SourceFilePath is not null || starting.SourceLineNumber is not null
				? starting
				: new TestCaseStarting
				{
					AssemblyUniqueID = starting.AssemblyUniqueID,
					Explicit = starting.Explicit,
					SkipReason = starting.SkipReason,
					SourceFilePath = sourceFilePath,
					SourceLineNumber = sourceLineNumber,
					TestCaseDisplayName = starting.TestCaseDisplayName,
					TestCaseUniqueID = starting.TestCaseUniqueID,
					TestClassMetadataToken = starting.TestClassMetadataToken,
					TestClassName = starting.TestClassName,
					TestClassNamespace = starting.TestClassNamespace,
					TestClassSimpleName = starting.TestClassSimpleName,
					TestClassUniqueID = starting.TestClassUniqueID,
					TestCollectionUniqueID = starting.TestCollectionUniqueID,
					TestMethodMetadataToken = starting.TestMethodMetadataToken,
					TestMethodName = starting.TestMethodName,
					TestMethodParameterTypesVSTest = starting.TestMethodParameterTypesVSTest,
					TestMethodReturnTypeVSTest = starting.TestMethodReturnTypeVSTest,
					TestMethodUniqueID = starting.TestMethodUniqueID,
					Traits = starting.Traits,
				};

	/// <summary>
	/// Creates a new <see cref="ITestCaseDiscovered"/>, replacing the source file and line number information
	/// with values from the source information provider.
	/// </summary>
	/// <param name="starting"/>
	/// <param name="sourceInformationProvider">The source information provider</param>
	public static ITestCaseStarting WithSourceInfo(
		this ITestCaseStarting starting,
		ISourceInformationProvider sourceInformationProvider)
	{
		Guard.ArgumentNotNull(starting);
		Guard.ArgumentNotNull(sourceInformationProvider);

		if (starting.SourceFilePath is not null || starting.SourceLineNumber is not null)
			return starting;

		var sourceInfo = sourceInformationProvider.GetSourceInformation(starting.TestClassName, starting.TestMethodName);

		return WithSourceInfo(starting, sourceInfo.SourceFile, sourceInfo.SourceLine);
	}
}
