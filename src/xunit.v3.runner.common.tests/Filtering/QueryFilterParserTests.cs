using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Sdk;

public class QueryFilterParserTests
{
	public class Parse
	{
		[Theory]
		[InlineData("/")]
		[InlineData("/*")]
		[InlineData("/*/")]
		[InlineData("/*/*")]
		[InlineData("/*/*/")]
		[InlineData("/*/*/*")]
		[InlineData("/*/*/*/")]
		[InlineData("/*/*/*/*")]
		[InlineData("/*/*/*/*/")]
		public void EmptyFilter(string query)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.IsType<FilterPass>(filter);
		}

		[Theory]
		// Query parsing (segment issues)
		[InlineData("", "Query filter '' is not valid: query must begin with '/'")]
		[InlineData("foo", "Query filter 'foo' is not valid: query must begin with '/'")]
		[InlineData("/1/2/3/4/5", "Query filter '/1/2/3/4/5' is not valid: too many segments (max 4)")]
		[InlineData("/1/2/3/4/*", "Query filter '/1/2/3/4/*' is not valid: too many segments (max 4)")]
		[InlineData("/(", "Query filter '/(' is not valid: open '(' does not have matching closing ')'")]
		[InlineData("/(abc", "Query filter '/(abc' is not valid: open '(' does not have matching closing ')'")]
		[InlineData("/!(foo)/bar", "Query filter '/!(foo)/bar' is not valid: negating operator '!' must be inside parenthesis, not outside")]
		// Query parsing (trait issues)
		[InlineData("/[foo=bar", "Query filter '/[foo=bar' is not valid: saw opening '[' without ending ']'")]
		[InlineData("/![foo=bar]", "Query filter '/![foo=bar]' is not valid: negate a trait expression with 'name!=value'")]
		[InlineData("/[!(foo=bar)|(baz=biff)]", "Query filter '/[!(foo=bar)|(baz=biff)]' is not valid: negate a trait expression with 'name!=value'")]
		// Query parsing (composite logical expression issues)
		[InlineData("/(foo)|(bar)&(baz)", "Query filter '/(foo)|(bar)&(baz)' is not valid: logical expressions cannot mix '|' and '&' without grouping parentheses")]
		[InlineData("/(foo)*", "Query filter '/(foo)*' is not valid: unexpected character '*' after closing parenthesis")]
		[InlineData("/(foo)|", "Query filter '/(foo)|' is not valid: logical operator '|' cannot end a query")]
		[InlineData("/(foo)&bar", "Query filter '/(foo)&bar' is not valid: logical operator '&' must be followed by an open parenthesis")]
		[InlineData("/[(foo=bar)&(foo=baz)|(foo=biff)]", "Query filter '/[(foo=bar)&(foo=baz)|(foo=biff)]' is not valid: logical expressions cannot mix '|' and '&' without grouping parentheses")]
		[InlineData("/[(foo=bar)*]", "Query filter '/[(foo=bar)*]' is not valid: unexpected character '*' after closing parenthesis")]
		[InlineData("/[(foo=bar)|]", "Query filter '/[(foo=bar)|]' is not valid: logical operator '|' cannot end a query")]
		[InlineData("/[(foo=bar)&baz=biff]", "Query filter '/[(foo=bar)&baz=biff]' is not valid: logical operator '&' must be followed by an open parenthesis")]
		// Segment parsing
		[InlineData("/1/**", "Filter expression '**' is not valid: wildcards must include text in the middle")]
		[InlineData("/1/a*b", "Filter expression 'a*b' is not valid: wildcards may only be at the beginning and/or end of a filter expression")]
		[InlineData("/1/!/3", "Filter expression '!' is not valid: missing the segment query to negate")]
		[InlineData("/1/!*/3", "Filter expression '!*' is not valid: this would exclude all tests")]
		// Trait parsing
		[InlineData("/[!]", "Trait expression '!' is not valid: negate a trait expression with 'name!=value'")]
		[InlineData("/[*]", "Trait expression '*' is not valid: trait queries must be 'name=value' or 'name!=value'")]
		[InlineData("/[name]", "Trait expression 'name' is not valid: trait queries must be 'name=value' or 'name!=value'")]
		[InlineData("/[name=]", "Trait expression 'name=' is not valid: trait queries must be 'name=value' or 'name!=value'")]
		[InlineData("/[name!=]", "Trait expression 'name!=' is not valid: trait queries must be 'name=value' or 'name!=value'")]
		[InlineData("/[=value]", "Trait expression '=value' is not valid: trait queries must be 'name=value' or 'name!=value'")]
		[InlineData("/[!=value]", "Trait expression '!=value' is not valid: negate a trait expression with 'name!=value'")]
		[InlineData("/[!foo=bar]", "Trait expression '!foo=bar' is not valid: negate a trait expression with 'name!=value'")]
		[InlineData("/[(!foo=bar)]", "Trait expression '!foo=bar' is not valid: negate a trait expression with 'name!=value'")]
		public void MalformedQueries(
			string query,
			string expectedMessage)
		{
			var ex = Record.Exception(() => QueryFilterParser.Parse(query));

			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("query", argEx.ParamName);
			Assert.StartsWith(expectedMessage, argEx.Message);
		}

