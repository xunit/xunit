// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.

#pragma warning disable SA1602 // Enumeration items should be documented
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Xunit.Runner.Common;

internal enum Operation
{
	Equal,
	NotEqual,
	Contains,
	NotContains,
}

/// <summary>
/// Operator in order of precedence.
/// Precedence(And) > Precedence(Or)
/// Precedence of OpenBrace and CloseBrace operators is not used, instead parsing code takes care of same.
/// </summary>
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

	internal Condition(string name, Operation operation, string value)
	{
		Name = name;
		Operation = operation;
		Value = value;
	}

	/// <summary>
	/// Gets toolName of the property used in condition.
	/// </summary>
	internal string Name { get; }

	/// <summary>
	/// Gets value for the property.
	/// </summary>
	internal string Value { get; }

	/// <summary>
	/// Gets operation to be performed.
	/// </summary>
	internal Operation Operation { get; }

	/// <summary>
	/// Evaluate this condition for testObject.
	/// </summary>
	internal bool Evaluate(Func<string, object?> propertyValueProvider)
	{
		if (propertyValueProvider is null)
		{
			throw new ArgumentNullException(nameof(propertyValueProvider));
		}

		bool result = false;
		string[]? multiValue = GetPropertyValue(propertyValueProvider);
		switch (Operation)
		{
			case Operation.Equal:
				// if any value in multi-valued property matches 'this.Value', for Equal to evaluate true.
				if (multiValue != null)
				{
					foreach (string propertyValue in multiValue)
					{
						result = result || string.Equals(propertyValue, Value, StringComparison.OrdinalIgnoreCase);
						if (result)
						{
							break;
						}
					}
				}

				break;

			case Operation.NotEqual:
				// all values in multi-valued property should not match 'this.Value' for NotEqual to evaluate true.
				result = true;

				// if value is null.
				if (multiValue != null)
				{
					foreach (string propertyValue in multiValue)
					{
						result = result && !string.Equals(propertyValue, Value, StringComparison.OrdinalIgnoreCase);
						if (!result)
						{
							break;
						}
					}
				}

				break;

			case Operation.Contains:
				// if any value in multi-valued property contains 'this.Value' for 'Contains' to be true.
				if (multiValue != null)
				{
					foreach (string propertyValue in multiValue)
					{
						Debug.Assert(propertyValue != null, "PropertyValue can not be null.");
						result = result || propertyValue!.IndexOf(Value, StringComparison.OrdinalIgnoreCase) >= 0;
						if (result)
						{
							break;
						}
					}
				}

				break;

			case Operation.NotContains:
				// all values in multi-valued property should not contain 'this.Value' for NotContains to evaluate true.
				result = true;

				if (multiValue != null)
				{
					foreach (string propertyValue in multiValue)
					{
						Debug.Assert(propertyValue != null, "PropertyValue can not be null.");
						result = result && propertyValue!.IndexOf(Value, StringComparison.OrdinalIgnoreCase) < 0;
						if (!result)
						{
							break;
						}
					}
				}

				break;
		}

		return result;
	}

	/// <summary>
	/// Returns a condition object after parsing input string of format  '<![CDATA[ <propertyName>Operation<propertyValue> ]]>'.
	/// </summary>
	internal static Condition Parse(string? conditionString)
	{
		if (string.IsNullOrWhiteSpace(conditionString))
		{
			ThrownFormatExceptionForInvalidCondition(conditionString);
		}

		string[] parts = [.. TokenizeFilterConditionString(conditionString!)];
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
			if (string.IsNullOrWhiteSpace(parts[index]))
			{
				ThrownFormatExceptionForInvalidCondition(conditionString);
			}

			parts[index] = parts[index].Trim();
		}

		Operation operation = GetOperator(parts[1]);
		Condition condition = new(parts[0], operation, FilterHelper.Unescape(parts[2]));
		return condition;
	}

	[DoesNotReturn]
	private static void ThrownFormatExceptionForInvalidCondition(string? conditionString) =>
		throw new FormatException(
			$"Incorrect format for TestCaseFilter Error: Invalid Condition '{conditionString}'. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed..");

	/// <summary>
	/// Check if condition validates any property in properties.
	/// </summary>
	internal bool ValidForProperties(IEnumerable<string> properties)
		=> properties.Contains(Name, StringComparer.OrdinalIgnoreCase);

	/// <summary>
	/// Return Operation corresponding to the operationString.
	/// </summary>
	private static Operation GetOperator(string operationString) => operationString switch
	{
		"=" => Operation.Equal,
		"!=" => Operation.NotEqual,
		"~" => Operation.Contains,
		"!~" => Operation.NotContains,
		_ => throw new FormatException(
			$"Incorrect format for TestCaseFilter Error: Invalid operator '{operationString}'. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.."),
	};

	/// <summary>
	/// Returns property value for Property using propertyValueProvider.
	/// </summary>
	private string[]? GetPropertyValue(Func<string, object?> propertyValueProvider)
	{
		object? propertyValue = propertyValueProvider(Name);
		if (propertyValue is not null)
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
		return TokenizeFilterConditionStringWorker(str ?? throw new ArgumentNullException(nameof(str)));

		static IEnumerable<string> TokenizeFilterConditionStringWorker(string s)
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
							int next = i + 1;
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
