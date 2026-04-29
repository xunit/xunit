// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
#if IS_VSTEST_REPO
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using System.Text;

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using static Microsoft.VisualStudio.TestPlatform.Common.Resources.Resources;
#endif

namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;

#if !IS_VSTEST_REPO
[Embedded]
#endif
internal enum Operation
{
    Equal,
    NotEqual,
    Contains,
    NotContains
}

/// <summary>
/// Operator in order of precedence.
/// Precedence(And) > Precedence(Or)
/// Precedence of OpenBrace and CloseBrace operators is not used, instead parsing code takes care of same.
/// </summary>
#if !IS_VSTEST_REPO
[Embedded]
#endif
internal enum Operator
{
    None,
    Or,
    And,
    OpenBrace,
    CloseBrace,
}

/// <summary>
/// Represents a condition in filter expression.
/// </summary>
#if !IS_VSTEST_REPO
[Embedded]
#endif
internal sealed class Condition
{
    /// <summary>
    ///  Default property name which will be used when filter has only property value.
    /// </summary>
    public const string DefaultPropertyName = "FullyQualifiedName";

    /// <summary>
    ///  Default operation which will be used when filter has only property value.
    /// </summary>
    public const Operation DefaultOperation = Operation.Contains;

#if !IS_VSTEST_REPO
    private const string TestCaseFilterFormatException = "Incorrect format for TestCaseFilter {0}. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.";

    private const string InvalidCondition = "Error: Invalid Condition '{0}'";

    private const string InvalidOperator = "Error: Invalid operator '{0}'";
#endif

    internal Condition(string name, Operation operation, string value)
    {
        Name = name;
        Operation = operation;
        Value = value;
    }

    /// <summary>
    /// Name of the property used in condition.
    /// </summary>
    internal string Name { get; }

    /// <summary>
    /// Value for the property.
    /// </summary>
    internal string Value { get; }

    /// <summary>
    /// Operation to be performed.
    /// </summary>
    internal Operation Operation { get; }

