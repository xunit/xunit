using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit.Internal;

internal static class JsonObjectSerializerExtensions
{
	internal static void DeserializeAssemblyMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableAssemblyMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(IAssemblyMetadata.AssemblyName)) is string assemblyName)
			metadata.AssemblyName = assemblyName;
		if (JsonDeserializer.TryGetString(obj, nameof(IAssemblyMetadata.AssemblyPath)) is string assemblyPath)
			metadata.AssemblyPath = assemblyPath;
		if (JsonDeserializer.TryGetString(obj, nameof(IAssemblyMetadata.ConfigFilePath)) is string configFilePath)
			metadata.ConfigFilePath = configFilePath;
		if (JsonDeserializer.TryGetTraits(obj, nameof(IAssemblyMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeErrorMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableErrorMetadata metadata)
	{
		if (JsonDeserializer.TryGetArrayOfInt(obj, nameof(IErrorMetadata.ExceptionParentIndices)) is int[] expectedParentIndices)
			metadata.ExceptionParentIndices = expectedParentIndices;
		if (JsonDeserializer.TryGetArrayOfNullableString(obj, nameof(IErrorMetadata.ExceptionTypes)) is string?[] exceptionTypes)
			metadata.ExceptionTypes = exceptionTypes;
		if (JsonDeserializer.TryGetArrayOfString(obj, nameof(IErrorMetadata.Messages)) is string?[] messages)
			metadata.Messages = messages;
		if (JsonDeserializer.TryGetArrayOfNullableString(obj, nameof(IErrorMetadata.StackTraces)) is string?[] stackTraces)
			metadata.StackTraces = stackTraces;
	}

	internal static void DeserializeExecutionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableExecutionMetadata metadata)
	{
		if (JsonDeserializer.TryGetDecimal(obj, nameof(IExecutionMetadata.ExecutionTime)) is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (JsonDeserializer.TryGetString(obj, nameof(IExecutionMetadata.Output)) is string output)
			metadata.Output = output;
		if (JsonDeserializer.TryGetArrayOfString(obj, nameof(IExecutionMetadata.Warnings)) is string?[] warnings)
			metadata.Warnings = warnings;
	}

	internal static void DeserializeExecutionSummaryMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableExecutionSummaryMetadata metadata)
	{
		if (JsonDeserializer.TryGetDecimal(obj, nameof(IExecutionSummaryMetadata.ExecutionTime)) is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (JsonDeserializer.TryGetInt(obj, nameof(IExecutionSummaryMetadata.TestsFailed)) is int testsFailed)
			metadata.TestsFailed = testsFailed;
		if (JsonDeserializer.TryGetInt(obj, nameof(IExecutionSummaryMetadata.TestsNotRun)) is int testsNotRun)
			metadata.TestsNotRun = testsNotRun;
		if (JsonDeserializer.TryGetInt(obj, nameof(IExecutionSummaryMetadata.TestsSkipped)) is int testsSkipped)
			metadata.TestsSkipped = testsSkipped;
		if (JsonDeserializer.TryGetInt(obj, nameof(IExecutionSummaryMetadata.TestsTotal)) is int testsTotal)
			metadata.TestsTotal = testsTotal;
	}

	internal static void DeserializeTestCaseMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableTestCaseMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.SkipReason)) is string skipReason)
			metadata.SkipReason = skipReason;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.SourceFilePath)) is string sourceFilePath)
			metadata.SourceFilePath = sourceFilePath;
		if (JsonDeserializer.TryGetInt(obj, nameof(ITestCaseMetadata.SourceLineNumber)) is int sourceLineNumber)
			metadata.SourceLineNumber = sourceLineNumber;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.TestCaseDisplayName)) is string testCaseDisplayName)
			metadata.TestCaseDisplayName = testCaseDisplayName;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.TestClassName)) is string testClassName)
			metadata.TestClassName = testClassName;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.TestClassNamespace)) is string testClassNamespace)
			metadata.TestClassNamespace = testClassNamespace;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCaseMetadata.TestMethodName)) is string testMethodName)
			metadata.TestMethodName = testMethodName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(ITestCaseMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestClassMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableTestClassMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(ITestClassMetadata.TestClassName)) is string testClassName)
			metadata.TestClassName = testClassName;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestClassMetadata.TestClassNamespace)) is string testClassNamespace)
			metadata.TestClassNamespace = testClassNamespace;
		if (JsonDeserializer.TryGetTraits(obj, nameof(ITestClassMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestCollectionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableTestCollectionMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCollectionMetadata.TestCollectionClassName)) is string testCollectionClass)
			metadata.TestCollectionClassName = testCollectionClass;
		if (JsonDeserializer.TryGetString(obj, nameof(ITestCollectionMetadata.TestCollectionDisplayName)) is string testCollectionDisplayName)
			metadata.TestCollectionDisplayName = testCollectionDisplayName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(ITestCollectionMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableTestMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(ITestMetadata.TestDisplayName)) is string testDisplayName)
			metadata.TestDisplayName = testDisplayName;
		if (JsonDeserializer.TryGetTraits(obj, nameof(ITestMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void DeserializeTestMethodMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		IWritableTestMethodMetadata metadata)
	{
		if (JsonDeserializer.TryGetString(obj, nameof(ITestMethodMetadata.MethodName)) is string testMethod)
			metadata.MethodName = testMethod;
		if (JsonDeserializer.TryGetTraits(obj, nameof(ITestMethodMetadata.Traits)) is IReadOnlyDictionary<string, IReadOnlyList<string>> traits)
			metadata.Traits = traits;
	}

	internal static void SerializeAssemblyMetadata(
		this JsonObjectSerializer serializer,
		IAssemblyMetadata metadata,
		bool excludeTraits = false)
	{
		serializer.Serialize(nameof(IAssemblyMetadata.AssemblyName), metadata.AssemblyName);
		serializer.Serialize(nameof(IAssemblyMetadata.AssemblyPath), metadata.AssemblyPath);
		serializer.Serialize(nameof(IAssemblyMetadata.ConfigFilePath), metadata.ConfigFilePath);

		if (!excludeTraits)
			serializer.Serialize(nameof(IAssemblyMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeErrorMetadata(
		this JsonObjectSerializer serializer,
		IErrorMetadata metadata)
	{
		using (var indexArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.ExceptionParentIndices)))
			foreach (var index in metadata.ExceptionParentIndices)
				indexArraySerializer.Serialize(index);

		using (var typeArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.ExceptionTypes)))
			foreach (var type in metadata.ExceptionTypes)
				typeArraySerializer.Serialize(type);

		using (var messageArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.Messages)))
			foreach (var message in metadata.Messages)
				messageArraySerializer.Serialize(message);

		using (var stackTraceArraySerializer = serializer.SerializeArray(nameof(IErrorMetadata.StackTraces)))
			foreach (var stackTrace in metadata.StackTraces)
				stackTraceArraySerializer.Serialize(stackTrace);
	}

	internal static void SerializeExecutionMetadata(
		this JsonObjectSerializer serializer,
		IExecutionMetadata metadata)
	{
		serializer.Serialize(nameof(IExecutionMetadata.ExecutionTime), metadata.ExecutionTime);
		serializer.Serialize(nameof(IExecutionMetadata.Output), metadata.Output);

		if (metadata.Warnings is not null)
			using (var arraySerializer = serializer.SerializeArray(nameof(IExecutionMetadata.Warnings)))
				foreach (var warning in metadata.Warnings)
					arraySerializer.Serialize(warning);
	}

	internal static void SerializeExecutionSummaryMetadata(
		this JsonObjectSerializer serializer,
		IExecutionSummaryMetadata metadata)
	{
		serializer.Serialize(nameof(IExecutionSummaryMetadata.ExecutionTime), metadata.ExecutionTime);
		serializer.Serialize(nameof(IExecutionSummaryMetadata.TestsFailed), metadata.TestsFailed);
		serializer.Serialize(nameof(IExecutionSummaryMetadata.TestsNotRun), metadata.TestsNotRun);
		serializer.Serialize(nameof(IExecutionSummaryMetadata.TestsSkipped), metadata.TestsSkipped);
		serializer.Serialize(nameof(IExecutionSummaryMetadata.TestsTotal), metadata.TestsTotal);
	}

	internal static void SerializeTestCaseMetadata(
		this JsonObjectSerializer serializer,
		ITestCaseMetadata metadata)
	{
		serializer.Serialize(nameof(ITestCaseMetadata.SkipReason), metadata.SkipReason);
		serializer.Serialize(nameof(ITestCaseMetadata.SourceFilePath), metadata.SourceFilePath);
		serializer.Serialize(nameof(ITestCaseMetadata.SourceLineNumber), metadata.SourceLineNumber);
		serializer.Serialize(nameof(ITestCaseMetadata.TestCaseDisplayName), metadata.TestCaseDisplayName);
		serializer.Serialize(nameof(ITestCaseMetadata.TestClassName), metadata.TestClassName);
		serializer.Serialize(nameof(ITestCaseMetadata.TestClassNamespace), metadata.TestClassNamespace);
		serializer.Serialize(nameof(ITestCaseMetadata.TestMethodName), metadata.TestMethodName);
		serializer.Serialize(nameof(ITestCaseMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestClassMetadata(
		this JsonObjectSerializer serializer,
		ITestClassMetadata metadata)
	{
		serializer.Serialize(nameof(ITestClassMetadata.TestClassName), metadata.TestClassName);
		serializer.Serialize(nameof(ITestClassMetadata.TestClassNamespace), metadata.TestClassNamespace);
		serializer.Serialize(nameof(ITestClassMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestCollectionMetadata(
		this JsonObjectSerializer serializer,
		ITestCollectionMetadata metadata)
	{
		serializer.Serialize(nameof(ITestCollectionMetadata.TestCollectionClassName), metadata.TestCollectionClassName);
		serializer.Serialize(nameof(ITestCollectionMetadata.TestCollectionDisplayName), metadata.TestCollectionDisplayName);
		serializer.Serialize(nameof(ITestCollectionMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestMetadata(
		this JsonObjectSerializer serializer,
		ITestMetadata metadata)
	{
		serializer.Serialize(nameof(ITestMetadata.TestDisplayName), metadata.TestDisplayName);
		serializer.Serialize(nameof(ITestMetadata.Traits), metadata.Traits);
	}

	internal static void SerializeTestMethodMetadata(
		this JsonObjectSerializer serializer,
		ITestMethodMetadata metadata)
	{
		serializer.Serialize(nameof(ITestMethodMetadata.MethodName), metadata.MethodName);
		serializer.Serialize(nameof(ITestMethodMetadata.Traits), metadata.Traits);
	}
}
