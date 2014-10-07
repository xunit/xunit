/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * ironruby@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Xunit.ConsoleClient.Utility
{
    public static class Glob
    {
        private class CharClass
        {
            private readonly StringBuilder/*!*/ _chars = new StringBuilder();

            internal void Add(char c)
            {
                if (c == ']' || c == '\\')
                {
                    _chars.Append('\\');
                }
                _chars.Append(c);
            }

            internal string MakeString()
            {
                if (_chars.Length == 0)
                {
                    return null;
                }
                if (_chars.Length == 1 && _chars[0] == '^')
                {
                    _chars.Insert(0, "\\");
                }
                _chars.Insert(0, "[");
                _chars.Append(']');
                return _chars.ToString();
            }
        }

        private static void AppendExplicitRegexChar(StringBuilder/*!*/ builder, char c)
        {
            builder.Append('[');
            if (c == '^' || c == '\\')
            {
                builder.Append('\\');
            }
            builder.Append(c);
            builder.Append(']');
        }

        private static string/*!*/ PatternToRegex(string/*!*/ pattern)
        {
            var result = new StringBuilder(pattern.Length);
            result.Append("\\G");

            var inEscape = false;
            CharClass charClass = null;

            foreach (char c in pattern)
            {
                if (inEscape)
                {
                    if (charClass != null)
                    {
                        charClass.Add(c);
                    }
                    else
                    {
                        AppendExplicitRegexChar(result, c);
                    }
                    inEscape = false;
                    continue;
                }
                else if (c == '\\')
                {
                    inEscape = true;
                    continue;
                }

                if (charClass != null)
                {
                    if (c == ']')
                    {
                        string set = charClass.MakeString();
                        if (set == null)
                        {
                            // Ruby regex "[]" matches nothing
                            // CLR regex "[]" throws exception
                            return String.Empty;
                        }
                        result.Append(set);
                        charClass = null;
                    }
                    else
                    {
                        charClass.Add(c);
                    }
                    continue;
                }
                switch (c)
                {
                    case '*':
                        result.Append(".*");
                        break;

                    case '?':
                        result.Append('.');
                        break;

                    case '[':
                        charClass = new CharClass();
                        break;

                    default:
                        AppendExplicitRegexChar(result, c);
                        break;
                }
            }

            return (charClass == null) ? result.ToString() : String.Empty;
        }

        private static bool FnMatch(string/*!*/ pattern, string/*!*/ path)
        {
            if (pattern.Length == 0)
            {
                return path.Length == 0;
            }

            string regexPattern = PatternToRegex(pattern);
            if (regexPattern.Length == 0)
            {
                return false;
            }

            if (path.Length > 0 && path[0] == '.')
            {
                // Starting dot requires an explicit dot in the pattern
                if (regexPattern.Length < 4 || regexPattern[2] != '[' || regexPattern[3] != '.')
                {
                    return false;
                }
            }

            RegexOptions options = RegexOptions.None;
            Match match = Regex.Match(path, regexPattern, options);
            return match.Success && (match.Length == path.Length);
        }

        internal sealed class GlobMatcher
        {
            private readonly string/*!*/ _pattern;
            private readonly bool _dirOnly;
            private readonly List<string>/*!*/ _result;
            private bool _stripTwo;

            internal GlobMatcher(string/*!*/ pattern, int flags)
            {
                _pattern = CanonicalizePath((pattern == "**") ? "*" : pattern);
                _result = new List<string>();
                _dirOnly = _pattern[_pattern.Length - 1] == '/';
                _stripTwo = false;
            }

            private int FindNextSeparator(int position, bool allowWildcard, out bool containsWildcard)
            {
                int lastSlash = -1;
                bool inEscape = false;
                containsWildcard = false;
                for (int i = position; i < _pattern.Length; i++)
                {
                    if (inEscape)
                    {
                        inEscape = false;
                        continue;
                    }
                    char c = _pattern[i];
                    if (c == '\\')
                    {
                        inEscape = true;
                        continue;
                    }
                    else if (c == '*' || c == '?' || c == '[')
                    {
                        if (!allowWildcard)
                        {
                            return lastSlash + 1;
                        }
                        else if (lastSlash >= 0)
                        {
                            return lastSlash;
                        }
                        containsWildcard = true;
                    }
                    else if (c == '/' || c == ':')
                    {
                        if (containsWildcard)
                        {
                            return i;
                        }
                        lastSlash = i;
                    }
                }
                return _pattern.Length;
            }

            private void TestPath(string path, int patternEnd, bool isLastPathSegment)
            {
                if (!isLastPathSegment)
                {
                    DoGlob(path, patternEnd, false);
                    return;
                }

                path = Unescape(path, _stripTwo ? 2 : 0);
                if (_stripTwo)
                {
                    path = path.Substring(2);
                }

                if (Directory.Exists(path))
                {
                    _result.Add(path);
                }
                else if (!_dirOnly && File.Exists(path))
                {
                    _result.Add(path);
                }
            }

            private static string/*!*/ Unescape(string/*!*/ path, int start)
            {
                var unescaped = new StringBuilder();
                bool inEscape = false;
                for (int i = start; i < path.Length; i++)
                {
                    char c = path[i];
                    if (inEscape)
                    {
                        inEscape = false;
                    }
                    else if (c == '\\')
                    {
                        inEscape = true;
                        continue;
                    }
                    unescaped.Append(c);
                }

                if (inEscape)
                {
                    unescaped.Append('\\');
                }

                return unescaped.ToString();
            }

            internal IList<string> DoGlob(string baseDirectory)
            {
                DoGlob(CanonicalizePath(baseDirectory), 0, false);
                return _result;
            }

            private void DoGlob(string/*!*/ baseDirectory, int position, bool isPreviousDoubleStar)
            {
                if (!Directory.Exists(baseDirectory))
                {
                    return;
                }

                bool containsWildcard;
                int patternEnd = FindNextSeparator(position, true, out containsWildcard);
                bool isLastPathSegment = (patternEnd == _pattern.Length);
                string dirSegment = _pattern.Substring(position, patternEnd - position);

                if (!isLastPathSegment)
                {
                    patternEnd++;
                }

                if (!containsWildcard)
                {
                    string path = baseDirectory + "/" + dirSegment;
                    TestPath(path, patternEnd, isLastPathSegment);
                    return;
                }

                bool doubleStar = dirSegment.Equals("**");
                if (doubleStar && !isPreviousDoubleStar)
                {
                    DoGlob(baseDirectory, patternEnd, true);
                }

                foreach (string file in Directory.GetFileSystemEntries(baseDirectory, "*"))
                {
                    string objectName = Path.GetFileName(file);
                    if (FnMatch(dirSegment, objectName))
                    {
                        var canon = file.Replace('\\', '/'); ;
                        TestPath(canon, patternEnd, isLastPathSegment);
                        if (doubleStar)
                        {
                            DoGlob(canon, position, true);
                        }
                    }
                }
                if (isLastPathSegment && dirSegment[0] == '.')
                {
                    if (FnMatch(dirSegment, "."))
                    {
                        string directory = baseDirectory + "/.";
                        if (_dirOnly)
                        {
                            directory += '/';
                        }
                        TestPath(directory, patternEnd, true);
                    }
                    if (FnMatch(dirSegment, ".."))
                    {
                        string directory = baseDirectory + "/..";
                        if (_dirOnly)
                        {
                            directory += '/';
                        }
                        TestPath(directory, patternEnd, true);
                    }
                }
            }
        }

        private static String CanonicalizePath(string path)
        {
            return path.Replace('\\', '/');
        }
    }

    public static class DirectoryExtensions
    {
        public static IEnumerable<string> Glob(this DirectoryInfo directory, string pattern)
        {
            var matcher = new Glob.GlobMatcher(pattern, 0);
            return matcher.DoGlob(directory.ToString());
        }
    }
}