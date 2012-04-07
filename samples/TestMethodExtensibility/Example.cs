using System.Threading;
using Xunit;

public class Example
{
    static int val;

    [RepeatTest(5, Timeout = 250)]
    public void RepeatingTestMethod()
    {
        Thread.Sleep(100);
        Assert.Equal(2, 2);

        if (val == 0)
        {
            val++;
            Thread.Sleep(1000);
        }
    }
}