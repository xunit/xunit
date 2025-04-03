// This source is copied from
// https://github.com/microsoft/testfx/blob/3564400fca5863f716d89fc84326e420f0b33d1a/src/Platform/Microsoft.Testing.Platform.MSBuild/Tasks/NamespaceHelpers.cs
// because we need the namespace to be identical to what's generated in SelfRegisteredExtensions.cs
//
// Licensed by Microsoft under the MIT license. https://github.com/microsoft/testfx/blob/3564400fca5863f716d89fc84326e420f0b33d1a/LICENSE

using System.Text;

namespace Xunit.Internal;

internal static class NamespaceHelpers
{
	// https://github.com/dotnet/templating/blob/b0b1283f8c96be35f1b65d4b0c1ec0534d86fc2f/src/Microsoft.TemplateEngine.Orchestrator.RunnableProjects/ValueForms/DefaultSafeNamespaceValueFormFactory.cs#L17-L59
	internal static string ToSafeNamespace(string value)
	{
		const char invalidCharacterReplacement = '_';

		value = value.Trim();

		StringBuilder safeValueStr = new(value.Length);

		for (int i = 0; i < value.Length; i++)
		{
			if (i < value.Length - 1 && char.IsSurrogatePair(value[i], value[i + 1]))
			{
				safeValueStr.Append(invalidCharacterReplacement);
				// Skip both chars that make up this symbol.
				i++;
				continue;
			}

			bool isFirstCharacterOfIdentifier = safeValueStr.Length == 0 || safeValueStr[safeValueStr.Length - 1] == '.';
			bool isValidFirstCharacter = UnicodeCharacterUtilities.IsIdentifierStartCharacter(value[i]);
			bool isValidPartCharacter = UnicodeCharacterUtilities.IsIdentifierPartCharacter(value[i]);

			if (isFirstCharacterOfIdentifier && !isValidFirstCharacter && isValidPartCharacter)
			{
				// This character cannot be at the beginning, but is good otherwise. Prefix it with something valid.
				safeValueStr.Append(invalidCharacterReplacement);
				safeValueStr.Append(value[i]);
			}
			else if ((isFirstCharacterOfIdentifier && isValidFirstCharacter) ||
					(!isFirstCharacterOfIdentifier && isValidPartCharacter) ||
					(safeValueStr.Length > 0 && i < value.Length - 1 && value[i] == '.'))
			{
				// This character is allowed to be where it is.
				safeValueStr.Append(value[i]);
			}
			else
			{
				safeValueStr.Append(invalidCharacterReplacement);
			}
		}

		return safeValueStr.ToString();
	}
}
