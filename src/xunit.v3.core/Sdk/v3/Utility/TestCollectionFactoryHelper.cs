using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// A helper class that gets the list of test collection definitions for a given assembly.
/// Reports any misconfigurations of the test assembly via the diagnostic message sink.
/// </summary>
public static class TestCollectionFactoryHelper
{
	/// <summary>
	/// Gets the test collection definitions for the given assembly.
	/// </summary>
	/// <param name="assemblyInfo">The assembly.</param>
	/// <returns>A list of mappings from test collection name to test collection definitions (as <see cref="_ITypeInfo"/></returns>
	public static Dictionary<string, _ITypeInfo> GetTestCollectionDefinitions(_IAssemblyInfo assemblyInfo)
	{
		Guard.ArgumentNotNull(assemblyInfo);

		var attributeTypesByName =
			assemblyInfo
				.GetTypes(false)
				.Select(type => new { Type = type, Attribute = type.GetCustomAttributes(typeof(CollectionDefinitionAttribute)).FirstOrDefault() })
				.Where(list => list.Attribute is not null)
				.GroupBy(
					list => list.Attribute!.GetConstructorArguments().Cast<string>().SingleOrDefault()
						?? UniqueIDGenerator.ForType(list.Type),
					list => list.Type,
					StringComparer.OrdinalIgnoreCase
				);

		var result = new Dictionary<string, _ITypeInfo>();

		foreach (var grouping in attributeTypesByName)
		{
			var types = grouping.ToList();
			result[grouping.Key] = types[0];

			if (types.Count > 1)
				TestContext.Current?.SendDiagnosticMessage("Multiple test collections declared with name '{0}': {1}", grouping.Key, string.Join(", ", types.Select(type => type.Name)));
		}

		return result;
	}
}