		public static IEnumerable<TheoryDataRow<string, bool>> AssemblyDataSource()
		{
			foreach (var name in MakeWildcards("asm1"))
				foreach (var suffix in new[] { "", "/*" })
				{
					yield return new($"/{name}{suffix}", false);
					yield return new($"/!{name}{suffix}", true);
				}
		}

		[Theory]
		[MemberData(nameof(AssemblyDataSource))]
		public void AssemblyFilter(
			string query,
			bool negated)
		{
			var testCase = GetTestCase();

			var filter = QueryFilterParser.Parse(query);

			Assert.IsType(negated ? typeof(FilterLogicalNot) : typeof(FilterAssembly), filter);
			Assert.Equal(!negated, filter.Filter("asm1", testCase));
			Assert.Equal(negated, filter.Filter("as1m", testCase));
		}

		public static IEnumerable<TheoryDataRow<string, bool>> NamespaceDataSource()
		{
			foreach (var name in MakeWildcards("name1"))
				foreach (var suffix in new[] { "", "/*" })
				{
					yield return new($"/*/{name}{suffix}", false);
					yield return new($"/*/!{name}{suffix}", true);
				}
		}

		[Theory]
		[MemberData(nameof(NamespaceDataSource))]
		public void NamespaceFilter(
			string query,
			bool negated)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.IsType(negated ? typeof(FilterLogicalNot) : typeof(FilterNamespace), filter);
			Assert.Equal(!negated, filter.Filter("asm1", GetTestCase(@namespace: "name1")));
			Assert.Equal(negated, filter.Filter("asm1", GetTestCase(@namespace: "nam1e")));
		}

		public static IEnumerable<TheoryDataRow<string, bool>> ClassDataSource()
		{
			foreach (var name in MakeWildcards("class1"))
				foreach (var suffix in new[] { "", "/*" })
				{
					yield return new($"/*/*/{name}{suffix}", false);
					yield return new($"/*/*/!{name}{suffix}", true);
				}
		}

		[Theory]
		[MemberData(nameof(ClassDataSource))]
		public void ClassFilter(
			string query,
			bool negated)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.IsType(negated ? typeof(FilterLogicalNot) : typeof(FilterClassSimpleName), filter);
			Assert.Equal(!negated, filter.Filter("asm1", GetTestCase(@class: "class1")));
			Assert.Equal(negated, filter.Filter("asm1", GetTestCase(@class: "clas1s")));
		}

		public static IEnumerable<TheoryDataRow<string, bool>> MethodDataSource()
		{
			foreach (var name in MakeWildcards("method1"))
			{
				yield return new($"/*/*/*/{name}", false);
				yield return new($"/*/*/*/!{name}", true);
			}
		}

		[Theory]
		[MemberData(nameof(MethodDataSource))]
		public void MethodFilter(
			string query,
			bool negated)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.IsType(negated ? typeof(FilterLogicalNot) : typeof(FilterMethodSimpleName), filter);
			Assert.Equal(!negated, filter.Filter("asm1", GetTestCase(method: "method1")));
			Assert.Equal(negated, filter.Filter("asm1", GetTestCase(method: "metho1d")));
		}

		public static IEnumerable<TheoryDataRow<string, bool>> TraitDataSource()
		{
			foreach (var key in MakeWildcards("keyOne"))
				foreach (var value in MakeWildcards("valueOne"))
				{
					yield return new($"/[{key}={value}]", false);
					yield return new($"/[{key}!={value}]", true);
				}
		}

		[Theory]
		[MemberData(nameof(TraitDataSource))]
		public void TraitFilter(
			string query,
			bool negated)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.IsType(negated ? typeof(FilterLogicalNot) : typeof(FilterTrait), filter);
			Assert.Equal(!negated, filter.Filter("asm1", GetTestCase(traitKey: "keyOne", traitValue: "valueOne")));
			Assert.Equal(negated, filter.Filter("asm1", GetTestCase(traitKey: "keyOne", traitValue: "valueTwo")));
			Assert.Equal(negated, filter.Filter("asm1", GetTestCase(traitKey: "keyTwo", traitValue: "valueOne")));
		}

		[Theory]
		[InlineData("/(asm1)|(asm2)", false)]
		[InlineData("/((asm1)|(asm2))&(!asm5)", false)]
		[InlineData("/(!asm1)&(!asm2)", true)]
		[InlineData("/((!asm1)&(!asm2))|(asm5)", true)]
		public void CompositeSegmentFilter(
			string query,
			bool negated)
		{
			var testCase = GetTestCase();

			var filter = QueryFilterParser.Parse(query);

			Assert.Equal(!negated, filter.Filter("asm1", testCase));
			Assert.Equal(!negated, filter.Filter("asm2", testCase));
			Assert.Equal(negated, filter.Filter("asm3", testCase));
		}

		[Theory]
		[InlineData("/[(foo=bar)|(foo=baz)]", false)]
		[InlineData("/[((foo=bar)|(foo=baz))&(yes!=no)]", false)]
		[InlineData("/[(foo!=bar)&(foo!=baz)]", true)]
		[InlineData("/[((foo!=bar)&(foo!=baz))|(yes==no)]", true)]
		public void CompositTraitFilter(
			string query,
			bool negated)
		{
			var filter = QueryFilterParser.Parse(query);

			Assert.Equal(!negated, filter.Filter("asm1", GetTestCase(traitKey: "foo", traitValue: "bar")));
			Assert.Equal(!negated, filter.Filter("asm2", GetTestCase(traitKey: "foo", traitValue: "baz")));
			Assert.Equal(negated, filter.Filter("asm3", GetTestCase(traitKey: "foo", traitValue: "biff")));
		}

		[Fact]
		public void EscapedSegmentCharacters()
		{
			var filter = QueryFilterParser.Parse("/a&#x28;b&#x29;c");

			Assert.True(filter.Filter("a(b)c", GetTestCase()));
		}

		[Fact]
		public void EscapedTraitCharacters()
		{
			var filter = QueryFilterParser.Parse("/[&#x5b;&#x3d;=&#x3d;&#x5d;]");

			Assert.True(filter.Filter("asm1", GetTestCase(traitKey: "[=", traitValue: "=]")));
		}
	}

	static ITestCaseMetadata GetTestCase(
		string @namespace = "name1",
		string @class = "class1",
		string method = "method1",
		string? traitKey = null,
		string? traitValue = null)
	{
		var testClass = Mocks.TestClass(testClassSimpleName: @class, testClassNamespace: @namespace);
		var testMethod = Mocks.TestMethod(methodName: method, testClass: testClass);
		var traits = new Dictionary<string, IReadOnlyCollection<string>>();
		if (traitKey is not null && traitValue is not null)
			traits[traitKey] = [traitValue];

		return Mocks.TestCase(testMethod: testMethod, traits: traits);
	}

	static IEnumerable<string> MakeWildcards(string value)
	{
		yield return value;
		yield return $"*{value.Substring(1)}";
		yield return $"{value.Substring(0, value.Length - 1)}*";
		yield return $"*{value.Substring(1, value.Length - 2)}*";
	}
}
