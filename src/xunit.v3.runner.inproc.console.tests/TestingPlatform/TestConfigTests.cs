using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Testing.Platform.Configurations;
using Xunit;
using Xunit.Runner.Common;
using Xunit.MicrosoftTestingPlatform;
using Xunit.Sdk;

public class TestConfigTests
{
	[Fact]
	public void DefaultValues()
	{
		var config = new StubConfiguration();
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

		TestConfig.Parse(config, projectAssembly);

		Assert.Null(projectAssembly.Configuration.AssertEquivalentMaxDepth);
		Assert.Null(projectAssembly.Configuration.Culture);
		Assert.Null(projectAssembly.Configuration.DiagnosticMessages);
		Assert.Null(projectAssembly.Configuration.ExplicitOption);
		Assert.Null(projectAssembly.Configuration.FailSkips);
		Assert.Null(projectAssembly.Configuration.FailTestsWithWarnings);
		Assert.Null(projectAssembly.Configuration.InternalDiagnosticMessages);
		Assert.Null(projectAssembly.Configuration.LongRunningTestSeconds);
		Assert.Null(projectAssembly.Configuration.MaxParallelThreads);
		Assert.Null(projectAssembly.Configuration.MethodDisplay);
		Assert.Null(projectAssembly.Configuration.MethodDisplayOptions);
		Assert.Null(projectAssembly.Configuration.ParallelAlgorithm);
		Assert.Null(projectAssembly.Configuration.ParallelizeTestCollections);
		Assert.Null(projectAssembly.Configuration.PrintMaxEnumerableLength);
		Assert.Null(projectAssembly.Configuration.PrintMaxObjectDepth);
		Assert.Null(projectAssembly.Configuration.PrintMaxObjectMemberCount);
		Assert.Null(projectAssembly.Configuration.PrintMaxStringLength);
		Assert.Null(projectAssembly.Configuration.Seed);
		Assert.Null(projectAssembly.Configuration.ShowLiveOutput);
		Assert.Null(projectAssembly.Configuration.StopOnFail);
	}

	[Theory]
	[InlineData(null, "<unset>")]  // Sentinel value
	[InlineData("default", null)]
	[InlineData("invariant", "")]
	[InlineData("foo", "foo")]
	public void Culture(
		string? value,
		string? expected)
	{
		var config = new StubConfiguration((TestConfig.Keys.Culture, value));
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();
		projectAssembly.Configuration.Culture = "<unset>";

		TestConfig.Parse(config, projectAssembly);

		Assert.Equal(expected, projectAssembly.Configuration.Culture);
	}

	public static IEnumerable<TheoryDataRow<string, int?>> MaxThreadsData()
	{
		// Invalid values will return the sentinel value
		yield return ("abc", 26002112);
		yield return ("0.ax", 26002112);  // Non-digit
		yield return ("0,ax", 26002112);  // Non-digit
		yield return (".0x", 26002112);   // Missing leading digit(s)
		yield return (",0x", 26002112);   // Missing leading digit(s)

		// Special values
		yield return ("default", null);
		yield return ("0", null);
		yield return ("unlimited", -1);
		yield return ("-1", -1);

		// Valid constant value
		yield return ("16", 16);

		// Valid multiplier value
		yield return ("2x", Environment.ProcessorCount * 2);
		yield return ("3.5x", (int)(Environment.ProcessorCount * 3.5));
		yield return ("5,0x", Environment.ProcessorCount * 5);
	}

	[Theory]
	[MemberData(nameof(MaxThreadsData))]
	public void MaxParallelThreads(
		string value,
		int? expected)
	{
		var config = new StubConfiguration((TestConfig.Keys.MaxParallelThreads, value));
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();
		projectAssembly.Configuration.MaxParallelThreads = 26002112;

		TestConfig.Parse(config, projectAssembly);

		Assert.Equal(expected, projectAssembly.Configuration.MaxParallelThreads);
	}

	[Theory]
	[InlineData("unknownValue", null)]
	[InlineData("classAndMethod", TestMethodDisplay.ClassAndMethod)]
	[InlineData("method", TestMethodDisplay.Method)]
	public void MethodDisplay(
		string value,
		TestMethodDisplay? expected)
	{
		var config = new StubConfiguration((TestConfig.Keys.MethodDisplay, value));
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

		TestConfig.Parse(config, projectAssembly);

		Assert.Equal(expected, projectAssembly.Configuration.MethodDisplay);
	}

	[Theory]
	[InlineData("unknownValue", null)]
	[InlineData("none", TestMethodDisplayOptions.None)]
	[InlineData("all", TestMethodDisplayOptions.All)]
	[InlineData("UseOperatorMonikers, UseEscapeSequences", TestMethodDisplayOptions.UseOperatorMonikers | TestMethodDisplayOptions.UseEscapeSequences)]
	public void MethodDisplayOptions(
		string value,
		TestMethodDisplayOptions? expected)
	{
		var config = new StubConfiguration((TestConfig.Keys.MethodDisplayOptions, value));
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

		TestConfig.Parse(config, projectAssembly);

		Assert.Equal(expected, projectAssembly.Configuration.MethodDisplayOptions);
	}

	[Theory]
	[InlineData("unknownValue", null)]
	[InlineData("aggressive", ParallelAlgorithm.Aggressive)]
	[InlineData("conservative", ParallelAlgorithm.Conservative)]
	public void ParallelAlgorithms(
		string value,
		ParallelAlgorithm? expected)
	{
		var config = new StubConfiguration((TestConfig.Keys.ParallelAlgorithm, value));
		var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

		TestConfig.Parse(config, projectAssembly);

		Assert.Equal(expected, projectAssembly.Configuration.ParallelAlgorithm);
	}

