using System;
using NSubstitute;
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
			CollectionBehaviorAttribute(
				collectionBehavior == CollectionBehavior.CollectionPerClass ? typeof(CollectionPerClassTestCollectionFactory) : typeof(CollectionPerAssemblyTestCollectionFactory),
				disableTestParallelization,
				maxParallelThreads,
				parallelAlgorithm
			);

	public static ICollectionBehaviorAttribute CollectionBehaviorAttribute(
		Type? collectionFactoryType = null,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0,
		ParallelAlgorithm parallelAlgorithm = ParallelAlgorithm.Conservative)
	{
		var result = Substitute.For<ICollectionBehaviorAttribute, InterfaceProxy<ICollectionBehaviorAttribute>>();
		result.CollectionFactoryType.Returns(collectionFactoryType);
		result.DisableTestParallelization.Returns(disableTestParallelization);
		result.MaxParallelThreads.Returns(maxParallelThreads);
		result.ParallelAlgorithm.Returns(parallelAlgorithm);
		return result;
	}

	public static IFactAttribute FactAttribute(
		string? displayName = null,
		bool? @explicit = null,
		string? skip = null,
		Type[]? skipExceptions = null,
		Type? skipType = null,
		string? skipUnless = null,
		string? skipWhen = null,
		int timeout = 0)
	{
		var result = Substitute.For<IFactAttribute, InterfaceProxy<IFactAttribute>>();
		result.DisplayName.Returns(displayName);
		result.Explicit.Returns(@explicit ?? false);
		result.Skip.Returns(skip);
		result.SkipExceptions.Returns(skipExceptions);
		result.SkipType.Returns(skipType);
		result.SkipUnless.Returns(skipUnless);
		result.SkipWhen.Returns(skipWhen);
		result.Timeout.Returns(timeout);
		return result;
	}
}
