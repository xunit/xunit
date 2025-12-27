// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents an expression tree.
/// Supports:
///     Logical Operators:  !<![CDATA[&]]>, |
///     Equality Operators: =, !=
///     Parenthesis (, ) for grouping.
/// </summary>
internal sealed partial class FilterExpression
{
	private const string TestCaseFilterFormatException = "Incorrect format for TestCaseFilter {0}. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.";

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
	/// If logical expression is using logical And <![CDATA[('&')]]>  operator.
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
	private FilterExpression And(FilterExpression filter) => new(this, filter, true);

	/// <summary>
	/// Create a new filter expression 'Or'ing 'this' with 'filter'.
	/// </summary>
	private FilterExpression Or(FilterExpression filter) => new(this, filter, false);

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
				throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Missing operand"));
			}

			FilterExpression filterRight = filterStack.Pop();
			FilterExpression filterLeft = filterStack.Pop();
			FilterExpression result = filterLeft.And(filterRight);
			filterStack.Push(result);
		}
		else if (op == Operator.Or)
		{
			if (filterStack.Count < 2)
			{
				throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Missing operand"));
			}

			FilterExpression filterRight = filterStack.Pop();
			FilterExpression filterLeft = filterStack.Pop();
			FilterExpression result = filterLeft.Or(filterRight);
			filterStack.Push(result);
		}
		else if (op == Operator.OpenBrace)
		{
			throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Missing ')'"));
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
	internal string[]? ValidForProperties(IEnumerable<string>? properties)
	{
		// if null, initialize to empty list so that invalid properties can be found.
		properties ??= [];

		return IterateFilterExpression<string[]?>((current, result) =>
		{
			// Only the leaves have a condition value.
			if (current._condition != null)
			{
				bool valid = current._condition.ValidForProperties(properties);

				// If it's not valid will add it to the function's return array.
				return !valid ? [current._condition.Name] : null;
			}

			// Concatenate the children node's result to get their parent result.
			string[]? invalidRight = current._right != null ? result.Pop() : null;
			string[]? invalidProperties = current._left != null ? result.Pop() : null;

			if (invalidProperties == null)
			{
				invalidProperties = invalidRight;
			}
			else if (invalidRight != null)
			{
				invalidProperties = [.. invalidProperties, .. invalidRight];
			}

			return invalidProperties;
		});
	}

	/// <summary>
	/// Return FilterExpression after parsing the given filter expression, and a FastFilter when possible.
	/// </summary>
	internal static FilterExpression Parse(string filterString, out FastFilter? fastFilter)
	{
		if (filterString is null)
		{
			throw new ArgumentNullException(nameof(filterString));
		}

		// Below parsing doesn't error out on pattern (), so explicitly search for that (empty parenthesis).
		Match invalidInput = GetEmptyParenthesisPattern().Match(filterString);
		if (invalidInput.Success)
		{
			throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Empty parenthesis ( )"));
		}

		IEnumerable<string> tokens = TokenizeFilterExpressionString(filterString);
		var operatorStack = new Stack<Operator>();
		var filterStack = new Stack<FilterExpression>();

		FastFilter.Builder fastFilterBuilder = FastFilter.CreateBuilder();

		// This is based on standard parsing of in order expression using two stacks (operand stack and operator stack)
		// Precedence(And) > Precedence(Or)
		foreach (string inputToken in tokens)
		{
			string token = inputToken.Trim();
			if (string.IsNullOrEmpty(token))
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
						throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Missing '('"));
					}

					Operator temp = operatorStack.Pop();
					while (temp != Operator.OpenBrace)
					{
						ProcessOperator(filterStack, temp);
						if (operatorStack.Count == 0)
						{
							throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Error: Missing '('"));
						}

						temp = operatorStack.Pop();
					}

					break;

				default:
					// push the operand to the operand stack.
					var condition = Condition.Parse(token);
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
			throw new FormatException(string.Format(CultureInfo.CurrentCulture, TestCaseFilterFormatException, "Missing Operator '|' or '&'"));
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
		if (propertyValueProvider is null)
		{
			throw new ArgumentNullException(nameof(propertyValueProvider));
		}

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
		if (str is null)
		{
			throw new ArgumentNullException(nameof(str));
		}

		return TokenizeFilterExpressionStringHelper(str);

		static IEnumerable<string> TokenizeFilterExpressionStringHelper(string s)
		{
			StringBuilder tokenBuilder = new();

			char last = '\0';
			for (int i = 0; i < s.Length; ++i)
			{
				char current = s[i];

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

#if NET7_0_OR_GREATER
    [GeneratedRegex(@"\(\s*\)")]
    private static partial Regex GetEmptyParenthesisPattern();
#else
	private static Regex GetEmptyParenthesisPattern()
		=> new(@"\(\s*\)");
#endif
}
