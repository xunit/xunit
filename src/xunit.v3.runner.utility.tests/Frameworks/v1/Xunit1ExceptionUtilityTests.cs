#if NETFRAMEWORK

using System;
using Xunit;
using Xunit.Runner.v1;
using Xunit.v3;

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
			var errorMetadata = Xunit1ExceptionUtility.ConvertToErrorMetadata(ex);

			Assert.Collection(
				errorMetadata.ExceptionTypes,
				type => Assert.Equal("System.Exception", type),
				type => Assert.Equal("System.DivideByZeroException", type)
			);
			Assert.Collection(
				errorMetadata.Messages,
				msg => Assert.Equal("failure", msg),
				msg => Assert.Equal("Attempted to divide by zero.", msg)
			);
			Assert.Collection(
				errorMetadata.StackTraces,
				stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions", stack),
				stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions", stack)
			);
			Assert.Collection(
				errorMetadata.ExceptionParentIndices,
				index => Assert.Equal(-1, index),
				index => Assert.Equal(0, index)
			);
		}
	}
}

#endif
