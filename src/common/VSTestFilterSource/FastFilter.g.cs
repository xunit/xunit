// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
#if IS_VSTEST_REPO
using System.Collections.Immutable;
#endif
using System.Linq;
using System.Text.RegularExpressions;

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
#endif

namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;

#if !IS_VSTEST_REPO
[Embedded]
#endif
internal sealed class FastFilter
{
#if IS_VSTEST_REPO
    internal FastFilter(ImmutableDictionary<string, ISet<string>> filterProperties, Operation filterOperation, Operator filterOperator)
#else
    internal FastFilter(Dictionary<string, ISet<string>> filterProperties, Operation filterOperation, Operator filterOperator)
#endif
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNullOrEmpty(filterProperties, nameof(filterProperties));
#endif

        FilterProperties = filterProperties;

        IsFilteredOutWhenMatched =
            (filterOperation != Operation.Equal || filterOperator != Operator.Or && filterOperator != Operator.None)
            && (filterOperation == Operation.NotEqual && (filterOperator == Operator.And || filterOperator == Operator.None)
                ? true
#if IS_VSTEST_REPO
                : throw new ArgumentException(Resources.Resources.FastFilterException));
#else
                : throw new ArgumentException("An error occurred while creating Fast filter."));
#endif
    }

#if IS_VSTEST_REPO
    internal ImmutableDictionary<string, ISet<string>> FilterProperties { get; }
#else
    internal Dictionary<string, ISet<string>> FilterProperties { get; }
#endif

    internal bool IsFilteredOutWhenMatched { get; }

    internal Regex? PropertyValueRegex { get; set; }

    internal string? PropertyValueRegexReplacement { get; set; }

    internal string[]? ValidForProperties(IEnumerable<string>? properties)
    {
        if (properties is null)
        {
            return null;
        }

        return FilterProperties.Keys.All(name => properties.Contains(name))
            ? null
            : FilterProperties.Keys.Where(name => !properties.Contains(name)).ToArray();
    }

    internal bool Evaluate(Func<string, object?> propertyValueProvider)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(propertyValueProvider, nameof(propertyValueProvider));
#endif

        bool matched = false;
        foreach (var name in FilterProperties.Keys)
        {
            // If there is no value corresponding to given name, treat it as unmatched.
            if (!TryGetPropertyValue(name, propertyValueProvider, out var singleValue, out var multiValues))
            {
                continue;
            }

            if (singleValue != null)
            {
                var value = PropertyValueRegex == null ? singleValue : ApplyRegex(singleValue);
                matched = value != null && FilterProperties[name].Contains(value);
            }
            else
            {
                var values = PropertyValueRegex == null ? multiValues : multiValues?.Select(value => ApplyRegex(value));
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
    /// Apply regex matching or replacement to given value.
    /// </summary>
    /// <returns>For matching, returns the result of matching, null if no match found. For replacement, returns the result of replacement.</returns>
    private string? ApplyRegex(string value)
    {
#if IS_VSTEST_REPO
        TPDebug.Assert(PropertyValueRegex != null);
#endif

        string? result = null;
        if (PropertyValueRegexReplacement == null)
        {
            var match = PropertyValueRegex!.Match(value);
            if (match.Success)
            {
                result = match.Value;
            }
        }
        else
        {
            result = PropertyValueRegex!.Replace(value, PropertyValueRegexReplacement);
        }
        return result;
    }

    /// <summary>
    /// Returns property value for Property using propertValueProvider.
    /// </summary>
    private static bool TryGetPropertyValue(string name, Func<string, object?> propertyValueProvider, out string? singleValue, out string[]? multiValues)
    {
        var propertyValue = propertyValueProvider(name);
        if (null != propertyValue)
        {
            multiValues = propertyValue as string[];
            singleValue = multiValues == null ? propertyValue.ToString() : null;
            return true;
        }

        singleValue = null;
        multiValues = null;
        return false;
    }

    internal static Builder CreateBuilder()
    {
        return new Builder();
    }

    internal sealed class Builder
    {
        private bool _operatorEncountered;
        private Operator _fastFilterOperator = Operator.None;

        private bool _conditionEncountered;
        private Operation _fastFilterOperation;

#if IS_VSTEST_REPO
        private readonly ImmutableDictionary<string, ImmutableHashSet<string>.Builder>.Builder _filterDictionaryBuilder = ImmutableDictionary.CreateBuilder<string, ImmutableHashSet<string>.Builder>(StringComparer.OrdinalIgnoreCase);
#else
        private readonly Dictionary<string, ISet<string>> _filterDictionaryBuilder = new Dictionary<string, ISet<string>>(StringComparer.OrdinalIgnoreCase);
#endif

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
            if (!_filterDictionaryBuilder.TryGetValue(name, out var values))
            {
#if IS_VSTEST_REPO
                values = ImmutableHashSet.CreateBuilder<string>(StringComparer.OrdinalIgnoreCase);
#else
                values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
#endif
                _filterDictionaryBuilder.Add(name, values);
            }

            values.Add(value);
        }

        internal FastFilter? ToFastFilter()
        {
            return ContainsValidFilter
                ? new FastFilter(
#if IS_VSTEST_REPO
                    _filterDictionaryBuilder.ToImmutableDictionary(kvp => kvp.Key, kvp => (ISet<string>)_filterDictionaryBuilder[kvp.Key].ToImmutable()),
#else
                    _filterDictionaryBuilder,
#endif
                    _fastFilterOperation,
                    _fastFilterOperator)
                : null;
        }
    }
}
