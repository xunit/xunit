using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NSubstitute;
using Xunit;
using Xunit.Internal;
using Xunit.v3;

// This file contains mocks of reflection abstractions.
public static partial class Mocks
{
	public static _IAssemblyInfo AssemblyInfo(
		_ITypeInfo[]? types = null,
		_IAttributeInfo[]? attributes = null,
		string? assemblyFileName = null)
	{
		attributes ??= EmptyAttributeInfos;
		assemblyFileName ??= $"assembly-{Guid.NewGuid():n}.dll";

		var result = Substitute.For<_IAssemblyInfo, InterfaceProxy<_IAssemblyInfo>>();
		result.AssemblyPath.Returns(assemblyFileName);
		result.Name.Returns(Path.GetFileNameWithoutExtension(assemblyFileName));
		result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
		result.GetType("").ReturnsForAnyArgs(types?.FirstOrDefault());
		result.GetTypes(true).ReturnsForAnyArgs(types ?? new _ITypeInfo[0]);
		return result;
	}

	static IReadOnlyDictionary<string, IReadOnlyList<string>> GetTraits(_IMethodInfo method)
	{
		var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		foreach (var traitAttribute in method.GetCustomAttributes(typeof(TraitAttribute)))
		{
			var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
			result.Add((string)ctorArgs[0]!, (string)ctorArgs[1]!);
		}

		return result.ToReadOnly();
	}

	public static _IMethodInfo MethodInfo(
		string? methodName = null,
		_IAttributeInfo[]? attributes = null,
		_IParameterInfo[]? parameters = null,
		_ITypeInfo? type = null,
		_ITypeInfo? returnType = null,
		_ITypeInfo[]? genericArguments = null,
		bool isAbstract = false,
		bool isGenericMethodDefinition = false,
		bool isPublic = true,
		bool isStatic = false)
	{
		methodName ??= $"method_{Guid.NewGuid():n}";
		attributes ??= EmptyAttributeInfos;
		parameters ??= EmptyParameterInfos;
		genericArguments ??= EmptyTypeInfos;

		var result = Substitute.For<_IMethodInfo, InterfaceProxy<_IMethodInfo>>();
		result.IsAbstract.Returns(isAbstract);
		result.IsGenericMethodDefinition.Returns(isGenericMethodDefinition);
		result.IsPublic.Returns(isPublic);
		result.IsStatic.Returns(isStatic);
		result.Name.Returns(methodName);
		result.ReturnType.Returns(returnType);
		result.Type.Returns(type);
		result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
		result.GetGenericArguments().Returns(genericArguments);
		result.GetParameters().Returns(parameters);
		// Difficult to simulate MakeGenericMethod here, so better to throw then just return null
		// and if any tests need this, then can override the substitution.
		result.MakeGenericMethod().WhenForAnyArgs(_ => throw new NotImplementedException());
		return result;
	}

	public static _IParameterInfo ParameterInfo(
		string name,
		_ITypeInfo? parameterType = null)
	{
		parameterType ??= TypeObject;

		var result = Substitute.For<_IParameterInfo, InterfaceProxy<_IParameterInfo>>();
		result.Name.Returns(name);
		result.ParameterType.Returns(parameterType);
		return result;
	}

	public static _ITypeInfo TypeInfo(
		string? name = null,
		_IMethodInfo[]? methods = null,
		_IAttributeInfo[]? attributes = null,
		_ITypeInfo? baseType = null,
		_ITypeInfo[]? interfaces = null,
		bool isAbstract = false,
		bool isGenericParameter = false,
		bool isGenericType = false,
		bool isValueType = false,
		_ITypeInfo[]? genericArguments = null,
		string? assemblyFileName = null)
	{
		baseType ??= TypeObject;
		name ??= $"type_{Guid.NewGuid():n}";
		methods ??= EmptyMethodInfos;
		interfaces ??= EmptyTypeInfos;
		genericArguments ??= EmptyTypeInfos;

		var assemblyInfo = AssemblyInfo(assemblyFileName: assemblyFileName);

		var result = Substitute.For<_ITypeInfo, InterfaceProxy<_ITypeInfo>>();
		result.Assembly.Returns(assemblyInfo);
		result.BaseType.Returns(baseType);
		result.IsAbstract.Returns(isAbstract);
		result.IsGenericParameter.Returns(isGenericParameter);
		result.IsGenericType.Returns(isGenericType);
		result.IsValueType.Returns(isValueType);
		result.Name.Returns(name);
		result.GetCustomAttributes("").ReturnsForAnyArgs(callInfo => LookupAttribute(callInfo.Arg<string>(), attributes));
		result.GetGenericArguments().Returns(genericArguments);
		result.GetMethod("", false).ReturnsForAnyArgs(callInfo => methods.FirstOrDefault(m => m.Name == callInfo.Arg<string>()));
		result.GetMethods(false).ReturnsForAnyArgs(methods);
		return result;
	}
}