    private bool EvaluateEqualOperation(string[]? multiValue)
    {
        // if any value in multi-valued property matches 'this.Value', for Equal to evaluate true.
        if (multiValue != null)
        {
            foreach (string propertyValue in multiValue)
            {
                if (string.Equals(propertyValue, Value, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool EvaluateContainsOperation(string[]? multiValue)
    {
        if (multiValue != null)
        {
            foreach (string propertyValue in multiValue)
            {
#if IS_VSTEST_REPO
                TPDebug.Assert(null != propertyValue, "PropertyValue can not be null.");
#endif
                if (propertyValue!.IndexOf(Value, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Evaluate this condition for testObject.
    /// </summary>
    internal bool Evaluate(Func<string, object?> propertyValueProvider)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(propertyValueProvider, nameof(propertyValueProvider));
#endif
        var multiValue = GetPropertyValue(propertyValueProvider);
        var result = Operation switch
        {
            // if any value in multi-valued property matches 'this.Value', for Equal to evaluate true.
            Operation.Equal => EvaluateEqualOperation(multiValue),
            // all values in multi-valued property should not match 'this.Value' for NotEqual to evaluate true.
            Operation.NotEqual => !EvaluateEqualOperation(multiValue),
            // if any value in multi-valued property contains 'this.Value' for 'Contains' to be true.
            Operation.Contains => EvaluateContainsOperation(multiValue),
            // all values in multi-valued property should not contain 'this.Value' for NotContains to evaluate true.
            Operation.NotContains => !EvaluateContainsOperation(multiValue),
            _ => false,
        };

        return result;
    }

    /// <summary>
    /// Returns a condition object after parsing input string of format '<propertyName>Operation</propertyName>'
    /// </summary>
    internal static Condition Parse(string? conditionString)
    {
#if IS_VSTEST_REPO
        if (conditionString.IsNullOrWhiteSpace())
#else
        if (string.IsNullOrWhiteSpace(conditionString))
#endif
        {
            ThrownFormatExceptionForInvalidCondition(conditionString);
        }

        var parts = TokenizeFilterConditionString(conditionString!).ToArray();
        if (parts.Length == 1)
        {
            // If only parameter values is passed, create condition with default property name,
            // default operation and given condition string as parameter value.
            return new Condition(DefaultPropertyName, DefaultOperation, FilterHelper.Unescape(conditionString!.Trim()));
        }

        if (parts.Length != 3)
        {
            ThrownFormatExceptionForInvalidCondition(conditionString);
        }

        for (int index = 0; index < 3; index++)
        {
#if IS_VSTEST_REPO
            if (parts[index].IsNullOrWhiteSpace())
#else
            if (string.IsNullOrWhiteSpace(parts[index]))
#endif
            {
                ThrownFormatExceptionForInvalidCondition(conditionString);
            }
            parts[index] = parts[index].Trim();
        }

        Operation operation = GetOperator(parts[1]);
        Condition condition = new(parts[0], operation, FilterHelper.Unescape(parts[2]));
        return condition;
    }

#if IS_VSTEST_REPO
    [DoesNotReturn]
#endif
    private static void ThrownFormatExceptionForInvalidCondition(string? conditionString)
    {
        throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException,
            string.Format(CultureInfo.CurrentCulture, InvalidCondition, conditionString)));
    }

    /// <summary>
    /// Check if condition validates any property in properties.
    /// </summary>
#if IS_VSTEST_REPO
    internal bool ValidForProperties(IEnumerable<string> properties, Func<string, TestProperty?>? propertyProvider)
#else
    internal bool ValidForProperties(IEnumerable<string> properties)
#endif
    {
        bool valid = false;

        if (properties.Contains(Name, StringComparer.OrdinalIgnoreCase))
        {
            valid = true;

#if IS_VSTEST_REPO
            // Check if operation ~ (Contains) is on property of type string.
            if (Operation == Operation.Contains)
            {
                valid = ValidForContainsOperation(propertyProvider);
            }
#endif
        }
        return valid;
    }

#if IS_VSTEST_REPO
    private bool ValidForContainsOperation(Func<string, TestProperty?>? propertyProvider)
    {
        bool valid = true;

        // It is OK for propertyProvider to be null, no syntax check will happen.

        // Check validity of operator only if related TestProperty is non-null.
        // if null, it might be custom validation ignore it.
        if (null != propertyProvider)
        {
            TestProperty? testProperty = propertyProvider(Name);
            if (null != testProperty)
            {
                Type propertyType = testProperty.GetValueType();
                valid = typeof(string) == propertyType ||
                        typeof(string[]) == propertyType;
            }
        }
        return valid;
    }
#endif

    /// <summary>
    /// Return Operation corresponding to the operationString
    /// </summary>
    private static Operation GetOperator(string operationString)
    {
        return operationString switch
        {
            "=" => Operation.Equal,
            "!=" => Operation.NotEqual,
            "~" => Operation.Contains,
            "!~" => Operation.NotContains,
            _ => throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, string.Format(CultureInfo.CurrentCulture, InvalidOperator, operationString))),
        };
    }

    /// <summary>
    /// Returns property value for Property using propertValueProvider.
    /// </summary>
    private string[]? GetPropertyValue(Func<string, object?> propertyValueProvider)
    {
        var propertyValue = propertyValueProvider(Name);
        if (null != propertyValue)
        {
            if (propertyValue is not string[] multiValue)
            {
                multiValue = new string[1];
                multiValue[0] = propertyValue.ToString()!;
            }
            return multiValue;
        }

        return null;
    }

    internal static IEnumerable<string> TokenizeFilterConditionString(string str)
    {
        return str == null ? throw new ArgumentNullException(nameof(str)) : TokenizeFilterConditionStringWorker(str);

        static IEnumerable<string> TokenizeFilterConditionStringWorker(string s)
        {
            StringBuilder tokenBuilder = new();

            var last = '\0';
            for (int i = 0; i < s.Length; ++i)
            {
                var current = s[i];

                if (last == FilterHelper.EscapeCharacter)
                {
                    // Don't check if `current` is one of the special characters here.
                    // Instead, we blindly let any character follows '\' pass though and
                    // relies on `FilterHelpers.Unescape` to report such errors.
                    tokenBuilder.Append(current);

                    if (current == FilterHelper.EscapeCharacter)
                    {
                        // We just encountered double backslash (i.e. escaped '\'), therefore set `last` to '\0'
                        // so the second '\' (i.e. current) will not be treated as the prefix of escape sequence
                        // in next iteration.
                        current = '\0';
                    }
                }
                else
                {
                    switch (current)
                    {
                        case '=':
                            if (tokenBuilder.Length > 0)
                            {
                                yield return tokenBuilder.ToString();
                                tokenBuilder.Clear();
                            }
                            yield return "=";
                            break;

                        case '!':
                            if (tokenBuilder.Length > 0)
                            {
                                yield return tokenBuilder.ToString();
                                tokenBuilder.Clear();
                            }
                            // Determine if this is a "!=" or "!~" or just a single "!".
                            var next = i + 1;
                            if (next < s.Length && s[next] == '=')
                            {
                                i = next;
                                current = '=';
                                yield return "!=";
                            }
                            else if (next < s.Length && s[next] == '~')
                            {
                                i = next;
                                current = '~';
                                yield return "!~";
                            }
                            else
                            {
                                yield return "!";
                            }
                            break;

                        case '~':
                            if (tokenBuilder.Length > 0)
                            {
                                yield return tokenBuilder.ToString();
                                tokenBuilder.Clear();
                            }
                            yield return "~";
                            break;

                        default:
                            tokenBuilder.Append(current);
                            break;
                    }
                }
                last = current;
            }

            if (tokenBuilder.Length > 0)
            {
                yield return tokenBuilder.ToString();
            }
        }
    }
}
