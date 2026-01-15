// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.
using System;
using System.Collections.Generic;

namespace Xunit.Runner.Common;

internal sealed class TestCaseFilterExpression
{
	private readonly FilterExpressionWrapper _filterWrapper;

	/// <summary>
	/// If filter Expression is valid for performing TestCase matching
	/// (has only supported properties, syntax etc).
	/// </summary>
	private readonly bool _validForMatch;

	/// <summary>
	/// Initializes a new instance of the <see cref="TestCaseFilterExpression"/> class.
	/// Adapter specific filter expression.
	/// </summary>
	public TestCaseFilterExpression(FilterExpressionWrapper filterWrapper)
	{
		_filterWrapper = filterWrapper ?? throw new ArgumentNullException(nameof(filterWrapper));
		_validForMatch = string.IsNullOrEmpty(filterWrapper.ParseError);
	}

	/// <summary>
	/// Gets user specified filter criteria.
	/// </summary>
	public string TestCaseFilterValue => _filterWrapper.FilterString;

	/// <summary>
	/// Validate if underlying filter expression is valid for given set of supported properties.
	/// </summary>
	public string[]? ValidForProperties(IEnumerable<string>? supportedProperties)
	{
		string[]? invalidProperties = null;
		if (_validForMatch)
		{
			invalidProperties = _filterWrapper.ValidForProperties(supportedProperties);
		}

		return invalidProperties;
	}

	/// <summary>
	/// Match test case with filter criteria.
	/// </summary>
	public bool MatchTestCase(Func<string, object?> propertyValueProvider)
		=> _validForMatch && _filterWrapper.Evaluate(propertyValueProvider);
}
