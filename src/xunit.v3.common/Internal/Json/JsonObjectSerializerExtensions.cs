using Xunit.v3;

namespace Xunit.Internal;

internal static class JsonObjectSerializerExtensions
{
	internal static void SerializeAssemblyMetadata(
		this JsonObjectSerializer serializer,
		_IAssemblyMetadata metadata)
	{
		serializer.Serialize(nameof(_IAssemblyMetadata.AssemblyName), metadata.AssemblyName);
		serializer.Serialize(nameof(_IAssemblyMetadata.AssemblyPath), metadata.AssemblyPath);
		serializer.Serialize(nameof(_IAssemblyMetadata.ConfigFilePath), metadata.ConfigFilePath);
	}

	internal static void SerializeErrorMetadata(
		this JsonObjectSerializer serializer,
		_IErrorMetadata metadata)
	{
		using (var indexArraySerializer = serializer.SerializeArray(nameof(_IErrorMetadata.ExceptionParentIndices)))
			foreach (var index in metadata.ExceptionParentIndices)
				indexArraySerializer.Serialize(index);

		using (var typeArraySerializer = serializer.SerializeArray(nameof(_IErrorMetadata.ExceptionTypes)))
			foreach (var type in metadata.ExceptionTypes)
				typeArraySerializer.Serialize(type);

		using (var messageArraySerializer = serializer.SerializeArray(nameof(_IErrorMetadata.Messages)))
			foreach (var message in metadata.Messages)
				messageArraySerializer.Serialize(message);

		using (var stackTraceArraySerializer = serializer.SerializeArray(nameof(_IErrorMetadata.StackTraces)))
			foreach (var stackTrace in metadata.StackTraces)
				stackTraceArraySerializer.Serialize(stackTrace);
	}

	internal static void SerializeExecutionMetadata(
		this JsonObjectSerializer serializer,
		_IExecutionMetadata metadata)
	{
		serializer.Serialize(nameof(_IExecutionMetadata.ExecutionTime), metadata.ExecutionTime);
		serializer.Serialize(nameof(_IExecutionMetadata.Output), metadata.Output);

		if (metadata.Warnings is not null)
			using (var arraySerializer = serializer.SerializeArray(nameof(_IExecutionMetadata.Warnings)))
				foreach (var warning in metadata.Warnings)
					arraySerializer.Serialize(warning);
	}

	internal static void SerializeExecutionSummaryMetadata(
		this JsonObjectSerializer serializer,
		_IExecutionSummaryMetadata metadata)
	{
		serializer.Serialize(nameof(_IExecutionSummaryMetadata.ExecutionTime), metadata.ExecutionTime);
		serializer.Serialize(nameof(_IExecutionSummaryMetadata.TestsFailed), metadata.TestsFailed);
		serializer.Serialize(nameof(_IExecutionSummaryMetadata.TestsNotRun), metadata.TestsNotRun);
		serializer.Serialize(nameof(_IExecutionSummaryMetadata.TestsSkipped), metadata.TestsSkipped);
		serializer.Serialize(nameof(_IExecutionSummaryMetadata.TestsTotal), metadata.TestsTotal);
	}

	internal static void SerializeTestCaseMetadata(
		this JsonObjectSerializer serializer,
		_ITestCaseMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestCaseMetadata.SkipReason), metadata.SkipReason);
		serializer.Serialize(nameof(_ITestCaseMetadata.SourceFilePath), metadata.SourceFilePath);
		serializer.Serialize(nameof(_ITestCaseMetadata.SourceLineNumber), metadata.SourceLineNumber);
		serializer.Serialize(nameof(_ITestCaseMetadata.TestCaseDisplayName), metadata.TestCaseDisplayName);
		serializer.Serialize(nameof(_ITestCaseMetadata.TestClassName), metadata.TestClassName);
		serializer.Serialize(nameof(_ITestCaseMetadata.TestClassNamespace), metadata.TestClassNamespace);
		serializer.Serialize(nameof(_ITestCaseMetadata.TestClassNameWithNamespace), metadata.TestClassNameWithNamespace);
		serializer.Serialize(nameof(_ITestCaseMetadata.TestMethodName), metadata.TestMethodName);
		serializer.Serialize(nameof(_ITestCaseMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestClassMetadata(
		this JsonObjectSerializer serializer,
		_ITestClassMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestClassMetadata.TestClass), metadata.TestClass);
	}

	internal static void SerializeTestCollectionMetadata(
		this JsonObjectSerializer serializer,
		_ITestCollectionMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestCollectionMetadata.TestCollectionClass), metadata.TestCollectionClass);
		serializer.Serialize(nameof(_ITestCollectionMetadata.TestCollectionDisplayName), metadata.TestCollectionDisplayName);
	}

	internal static void SerializeTestMetadata(
		this JsonObjectSerializer serializer,
		_ITestMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestMetadata.Explicit), metadata.Explicit);
		serializer.Serialize(nameof(_ITestMetadata.TestDisplayName), metadata.TestDisplayName);
		serializer.Serialize(nameof(_ITestMetadata.Timeout), metadata.Timeout);
		serializer.Serialize(nameof(_ITestMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestMethodMetadata(
		this JsonObjectSerializer serializer,
		_ITestMethodMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestMethodMetadata.TestMethod), metadata.TestMethod);
	}
}
