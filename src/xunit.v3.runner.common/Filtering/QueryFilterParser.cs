using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// This class is used to parse a graphy query.
/// </summary>
/// <remarks>
/// See <see href="https://xunit.net/docs/query-filter-language"/> for the query filter language description.
/// </remarks>
public static class QueryFilterParser
{
	static readonly Regex escapeRegex = new("&#[xX]([0-9a-fA-F]{1,4});");

	static readonly List<Func<string, ITestCaseFilter>> segmentFactories =
	[
		filter => new FilterAssembly(filter),
		filter => new FilterNamespace(filter),
		filter => new FilterClassSimpleName(filter),
		filter => new FilterMethodSimpleName(filter),
	];

	/// <summary>
	/// Parse and return a filter that represents the query.
	/// </summary>
	public static ITestCaseFilter Parse(string query)
	{
		Guard.ArgumentNotNull(query);

		if (query.Length == 0 || query[0] != '/')
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: query must begin with '/'", query), nameof(query));

		var filterRemainder = query.Substring(1);
		var baseFilter = default(ITestCaseFilter);
		var segmentIndex = 0;

		while (filterRemainder.Length > 0)
		{
			if (filterRemainder.StartsWith("![", StringComparison.Ordinal))
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: negate a trait expression with 'name!=value'", query), nameof(query));

			var isTraitFilter = false;
			if (filterRemainder[0] == '[')
			{
				if (filterRemainder[filterRemainder.Length - 1] != ']')
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: saw opening '[' without ending ']'", query), nameof(query));

				isTraitFilter = true;
				filterRemainder = filterRemainder.Substring(1, filterRemainder.Length - 2);
			}
			else if (segmentIndex >= segmentFactories.Count)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: too many segments (max {1})", query, segmentFactories.Count), nameof(query));

			(var parsedFilter, filterRemainder) = ParseLogicalExpression(
				query,
				filterRemainder,
				isTraitFilter ? ParseTraitExpression : (q, n) => ParseSegmentExpression(q, n, segmentFactories[segmentIndex]),
				isTraitFilter
			);

			if (parsedFilter is not null)
			{
				if (baseFilter is null)
					baseFilter = parsedFilter;
				else if (baseFilter is FilterLogicalAnd andFilter)
					andFilter.Filters.Add(parsedFilter);
				else
					baseFilter = new FilterLogicalAnd(baseFilter, parsedFilter);
			}

			segmentIndex++;
		}

		return baseFilter ?? FilterPass.Instance;
	}

	static (ITestCaseFilter?, string) ParseLogicalExpression(
		string query,
		string partialQuery,
		Func<string, bool, ITestCaseFilter?> queryParser,
		bool isTraitFilter,
		bool insideParenthesis = false)
	{
		var negated = false;
		if (partialQuery.Length != 0 && partialQuery[0] == '!')
		{
			negated = true;
			partialQuery = partialQuery.Substring(1);
		}

		if (partialQuery.Length != 0 && partialQuery[0] == '(')
		{
			if (negated)
				if (isTraitFilter)
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: negate a trait expression with 'name!=value'", query), nameof(query));
				else
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: negating operator '!' must be inside parenthesis, not outside", query), nameof(query));

			var residual = partialQuery.Substring(1);
			ITestCaseFilterComposite? baseComposite = default;

			while (true)
			{
				(var innerFilter, residual) = ParseLogicalExpression(query, residual, queryParser, isTraitFilter, true);

				if (residual.Length == 0)
				{
					if (baseComposite is null)
						return (innerFilter, string.Empty);
					if (innerFilter is null)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unexpected null filter from partial query '{0}'", partialQuery));

					baseComposite.Filters.Add(innerFilter);
					return (baseComposite, string.Empty);
				}

				var @operator = residual[0];

				// expression termination
				if ((@operator == '/' && !insideParenthesis) || (@operator == ')' && insideParenthesis))
				{
					if (baseComposite is null)
						return (innerFilter, residual.Substring(1));
					if (innerFilter is null)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unexpected null filter from partial query '{0}'", partialQuery));

					baseComposite.Filters.Add(innerFilter);
					return (baseComposite, residual.Substring(1));
				}

				// logical OR
				if (@operator == '|')
				{
					if (baseComposite is null)
						baseComposite = new FilterLogicalOr();
					else if (baseComposite is not FilterLogicalOr)
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: logical expressions cannot mix '|' and '&' without grouping parentheses", query), nameof(query));

					if (innerFilter is null)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unexpected null filter from partial query '{0}'", partialQuery));

					baseComposite.Filters.Add(innerFilter);
				}
				// logical AND
				else if (@operator == '&')
				{
					if (baseComposite is null)
						baseComposite = new FilterLogicalAnd();
					else if (baseComposite is not FilterLogicalAnd)
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: logical expressions cannot mix '|' and '&' without grouping parentheses", query), nameof(query));

					if (innerFilter is null)
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Unexpected null filter from partial query '{0}'", partialQuery));

					baseComposite.Filters.Add(innerFilter);
				}
				// invalid character
				else
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: unexpected character '{1}' after closing parenthesis", query, residual[0]), nameof(query));

				if (residual.Length < 2)
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: logical operator '{1}' cannot end a query", query, @operator), nameof(query));
				if (residual[1] != '(')
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: logical operator '{1}' must be followed by an open parenthesis", query, @operator), nameof(query));

				residual = residual.Substring(2);
			}
		}
		else
		{
			var endIdx =
				insideParenthesis
					? partialQuery.IndexOf(')')
					: isTraitFilter
						? -1
						: partialQuery.IndexOf('/');

			if (insideParenthesis && endIdx == -1)
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Query filter '{0}' is not valid: open '(' does not have matching closing ')'", query), nameof(query));

			var segment = endIdx < 0 ? partialQuery : partialQuery.Substring(0, endIdx);

			if (negated)
			{
				if (isTraitFilter)
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Trait expression '!{0}' is not valid: negate a trait expression with 'name!=value'", segment), nameof(query));

				if (segment.Length == 0)
					throw new ArgumentException("Filter expression '!' is not valid: missing the segment query to negate", nameof(query));

				if (segment is "*")
					throw new ArgumentException("Filter expression '!*' is not valid: this would exclude all tests", nameof(query));
			}

			var remainder = endIdx < 0 ? string.Empty : partialQuery.Substring(endIdx + 1);
			var resultFilter = queryParser(segment, negated);
			return (resultFilter, remainder);
		}
	}

