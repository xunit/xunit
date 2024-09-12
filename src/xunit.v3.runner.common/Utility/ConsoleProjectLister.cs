using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// Helper class to list project contents out to <see cref="Console"/>.
/// </summary>
public static class ConsoleProjectLister
{
	static string Escape(string value) =>
		value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");

	/// <summary>
	/// List the contents of the test cases to the console, based on the provided option and format.
	/// </summary>
	public static void List<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListOption listOption,
		ListFormat listFormat)
			where TTestCase : ITestCaseMetadata
	{
		Guard.ArgumentNotNull(consoleHelper);
		Guard.ArgumentNotNull(testCasesByAssembly);

		Action<ConsoleHelper, IReadOnlyDictionary<string, List<TTestCase>>, ListFormat>? lister = listOption switch
		{
			ListOption.Classes => Classes,
			ListOption.Full => Full,
			ListOption.Methods => Methods,
			ListOption.Tests => Tests,
			ListOption.Traits => Traits,
			_ => null
		};

		lister?.Invoke(consoleHelper, testCasesByAssembly, listFormat);
	}

	static void Classes<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : ITestCaseMetadata
	{
		var testClasses =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Select(tc => tc.TestClassName)
				.WhereNotNull()
				.Distinct()
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var testClass in testClasses)
					serializer.Serialize(testClass);

			consoleHelper.WriteLine(buffer.ToString());
		}
		else
		{
			consoleHelper.WriteLine();

			foreach (var testClass in testClasses)
				consoleHelper.WriteLine(testClass);
		}
	}

	static void Full<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : ITestCaseMetadata
	{
		var fullTestCases =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value.Select(tc => new { assemblyFileName = kvp.Key, testCase = tc }))
				.Select(tuple => new
				{
					Assembly = tuple.assemblyFileName,
					DisplayName = tuple.testCase.TestCaseDisplayName,
					Class = tuple.testCase.TestClassName,
					Method = tuple.testCase.TestMethodName,
					Skip = tuple.testCase.SkipReason,
					Traits = tuple.testCase.Traits.Count > 0 ? tuple.testCase.Traits : null,
				});

		if (format == ListFormat.Json)
		{
			var buffer = new StringBuilder();

			using (var rootSerializer = new JsonArraySerializer(buffer))
				foreach (var testCase in fullTestCases.OrderBy(x => x.Assembly).ThenBy(x => x.DisplayName))
				{
					using var testCaseSerializer = rootSerializer.SerializeObject();

					testCaseSerializer.Serialize("Assembly", testCase.Assembly);
					testCaseSerializer.Serialize("DisplayName", testCase.DisplayName);
					testCaseSerializer.Serialize("Class", testCase.Class);
					testCaseSerializer.Serialize("Method", testCase.Method);

					if (testCase.Skip is not null)
						testCaseSerializer.Serialize("Skip", testCase.Skip);
					if (testCase.Traits is not null)
						testCaseSerializer.SerializeTraits("Traits", testCase.Traits);
				}

			consoleHelper.WriteLine(buffer.ToString());
		}
		else
		{
			foreach (var assemblyGroup in fullTestCases.OrderBy(x => x.Assembly).GroupBy(x => x.Assembly))
			{
				consoleHelper.WriteLine();
				consoleHelper.WriteLine("Assembly: {0}", assemblyGroup.Key);

				foreach (var testCase in assemblyGroup.OrderBy(x => x.Class).ThenBy(x => x.Method))
				{
					consoleHelper.WriteLine("      - Display name: \"{0}\"", Escape(testCase.DisplayName));

					if (testCase.Method is not null)
						consoleHelper.WriteLine("        Test method:  {0}.{1}", testCase.Class, testCase.Method);
					else if (testCase.Class is not null)
						consoleHelper.WriteLine("        Test class:   {0}", testCase.Class);

					if (testCase.Skip is not null)
						consoleHelper.WriteLine("        Skip reason:  \"{0}\"", Escape(testCase.Skip));

					if (testCase.Traits is not null)
					{
						consoleHelper.WriteLine("        Traits:");
						foreach (var trait in testCase.Traits)
							consoleHelper.WriteLine("          \"{0}\": [{1}]", Escape(trait.Key), string.Join(", ", trait.Value.OrderBy(v => v).Select(v => '"' + Escape(v) + '"')));
					}
				}
			}
		}
	}

	static void Methods<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : ITestCaseMetadata
	{
		var testMethods =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Where(tc => tc.TestClassName is not null && tc.TestMethodName is not null)
				.Select(tc => string.Format(CultureInfo.CurrentCulture, "{0}.{1}", tc.TestClassName, tc.TestMethodName))
				.Distinct()
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var testMethod in testMethods)
					serializer.Serialize(testMethod);

			consoleHelper.WriteLine(buffer.ToString());
		}
		else
		{
			consoleHelper.WriteLine();

			foreach (var testMethod in testMethods)
				consoleHelper.WriteLine(testMethod);
		}
	}

	static void Tests<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : ITestCaseMetadata
	{
		var displayNames =
			testCasesByAssembly
				.SelectMany(kvp => kvp.Value)
				.Select(tc => tc.TestCaseDisplayName)
				.OrderBy(x => x)
				.ToList();

		if (format == ListFormat.Json)
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var displayName in displayNames)
					serializer.Serialize(displayName);

			consoleHelper.WriteLine(buffer.ToString());
		}
		else
		{
			consoleHelper.WriteLine();

			foreach (var displayName in displayNames)
				consoleHelper.WriteLine(displayName);
		}
	}

	static void Traits<TTestCase>(
		ConsoleHelper consoleHelper,
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : ITestCaseMetadata
	{
		var combinedTraits = new Dictionary<string, HashSet<string>>();

		foreach (var traits in testCasesByAssembly.SelectMany(kvp => kvp.Value).Select(tc => tc.Traits).WhereNotNull())
			foreach (var kvp in traits)
			{
				var list = combinedTraits.AddOrGet(kvp.Key, () => []);
				foreach (var value in kvp.Value)
					list.Add(value);
			}

		if (format == ListFormat.Json)
		{
			var buffer = new StringBuilder();

			using (var rootSerializer = new JsonObjectSerializer(buffer))
				foreach (var trait in combinedTraits.OrderBy(kvp => kvp.Key))
					using (var valueSerializer = rootSerializer.SerializeArray(trait.Key))
						foreach (var value in trait.Value.OrderBy(v => v))
							valueSerializer.Serialize(value);

			consoleHelper.WriteLine(buffer.ToString());
		}
		else
		{
			consoleHelper.WriteLine();

			foreach (var trait in combinedTraits.OrderBy(kvp => kvp.Key))
				consoleHelper.WriteLine("\"{0}\": [{1}]", Escape(trait.Key), string.Join(", ", trait.Value.OrderBy(v => v).Select(v => '"' + Escape(v) + '"')));
		}
	}
}
