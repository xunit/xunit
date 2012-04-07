using System.Threading;
using Xunit;

public class Example
{
    [Fact, Category("Slow Test")]
    public void LongTest()
    {
        Thread.Sleep(500);
    }

    // these two tests have the same traits

    [Fact, Trait("Category", "Slow Test")]
    public void LongTest2()
    {
        Thread.Sleep(500);
    }
}