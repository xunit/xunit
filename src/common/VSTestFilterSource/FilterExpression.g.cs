// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Utilities;

using static Microsoft.VisualStudio.TestPlatform.Common.Resources.Resources;
#endif

namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;

/// <summary>
/// Represents an expression tree.
/// Supports:
///     Logical Operators:  &amp;, |
///     Equality Operators: =, !=
///     Parenthesis (, ) for grouping.
/// </summary>
#if !IS_VSTEST_REPO
[Embedded]
#endif
internal sealed class FilterExpression
{
#if !IS_VSTEST_REPO
    private const string TestCaseFilterFormatException = "Incorrect format for TestCaseFilter {0}. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.";

    private const string MissingOperand = "Error: Missing operand";

    private const string MissingCloseParenthesis = "Error: Missing ')'";

    private const string EmptyParenthesis = "Error: Empty parenthesis ( )";

    private const string MissingOpenParenthesis = "Error: Missing '('";

    private const string MissingOperator = "Missing Operator '|' or '&'";
#endif

    /// <summary>
    /// Condition, if expression is conditional expression.
    /// </summary>
    private readonly Condition? _condition;

    /// <summary>
    /// Left operand, when expression is logical expression.
    /// </summary>
    private readonly FilterExpression? _left;

    /// <summary>
    /// Right operand, when expression is logical expression.
    /// </summary>
    private readonly FilterExpression? _right;

    /// <summary>
    /// If logical expression is using logical And ('&amp;') operator.
    /// </summary>
    private readonly bool _areJoinedByAnd;

    private FilterExpression(FilterExpression left, FilterExpression right, bool areJoinedByAnd)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
        _areJoinedByAnd = areJoinedByAnd;
    }

    private FilterExpression(Condition condition)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
    }
    /// <summary>
    /// Create a new filter expression 'And'ing 'this' with 'filter'.
    /// </summary>
    private FilterExpression And(FilterExpression filter)
        => new FilterExpression(this, filter, true);

    /// <summary>
    /// Create a new filter expression 'Or'ing 'this' with 'filter'.
    /// </summary>
    private FilterExpression Or(FilterExpression filter)
        => new FilterExpression(this, filter, false);

    /// <summary>
    /// Process the given operator from the filterStack.
    /// Puts back the result of operation back to filterStack.
    /// </summary>
    private static void ProcessOperator(Stack<FilterExpression> filterStack, Operator op)
    {
        if (op == Operator.And)
        {
            if (filterStack.Count < 2)
            {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingOperand));
            }

            var filterRight = filterStack.Pop();
            var filterLeft = filterStack.Pop();
            var result = filterLeft.And(filterRight);
            filterStack.Push(result);
        }
        else if (op == Operator.Or)
        {
            if (filterStack.Count < 2)
            {
                throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingOperand));
            }

            var filterRight = filterStack.Pop();
            var filterLeft = filterStack.Pop();
            var result = filterLeft.Or(filterRight);
            filterStack.Push(result);
        }
        else if (op == Operator.OpenBrace)
        {
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingCloseParenthesis));
        }
        else
        {
            Debug.Fail("ProcessOperator called for Unexpected operator.");
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, string.Empty));
        }
    }

    /// <summary>
    /// True, if filter is valid for given set of properties.
    /// When False, invalidProperties would contain properties making filter invalid.
    /// </summary>
#if IS_VSTEST_REPO
    internal string[]? ValidForProperties(IEnumerable<string>? properties, Func<string, TestProperty?>? propertyProvider)
#else
    internal string[]? ValidForProperties(IEnumerable<string>? properties)
#endif
    {
        properties ??= [];

        return IterateFilterExpression<string[]?>((current, result) =>
        {
            // Only the leaves have a condition value.
            if (current._condition != null)
            {
#if IS_VSTEST_REPO
                var valid = current._condition.ValidForProperties(properties, propertyProvider);
#else
                var valid = current._condition.ValidForProperties(properties);
#endif
                // If it's not valid will add it to the function's return array.
                return !valid ? [current._condition.Name] : null;
            }

            // Concatenate the children node's result to get their parent result.
            var invalidRight = current._right != null ? result.Pop() : null;
            var invalidProperties = current._left != null ? result.Pop() : null;

            if (invalidProperties == null)
            {
                invalidProperties = invalidRight;
            }
            else if (invalidRight != null)
            {
                invalidProperties = invalidProperties.Concat(invalidRight).ToArray();
            }

            return invalidProperties;
        });

    }

    /// <summary>
    /// Return FilterExpression after parsing the given filter expression, and a FastFilter when possible.
    /// </summary>
    internal static FilterExpression Parse(string filterString, out FastFilter? fastFilter)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(filterString, nameof(filterString));
