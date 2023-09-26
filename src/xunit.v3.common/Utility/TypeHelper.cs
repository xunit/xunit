using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;

namespace Xunit.Sdk;

/// <summary>
/// Utility methods related to <see cref="Type"/>.
/// </summary>
public static class TypeHelper
{
	/// <summary>
	/// Converts an assembly qualified type name into a <see cref="Type"/> object.
	/// </summary>
	/// <param name="assemblyQualifiedTypeName">The assembly qualified type name.</param>
	/// <returns>The instance of the <see cref="Type"/>, if available; <c>null</c>, otherwise.</returns>
	public static Type? GetType(string assemblyQualifiedTypeName)
	{
		Guard.ArgumentNotNull(assemblyQualifiedTypeName);

		var firstOpenSquare = assemblyQualifiedTypeName.IndexOf('[');
		if (firstOpenSquare > 0)
		{
			var backtick = assemblyQualifiedTypeName.IndexOf('`');
			if (backtick > 0 && backtick < firstOpenSquare)
			{
				// Run the string looking for the matching closing square brace. Can't just assume the last one
				// is the end, since the type could be trailed by array designators.
				var depth = 1;
				var lastOpenSquare = firstOpenSquare + 1;
				var sawNonArrayDesignator = false;
				for (; depth > 0 && lastOpenSquare < assemblyQualifiedTypeName.Length; ++lastOpenSquare)
				{
					switch (assemblyQualifiedTypeName[lastOpenSquare])
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

					var genericArgument = assemblyQualifiedTypeName.Substring(firstOpenSquare + 1, lastOpenSquare - firstOpenSquare - 2);  // Strip surrounding [ and ]
					var innerTypeNames = SplitAtOuterCommas(genericArgument).Select(x => x.Substring(1, x.Length - 2));  // Strip surrounding [ and ] from each type name
					var innerTypes = innerTypeNames.Select(s => GetType(s)).ToArray();
					if (innerTypes.Any(t => t is null))
						return null;

					var genericDefinitionName = assemblyQualifiedTypeName.Substring(0, firstOpenSquare) + assemblyQualifiedTypeName.Substring(lastOpenSquare);
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

		var parts = SplitAtOuterCommas(assemblyQualifiedTypeName, true);
		return
			parts.Count == 0 ? null :
			parts.Count == 1 ? Type.GetType(parts[0]) :
			GetType(parts[1], parts[0]);
	}

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
