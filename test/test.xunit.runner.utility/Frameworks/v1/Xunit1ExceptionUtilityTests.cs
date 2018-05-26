#if NETFRAMEWORK

using System;
using System.Globalization;
using System.Threading;
using Xunit;

public class Xunit1ExceptionUtilityTests
{
    [Fact]
    public static void CanParseEmbeddedExceptions()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        try
        {
            try { throw new DivideByZeroException(); }
            catch (Exception ex) { throw new Exception("failure", ex); }
        }
        catch (Exception ex)
        {
            var failureInfo = Xunit1ExceptionUtility.ConvertToFailureInformation(ex);

            Assert.Collection(failureInfo.ExceptionTypes,
                type => Assert.Equal("System.Exception", type),
                type => Assert.Equal("System.DivideByZeroException", type)
            );
            Assert.Collection(failureInfo.Messages,
                msg => Assert.Equal("failure", msg),
                msg => Assert.Equal("Attempted to divide by zero.", msg)
            );
            Assert.Collection(failureInfo.StackTraces,
                stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions()", stack),
                stack => Assert.Contains("Xunit1ExceptionUtilityTests.CanParseEmbeddedExceptions()", stack)
            );
            Assert.Collection(failureInfo.ExceptionParentIndices,
                index => Assert.Equal(-1, index),
                index => Assert.Equal(0, index)
            );
        }
    }
}

#endif
