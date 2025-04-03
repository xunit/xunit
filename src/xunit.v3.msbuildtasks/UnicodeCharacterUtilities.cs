// This source is copied from
// https://github.com/microsoft/testfx/blob/3564400fca5863f716d89fc84326e420f0b33d1a/src/Analyzers/MSTest.SourceGeneration/Helpers/UnicodeCharacterUtilities.cs#L11
// because we need the namespace to be identical to what's generated in SelfRegisteredExtensions.cs
//
// Licensed by Microsoft under the MIT license. https://github.com/microsoft/testfx/blob/3564400fca5863f716d89fc84326e420f0b33d1a/LICENSE

using System.Globalization;

namespace Xunit.Internal;

/// <summary>
/// Defines a set of helper methods to classify Unicode characters.
/// </summary>
internal static class UnicodeCharacterUtilities
{
	public static bool IsIdentifierStartCharacter(char ch)
	{
		// identifier-start-character:
		//   letter-character
		//   _ (the underscore character U+005F)
		if (ch < 'a') // '\u0061'
		{
			if (ch < 'A') // '\u0041'
			{
				return false;
			}

			return ch is <= 'Z' // '\u005A'
				or '_'; // '\u005F'
		}

		if (ch <= 'z') // '\u007A'
		{
			return true;
		}

		if (ch <= '\u007F') // max ASCII
		{
			return false;
		}

		// Check if letter-character
		return IsLetterChar(CharUnicodeInfo.GetUnicodeCategory(ch));
	}

	/// <summary>
	/// Returns true if the Unicode character can be a part of an identifier.
	/// </summary>
	/// <param name="ch">The Unicode character.</param>
	public static bool IsIdentifierPartCharacter(char ch)
	{
		// identifier-part-character:
		//   letter-character
		//   decimal-digit-character
		//   connecting-character
		//   combining-character
		//   formatting-character
		if (ch < 'a') // '\u0061'
		{
			if (ch < 'A') // '\u0041'
			{
				return ch is >= '0' // '\u0030'
					and <= '9'; // '\u0039'
			}

			return ch is <= 'Z' // '\u005A'
				or '_'; // '\u005F'
		}

		if (ch <= 'z') // '\u007A'
		{
			return true;
		}

		if (ch <= '\u007F') // max ASCII
		{
			return false;
		}

		UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(ch);
		return IsLetterChar(cat)
			|| IsDecimalDigitChar(cat)
			|| IsConnectingChar(cat)
			|| IsCombiningChar(cat)
			|| IsFormattingChar(cat);
	}

	private static bool IsLetterChar(UnicodeCategory cat)
		// letter-character:
		//   A Unicode character of classes Lu, Ll, Lt, Lm, Lo, or Nl
		//   A Unicode-escape-sequence representing a character of classes Lu, Ll, Lt, Lm, Lo, or Nl
		=> cat switch
		{
			UnicodeCategory.UppercaseLetter or UnicodeCategory.LowercaseLetter or UnicodeCategory.TitlecaseLetter or UnicodeCategory.ModifierLetter or UnicodeCategory.OtherLetter or UnicodeCategory.LetterNumber => true,
			_ => false,
		};

	private static bool IsCombiningChar(UnicodeCategory cat)
		// combining-character:
		//   A Unicode character of classes Mn or Mc
		//   A Unicode-escape-sequence representing a character of classes Mn or Mc
		=> cat switch
		{
			UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark => true,
			_ => false,
		};

	private static bool IsDecimalDigitChar(UnicodeCategory cat)
		// decimal-digit-character:
		//   A Unicode character of the class Nd
		//   A unicode-escape-sequence representing a character of the class Nd
		=> cat == UnicodeCategory.DecimalDigitNumber;

	private static bool IsConnectingChar(UnicodeCategory cat)
		// connecting-character:
		//   A Unicode character of the class Pc
		//   A unicode-escape-sequence representing a character of the class Pc
		=> cat == UnicodeCategory.ConnectorPunctuation;

	/// <summary>
	/// Returns true if the Unicode character is a formatting character (Unicode class Cf).
	/// </summary>
	/// <param name="cat">The Unicode character.</param>
	private static bool IsFormattingChar(UnicodeCategory cat)
		// formatting-character:
		//   A Unicode character of the class Cf
		//   A unicode-escape-sequence representing a character of the class Cf
		=> cat == UnicodeCategory.Format;
}
