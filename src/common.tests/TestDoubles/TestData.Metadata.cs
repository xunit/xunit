using Xunit.Sdk;

// This file manufactures instances of the test metadata
partial class TestData
{
	public static IAssemblyMetadata AssemblyMetadata(
		string assemblyName = DefaultAssemblyName,
		string assemblyPath = DefaultAssemblyPath,
		string? configFilePath = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = DefaultAssemblyUniqueID) =>
			new _AssemblyMetadata
			{
				AssemblyName = assemblyName,
				AssemblyPath = assemblyPath,
				ConfigFilePath = configFilePath,
				Traits = traits ?? DefaultTraits,
				UniqueID = uniqueID,
			};

	class _AssemblyMetadata : IAssemblyMetadata
	{
		public required string AssemblyName { get; set; }
		public required string AssemblyPath { get; set; }
		public required string? ConfigFilePath { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}

	public static ITestCaseMetadata TestCaseMetadata(
		bool @explicit = false,
		string? skipReason = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		string testCaseDisplayName = DefaultTestCaseDisplayName,
		int? testClassMetadataToken = null,
		string? testClassName = null,
		string? testClassNamespace = null,
		string? testClassSimpleName = null,
		int? testMethodArity = null,
		int? testMethodMetadataToken = null,
		string? testMethodName = null,
		string[]? testMethodParameterTypesVSTest = null,
		string? testMethodReturnTypeVSTest = null,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
		string uniqueID = DefaultTestCaseUniqueID) =>
			new _TestCaseMetadata
			{
				Explicit = @explicit,
				SkipReason = skipReason,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				TestCaseDisplayName = testCaseDisplayName,
				TestClassMetadataToken = testClassMetadataToken,
				TestClassName = testClassName,
				TestClassNamespace = testClassNamespace,
				TestClassSimpleName = testClassSimpleName,
				TestMethodArity = testMethodArity,
				TestMethodMetadataToken = testMethodMetadataToken,
				TestMethodName = testMethodName,
				TestMethodParameterTypesVSTest = testMethodParameterTypesVSTest,
				TestMethodReturnTypeVSTest = testMethodReturnTypeVSTest,
				Traits = traits ?? EmptyTraits,
				UniqueID = uniqueID,
			};

	class _TestCaseMetadata : ITestCaseMetadata
	{
		public required bool Explicit { get; set; }
		public required string? SkipReason { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required string TestCaseDisplayName { get; set; }
		public required int? TestClassMetadataToken { get; set; }
		public required string? TestClassName { get; set; }
		public required string? TestClassNamespace { get; set; }
		public required string? TestClassSimpleName { get; set; }
		public required int? TestMethodArity { get; set; }
		public required int? TestMethodMetadataToken { get; set; }
		public required string? TestMethodName { get; set; }
		public required string[]? TestMethodParameterTypesVSTest { get; set; }
		public required string? TestMethodReturnTypeVSTest { get; set; }
		public required IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits { get; set; }
		public required string UniqueID { get; set; }
	}
}
