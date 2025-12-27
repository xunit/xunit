// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.
using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit.Runner.Common;

internal sealed class FastFilter
{
	internal FastFilter(Dictionary<string, HashSet<string>> filterProperties, Operation filterOperation, Operator filterOperator)
	{
		if (filterProperties is null || filterProperties.Count == 0)
		{
			throw new ArgumentException($"Expected dictionary to be not null and not empty.");
		}

		FilterProperties = filterProperties;

		IsFilteredOutWhenMatched =
			(filterOperation != Operation.Equal || (filterOperator != Operator.Or && filterOperator != Operator.None))
			&& (filterOperation == Operation.NotEqual && (filterOperator == Operator.And || filterOperator == Operator.None)
				? true
				: throw new ArgumentException("An error occurred while creating Fast filter."));
	}

	internal Dictionary<string, HashSet<string>> FilterProperties { get; }

	internal bool IsFilteredOutWhenMatched { get; }

	internal string[]? ValidForProperties(IEnumerable<string>? properties)
		=> properties is null
			? null
			: FilterProperties.Keys.All(name => properties.Contains(name))
				? null
				: [.. FilterProperties.Keys.Where(name => !properties.Contains(name))];

	internal bool Evaluate(Func<string, object?> propertyValueProvider)
	{
		if (propertyValueProvider is null)
		{
			throw new ArgumentNullException(nameof(propertyValueProvider));
		}

		bool matched = false;
		foreach (string name in FilterProperties.Keys)
		{
			// If there is no value corresponding to given name, treat it as unmatched.
			if (!TryGetPropertyValue(name, propertyValueProvider, out string? singleValue, out string[]? multiValues))
			{
				continue;
			}

			if (singleValue != null)
			{
				string? value = singleValue;
				matched = FilterProperties[name].Contains(value);
			}
			else
			{
				IEnumerable<string?>? values = multiValues;
				matched = values?.Any(result => result != null && FilterProperties[name].Contains(result)) == true;
			}

			if (matched)
			{
				break;
			}
		}

		return IsFilteredOutWhenMatched ? !matched : matched;
	}

	/// <summary>
	/// Returns property value for Property using propertyValueProvider.
	/// </summary>
	private static bool TryGetPropertyValue(string name, Func<string, object?> propertyValueProvider, out string? singleValue, out string[]? multiValues)
	{
		object? propertyValue = propertyValueProvider(name);
		if (propertyValue != null)
		{
			multiValues = propertyValue as string[];
			singleValue = multiValues == null ? propertyValue.ToString() : null;
			return true;
		}

		singleValue = null;
		multiValues = null;
		return false;
	}

	internal static Builder CreateBuilder() => new();

	internal sealed class Builder
	{
		private readonly Dictionary<string, HashSet<string>> _filterDictionaryBuilder = new(StringComparer.OrdinalIgnoreCase);

		private bool _operatorEncountered;
		private Operator _fastFilterOperator = Operator.None;

		private bool _conditionEncountered;
		private Operation _fastFilterOperation;

		private bool _containsValidFilter = true;

		internal bool ContainsValidFilter => _containsValidFilter && _conditionEncountered;

		internal void AddOperator(Operator @operator)
		{
			if (_containsValidFilter && (@operator == Operator.And || @operator == Operator.Or))
			{
				if (_operatorEncountered)
				{
					_containsValidFilter = _fastFilterOperator == @operator;
				}
				else
				{
					_operatorEncountered = true;
					_fastFilterOperator = @operator;
					if ((_fastFilterOperation == Operation.NotEqual && _fastFilterOperator == Operator.Or)
						|| (_fastFilterOperation == Operation.Equal && _fastFilterOperator == Operator.And))
					{
						_containsValidFilter = false;
					}
				}
			}
			else
			{
				_containsValidFilter = false;
			}
		}

		internal void AddCondition(Condition condition)
		{
			if (!_containsValidFilter)
			{
				return;
			}

			if (_conditionEncountered)
			{
				if (condition.Operation == _fastFilterOperation)
				{
					AddProperty(condition.Name, condition.Value);
				}
				else
				{
					_containsValidFilter = false;
				}
			}
			else
			{
				_conditionEncountered = true;
				_fastFilterOperation = condition.Operation;
				AddProperty(condition.Name, condition.Value);

				// Don't support `Contains`.
				if (_fastFilterOperation is not Operation.Equal and not Operation.NotEqual)
				{
					_containsValidFilter = false;
				}
			}
		}

		private void AddProperty(string name, string value)
		{
			if (!_filterDictionaryBuilder.TryGetValue(name, out HashSet<string>? values))
			{
				values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				_filterDictionaryBuilder.Add(name, values);
			}

			values.Add(value);
		}

		internal FastFilter? ToFastFilter()
			=> ContainsValidFilter
				? new FastFilter(
					_filterDictionaryBuilder,
					_fastFilterOperation,
					_fastFilterOperator)
				: null;
	}
}