	public class Booleans
	{
		static readonly (string, Expression<Func<XunitProjectAssembly, bool?>>)[] booleanOptions =
		[
			(TestConfig.Keys.DiagnosticMessages, assembly => assembly.Configuration.DiagnosticMessages),
			(TestConfig.Keys.FailSkips, assembly => assembly.Configuration.FailSkips),
			(TestConfig.Keys.FailWarns, assembly => assembly.Configuration.FailTestsWithWarnings),
			(TestConfig.Keys.InternalDiagnosticMessages, assembly => assembly.Configuration.InternalDiagnosticMessages),
			(TestConfig.Keys.ParallelizeTestCollections, assembly => assembly.Configuration.ParallelizeTestCollections),
			(TestConfig.Keys.PreEnumerateTheories, assembly => assembly.Configuration.PreEnumerateTheories),
			(TestConfig.Keys.ShowLiveOutput, assembly => assembly.Configuration.ShowLiveOutput),
			(TestConfig.Keys.StopOnFail, assembly => assembly.Configuration.StopOnFail),
		];

		public static IEnumerable<TheoryDataRow<string, string, Expression<Func<XunitProjectAssembly, bool?>>, bool?>> ValidValues()
		{
			foreach (var @override in new[] { ("on", true), ("true", true), ("1", true), ("off", false), ("false", false), ("0", false) })
				foreach (var option in booleanOptions)
					yield return new(option.Item1, @override.Item1, option.Item2, @override.Item2);
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(ValidValues))]
		public void ValidValue(
			string key,
			string value,
			Expression<Func<XunitProjectAssembly, bool?>> accessor,
			bool? expected)
		{
			var config = new StubConfiguration((key, value));
			var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

			TestConfig.Parse(config, projectAssembly);

			var result = accessor.Compile().Invoke(projectAssembly);
			Assert.Equal(expected, result);
		}

		public static IEnumerable<TheoryDataRow<string, string, Expression<Func<XunitProjectAssembly, bool?>>>> InvalidValues =
			[.. booleanOptions.Select(x => (x.Item1, "foo", x.Item2))];

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(InvalidValues))]
		public void InvalidValue(
			string key,
			string value,
			Expression<Func<XunitProjectAssembly, bool?>> accessor)
		{
			var config = new StubConfiguration((key, value));
			var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

			TestConfig.Parse(config, projectAssembly);

			var result = accessor.Compile().Invoke(projectAssembly);
			Assert.Null(result);
		}
	}

	public class Integers
	{
		static readonly (string, Expression<Func<XunitProjectAssembly, int?>>, int)[] integerOptions =
		[
			(TestConfig.Keys.AssertEquivalentMaxDepth, assembly => assembly.Configuration.AssertEquivalentMaxDepth, 1),
			(TestConfig.Keys.LongRunningTestSeconds, assembly => assembly.Configuration.LongRunningTestSeconds, 1),
			(TestConfig.Keys.PrintMaxEnumerableLength, assembly => assembly.Configuration.PrintMaxEnumerableLength, 0),
			(TestConfig.Keys.PrintMaxObjectDepth, assembly => assembly.Configuration.PrintMaxObjectDepth, 0),
			(TestConfig.Keys.PrintMaxObjectMemberCount, assembly => assembly.Configuration.PrintMaxObjectMemberCount, 0),
			(TestConfig.Keys.PrintMaxStringLength, assembly => assembly.Configuration.PrintMaxStringLength, 0),
			(TestConfig.Keys.Seed, assembly => assembly.Configuration.Seed, 0),
		];

		public static IEnumerable<TheoryDataRow<string, string, Expression<Func<XunitProjectAssembly, int?>>, int>> ValidValues()
		{
			var maxValueString = int.MaxValue.ToString();

			foreach (var option in integerOptions)
			{
				yield return new(option.Item1, option.Item3.ToString(), option.Item2, option.Item3);
				yield return new(option.Item1, maxValueString, option.Item2, int.MaxValue);
			}
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(ValidValues))]
		public void ValidValue(
			string key,
			string value,
			Expression<Func<XunitProjectAssembly, int?>> accessor,
			int expected)
		{
			var config = new StubConfiguration((key, value));
			var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

			TestConfig.Parse(config, projectAssembly);

			var result = accessor.Compile().Invoke(projectAssembly);
			Assert.Equal(expected, result);
		}

		public static IEnumerable<TheoryDataRow<string, string, Expression<Func<XunitProjectAssembly, int?>>>> InvalidValues()
		{
			foreach (var option in integerOptions)
			{
				yield return new(option.Item1, (option.Item3 - 1).ToString(), option.Item2);
				yield return new(option.Item1, "foo", option.Item2);
			}
		}

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(InvalidValues))]
		public void InvalidValue(
			string key,
			string value,
			Expression<Func<XunitProjectAssembly, int?>> accessor)
		{
			var config = new StubConfiguration((key, value));
			var projectAssembly = TestData.XunitProjectAssembly<TestConfigTests>();

			TestConfig.Parse(config, projectAssembly);

			var result = accessor.Compile().Invoke(projectAssembly);
			Assert.Null(result);
		}
	}

	class StubConfiguration : IConfiguration
	{
		readonly Dictionary<string, string> values = new(StringComparer.OrdinalIgnoreCase);

		public StubConfiguration(params (string key, string? value)[] values)
		{
			foreach (var kvp in values)
				if (kvp.value is not null)
					this.values[kvp.key] = kvp.value;
		}

		public string? this[string key]
		{
			get
			{
				if (values.TryGetValue(key, out var value))
					return value;

				return null;
			}
		}
	}
}
