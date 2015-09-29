using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// A helper class to retrieve the traits from a method.
    /// </summary>
    public static class TraitHelper
    {
        /// <summary>
        /// Get the traits from a method.
        /// </summary>
        /// <param name="member">The member (method, field, etc.) to get the traits for.</param>
        /// <returns>A list of traits that are defined on the method.</returns>
        public static IReadOnlyList<KeyValuePair<string, string>> GetTraits(MemberInfo member)
        {
            var messageSink = new NullMessageSink();
            var result = new List<KeyValuePair<string, string>>();

            foreach (var traitAttributeData in member.CustomAttributes)
            {
                var traitAttributeType = traitAttributeData.AttributeType;
                if (!typeof(ITraitAttribute).GetTypeInfo().IsAssignableFrom(traitAttributeType.GetTypeInfo()))
                    continue;

                var discovererAttributeData = traitAttributeType.GetTypeInfo().CustomAttributes.FirstOrDefault(cad => cad.AttributeType == typeof(TraitDiscovererAttribute));
                if (discovererAttributeData == null)
                    continue;

                var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(messageSink, Reflector.Wrap(discovererAttributeData));
                if (discoverer == null)
                    continue;

                var traits = discoverer.GetTraits(Reflector.Wrap(traitAttributeData));
                if (traits != null)
                    result.AddRange(traits);
            }

            return result;
        }
    }
}
