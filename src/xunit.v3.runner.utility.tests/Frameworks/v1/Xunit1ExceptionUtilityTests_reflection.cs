#if NETFRAMEWORK

using System;
using Xunit;
using Xunit.Runner.v1;

public class Xunit1ExceptionUtilityTests
{
	[Fact]
	public static void CanParseEmbeddedExceptions()
	{
		try
		{
			try
			{
				throw new DivideByZeroException();
			}
			catch (Exception ex)
			{
				throw new Exception("failure", ex);
			}
		}
		catch (Exception ex)
		{
			var (exceptionTypes, messages, stackTraces, exceptionParentIndices) = Xunit1ExceptionUtility.ConvertToErrorMetadata(ex);

			Assert.Collection(
				exceptionTypes,
				type => Assert.Equal("System.Exception", type),
				type => Assert.Equal("System.DivideByZeroException", type)
			);
			Assert.Collection(
				messages,
				msg => Assert.Equal("failure", msg),
				msg => Assert.Equal("Attempted to divide by zero.", msg)
			);
			Assert.Collection(
				stackTraces,
				stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions", stack),
				stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions", stack)
			);
			Assert.Collection(
				exceptionParentIndices,
				index => Assert.Equal(-1, index),
				index => Assert.Equal(0, index)
			);
		}
	}
}

#endif
