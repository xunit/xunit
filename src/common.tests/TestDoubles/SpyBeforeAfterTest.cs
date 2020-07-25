using System;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

public class SpyBeforeAfterTest : BeforeAfterTestAttribute
{
	public bool ThrowInBefore { get; set; }
	public bool ThrowInAfter { get; set; }

	public override void Before(MethodInfo methodUnderTest, ITest test)
	{
		if (ThrowInBefore)
			throw new BeforeException();
	}

	public override void After(MethodInfo methodUnderTest, ITest test)
	{
		if (ThrowInAfter)
			throw new AfterException();
	}

	public class BeforeException : Exception
	{ }

	public class AfterException : Exception
	{ }
}
