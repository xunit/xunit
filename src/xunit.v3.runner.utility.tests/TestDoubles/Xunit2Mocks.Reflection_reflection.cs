#pragma warning disable xUnit3000 // These derive from the "wrong" version (v3 instead of v2) of LLMBRO, which is acceptable

using Xunit.Abstractions;
using LongLivedMarshalByRefObject = Xunit.Sdk.LongLivedMarshalByRefObject;

namespace Xunit.Runner.v2;

// This file manufactures mocks reflection information
partial class Xunit2Mocks
{
	public static IAssemblyInfo AssemblyInfo(
		string? assemblyFileName = null,
		IReflectionAttributeInfo[]? customAttributes = null,
		ITypeInfo[]? types = null) =>
			new MockAssemblyInfo(customAttributes ?? [], types ?? [])
			{
				AssemblyPath = assemblyFileName,
				Name = assemblyFileName is null ? "assembly:" + Guid.NewGuid().ToString("n") : Path.GetFileNameWithoutExtension(assemblyFileName)
			};

	class MockAssemblyInfo(
		IReflectionAttributeInfo[] customAttributes,
		ITypeInfo[] types) :
			LongLivedMarshalByRefObject, IAssemblyInfo
	{
		public required string? AssemblyPath { get; set; }
		public required string Name { get; set; }

		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => LookupAttribute(assemblyQualifiedAttributeTypeName, customAttributes);
		public ITypeInfo? GetType(string typeName) => types.FirstOrDefault(t => t.Name == typeName);
		public IEnumerable<ITypeInfo> GetTypes(bool includePrivateTypes) => types;
	}

	static IEnumerable<IAttributeInfo> LookupAttribute(
		string fullyQualifiedTypeName,
		IReflectionAttributeInfo[]? customAttributes)
	{
		if (customAttributes is null)
			return [];

		var attributeType = Type.GetType(fullyQualifiedTypeName);
		if (attributeType is null)
			return [];

		return customAttributes.Where(attribute => attributeType.IsAssignableFrom(attribute.Attribute.GetType())).ToList();
	}

	public static IMethodInfo MethodInfo(
		IReflectionAttributeInfo[]? customAttributes = null,
		ITypeInfo[]? genericArguments = null,
		bool isAbstract = false,
		bool isGenericMethodDefinition = false,
		bool isPublic = true,
		bool isStatic = false,
		string? name = null,
		ITypeInfo? returnType = null,
		ITypeInfo? type = null,
		IParameterInfo[]? parameters = null) =>
			new MockMethodInfo(customAttributes, genericArguments ?? [], parameters ?? [])
			{
				IsAbstract = isAbstract,
				IsGenericMethodDefinition = isGenericMethodDefinition,
				IsPublic = isPublic,
				IsStatic = isStatic,
				Name = name ?? "test-method-name",
				ReturnType = returnType ?? TypeOfVoid,
				Type = type ?? TypeInfo(),
			};

	class MockMethodInfo(
		IReflectionAttributeInfo[]? customAttributes,
		ITypeInfo[] genericArguments,
		IParameterInfo[] parameters) :
			LongLivedMarshalByRefObject, IMethodInfo
	{
		public required bool IsAbstract { get; set; }
		public required bool IsGenericMethodDefinition { get; set; }
		public required bool IsPublic { get; set; }
		public required bool IsStatic { get; set; }
		public required string Name { get; set; }
		public required ITypeInfo ReturnType { get; set; }
		public required ITypeInfo Type { get; set; }

		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => LookupAttribute(assemblyQualifiedAttributeTypeName, customAttributes);
		public IEnumerable<ITypeInfo> GetGenericArguments() => genericArguments;
		public IEnumerable<IParameterInfo> GetParameters() => parameters;
		public IMethodInfo MakeGenericMethod(params ITypeInfo[] typeArguments) => throw new InvalidOperationException("Using IMethodInfo.MakeGenericMethod while testing is prohibited");
	}

	public static IReflectionAttributeInfo ReflectionAttributeInfo(
		Attribute attribute,
		object?[]? constructorArguments = null,
		IAttributeInfo[]? customAttributes = null,
		KeyValuePair<string, object?>[]? namedArguments = null) =>
			new MockReflectionAttributeInfo(constructorArguments ?? [], customAttributes ?? [], namedArguments ?? [])
			{
				Attribute = attribute,
			};

	class MockReflectionAttributeInfo(
		object?[] constructorArguments,
		IAttributeInfo[] customAttributes,
		KeyValuePair<string, object?>[] namedArguments) :
			LongLivedMarshalByRefObject, IReflectionAttributeInfo
	{
		public required Attribute Attribute { get; set; }

		public IEnumerable<object?> GetConstructorArguments() => constructorArguments;
		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => customAttributes;
		public TValue? GetNamedArgument<TValue>(string argumentName)
		{
			var match = namedArguments.FirstOrDefault(kvp => kvp.Key == argumentName);
			if (match.Key is not null && match.Value is TValue value)
				return value;

			return default;
		}
	}

	public static ITypeInfo TypeInfo(
		string? assemblyFileName = null,
		ITypeInfo? baseType = null,
		IReflectionAttributeInfo[]? customAttributes = null,
		ITypeInfo[]? genericArguments = null,
		ITypeInfo[]? interfaces = null,
		bool isAbstract = false,
		bool isGenericParameter = false,
		bool isGenericType = false,
		bool isSealed = false,
		bool isValueType = false,
		IMethodInfo[]? methods = null,
		string? name = null) =>
			new MockTypeInfo(customAttributes ?? [], genericArguments ?? [], methods ?? [])
			{
				Assembly = AssemblyInfo(assemblyFileName),
				BaseType = baseType,
				Interfaces = interfaces ?? [],
				IsAbstract = isAbstract,
				IsGenericParameter = isGenericParameter,
				IsGenericType = isGenericType,
				IsSealed = isSealed,
				IsValueType = isValueType,
				Name = name ?? "type:" + Guid.NewGuid().ToString("n"),
			};

	class MockTypeInfo(
		IReflectionAttributeInfo[] customAttributes,
		ITypeInfo[] genericArguments,
		IMethodInfo[] methods) :
			LongLivedMarshalByRefObject, ITypeInfo
	{
		public required IAssemblyInfo Assembly { get; set; }
		public required ITypeInfo? BaseType { get; set; }
		public required IEnumerable<ITypeInfo> Interfaces { get; set; }
		public required bool IsAbstract { get; set; }
		public required bool IsGenericParameter { get; set; }
		public required bool IsGenericType { get; set; }
		public required bool IsSealed { get; set; }
		public required bool IsValueType { get; set; }
		public required string Name { get; set; }

		public IEnumerable<IAttributeInfo> GetCustomAttributes(string assemblyQualifiedAttributeTypeName) => LookupAttribute(assemblyQualifiedAttributeTypeName, customAttributes);
		public IEnumerable<ITypeInfo> GetGenericArguments() => genericArguments;
		public IMethodInfo? GetMethod(string methodName, bool includePrivateMethod) => methods.FirstOrDefault(m => m.Name == methodName && (includePrivateMethod || m.IsPublic));
		public IEnumerable<IMethodInfo> GetMethods(bool includePrivateMethods) => methods;
	}
}
