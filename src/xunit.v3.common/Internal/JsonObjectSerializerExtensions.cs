using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.Internal;

internal static class JsonObjectSerializerExtensions
{
	internal static void DeserializeAssemblyMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableAssemblyMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_IAssemblyMetadata.AssemblyName)) is string assemblyName)
			metadata.AssemblyName = assemblyName;
		if (JsonDeserializer.TryGetString(obj, nameof(_IAssemblyMetadata.AssemblyPath)) is string assemblyPath)
			metadata.AssemblyPath = assemblyPath;
		if (JsonDeserializer.TryGetString(obj, nameof(_IAssemblyMetadata.ConfigFilePath)) is string configFilePath)
			metadata.ConfigFilePath = configFilePath;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_IAssemblyMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeErrorMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableErrorMetadata metadata)
	{
		if (JsonDeserializer.TryGetArrayOfInt(obj, nameof(_IErrorMetadata.ExceptionParentIndices)) is int[] expectedParentIndices)
			metadata.ExceptionParentIndices = expectedParentIndices;
		if (JsonDeserializer.TryGetArrayOfNullableString(obj, nameof(_IErrorMetadata.ExceptionTypes)) is string?[] exceptionTypes)
			metadata.ExceptionTypes = exceptionTypes;
		if (JsonDeserializer.TryGetArrayOfString(obj, nameof(_IErrorMetadata.Messages)) is string?[] messages)
			metadata.Messages = messages;
		if (JsonDeserializer.TryGetArrayOfNullableString(obj, nameof(_IErrorMetadata.StackTraces)) is string?[] stackTraces)
			metadata.StackTraces = stackTraces;
	}

	internal static void DeserializeExecutionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableExecutionMetadata metadata)
	{
		if (JsonDeserializer.TryGetDecimal(obj, nameof(_IExecutionMetadata.ExecutionTime)) is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (JsonDeserializer.TryGetString(obj, nameof(_IExecutionMetadata.Output)) is string output)
			metadata.Output = output;
		if (JsonDeserializer.TryGetArrayOfString(obj, nameof(_IExecutionMetadata.Warnings)) is string?[] warnings)
			metadata.Warnings = warnings;
	}

	internal static void DeserializeExecutionSummaryMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableExecutionSummaryMetadata metadata)
	{
		if (JsonDeserializer.TryGetDecimal(obj, nameof(_IExecutionSummaryMetadata.ExecutionTime)) is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (JsonDeserializer.TryGetInt(obj, nameof(_IExecutionSummaryMetadata.TestsFailed)) is int testsFailed)
			metadata.TestsFailed = testsFailed;
		if (JsonDeserializer.TryGetInt(obj, nameof(_IExecutionSummaryMetadata.TestsNotRun)) is int testsNotRun)
			metadata.TestsNotRun = testsNotRun;
		if (JsonDeserializer.TryGetInt(obj, nameof(_IExecutionSummaryMetadata.TestsSkipped)) is int testsSkipped)
			metadata.TestsSkipped = testsSkipped;
		if (JsonDeserializer.TryGetInt(obj, nameof(_IExecutionSummaryMetadata.TestsTotal)) is int testsTotal)
			metadata.TestsTotal = testsTotal;
	}

	internal static void DeserializeTestCaseMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestCaseMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.SkipReason)) is string skipReason)
			metadata.SkipReason = skipReason;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.SourceFilePath)) is string sourceFilePath)
			metadata.SourceFilePath = sourceFilePath;
		if (JsonDeserializer.TryGetInt(obj, nameof(_ITestCaseMetadata.SourceLineNumber)) is int sourceLineNumber)
			metadata.SourceLineNumber = sourceLineNumber;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.TestCaseDisplayName)) is string testCaseDisplayName)
			metadata.TestCaseDisplayName = testCaseDisplayName;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.TestClassName)) is string testClassName)
			metadata.TestClassName = testClassName;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.TestClassNamespace)) is string testClassNamespace)
			metadata.TestClassNamespace = testClassNamespace;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCaseMetadata.TestMethodName)) is string testMethodName)
			metadata.TestMethodName = testMethodName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_ITestCaseMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestClassMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestClassMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestClassMetadata.TestClassName)) is string testClassName)
			metadata.TestClassName = testClassName;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestClassMetadata.TestClassNamespace)) is string testClassNamespace)
			metadata.TestClassNamespace = testClassNamespace;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_ITestClassMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestCollectionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestCollectionMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCollectionMetadata.TestCollectionClassName)) is string testCollectionClass)
			metadata.TestCollectionClassName = testCollectionClass;
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestCollectionMetadata.TestCollectionDisplayName)) is string testCollectionDisplayName)
			metadata.TestCollectionDisplayName = testCollectionDisplayName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_ITestCollectionMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestMetadata.TestDisplayName)) is string testDisplayName)
			metadata.TestDisplayName = testDisplayName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_ITestMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestMethodMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestMethodMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(_ITestMethodMetadata.MethodName)) is string testMethod)
			metadata.MethodName = testMethod;
		if (JsonDeserializer.TryGetTraits(obj, nameof(_ITestMethodMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void SerializeAssemblyMetadata(
		this JsonObjectSerializer serializer,
		_IAssemblyMetadata metadata,
		bool excludeTraits = false)
	{
		serializer.Serialize(nameof(_IAssemblyMetadata.AssemblyName), metadata.AssemblyName);
		serializer.Serialize(nameof(_IAssemblyMetadata.AssemblyPath), metadata.AssemblyPath);
		serializer.Serialize(nameof(_IAssemblyMetadata.ConfigFilePath), metadata.ConfigFilePath);

		if (!excludeTraits)
			serializer.Serialize(nameof(_IAssemblyMetadata.Traits), metadata.Traits);
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
		serializer.Serialize(nameof(_ITestCaseMetadata.TestMethodName), metadata.TestMethodName);
		serializer.Serialize(nameof(_ITestCaseMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestClassMetadata(
		this JsonObjectSerializer serializer,
		_ITestClassMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestClassMetadata.TestClassName), metadata.TestClassName);
		serializer.Serialize(nameof(_ITestClassMetadata.TestClassNamespace), metadata.TestClassNamespace);
		serializer.Serialize(nameof(_ITestClassMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestCollectionMetadata(
		this JsonObjectSerializer serializer,
		_ITestCollectionMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestCollectionMetadata.TestCollectionClassName), metadata.TestCollectionClassName);
		serializer.Serialize(nameof(_ITestCollectionMetadata.TestCollectionDisplayName), metadata.TestCollectionDisplayName);
		serializer.Serialize(nameof(_ITestCollectionMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestMetadata(
		this JsonObjectSerializer serializer,
		_ITestMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestMetadata.TestDisplayName), metadata.TestDisplayName);
		serializer.Serialize(nameof(_ITestMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestMethodMetadata(
		this JsonObjectSerializer serializer,
		_ITestMethodMetadata metadata)
	{
		serializer.Serialize(nameof(_ITestMethodMetadata.MethodName), metadata.MethodName);
		serializer.Serialize(nameof(_ITestMethodMetadata.Traits), metadata.Traits);
	}
}
