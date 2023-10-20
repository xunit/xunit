using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Runner.Common;

/// <summary>
/// Helper class to list project contents out to <see cref="Console"/>.
/// </summary>
public static class ConsoleProjectLister
{
	/// <summary>
	/// List the contents of the test cases to the console, based on the provided option and format.
	/// </summary>
	public static void List<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListOption listOption,
		ListFormat listFormat)
			where TTestCase : _ITestCaseMetadata
	{
		Action<IReadOnlyDictionary<string, List<TTestCase>>, ListFormat>? lister = listOption switch
		{
			ListOption.Classes => Classes,
			ListOption.Full => Full,
			ListOption.Methods => Methods,
			ListOption.Tests => Tests,
			ListOption.Traits => Traits,
			_ => null
		};

		lister?.Invoke(testCasesByAssembly, listFormat);
	}

	static void Classes<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
	{
		var testClasses =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Where(tc => tc.TestClassName is not null)
				.Select(tc => tc.TestClassNameWithNamespace)
				.Distinct()
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
			Console.WriteLine(JsonSerializer.Serialize(testClasses));
		else
			foreach (var testClass in testClasses)
				Console.WriteLine(testClass);
	}

	static void Full<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
	{
		var fullTestCases =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value.Select(tc => new { assemblyFileName = kvp.Key, testCase = tc }))
				.Select(tuple => new
				{
					Assembly = tuple.assemblyFileName,
					DisplayName = tuple.testCase.TestCaseDisplayName,
					Class = tuple.testCase.TestClassNameWithNamespace,
					Method = tuple.testCase.TestMethodName,
					Skip = tuple.testCase.SkipReason,
					Traits = tuple.testCase.Traits.Count > 0 ? tuple.testCase.Traits : null,
				})
				.OrderBy(x => x.Assembly)
				.ThenBy(x => x.DisplayName);

		var jsonOptions = new JsonSerializerOptions { WriteIndented = format == ListFormat.Text, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

		if (format == ListFormat.Json)
			Console.WriteLine(JsonSerializer.Serialize(fullTestCases, jsonOptions));
		else
			foreach (var testCase in fullTestCases)
				Console.WriteLine(JsonSerializer.Serialize(testCase, jsonOptions));
	}

	static void Methods<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
	{
		var testMethods =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Where(tc => tc.TestClassName is not null && tc.TestMethodName is not null)
				.Select(tc => string.Format(CultureInfo.CurrentCulture, "{0}.{1}", tc.TestClassNameWithNamespace, tc.TestMethodName))
				.Distinct()
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
			Console.WriteLine(JsonSerializer.Serialize(testMethods));
		else
			foreach (var testMethod in testMethods)
				Console.WriteLine(testMethod);
	}

	static void Tests<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
	{
		var displayNames =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Select(tc => tc.TestCaseDisplayName)
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
			Console.WriteLine(JsonSerializer.Serialize(displayNames));
		else
			foreach (var displayName in displayNames)
				Console.WriteLine(displayName);
	}

	static void Traits<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
	{
		static string Escape(string value) =>
			ArgumentFormatter.EscapeString(value).Replace("\"", "\\\"");

		var emptyList = new List<string>();

		var traits =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.SelectMany(tc => tc.Traits.Keys.ToList())
				.Where(key => !string.IsNullOrWhiteSpace(key))
				.Distinct()
				.OrderBy(x => x)
				.Select(key => new
				{
					key,
					values =
						testCasesByAssembly
							.SelectMany(kvp => kvp.Value)
							.SelectMany(tc2 => tc2.Traits.FirstOrDefault(kvp => kvp.Key == key).Value ?? emptyList)
							.WhereNotNull()
							.Distinct()
							.OrderBy(x => x)
				})
				.ToList();

		if (format == ListFormat.Json)
		{
			// Hand-craft an object-style output, ala:
			//   {"Assembly":["trait"],"Culture":["en-US","fr-FR"]}

			Console.Write("{");
			var first = true;

			foreach (var trait in traits)
			{
				if (!first)
					Console.Write(",");
				else
					first = false;

				Console.Write("{0}:{1}", JsonSerializer.Serialize(trait.key), JsonSerializer.Serialize(trait.values));
			}

			Console.WriteLine("}");
		}
		else
		{
			foreach (var trait in traits)
				foreach (var value in trait.values)
					Console.WriteLine("\"{0}\" => \"{1}\"", Escape(trait.key), Escape(value));
		}
	}
}
