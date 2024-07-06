using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit.v3;

public class SpyBeforeAfterTest : BeforeAfterTestAttribute
{
	public bool ThrowInBefore { get; set; }
	public bool ThrowInAfter { get; set; }

	public override ValueTask Before(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		if (ThrowInBefore)
			throw new BeforeException();

		return default;
	}

	public override ValueTask After(
		MethodInfo methodUnderTest,
		IXunitTest test)
	{
		if (ThrowInAfter)
			throw new AfterException();

		return default;
	}

	public class BeforeException : Exception
	{ }

	public class AfterException : Exception
	{ }
}
