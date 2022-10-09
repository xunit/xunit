using System;
using System.Reflection;
using Xunit.Sdk;
using Xunit.v3;

public class SpyBeforeAfterTest : BeforeAfterTestAttribute
{
	public bool ThrowInBefore { get; set; }
	public bool ThrowInAfter { get; set; }

	public override void Before(
		MethodInfo methodUnderTest,
		_ITest test)
	{
		if (ThrowInBefore)
			throw new BeforeException();
	}

	public override void After(
		MethodInfo methodUnderTest,
		_ITest test)
	{
		if (ThrowInAfter)
			throw new AfterException();
	}

	public class BeforeException : Exception
	{ }

	public class AfterException : Exception
	{ }
}
