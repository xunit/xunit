using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Wrapper to return IReflection*Info implementations for System.Reflection types.
/// </summary>
public static class Reflector
{
	static readonly ConcurrentDictionary<(Type enumType, Type valueType), Delegate> enumConverters = new();

	internal readonly static object?[] EmptyArgs = Array.Empty<object?>();
	internal readonly static Type[] EmptyTypes = Array.Empty<Type>();

	readonly static MethodInfo EnumerableCast =
		typeof(Enumerable)
			.GetRuntimeMethods()
			.First(
				m => m.Name == "Cast"
				&& m.GetParameters().Length == 1
				&& m.GetParameters()[0].ParameterType == typeof(IEnumerable)
			);
	readonly static MethodInfo EnumerableToArray =
		typeof(Enumerable)
			.GetRuntimeMethods()
			.First(
				m => m.Name == "ToArray"
				&& m.GetParameters().Length == 1
				&& m.GetParameters()[0].ParameterType.IsGenericType
				&& m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
			);

	/// <summary>
	/// Converts an argument into its target type. Can be particularly useful when pulling attribute
	/// constructor arguments, whose types may not strictly match the parameter type.
	/// </summary>
	/// <param name="arg">The argument to be converted.</param>
	/// <param name="type">The target type for the conversion.</param>
	/// <returns>The converted argument.</returns>
	public static object? ConvertArgument(
		object? arg,
		Type type)
	{
		Guard.ArgumentNotNull(type);

		if (arg is not null && !type.IsAssignableFrom(arg.GetType()))
		{
			try
			{
				if (type.IsArray)
				{
					var elementType = type.GetElementType();
					if (elementType is null)
						throw new ArgumentException("Could not determine array element type", nameof(type));

					if (arg is IReadOnlyCollection<CustomAttributeTypedArgument> attributeArguments)
						return ConvertAttributeArgumentCollection(attributeArguments, elementType);

					var enumerable = (IEnumerable<object>)arg;
					var castMethod = EnumerableCast.MakeGenericMethod(elementType);
					var toArrayMethod = EnumerableToArray.MakeGenericMethod(elementType);
					return toArrayMethod.Invoke(null, new object?[] { castMethod.Invoke(null, new object[] { enumerable }) });
				}
				else if (type.IsEnum)
				{
					var valueType = arg.GetType();
					var enumConverter = enumConverters.GetOrAdd((type, valueType), _ =>
					{
						var parameter = Expression.Parameter(valueType);
						var funcType = typeof(Func<,>).MakeGenericType(valueType, type);
						var dynamicMethod = Expression.Lambda(funcType, Expression.Convert(parameter, type), parameter);
						return dynamicMethod.Compile();
					});
					return enumConverter.DynamicInvoke(arg);
				}
				else
				{
					if (type == typeof(Guid))
						return Guid.Parse(arg.ToString()!);

					if (type == typeof(DateTime))
						return DateTime.Parse(arg.ToString()!, CultureInfo.InvariantCulture);

					if (type == typeof(DateTimeOffset))
						return DateTimeOffset.Parse(arg.ToString()!, CultureInfo.InvariantCulture);

					return Convert.ChangeType(arg, type, CultureInfo.CurrentCulture);
				}
			}
			catch { } // Eat conversion-related exceptions; they'll get re-surfaced during execution
		}

		return arg;
	}

	/// <summary>
	/// Converts arguments into their target types. Can be particularly useful when pulling attribute
	/// constructor arguments, whose types may not strictly match the parameter types.
	/// </summary>
	/// <param name="args">The arguments to be converted.</param>
	/// <param name="types">The target types for the conversion.</param>
	/// <returns>The converted arguments.</returns>
	public static object?[] ConvertArguments(
		object?[]? args,
		Type[]? types)
	{
		if (args is null)
			args = EmptyArgs;
		if (types is null)
			types = EmptyTypes;

		if (args.Length == types.Length)
			for (var idx = 0; idx < args.Length; idx++)
				args[idx] = ConvertArgument(args[idx], types[idx]);

		return args;
	}

	/// <summary>
	/// Converts an argument collection from an attribute initializer into an array of the raw values.
	/// </summary>
	/// <param name="collection">The attribute argument collection.</param>
	/// <param name="elementType">The element type of the array.</param>
	/// <returns>The collection of the raw attribute values.</returns>
	public static Array ConvertAttributeArgumentCollection(
		IReadOnlyCollection<CustomAttributeTypedArgument> collection,
		Type elementType)
	{
		Guard.ArgumentNotNull(collection);

		var result = Array.CreateInstance(elementType, collection.Count);
		var idx = 0;

		foreach (var item in collection)
			result.SetValue(ConvertArgument(item.Value, item.ArgumentType), idx++);

		return result;
	}

	/// <summary>
	/// Converts an <see cref="Assembly"/> into an <see cref="_IReflectionAssemblyInfo"/>.
	/// </summary>
	/// <param name="assembly">The assembly to wrap.</param>
	/// <param name="additionalAssemblyAttributes">Additional custom attributes to return for this assembly. These
	/// attributes will be added to the existing assembly-level attributes that already exist. This is typically
	/// only used for unit/acceptance testing purposes.</param>
	/// <returns>The wrapper</returns>
	[return: NotNullIfNotNull("assembly")]
	public static _IReflectionAssemblyInfo? Wrap(
		Assembly? assembly,
		params _IReflectionAttributeInfo[] additionalAssemblyAttributes) =>
			assembly is null ? null : new ReflectionAssemblyInfo(assembly, additionalAssemblyAttributes);

	/// <summary>
	/// Converts an <see cref="Attribute"/> into an <see cref="_IAttributeInfo"/> using reflection.
	/// </summary>
	/// <param name="attribute">The attribute to wrap.</param>
	/// <returns>The wrapper</returns>
	[return: NotNullIfNotNull("attribute")]
	public static _IReflectionAttributeInfo? Wrap(CustomAttributeData? attribute) =>
		attribute is null ? null : new ReflectionAttributeInfo(attribute);

	/// <summary>
	/// Converts a <see cref="MethodInfo"/> into an <see cref="_IMethodInfo"/> using reflection.
	/// </summary>
	/// <param name="method">The method to wrap</param>
	/// <returns>The wrapper</returns>
	[return: NotNullIfNotNull("method")]
	public static _IReflectionMethodInfo? Wrap(MethodInfo? method) =>
		method is null ? null : new ReflectionMethodInfo(method);

	/// <summary>
	/// Converts a <see cref="ParameterInfo"/> into an <see cref="_IParameterInfo"/> using reflection.
	/// </summary>
	/// <param name="parameter">THe parameter to wrap</param>
	/// <returns>The wrapper</returns>
	[return: NotNullIfNotNull("parameter")]
	public static _IReflectionParameterInfo? Wrap(ParameterInfo? parameter) =>
		parameter is null ? null : new ReflectionParameterInfo(parameter);

	/// <summary>
	/// Converts a <see cref="Type"/> into an <see cref="_ITypeInfo"/> using reflection.
	/// </summary>
	/// <param name="type">The type to wrap</param>
	/// <returns>The wrapper</returns>
	[return: NotNullIfNotNull("type")]
	public static _IReflectionTypeInfo? Wrap(Type? type) =>
		type is null ? null : new ReflectionTypeInfo(type);
}
