using Xunit.v3;

#if !XUNIT_AOT
using System.Reflection;
#endif

public class SpyBeforeAfterTest : BeforeAfterTestAttribute
{
	public bool ThrowInBefore { get; set; }

	public bool ThrowInAfter { get; set; }

#if XUNIT_AOT
	public override void Before(ICodeGenTest test)
#else
	public override void Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
#endif
	{
		if (ThrowInBefore)
			throw new BeforeException();
	}

#if XUNIT_AOT
	public override void After(ICodeGenTest test)
#else
	public override void After(
		MethodInfo methodUnderTest,
		IXunitTest test)
#endif
	{
		if (ThrowInAfter)
			throw new AfterException();
	}

	public class BeforeException : Exception
	{ }

	public class AfterException : Exception
	{ }
}
