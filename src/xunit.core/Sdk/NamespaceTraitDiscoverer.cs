using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="IMethodedTraitDiscoverer"/> which returns the trait values
    /// for <see cref="NamespaceTraitAttribute"/>.
    /// </summary>
    public class NamespaceTraitDiscoverer : IMethodedTraitDiscoverer
    {
        /// <inheritdoc/>
        public virtual IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute, IMethodInfo methodInfo)
        {
            var ctorArgs = traitAttribute.GetConstructorArguments().Cast<string>().ToList();

            if (string.IsNullOrWhiteSpace(ctorArgs[0]))
                yield break;

            var attributeNamespaceParts = ctorArgs[0].Split('.');

            var methodNamespaceParts = GetMethodNamespaceParts(methodInfo);
            if (methodNamespaceParts.Length == 0)
                yield break;

            var caseInsensitiveMatching = traitAttribute.GetNamedArgument<bool>("NamespaceCaseInsensitive");
            var notForNestedNamespaces = traitAttribute.GetNamedArgument<bool>("NotApplicableToNestedNamespaces");

            if (!CorrespondingNamespace(methodNamespaceParts, attributeNamespaceParts, caseInsensitiveMatching, notForNestedNamespaces))
                yield break;

            yield return new KeyValuePair<string, string>(ctorArgs[1], ctorArgs[2]);
        }

        static string[] GetMethodNamespaceParts(IMethodInfo methodInfo)
        {
            var name = methodInfo?.Type?.Name;
            if(string.IsNullOrWhiteSpace(name))
                return new string[0];

            var nameParts = name.Split('.');

            if(nameParts.Length < 2)
                return new string[0];

            return nameParts.Take(nameParts.Length - 1).ToArray();
        }

        static bool CorrespondingNamespace(string[] methodNamespaceParts, string[] attributeNamespaceParts, bool caseInsensitiveMatching, bool notForNestedNamespaces)
        {
            if (attributeNamespaceParts.Length > methodNamespaceParts.Length)
                return false;

            if (notForNestedNamespaces && attributeNamespaceParts.Length < methodNamespaceParts.Length)
                return false;

            var comparison = caseInsensitiveMatching
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            for (int i = 0; i < attributeNamespaceParts.Length; i++)
            {
                if (!string.Equals(attributeNamespaceParts[i], methodNamespaceParts[i], comparison))
                    return false;
            }

            return true;
        }
    }
}
