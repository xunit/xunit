using System;
using Xunit;

public static class ExceptionAssert
{
    public static ArgumentException ThrowsArgument(Action action, string paramName)
    {
        var ex = Assert.Throws<ArgumentException>(action);
        Assert.Equal(paramName, ex.ParamName);
        return ex;
    }

    public static ArgumentNullException ThrowsArgumentNull(Action action, string paramName)
    {
        var ex = Assert.Throws<ArgumentNullException>(action);
        Assert.Equal(paramName, ex.ParamName);
        return ex;
    }
}
