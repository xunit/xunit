using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// A helper class that gets the list of test collection definitions for a given assembly.
    /// Reports any misconfigurations of the test assembly via <see cref="IMessageAggregator"/>.
    /// </summary>
    public static class TestCollectionFactoryHelper
    {
        /// <summary>
        /// Gets the test collection definitions for the given assembly.
        /// </summary>
        /// <param name="assemblyInfo">The assembly.</param>
        /// <param name="messageAggregator">The message aggregator.</param>
        /// <returns>A list of mappings from test collection name to test collection definitions (as <see cref="ITypeInfo"/></returns>
        public static Dictionary<string, ITypeInfo> GetTestCollectionDefinitions(IAssemblyInfo assemblyInfo, IMessageAggregator messageAggregator)
        {
            var attributeTypesByName =
                assemblyInfo.GetTypes(false)
                            .Select(type => new { Type = type, Attribute = type.GetCustomAttributes(typeof(CollectionDefinitionAttribute).AssemblyQualifiedName).FirstOrDefault() })
                            .Where(list => list.Attribute != null)
                            .GroupBy(list => (string)list.Attribute.GetConstructorArguments().Single(),
                                     list => list.Type,
                                     StringComparer.OrdinalIgnoreCase);

            var result = new Dictionary<string, ITypeInfo>();

            foreach (var grouping in attributeTypesByName)
            {
                var types = grouping.ToList();
                result[grouping.Key] = types[0];

                if (types.Count > 1)
                    messageAggregator.Add(new EnvironmentalWarning
                    {
                        Message = String.Format("Multiple test collections declared with name '{0}': {1}",
                                                grouping.Key,
                                                String.Join(", ", types.Select(type => type.Name)))
                    });
            }

            return result;
        }
    }
}
