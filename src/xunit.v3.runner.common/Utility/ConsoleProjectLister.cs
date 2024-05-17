using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xunit.Internal;
using Xunit.v3;

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
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var testClass in testClasses)
					serializer.Serialize(testClass);

			Console.WriteLine(buffer.ToString());
		}
		else
		{
			Console.WriteLine();

			foreach (var testClass in testClasses)
				Console.WriteLine(testClass);
		}
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
						testCaseSerializer.Serialize("Traits", testCase.Traits);
				}

			Console.WriteLine(buffer.ToString());
		}
		else
		{
			foreach (var assemblyGroup in fullTestCases.OrderBy(x => x.Assembly).GroupBy(x => x.Assembly))
			{
				Console.WriteLine();
				Console.WriteLine("Assembly: {0}", assemblyGroup.Key);

				foreach (var testCase in assemblyGroup.OrderBy(x => x.Class).ThenBy(x => x.Method))
				{
					Console.WriteLine("      - Display name: \"{0}\"", Escape(testCase.DisplayName));

					if (testCase.Method is not null)
						Console.WriteLine("        Test method:  {0}.{1}", testCase.Class, testCase.Method);
					else if (testCase.Class is not null)
						Console.WriteLine("        Test class:   {0}", testCase.Class);

					if (testCase.Skip is not null)
						Console.WriteLine("        Skip reason:  \"{0}\"", Escape(testCase.Skip));

					if (testCase.Traits is not null)
					{
						Console.WriteLine("        Traits:");
						foreach (var trait in testCase.Traits)
							Console.WriteLine("          \"{0}\": [{1}]", Escape(trait.Key), string.Join(", ", trait.Value.OrderBy(v => v).Select(v => '"' + Escape(v) + '"')));
					}
				}
			}
		}
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
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var testMethod in testMethods)
					serializer.Serialize(testMethod);

			Console.WriteLine(buffer.ToString());
		}
		else
		{
			Console.WriteLine();

			foreach (var testMethod in testMethods)
				Console.WriteLine(testMethod);
		}
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
		{
			var buffer = new StringBuilder();

			using (var serializer = new JsonArraySerializer(buffer))
				foreach (var displayName in displayNames)
					serializer.Serialize(displayName);

			Console.WriteLine(buffer.ToString());
		}
		else
		{
			Console.WriteLine();

			foreach (var displayName in displayNames)
				Console.WriteLine(displayName);
		}
	}

	static void Traits<TTestCase>(
		IReadOnlyDictionary<string, List<TTestCase>> testCasesByAssembly,
		ListFormat format)
			where TTestCase : _ITestCaseMetadata
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

			Console.WriteLine(buffer.ToString());
		}
		else
		{
			Console.WriteLine();

			foreach (var trait in combinedTraits.OrderBy(kvp => kvp.Key))
				Console.WriteLine("\"{0}\": [{1}]", Escape(trait.Key), string.Join(", ", trait.Value.OrderBy(v => v).Select(v => '"' + Escape(v) + '"')));
		}
	}
}
