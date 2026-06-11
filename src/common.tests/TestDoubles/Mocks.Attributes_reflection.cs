using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

// This file manufactures mocks attributes interfaces
public static partial class Mocks
{
	public static ICollectionBehaviorAttribute CollectionBehaviorAttribute(
		CollectionBehavior collectionBehavior,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new MockCollectionBehaviorAttribute
			{
				CollectionFactoryType = collectionBehavior == CollectionBehavior.CollectionPerAssembly ? typeof(CollectionPerAssemblyTestCollectionFactory) : typeof(CollectionPerClassTestCollectionFactory),
				DisableTestParallelization = disableTestParallelization,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm,
			};

	public static ICollectionBehaviorAttribute CollectionBehaviorAttribute(
		Type? collectionFactoryType = null,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative) =>
			new MockCollectionBehaviorAttribute
			{
				CollectionFactoryType = collectionFactoryType,
				DisableTestParallelization = disableTestParallelization,
				MaxParallelThreads = maxParallelThreads,
				ParallelAlgorithm = parallelAlgorithm,
			};

	class MockCollectionBehaviorAttribute : ICollectionBehaviorAttribute
	{
		public required Type? CollectionFactoryType { get; set; }
		public required bool DisableTestParallelization { get; set; }
		public required int MaxParallelThreads { get; set; }
		public required ParallelAlgorithm ParallelAlgorithm { get; set; }
	}

	public static CustomAttributeData CustomAttributeData(
		Type attributeType,
		params object[] constructorArguments)
	{
		var argumentTypes = constructorArguments.Select(a => a.GetType()).ToArray();
		var ctor =
			attributeType.GetConstructor(argumentTypes)
				?? throw new ArgumentException($"Could not find {attributeType.Name} constructor with argument types: {string.Join(", ", argumentTypes.Select(t => t.SafeName()))}", nameof(constructorArguments));

		return new MockCustomAttributeData(attributeType, ctor, constructorArguments.Select(a => new CustomAttributeTypedArgument(a)).ToArray());
	}

	class MockCustomAttributeData(
		Type attributeType,
		ConstructorInfo constructor,
		CustomAttributeTypedArgument[] constructorArguments) :
			CustomAttributeData
	{
#if !NETFRAMEWORK
		public override Type AttributeType => attributeType;
#endif
		public override ConstructorInfo Constructor => constructor;
		public override IList<CustomAttributeTypedArgument> ConstructorArguments => constructorArguments;
		public override IList<CustomAttributeNamedArgument> NamedArguments => [];
	}

	public static IFactAttribute FactAttribute(
		string? displayName = null,
		bool? @explicit = null,
		string? skip = null,
		Type[]? skipExceptions = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		string? sourceFilePath = null,
		int? sourceLineNumber = null,
		int timeout = 0) =>
			new MockFactAttribute
			{
				DisplayName = displayName,
				Explicit = @explicit ?? false,
				Skip = skip,
				SkipExceptions = skipExceptions,
				SkipType = skipType,
				SkipUnless = skipUnless,
				SkipWhen = skipWhen,
				SourceFilePath = sourceFilePath,
				SourceLineNumber = sourceLineNumber,
				Timeout = timeout,
			};

	class MockFactAttribute : IFactAttribute
	{
		public required string? DisplayName { get; set; }
		public required bool Explicit { get; set; }
		public required string? Skip { get; set; }
		public required Type[]? SkipExceptions { get; set; }
		public required Type? SkipType { get; set; }
		public required string? SkipUnless { get; set; }
		public required string? SkipWhen { get; set; }
		public required string? SourceFilePath { get; set; }
		public required int? SourceLineNumber { get; set; }
		public required int Timeout { get; set; }
	}

	public static IRegisterXunitSerializerAttribute RegisterXunitSerializerAttribute(
		Type serializerType,
		params Type[] supportedTypesForSerialization) =>
			new MockRegisterXunitSerializerAttribute
			{
				SerializerType = serializerType,
				SupportedTypesForSerialization = supportedTypesForSerialization,
			};

	class MockRegisterXunitSerializerAttribute : IRegisterXunitSerializerAttribute
	{
		public required Type SerializerType { get; set; }
		public required Type[] SupportedTypesForSerialization { get; set; }
	}
}
