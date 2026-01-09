using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.v3;

public class CleanEnvironmentAttribute(params string[] variables) :
	BeforeAfterTestAttribute, ICollectionAttribute
{
	readonly Dictionary<string, string?> preservation = [];

	public string Name => "Clean Environment";

	public Type? Type => typeof(CleanEnvironmentAttribute);

	public override void After(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		foreach (var kvp in preservation)
			Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
	}

	public override void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		foreach (var variable in variables)
		{
			preservation[variable] = Environment.GetEnvironmentVariable(variable);
			Environment.SetEnvironmentVariable(variable, null);
		}
	}
}
