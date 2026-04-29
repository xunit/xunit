// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using static Microsoft.VisualStudio.TestPlatform.ObjectModel.Resources.Resources;
#endif

// Because FilterHelper is public, changing the namespace for filter package to avoid collisions.
// This also makes it such that all types in filter source package use the same Common.Filtering namespace.
#if IS_VSTEST_REPO
namespace Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;
#else
namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;
#endif

#if IS_VSTEST_REPO
public static class FilterHelper
#else
[Embedded]
internal static class FilterHelper
#endif
{
    public const char EscapeCharacter = '\\';
    private static readonly char[] SpecialCharacters = ['\\', '(', ')', '&', '|', '=', '!', '~'];
    private static readonly HashSet<char> SpecialCharactersSet = new(SpecialCharacters);

#if !IS_VSTEST_REPO
    private const string TestCaseFilterEscapeException = "Filter string '{0}' includes unrecognized escape sequence.";
#endif

    /// <summary>
    /// Escapes a set of special characters for filter (%, (, ), &amp;, |, =, !, ~) by replacing them with their escape sequences.
    /// </summary>
    /// <param name="str">The input string that contains the text to convert.</param>
    /// <returns>A string of characters with special characters converted to their escaped form.</returns>
    public static string Escape(string str)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(str, nameof(str));
#endif

        if (str.IndexOfAny(SpecialCharacters) < 0)
        {
            return str;
        }

        var builder = new StringBuilder();
        for (int i = 0; i < str.Length; ++i)
        {
            var currentChar = str[i];
            if (SpecialCharactersSet.Contains(currentChar))
            {
                builder.Append(EscapeCharacter);
            }
            builder.Append(currentChar);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Converts any escaped characters in the input filter string.
    /// </summary>
    /// <param name="str">The input string that contains the text to convert.</param>
    /// <returns>A filter string of characters with any escaped characters converted to their un-escaped form.</returns>
    public static string Unescape(string str)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(str, nameof(str));
#endif

        if (str.IndexOf(EscapeCharacter) < 0)
        {
            return str;
        }

        var builder = new StringBuilder();
        for (int i = 0; i < str.Length; ++i)
        {
            var currentChar = str[i];
            if (currentChar == EscapeCharacter)
            {
                if (++i == str.Length || !SpecialCharactersSet.Contains(currentChar = str[i]))
                {
                    // "\" should be followed by a special character.
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterEscapeException, str));
                }
            }

            // Strictly speaking, string to be un-escaped shouldn't contain any of the special characters,
            // other than being part of escape sequence, but we will ignore that to avoid additional overhead.

            builder.Append(currentChar);
        }

        return builder.ToString();
    }
}
