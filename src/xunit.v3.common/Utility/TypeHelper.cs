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
				else if (type.IsEnum)
					return Enum.ToObject(type, arg);
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
	/// Converts an assembly qualified type name into a <see cref="Type"/> object.
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
							var innerTypeNames = SplitAtOuterCommas(genericArgument).Select(x => x.Substring(1, x.Length - 2));  // Strip surrounding [ and ] from each type name
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

				var parts = SplitAtOuterCommas(aqtn, true);
				return
					parts.Count == 0 ? null :
					parts.Count == 1 ? Type.GetType(parts[0]) :
					GetType(parts[1], parts[0]);
			}
		);

	/// <summary>
	/// Converts an assembly qualified type name into a <see cref="Type"/> object. If the
	/// type does not exist, throws an <see cref="ArgumentException"/>.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
	/// <returns>The instance of the <see cref="Type"/>.</returns>
	public static Type GetTypeStrict(string assemblyQualifiedTypeName) =>
		GetType(assemblyQualifiedTypeName) ?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Type name '{0}' could not be found", assemblyQualifiedTypeName), nameof(assemblyQualifiedTypeName));

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

		if (assembly is null)
			return null;

		return assembly.GetType(typeName);
	}

	static IList<string> SplitAtOuterCommas(
		string value,
		bool trimWhitespace = false)
	{
		var results = new List<string>();

		var startIndex = 0;
		var endIndex = 0;
		var depth = 0;

		for (; endIndex < value.Length; ++endIndex)
		{
			switch (value[endIndex])
			{
				case '[':
					++depth;
					break;

				case ']':
					--depth;
					break;

				case ',':
					if (depth == 0)
					{
						results.Add(
							trimWhitespace
								? SubstringTrim(value, startIndex, endIndex - startIndex)
								: value.Substring(startIndex, endIndex - startIndex)
						);

						startIndex = endIndex + 1;
					}
					break;
			}
		}

		if (depth != 0 || startIndex >= endIndex)
			results.Clear();
		else
			results.Add(
				trimWhitespace
					? SubstringTrim(value, startIndex, endIndex - startIndex)
					: value.Substring(startIndex, endIndex - startIndex)
			);

		return results;
	}

	static string SubstringTrim(
		string str,
		int startIndex,
		int length)
	{
		var endIndex = startIndex + length;

		while (startIndex < endIndex && char.IsWhiteSpace(str[startIndex]))
			startIndex++;

		while (endIndex > startIndex && char.IsWhiteSpace(str[endIndex - 1]))
			endIndex--;

		return str.Substring(startIndex, endIndex - startIndex);
	}
}
