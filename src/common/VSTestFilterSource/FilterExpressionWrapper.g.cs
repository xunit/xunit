// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;
#if IS_VSTEST_REPO
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
#endif

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
#endif

namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;

/// <summary>
/// Class holds information related to filtering criteria.
/// </summary>
#if IS_VSTEST_REPO
public class FilterExpressionWrapper
#else
[Embedded]
internal sealed class FilterExpressionWrapper
#endif
{
    /// <summary>
    /// FilterExpression corresponding to filter criteria
    /// </summary>
    private readonly FilterExpression? _filterExpression;

    /// <remarks>
    /// Exposed for testing purpose.
    /// </remarks>
    internal readonly FastFilter? FastFilter;

    /// <summary>
    /// Initializes FilterExpressionWrapper with given filterString and options.
    /// </summary>
#if IS_VSTEST_REPO
    public FilterExpressionWrapper(string filterString, FilterOptions? options)
#else
    public FilterExpressionWrapper(string filterString)
#endif
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNullOrEmpty(filterString, nameof(filterString));
#endif

        FilterString = filterString;

#if IS_VSTEST_REPO
        FilterOptions = options;
#endif

        try
        {
            // We prefer fast filter when it's available.
            _filterExpression = FilterExpression.Parse(filterString, out FastFilter);

            if (UseFastFilter)
            {
                _filterExpression = null;

                // Property value regex is only supported for fast filter,
                // so we ignore it if no fast filter is constructed.

#if IS_VSTEST_REPO
                // TODO: surface an error message to user.
                var regexString = options?.FilterRegEx;
                if (!regexString.IsNullOrEmpty())
                {
                    TPDebug.Assert(options!.FilterRegExReplacement == null || options.FilterRegEx != null);
                    FastFilter.PropertyValueRegex = new Regex(regexString, RegexOptions.Compiled);
                    FastFilter.PropertyValueRegexReplacement = options.FilterRegExReplacement;
                }
#endif
            }

        }
        catch (FormatException ex)
        {
            ParseError = ex.Message;
        }
        catch (ArgumentException ex)
        {
            FastFilter = null;
            ParseError = ex.Message;
        }
    }

#if IS_VSTEST_REPO
    /// <summary>
    /// Initializes FilterExpressionWrapper with given filterString.
    /// </summary>
    public FilterExpressionWrapper(string filterString)
        : this(filterString, null)
    {
    }
#endif

#if IS_VSTEST_REPO
    [MemberNotNullWhen(true, nameof(FastFilter))]
#endif
    private bool UseFastFilter => FastFilter != null;

    /// <summary>
    /// User specified filter criteria.
    /// </summary>
    public string FilterString { get; }

#if IS_VSTEST_REPO
    /// <summary>
    /// User specified additional filter options.
    /// </summary>
    public FilterOptions? FilterOptions { get; }
#endif

    /// <summary>
    /// Parsing error (if any), when parsing 'FilterString' with built-in parser.
    /// </summary>
    public string? ParseError { get; }

    /// <summary>
    /// Validate if underlying filter expression is valid for given set of supported properties.
    /// </summary>
#if IS_VSTEST_REPO
    public string[]? ValidForProperties(IEnumerable<string>? supportedProperties, Func<string, TestProperty?>? propertyProvider)
#else
    public string[]? ValidForProperties(IEnumerable<string>? supportedProperties)
#endif
        => UseFastFilter
            ? FastFilter!.ValidForProperties(supportedProperties)
#if IS_VSTEST_REPO
            : _filterExpression?.ValidForProperties(supportedProperties, propertyProvider);
#else
            : _filterExpression?.ValidForProperties(supportedProperties);
#endif

    /// <summary>
    /// Evaluate filterExpression with given propertyValueProvider.
    /// </summary>
    public bool Evaluate(Func<string, object?> propertyValueProvider)
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(propertyValueProvider, nameof(propertyValueProvider));
#endif

        return UseFastFilter
            ? FastFilter!.Evaluate(propertyValueProvider)
            : _filterExpression != null && _filterExpression.Evaluate(propertyValueProvider);
    }
}
