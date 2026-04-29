// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Collections.Generic;

#if !IS_VSTEST_REPO
using Microsoft.CodeAnalysis;
#endif

#if IS_VSTEST_REPO
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
#endif

namespace Microsoft.VisualStudio.TestPlatform.Common.Filtering;

/// <summary>
/// Implements ITestCaseFilterExpression, providing test case filtering functionality.
/// </summary>
#if IS_VSTEST_REPO
public class TestCaseFilterExpression : ITestCaseFilterExpression
#else
[Embedded]
internal sealed class TestCaseFilterExpression
#endif
{
    private readonly FilterExpressionWrapper _filterWrapper;

    /// <summary>
    /// If filter Expression is valid for performing TestCase matching
    /// (has only supported properties, syntax etc).
    /// </summary>
    private readonly bool _validForMatch;


    /// <summary>
    /// Adapter specific filter expression.
    /// </summary>
    public TestCaseFilterExpression(FilterExpressionWrapper filterWrapper)
    {
        _filterWrapper = filterWrapper ?? throw new ArgumentNullException(nameof(filterWrapper));
#if IS_VSTEST_REPO
        _validForMatch = filterWrapper.ParseError.IsNullOrEmpty();
#else
        _validForMatch = string.IsNullOrEmpty(filterWrapper.ParseError);
#endif
    }

    /// <summary>
    /// Gets the user specified filter criteria.
    /// </summary>
    public string TestCaseFilterValue => _filterWrapper.FilterString;

    /// <summary>
    /// Validate if underlying filter expression is valid for given set of supported properties.
    /// </summary>
#if IS_VSTEST_REPO
    public string[]? ValidForProperties(IEnumerable<string>? supportedProperties, Func<string, TestProperty?> propertyProvider)
#else
    public string[]? ValidForProperties(IEnumerable<string>? supportedProperties)
#endif
    {
        if (_validForMatch)
        {
#if IS_VSTEST_REPO
            return _filterWrapper.ValidForProperties(supportedProperties, propertyProvider);
#else
            return _filterWrapper.ValidForProperties(supportedProperties);
#endif
        }

        return null;
    }

    /// <summary>
    /// Match test case with filter criteria.
    /// </summary>
#if IS_VSTEST_REPO
    public bool MatchTestCase(TestCase testCase, Func<string, object?> propertyValueProvider)
#else
    public bool MatchTestCase(Func<string, object?> propertyValueProvider)
#endif
    {
#if IS_VSTEST_REPO
        ValidateArg.NotNull(testCase, nameof(testCase));
        ValidateArg.NotNull(propertyValueProvider, nameof(propertyValueProvider));
#endif
        if (_validForMatch)
        {
            return _filterWrapper.Evaluate(propertyValueProvider);
        }

        return false;
    }

}
