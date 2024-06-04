using System.Collections.Generic;
using System.Linq;
using Xunit.v3;

namespace Xunit.Internal;

internal static class JsonObjectSerializerExtensions
{
	internal static void DeserializeAssemblyMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableAssemblyMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_IAssemblyMetadata.AssemblyName), out var assemblyNameValue) && assemblyNameValue is string assemblyName)
			metadata.AssemblyName = assemblyName;
		if (obj.TryGetValue(nameof(_IAssemblyMetadata.AssemblyPath), out var assemblyPathValue) && assemblyPathValue is string assemblyPath)
			metadata.AssemblyPath = assemblyPath;
		if (obj.TryGetValue(nameof(_IAssemblyMetadata.ConfigFilePath), out var configFilePathValue) && configFilePathValue is string configFilePath)
			metadata.ConfigFilePath = configFilePath;
	}

	internal static void DeserializeErrorMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableErrorMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_IErrorMetadata.ExceptionParentIndices), out var exceptionParentIndicesValue) && exceptionParentIndicesValue is object?[] exceptionParentIndices)
			metadata.ExceptionParentIndices = exceptionParentIndices.Select(v => (v is decimal d && d % 1 == 0) ? (int?)d : null).WhereNotNull().ToArray();

		if (obj.TryGetValue(nameof(_IErrorMetadata.ExceptionTypes), out var exceptionTypesValue) && exceptionTypesValue is object?[] exceptionTypes)
			metadata.ExceptionTypes = exceptionTypes.Select(x => x as string).ToArray();

		if (obj.TryGetValue(nameof(_IErrorMetadata.Messages), out var messagesValue) && messagesValue is object?[] messages)
			metadata.Messages = messages.Cast<string>().ToArray();

		if (obj.TryGetValue(nameof(_IErrorMetadata.StackTraces), out var stackTracesValue) && stackTracesValue is object?[] stackTraces)
			metadata.StackTraces = stackTraces.Select(x => x as string).ToArray();
	}

	internal static void DeserializeExecutionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableExecutionMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_IExecutionMetadata.ExecutionTime), out var executionTimeValue) && executionTimeValue is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (obj.TryGetValue(nameof(_IExecutionMetadata.Output), out var outputValue) && outputValue is string output)
			metadata.Output = output;
		if (obj.TryGetValue(nameof(_IExecutionMetadata.Warnings), out var warningsValue) && warningsValue is object?[] warnings)
			metadata.Warnings = warnings.OfType<string>().ToArray();
	}

	internal static void DeserializeExecutionSummaryMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableExecutionSummaryMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_IExecutionSummaryMetadata.ExecutionTime), out var executionTimeValue) && executionTimeValue is decimal executionTime)
			metadata.ExecutionTime = executionTime;
		if (obj.TryGetValue(nameof(_IExecutionSummaryMetadata.TestsFailed), out var testsFailedValue) && testsFailedValue is decimal testsFailed && testsFailed % 1 == 0)
			metadata.TestsFailed = (int)testsFailed;
		if (obj.TryGetValue(nameof(_IExecutionSummaryMetadata.TestsNotRun), out var testsNotRunValue) && testsNotRunValue is decimal testsNotRun && testsNotRun % 1 == 0)
			metadata.TestsNotRun = (int)testsNotRun;
		if (obj.TryGetValue(nameof(_IExecutionSummaryMetadata.TestsSkipped), out var testsSkippedValue) && testsSkippedValue is decimal testsSkipped && testsSkipped % 1 == 0)
			metadata.TestsSkipped = (int)testsSkipped;
		if (obj.TryGetValue(nameof(_IExecutionSummaryMetadata.TestsTotal), out var testsTotalValue) && testsTotalValue is decimal testsTotal && testsTotal % 1 == 0)
			metadata.TestsTotal = (int)testsTotal;
	}

	internal static void DeserializeTestCaseMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestCaseMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.SkipReason), out var skipReasonValue) && skipReasonValue is string skipReason)
			metadata.SkipReason = skipReason;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.SourceFilePath), out var sourceFilePathValue) && sourceFilePathValue is string sourceFilePath)
			metadata.SourceFilePath = sourceFilePath;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.SourceLineNumber), out var sourceLineNumberValue) && sourceLineNumberValue is decimal sourceLineNumber && sourceLineNumber % 1 == 0)
			metadata.SourceLineNumber = (int)sourceLineNumber;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.TestCaseDisplayName), out var testCaseDisplayNameValue) && testCaseDisplayNameValue is string testCaseDisplayName)
			metadata.TestCaseDisplayName = testCaseDisplayName;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.TestClassName), out var testClassNameValue) && testClassNameValue is string testClassName)
			metadata.TestClassName = testClassName;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.TestClassNamespace), out var testClassNamespaceValue) && testClassNamespaceValue is string testClassNamespace)
			metadata.TestClassNamespace = testClassNamespace;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.TestClassNameWithNamespace), out var testClassNameWithNamespaceValue) && testClassNameWithNamespaceValue is string testClassNameWithNamespace)
			metadata.TestClassNameWithNamespace = testClassNameWithNamespace;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.TestMethodName), out var testMethodNameValue) && testMethodNameValue is string testMethodName)
			metadata.TestMethodName = testMethodName;
		if (obj.TryGetValue(nameof(_ITestCaseMetadata.Traits), out var traitsValue) && traitsValue is IReadOnlyDictionary<string, object?> traits)
		{
			var result = new Dictionary<string, List<string>>();

			foreach (var kvp in traits)
				if (kvp.Value is object?[] values)
					result[kvp.Key] = values.OfType<string>().WhereNotNull().ToList();

			metadata.Traits = result.ToReadOnly();
		}
	}

	internal static void DeserializeTestClassMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestClassMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_ITestClassMetadata.TestClass), out var testClassValue) && testClassValue is string testClass)
			metadata.TestClass = testClass;
	}

	internal static void DeserializeTestCollectionMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestCollectionMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_ITestCollectionMetadata.TestCollectionClass), out var testCollectionClassValue) && testCollectionClassValue is string testCollectionClass)
			metadata.TestCollectionClass = testCollectionClass;
		if (obj.TryGetValue(nameof(_ITestCollectionMetadata.TestCollectionDisplayName), out var testCollectionDisplayNameValue) && testCollectionDisplayNameValue is string testCollectionDisplayName)
			metadata.TestCollectionDisplayName = testCollectionDisplayName;
	}

	internal static void DeserializeTestMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_ITestMetadata.Explicit), out var explicitValue) && explicitValue is bool @explicit)
			metadata.Explicit = @explicit;
		if (obj.TryGetValue(nameof(_ITestMetadata.TestDisplayName), out var testDisplayNameValue) && testDisplayNameValue is string testDisplayName)
			metadata.TestDisplayName = testDisplayName;
		if (obj.TryGetValue(nameof(_ITestMetadata.Timeout), out var timeoutValue) && timeoutValue is decimal timeout && timeout % 1 == 0)
			metadata.Timeout = (int)timeout;
		if (obj.TryGetValue(nameof(_ITestMetadata.Traits), out var traitsValue) && traitsValue is IReadOnlyDictionary<string, object?> traits)
		{
			var result = new Dictionary<string, List<string>>();

			foreach (var kvp in traits)
				if (kvp.Value is object?[] values)
					result[kvp.Key] = values.OfType<string>().WhereNotNull().ToList();

			metadata.Traits = result.ToReadOnly();
		}
	}

	internal static void DeserializeTestMethodMetadata(
		this IReadOnlyDictionary<string, object?> obj,
		_IWritableTestMethodMetadata metadata)
	{
		if (obj.TryGetValue(nameof(_ITestMethodMetadata.TestMethod), out var testMethodValue) && testMethodValue is string testMethod)
			metadata.TestMethod = testMethod;
	}

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
