using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Utility methods related to <see cref="Type"/>.
/// </summary>
public static class TypeHelper
{
	static readonly MethodInfo enumerableCast =
		typeof(Enumerable)
			.GetRuntimeMethods()
			.First(
				m => m.Name == "Cast"
				&& m.GetParameters().Length == 1
				&& m.GetParameters()[0].ParameterType == typeof(IEnumerable)
			);
	static readonly MethodInfo enumerableToArray =
		typeof(Enumerable)
			.GetRuntimeMethods()
			.First(
				m => m.Name == "ToArray"
				&& m.GetParameters().Length == 1
				&& m.GetParameters()[0].ParameterType.IsGenericType
				&& m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
			);
	static readonly ConcurrentDictionary<string, Type?> typeCache = new();

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
					var elementType = type.GetElementType() ?? throw new ArgumentException("Could not determine array element type", nameof(type));

					if (arg is IReadOnlyCollection<CustomAttributeTypedArgument> attributeArguments)
						return ConvertAttributeArgumentCollection(attributeArguments, elementType);

					var enumerable = (IEnumerable<object>)arg;
					var castMethod = enumerableCast.MakeGenericMethod(elementType);
					var toArrayMethod = enumerableToArray.MakeGenericMethod(elementType);
					return toArrayMethod.Invoke(null, [castMethod.Invoke(null, [enumerable])]);
				}
				else
					return type.IsEnum
					? Enum.ToObject(type, arg)
					: type == typeof(Guid)
						? Guid.Parse(arg.ToString()!)
						: type == typeof(DateTime)
							? DateTime.Parse(arg.ToString()!, CultureInfo.InvariantCulture)
							: type == typeof(DateTimeOffset)
								? DateTimeOffset.Parse(arg.ToString()!, CultureInfo.InvariantCulture)
								: Convert.ChangeType(arg, type, CultureInfo.CurrentCulture);
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
		object?[] args,
		Type[] types)
	{
		Guard.ArgumentNotNull(args);
		Guard.ArgumentNotNull(types);

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
	/// Converts an assembly qualified type name from <see cref="GetTypeName"/> back into
	/// a <see cref="Type"/> object.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
	/// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
	public static Type? GetType(string assemblyQualifiedTypeName) =>
		typeCache.GetOrAdd(
			Guard.ArgumentNotNull(assemblyQualifiedTypeName),
			aqtn =>
			{
				var firstOpenSquare = aqtn.IndexOf('[');
				if (firstOpenSquare > 0)
				{
					var backtick = aqtn.IndexOf('`');
					if (backtick > 0 && backtick < firstOpenSquare)
					{
						// Run the string looking for the matching closing square brace. Can't just assume the last one
						// is the end, since the type could be trailed by array designators.
						var depth = 1;
						var lastOpenSquare = firstOpenSquare + 1;
						var sawNonArrayDesignator = false;
						for (; depth > 0 && lastOpenSquare < aqtn.Length; ++lastOpenSquare)
						{
							switch (aqtn[lastOpenSquare])
							{
								case '[':
									++depth;
									break;
								case ']':
									--depth;
									break;
								case ',':
									break;
								default:
									sawNonArrayDesignator = true;
									break;
							}
						}

						if (sawNonArrayDesignator)
						{
							if (depth != 0)  // Malformed, because we never closed what we opened
								return null;

							var genericArgument = aqtn.Substring(firstOpenSquare + 1, lastOpenSquare - firstOpenSquare - 2);  // Strip surrounding [ and ]
							var innerTypeNames = genericArgument.SplitAtOuterCommas().Select(x => x.Substring(1, x.Length - 2));  // Strip surrounding [ and ] from each type name
							var innerTypes = innerTypeNames.Select(s => GetType(s)).ToArray();
							if (innerTypes.Any(t => t is null))
								return null;

							var genericDefinitionName = aqtn.Substring(0, firstOpenSquare) + aqtn.Substring(lastOpenSquare);
							var genericDefinition = GetType(genericDefinitionName);
							if (genericDefinition is null)
								return null;

							// Push array ranks so we can get down to the actual generic definition
							var arrayRanks = new Stack<int>();
							while (genericDefinition.IsArray)
							{
								arrayRanks.Push(genericDefinition.GetArrayRank());
								genericDefinition = genericDefinition.GetElementType();
								if (genericDefinition is null)
									return null;
							}

							var closedGenericType = genericDefinition.MakeGenericType(innerTypes!);
							while (arrayRanks.Count > 0)
							{
								var rank = arrayRanks.Pop();
								closedGenericType = rank > 1 ? closedGenericType.MakeArrayType(rank) : closedGenericType.MakeArrayType();
							}

							return closedGenericType;
						}
					}
				}

				var parts = aqtn.SplitAtOuterCommas(trimWhitespace: true);
				return parts.Count switch
				{
					0 => null,
					1 => Type.GetType(parts[0]),
					_ => GetType(parts[1], parts[0]),
				};
			}
		);

	/// <summary>
	/// Converts an assembly name + type name into a <see cref="Type"/> object.
	/// </summary>
	/// <param name="assemblyName">The assembly name.</param>
	/// <param name="typeName">The type name.</param>
	/// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
	public static Type? GetType(
		string assemblyName,
		string typeName)
	{
		// Support both long name ("assembly, version=x.x.x.x, etc.") and short name ("assembly")
		var assembly =
			AppDomain
				.CurrentDomain
				.GetAssemblies()
				.FirstOrDefault(a => a.FullName == assemblyName || a.GetName().Name == assemblyName);

		if (assembly is null)
		{
			try
			{
				assembly = Assembly.Load(assemblyName);
			}
			catch { }
		}

		return assembly?.GetType(typeName);
	}

	/// <summary>
	/// Gets an assembly-qualified type name suitable for serialization.
	/// </summary>
	/// <param name="value">The type value</param>
	/// <returns>A string in "TypeName" format (for mscorlib types) or "TypeName,AssemblyName" format (for all others)</returns>
	/// <remarks>
	/// Dynamic types, or types which live in the GAC, are not supported.
	/// </remarks>
	public static string GetTypeName(Type value)
	{
		if (Guard.ArgumentNotNull(value).FullName is null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize typeof({0}) because it has no full name", value.Name), nameof(value));

		// Use the abstract Type instead of concretes like RuntimeType
		if (typeof(Type).IsAssignableFrom(value))
			value = typeof(Type);

		var typeToMap = value;
		if (typeToMap.Assembly.FullName is null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Cannot serialize type '{0}' because its assembly does not have a full name", value.SafeName()), nameof(value));

		var typeName = typeToMap.SafeName();
		var assemblyName = typeToMap.Assembly.FullName.Split(',')[0];

		var arrayRanks = new Stack<int>();
		while (true)
		{
			var elementType = typeToMap.GetElementType();
			if (elementType is null)
				break;

			arrayRanks.Push(typeToMap.GetArrayRank());
			typeToMap = elementType;
		}

		if (typeToMap.IsGenericType && !typeToMap.IsGenericTypeDefinition)
		{
			var typeDefinition = typeToMap.GetGenericTypeDefinition();
			var innerTypes =
				typeToMap
					.GetGenericArguments()
					.Select(t => string.Format(CultureInfo.InvariantCulture, "[{0}]", GetTypeName(t)))
					.ToArray();

			typeName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", typeDefinition.SafeName(), string.Join(",", innerTypes));

			while (arrayRanks.Count > 0)
			{
				typeName += '[';
				for (var commas = arrayRanks.Pop() - 1; commas > 0; --commas)
					typeName += ',';
				typeName += ']';
			}
		}

		return
			string.Equals(assemblyName, "mscorlib", StringComparison.OrdinalIgnoreCase) || string.Equals(assemblyName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase)
				? typeName
				: string.Format(CultureInfo.InvariantCulture, "{0},{1}", typeName, assemblyName);
	}

	/// <summary>
	/// Converts an assembly qualified type name into a <see cref="Type"/> object. If the
	/// type does not exist, throws an <see cref="ArgumentException"/>.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
	/// <returns>The instance of the <see cref="Type"/>.</returns>
	public static Type GetTypeStrict(string assemblyQualifiedTypeName) =>
		GetType(assemblyQualifiedTypeName) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Type name '{0}' could not be found", assemblyQualifiedTypeName), nameof(assemblyQualifiedTypeName));
}
