using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	// This file contains mocks of reflection-based attribute information.
	public static partial class Mocks
	{
		static readonly _IReflectionAttributeInfo[] EmptyAttributeInfos = new _IReflectionAttributeInfo[0];
		static readonly _IReflectionMethodInfo[] EmptyMethodInfos = new _IReflectionMethodInfo[0];
		static readonly _IReflectionParameterInfo[] EmptyParameterInfos = new _IReflectionParameterInfo[0];
		static readonly _IReflectionTypeInfo[] EmptyTypeInfos = new _IReflectionTypeInfo[0];

		public static _IReflectionAttributeInfo CollectionAttribute(string collectionName)
		{
			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(new CollectionAttribute(collectionName));
			result.GetConstructorArguments().Returns(new[] { collectionName });
			return result;
		}

		public static _IReflectionAttributeInfo CollectionBehaviorAttribute(
			CollectionBehavior? collectionBehavior = null,
			bool disableTestParallelization = false,
			int maxParallelThreads = 0)
		{
			var ctorArgs = collectionBehavior == null ? EmptyObjects : new object[] { collectionBehavior };
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
			int maxParallelThreads = 0)
				where TTestCollectionFactory : IXunitTestCollectionFactory =>
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
			result.GetConstructorArguments().Returns(constructorArguments);
			result.GetNamedArgument<bool>("DisableTestParallelization").Returns(disableTestParallelization);
			result.GetNamedArgument<int>("MaxParallelThreads").Returns(maxParallelThreads);
			return result;
		}

		public static _IReflectionAttributeInfo CollectionDefinitionAttribute(string collectionName)
		{
			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(new CollectionDefinitionAttribute(collectionName));
			result.GetConstructorArguments().Returns(new[] { collectionName });
			return result;
		}

		public static _IReflectionAttributeInfo DataAttribute(
			IEnumerable<object[]>? data = null,
			string? skip = null)
		{
			var dataAttribute = Substitute.For<DataAttribute>();
			dataAttribute.Skip.Returns(skip);
			dataAttribute.GetData(null!).ReturnsForAnyArgs(data.CastOrToReadOnlyCollection());

			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(dataAttribute);
			result.GetNamedArgument<string?>("Skip").Returns(skip);
			return result;
		}

		public static _IReflectionAttributeInfo FactAttribute(
			string? displayName = null,
			string? skip = null,
			int timeout = 0)
		{
			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(new FactAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout });
			result.GetNamedArgument<string?>("DisplayName").Returns(displayName);
			result.GetNamedArgument<string?>("Skip").Returns(skip);
			result.GetNamedArgument<int>("Timeout").Returns(timeout);
			return result;
		}

		static IReadOnlyCollection<_IReflectionAttributeInfo> LookupAttribute(
			string fullyQualifiedTypeName,
			_IReflectionAttributeInfo[]? attributes)
		{
			if (attributes == null)
				return EmptyAttributeInfos;

			var attributeType = Type.GetType(fullyQualifiedTypeName);
			if (attributeType == null)
				return EmptyAttributeInfos;

			return
				attributes
					.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType()))
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
			result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
			return result;
		}

		public static _IReflectionAttributeInfo TestCaseOrdererAttribute(Type type)
		{
			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(new TestCaseOrdererAttribute(type));
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
			result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
			return result;
		}

		public static _IReflectionAttributeInfo TestCollectionOrdererAttribute(Type type)
		{
			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(new TestCollectionOrdererAttribute(type));
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
			string? skip = null,
			int timeout = 0)
		{
			var attribute = new TheoryAttribute { DisplayName = displayName, Skip = skip, Timeout = timeout };

			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(attribute);
			result.GetNamedArgument<string>("DisplayName").Returns(displayName);
			result.GetNamedArgument<string>("Skip").Returns(skip);
			result.GetNamedArgument<int>("Timeout").Returns(timeout);
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
			result.GetConstructorArguments().Returns(new object[] { typeName, assemblyName });
			return result;
		}

		public static _IReflectionAttributeInfo TraitDiscovererAttribute<TTraitDiscoverer>()
			where TTraitDiscoverer : ITraitDiscoverer
		{
			var attribute = new TraitDiscovererAttribute(typeof(TTraitDiscoverer));

			var result = Substitute.For<_IReflectionAttributeInfo, InterfaceProxy<_IReflectionAttributeInfo>>();
			result.Attribute.Returns(attribute);
			result.GetConstructorArguments().Returns(new object[] { typeof(TTraitDiscoverer) });
			return result;
		}
	}
}
