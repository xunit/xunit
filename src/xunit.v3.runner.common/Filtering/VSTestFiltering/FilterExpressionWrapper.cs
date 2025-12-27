// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// NOTE: This file is copied as-is from VSTest source code.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Xunit.Runner.Common;

internal sealed class FilterExpressionWrapper
{
	/// <summary>
	/// FilterExpression corresponding to filter criteria.
	/// </summary>
	private readonly FilterExpression? _filterExpression;

	/// <remarks>
	/// Exposed for testing purpose.
	/// </remarks>
#pragma warning disable SA1401 // Fields should be private
#pragma warning disable SA1604 // Element documentation should have summary
	internal readonly FastFilter? _fastFilter;
#pragma warning restore SA1604 // Element documentation should have summary
#pragma warning restore SA1401 // Fields should be private

	/// <summary>
	/// Initializes a new instance of the <see cref="FilterExpressionWrapper"/> class.
	/// Initializes FilterExpressionWrapper with given filterString.
	/// </summary>
	public FilterExpressionWrapper(string filterString)
	{
		if (string.IsNullOrEmpty(filterString))
		{
			throw new ArgumentException("Expected filterString to be not null or empty.", nameof(filterString));
		}

		FilterString = filterString;

		try
		{
			// We prefer fast filter when it's available.
			_filterExpression = FilterExpression.Parse(filterString, out _fastFilter);

			if (UseFastFilter)
			{
				_filterExpression = null;
			}
		}
		catch (FormatException ex)
		{
			ParseError = ex.Message;
		}
		catch (ArgumentException ex)
		{
			_fastFilter = null;
			ParseError = ex.Message;
		}
	}

	private bool UseFastFilter => _fastFilter != null;

	/// <summary>
	/// Gets user specified filter criteria.
	/// </summary>
	public string FilterString { get; }

	/// <summary>
	/// Gets parsing error (if any), when parsing 'FilterString' with built-in parser.
	/// </summary>
	public string? ParseError { get; }

	/// <summary>
	/// Validate if underlying filter expression is valid for given set of supported properties.
	/// </summary>
	public string[]? ValidForProperties(IEnumerable<string>? supportedProperties)
		=> UseFastFilter
			? _fastFilter!.ValidForProperties(supportedProperties)
			: _filterExpression?.ValidForProperties(supportedProperties);

	/// <summary>
	/// Evaluate filterExpression with given propertyValueProvider.
	/// </summary>
	public bool Evaluate(Func<string, object?> propertyValueProvider)
	{
		if (propertyValueProvider is null)
		{
			throw new ArgumentNullException(nameof(propertyValueProvider));
		}

		return UseFastFilter
			? _fastFilter!.Evaluate(propertyValueProvider)
			: _filterExpression != null && _filterExpression.Evaluate(propertyValueProvider);
	}
}
