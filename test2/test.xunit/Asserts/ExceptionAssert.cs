using System;
using Xunit;

public static class ExceptionAssert
{
    public static ArgumentNullException ThrowsArgumentNull(Assert.ThrowsDelegate action, string paramName)
    {
        var ex = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(paramName, ex.ParamName);
        return ex;
    }
}