	static ITestCaseFilter? ParseSegmentExpression(
		string query,
		bool negated,
		Func<string, ITestCaseFilter> queryFactory) =>
			query is "*"
				? (negated ? FilterFail.Instance : null)
				: (negated ? new FilterLogicalNot(queryFactory(query)) : queryFactory(query));

	static ITestCaseFilter? ParseTraitExpression(
		string query,
		bool negated)
	{
		if (negated)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Trait expression '!{0}' is not valid: negate a trait expression with 'name!=value'", query), nameof(query));

		var equalIdx = query.IndexOf('=');
		var equalLen = 1;
		if (equalIdx > 0 && query[equalIdx - 1] == '!')
		{
			equalIdx--;
			equalLen++;
		}

		if (equalIdx > 0)
		{
			var name = query.Substring(0, equalIdx);
			var value = query.Substring(equalIdx + equalLen);

			if (name.Length > 0 && value.Length > 0)
			{
				var filter = new FilterTrait(name, value);
				return
					equalLen == 2
						? new FilterLogicalNot(filter)
						: filter;
			}
		}

		throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Trait expression '{0}' is not valid: trait queries must be 'name=value' or 'name!=value'", query), nameof(query));
	}

	internal static Func<string?, bool> ToEvaluator(string query)
	{
		if (query.Length == 0 || query == "*")
			return _ => true;

		var mutatedSearchString = query;
		var startsWith = false;
		var endsWith = false;

		if (mutatedSearchString.StartsWith("*", StringComparison.Ordinal))
		{
			endsWith = true;
			mutatedSearchString = mutatedSearchString.Substring(1);
		}

		if (mutatedSearchString.EndsWith("*", StringComparison.Ordinal))
		{
			startsWith = true;
			mutatedSearchString = mutatedSearchString.Substring(0, mutatedSearchString.Length - 1);
		}

		if (mutatedSearchString.Length == 0)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Filter expression '{0}' is not valid: wildcards must include text in the middle", query), nameof(query));

		if (mutatedSearchString.Contains("*"))
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Filter expression '{0}' is not valid: wildcards may only be at the beginning and/or end of a filter expression", query), nameof(query));

		while (true)
		{
			var match = escapeRegex.Match(mutatedSearchString);
			if (!match.Success)
				break;

			mutatedSearchString =
				mutatedSearchString.Substring(0, match.Index) +
				(char)Convert.ToInt32(match.Captures[0].Value.Substring(3).TrimEnd(';'), 16) +
				mutatedSearchString.Substring(match.Index + match.Length);
		}

		return (startsWith, endsWith) switch
		{
#pragma warning disable CA1862  // netstandard2.0 does not offer a Contains overload with StringComparison
			(true, true) => value => value?.ToUpperInvariant().Contains(mutatedSearchString.ToUpperInvariant()) == true,
#pragma warning restore CA1862
			(true, false) => value => value?.StartsWith(mutatedSearchString, StringComparison.OrdinalIgnoreCase) == true,
			(false, true) => value => value?.EndsWith(mutatedSearchString, StringComparison.OrdinalIgnoreCase) == true,
			_ => value => value?.Equals(mutatedSearchString, StringComparison.OrdinalIgnoreCase) == true,
		};
	}
}
