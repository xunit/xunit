using Xunit.v3;

#if !XUNIT_AOT
using System.Reflection;
#endif

public class CleanEnvironmentAttribute(params string[] variables) :
	BeforeAfterTestAttribute
{
	readonly Dictionary<string, string?> preservation = [];

#if XUNIT_AOT
	public override void After(ICodeGenTest test)
#else
	public override void After(
		MethodInfo methodUnderTest,
		IXunitTest test)
#endif
	{
		foreach (var kvp in preservation)
			Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
	}

#if XUNIT_AOT
	public override void Before(ICodeGenTest test)
#else
	public override void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
#endif
	{
		foreach (var variable in variables)
		{
			preservation[variable] = Environment.GetEnvironmentVariable(variable);
			Environment.SetEnvironmentVariable(variable, null);
		}
	}
}