#endif

        // Below parsing doesn't error out on pattern (), so explicitly search for that (empty parenthesis).
        var invalidInput = Regex.Match(filterString, @"\(\s*\)");
        if (invalidInput.Success)
        {
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, EmptyParenthesis));
        }

        var tokens = TokenizeFilterExpressionString(filterString);
        var operatorStack = new Stack<Operator>();
        var filterStack = new Stack<FilterExpression>();

        FastFilter.Builder fastFilterBuilder = FastFilter.CreateBuilder();

        // This is based on standard parsing of in order expression using two stacks (operand stack and operator stack)
        // Precedence(And) > Precedence(Or)
        foreach (var inputToken in tokens)
        {
            var token = inputToken.Trim();
#if IS_VSTEST_REPO
            if (token.IsNullOrEmpty())
#else
            if (string.IsNullOrEmpty(token))
#endif
            {
                // ignore empty tokens
                continue;
            }

            switch (token)
            {
                case "&":
                case "|":

                    Operator currentOperator = Operator.And;
                    if (string.Equals("|", token, StringComparison.Ordinal))
                    {
                        currentOperator = Operator.Or;
                    }

                    fastFilterBuilder.AddOperator(currentOperator);

                    // Always put only higher priority operator on stack.
                    //  if lesser priority -- pop up the stack and process the operator to maintain operator precedence.
                    //  if equal priority -- pop up the stack and process the operator to maintain operator associativity.
                    //  OpenBrace is special condition. & or | can come on top of OpenBrace for case like ((a=b)&c=d)
                    while (true)
                    {
                        bool isEmpty = operatorStack.Count == 0;
                        Operator stackTopOperator = isEmpty ? Operator.None : operatorStack.Peek();
                        if (isEmpty || stackTopOperator == Operator.OpenBrace || stackTopOperator < currentOperator)
                        {
                            operatorStack.Push(currentOperator);
                            break;
                        }

                        stackTopOperator = operatorStack.Pop();
                        ProcessOperator(filterStack, stackTopOperator);
                    }

                    break;

                case "(":
                    operatorStack.Push(Operator.OpenBrace);
                    break;

                case ")":
                    // process operators from the stack till OpenBrace is found.
                    // If stack is empty at any time, than matching OpenBrace is missing from the expression.
                    if (operatorStack.Count == 0)
                    {
                        throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingOpenParenthesis));
                    }

                    Operator temp = operatorStack.Pop();
                    while (temp != Operator.OpenBrace)
                    {
                        ProcessOperator(filterStack, temp);
                        if (operatorStack.Count == 0)
                        {
                            throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingOpenParenthesis));
                        }
                        temp = operatorStack.Pop();
                    }

                    break;

                default:
                    // push the operand to the operand stack.
                    Condition condition = Condition.Parse(token);
                    FilterExpression filter = new(condition);
                    filterStack.Push(filter);

                    fastFilterBuilder.AddCondition(condition);
                    break;
            }
        }

        while (operatorStack.Count != 0)
        {
            Operator temp = operatorStack.Pop();
            ProcessOperator(filterStack, temp);
        }

        if (filterStack.Count != 1)
        {
            throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, MissingOperator));
        }

        fastFilter = fastFilterBuilder.ToFastFilter();

        return filterStack.Pop();
    }

    private T IterateFilterExpression<T>(Func<FilterExpression, Stack<T>, T> getNodeValue)
    {
        FilterExpression? current = this;
        // Will have the nodes.
        Stack<FilterExpression> filterStack = new();
        // Will contain the nodes results to use them in their parent result's calculation
        // and at the end will have the root result.
        Stack<T> result = new();

        do
        {
            // Push root's right child and then root to stack then Set root as root's left child.
            while (current != null)
            {
                if (current._right != null)
                {
                    filterStack.Push(current._right);
                }

                filterStack.Push(current);
                current = current._left;
            }

            // If the popped item has a right child and the right child is at top of stack,
            // then remove the right child from stack, push the root back and set root as root's right child.
            current = filterStack.Pop();
            if (filterStack.Count > 0 && current._right == filterStack.Peek())
            {
                filterStack.Pop();
                filterStack.Push(current);
                current = current._right;
                continue;
            }

            result.Push(getNodeValue(current, result));
            current = null;
        }
        while (filterStack.Count > 0);

        Debug.Assert(result.Count == 1, "Result stack should have one element at the end.");
        return result.Peek();
    }
    /// <summary>
    /// Evaluate filterExpression with given propertyValueProvider.
    /// </summary>
    /// <param name="propertyValueProvider"> The property Value Provider.</param>
    /// <returns> True if evaluation is successful. </returns>
    internal bool Evaluate(Func<string, object?> propertyValueProvider)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(propertyValueProvider, nameof(propertyValueProvider));
#endif

        return IterateFilterExpression<bool>((current, result) =>
        {
            // Only the leaves have a condition value.
            if (current._condition != null)
            {
                return current._condition.Evaluate(propertyValueProvider);
            }
            else
            {
                // & or | operator
                bool rightResult = current._right != null && result.Pop();
                bool leftResult = current._left != null && result.Pop();
                // Concatenate the children node's result to get their parent result.
                return current._areJoinedByAnd ? leftResult && rightResult : leftResult || rightResult;
            }
        });
    }

    internal static IEnumerable<string> TokenizeFilterExpressionString(string str)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(str, nameof(str));
#endif
        return TokenizeFilterExpressionStringHelper(str);

        static IEnumerable<string> TokenizeFilterExpressionStringHelper(string s)
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
                        // We just encountered "\\" (escaped '\'), this will set last to '\0'
                        // so the next char will not be treated as a suffix of escape sequence.
                        current = '\0';
                    }
                }
                else
                {
                    switch (current)
                    {
                        case '(':
                        case ')':
                        case '&':
                        case '|':
                            if (tokenBuilder.Length > 0)
                            {
                                yield return tokenBuilder.ToString();
                                tokenBuilder.Clear();
                            }
                            yield return current.ToString();
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
