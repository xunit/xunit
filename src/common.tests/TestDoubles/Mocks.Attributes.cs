using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

// This file contains mocks of reflection-based attribute information.
public static partial class Mocks
{
	static readonly _IReflectionAttributeInfo[] EmptyAttributeInfos = new _IReflectionAttributeInfo[0];
	static readonly _IReflectionMethodInfo[] EmptyMethodInfos = new _IReflectionMethodInfo[0];
	static readonly _IReflectionParameterInfo[] EmptyParameterInfos = new _IReflectionParameterInfo[0];
	static readonly _IReflectionTypeInfo[] EmptyTypeInfos = new _IReflectionTypeInfo[0];

	public static _IReflectionAttributeInfo AssemblyFixtureAttribute(Type fixtureType)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new AssemblyFixtureAttribute(fixtureType));
		result.AttributeType.Returns(Reflector.Wrap(typeof(AssemblyFixtureAttribute)));
		result.GetConstructorArguments().Returns(new[] { fixtureType });
		return result;
	}

	public static _IReflectionAttributeInfo CollectionAttribute(string collectionName)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new CollectionAttribute(collectionName));
		result.AttributeType.Returns(Reflector.Wrap(typeof(CollectionAttribute)));
		result.GetConstructorArguments().Returns(new[] { collectionName });
		return result;
	}

	public static _IReflectionAttributeInfo CollectionBehaviorAttribute(
		CollectionBehavior? collectionBehavior = null,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0)
	{
		var ctorArgs = collectionBehavior is null ? EmptyObjects : new object[] { collectionBehavior };
		return CollectionBehaviorAttribute(disableTestParallelization, maxParallelThreads, ctorArgs);
	}

	public static _IReflectionAttributeInfo CollectionBehaviorAttribute(
		string factoryTypeName,
		string factoryAssemblyName,
		bool disableTestParallelization = false,
		int maxParallelThreads = 0) =>
			CollectionBehaviorAttribute(disableTestParallelization, maxParallelThreads, factoryTypeName, factoryAssemblyName);

	public static _IReflectionAttributeInfo CollectionBehaviorAttribute<TTestCollectionFactory>(
		bool disableTestParallelization = false,
		int maxParallelThreads = 0) =>
			CollectionBehaviorAttribute(disableTestParallelization, maxParallelThreads, typeof(TTestCollectionFactory));

	static _IReflectionAttributeInfo CollectionBehaviorAttribute(
		bool disableTestParallelization,
		int maxParallelThreads,
		params object?[] constructorArguments)
	{
		var attribute = Activator.CreateInstance(typeof(CollectionBehaviorAttribute), constructorArguments) as CollectionBehaviorAttribute;
		Guard.NotNull($"Could not create an instance of '{typeof(CollectionBehaviorAttribute).FullName}'", attribute);
		attribute.DisableTestParallelization = disableTestParallelization;
		attribute.MaxParallelThreads = maxParallelThreads;

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(CollectionBehaviorAttribute)));
		result.GetConstructorArguments().Returns(constructorArguments);
		result.GetNamedArgument<bool>(nameof(Xunit.CollectionBehaviorAttribute.DisableTestParallelization)).Returns(disableTestParallelization);
		result.GetNamedArgument<int>(nameof(Xunit.CollectionBehaviorAttribute.MaxParallelThreads)).Returns(maxParallelThreads);
		return result;
	}

	public static _IReflectionAttributeInfo CollectionDefinitionAttribute(string collectionName)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new CollectionDefinitionAttribute(collectionName));
		result.AttributeType.Returns(Reflector.Wrap(typeof(CollectionDefinitionAttribute)));
		result.GetConstructorArguments().Returns(new[] { collectionName });
		return result;
	}

	public static _IReflectionAttributeInfo DataAttribute(
		IEnumerable<object?[]>? data = null,
		bool? @explicit = null,
		string? skip = null)
	{
		data ??= Array.Empty<object?[]>();

		var dataRows =
			data
				.Select(d => new TheoryDataRow(d))
				.CastOrToReadOnlyCollection();

		var dataAttribute = Substitute.For<DataAttribute>();
		dataAttribute.Explicit = @explicit.GetValueOrDefault();
		dataAttribute.Skip = skip;
		dataAttribute.GetData(null!, null!).ReturnsForAnyArgs(dataRows);

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(dataAttribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(DataAttribute)));
		result.GetNamedArgument<bool?>(nameof(Xunit.Sdk.DataAttribute.Explicit)).Returns(@explicit);
		result.GetNamedArgument<string?>(nameof(Xunit.Sdk.DataAttribute.Skip)).Returns(skip);
		return result;
	}

	public static _IReflectionAttributeInfo DataAttribute(params ITheoryDataRow[] data)
	{
		var dataAttribute = Substitute.For<DataAttribute>();
		dataAttribute.GetData(null!, null!).ReturnsForAnyArgs(data.CastOrToReadOnlyCollection());

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(dataAttribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(DataAttribute)));
		result.GetNamedArgument<bool?>(nameof(Xunit.Sdk.DataAttribute.Explicit)).Returns(default(bool?));
		result.GetNamedArgument<string?>(nameof(Xunit.Sdk.DataAttribute.Skip)).Returns(default(string));
		return result;
	}

	public static _IReflectionAttributeInfo FactAttribute(
		string? displayName = null,
		bool? @explicit = null,
		string? skip = null,
		int timeout = 0)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
		result.AttributeType.Returns(Reflector.Wrap(typeof(FactAttribute)));
		result.GetNamedArgument<string?>(nameof(Xunit.FactAttribute.DisplayName)).Returns(displayName);
		result.GetNamedArgument<bool>(nameof(Xunit.FactAttribute.Explicit)).Returns(@explicit ?? false);
		result.GetNamedArgument<string?>(nameof(Xunit.FactAttribute.Skip)).Returns(skip);
		result.GetNamedArgument<int>(nameof(Xunit.FactAttribute.Timeout)).Returns(timeout);
		return result;
	}

	static IReadOnlyCollection<_IAttributeInfo> LookupAttribute(
		string fullyQualifiedTypeName,
		_IAttributeInfo[]? attributes)
	{
		if (attributes is null)
			return EmptyAttributeInfos;

		var attributeType = Type.GetType(fullyQualifiedTypeName);
		if (attributeType is null)
			return EmptyAttributeInfos;

		return
			attributes
				.Where(attribute => attributeType.IsAssignableFrom(attribute.AttributeType))
				.CastOrToReadOnlyCollection();
	}

	static IReadOnlyCollection<_IReflectionAttributeInfo> LookupAttribute<TLookupType, TAttributeType>()
		where TAttributeType : Attribute =>
			typeof(TLookupType)
				.GetCustomAttributesData()
				.Where(cad => typeof(TAttributeType).IsAssignableFrom(cad.AttributeType))
				.Select(cad => Reflector.Wrap(cad))
				.CastOrToReadOnlyCollection();

	public static _IReflectionAttributeInfo TestCaseOrdererAttribute(
		string typeName,
		string assemblyName)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new TestCaseOrdererAttribute(typeName, assemblyName));
		result.AttributeType.Returns(Reflector.Wrap(typeof(TestCaseOrdererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		return result;
	}

	public static _IReflectionAttributeInfo TestCaseOrdererAttribute(Type type)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new TestCaseOrdererAttribute(type));
		result.AttributeType.Returns(Reflector.Wrap(typeof(TestCaseOrdererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { type });
		return result;
	}

	public static _IReflectionAttributeInfo TestCaseOrdererAttribute<TOrdererAttribute>()
		where TOrdererAttribute : ITestCaseOrderer =>
			TestCaseOrdererAttribute(typeof(TOrdererAttribute));

	public static _IReflectionAttributeInfo TestCollectionOrdererAttribute(string typeName, string assemblyName)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new TestCollectionOrdererAttribute(typeName, assemblyName));
		result.AttributeType.Returns(Reflector.Wrap(typeof(TestCollectionOrdererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		return result;
	}

	public static _IReflectionAttributeInfo TestCollectionOrdererAttribute(Type type)
	{
		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(new TestCollectionOrdererAttribute(type));
		result.AttributeType.Returns(Reflector.Wrap(typeof(TestCollectionOrdererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { type });
		return result;
	}

	public static _IReflectionAttributeInfo TestCollectionOrdererAttribute<TOrdererAttribute>()
		where TOrdererAttribute : ITestCollectionOrderer =>
			TestCollectionOrdererAttribute(typeof(TOrdererAttribute));

	public static _IReflectionAttributeInfo TestFrameworkAttribute<TTestFrameworkAttribute>()
		where TTestFrameworkAttribute : Attribute, ITestFrameworkAttribute, new()
	{
		var attribute = new TTestFrameworkAttribute();

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TTestFrameworkAttribute)));
		result.GetCustomAttributes(null!).ReturnsForAnyArgs(
			callInfo => LookupAttribute(
				callInfo.Arg<string>(),
				CustomAttributeData.GetCustomAttributes(attribute.GetType()).Select(x => Reflector.Wrap(x)).ToArray()
			)
		);
		return result;
	}

	public static _IReflectionAttributeInfo TheoryAttribute(
		string? displayName = null,
		bool @explicit = false,
		string? skip = null,
		int timeout = 0)
	{
		var attribute = new TheoryAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout };

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TheoryAttribute)));
		result.GetNamedArgument<string>(nameof(Xunit.FactAttribute.DisplayName)).Returns(displayName);
		result.GetNamedArgument<string>(nameof(Xunit.FactAttribute.Skip)).Returns(skip);
		result.GetNamedArgument<int>(nameof(Xunit.FactAttribute.Timeout)).Returns(timeout);
		result.GetNamedArgument<bool>(nameof(Xunit.FactAttribute.Explicit)).Returns(@explicit);
		return result;
	}

	public static _IReflectionAttributeInfo TraitAttribute(
		string name,
		string value)
	{
		var attribute = new TraitAttribute(name, value);
		var traitDiscovererAttributes = new[] { TraitDiscovererAttribute() };

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TraitAttribute)));
		result.GetConstructorArguments().Returns(new object[] { name, value });
		result.GetCustomAttributes(typeof(TraitDiscovererAttribute)).Returns(traitDiscovererAttributes);
		return result;
	}

	public static _IReflectionAttributeInfo TraitAttribute<TTraitAttribute>()
		where TTraitAttribute : Attribute, ITraitAttribute, new()
	{
		var attribute = new TTraitAttribute();
		var traitDiscovererAttributes = LookupAttribute<TTraitAttribute, TraitDiscovererAttribute>();

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TTraitAttribute)));
		result.GetCustomAttributes(typeof(TraitDiscovererAttribute)).Returns(traitDiscovererAttributes);
		return result;
	}

	public static _IReflectionAttributeInfo TraitDiscovererAttribute(
		string typeName = "Xunit.Sdk.TraitDiscoverer",
		string assemblyName = "xunit.v3.core")
	{
		var attribute = new TraitDiscovererAttribute(typeName, assemblyName);

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TraitDiscovererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
		return result;
	}

	public static _IReflectionAttributeInfo TraitDiscovererAttribute<TTraitDiscoverer>()
		where TTraitDiscoverer : ITraitDiscoverer
	{
		var attribute = new TraitDiscovererAttribute(typeof(TTraitDiscoverer));

		var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
		result.Attribute.Returns(attribute);
		result.AttributeType.Returns(Reflector.Wrap(typeof(TraitDiscovererAttribute)));
		result.GetConstructorArguments().Returns(new object[] { typeof(TTraitDiscoverer) });
		return result;
	}
}
