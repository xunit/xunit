using System.Threading;
using Xunit;

public class Example
{
//    [Fact(Skip="Trait Extensibility is not working in 1654"), Category("Slow Test")]
//    public void LongTest()
//    {
//        Thread.Sleep(500);
//    }

    [Fact, Trait("Category", "Slow Test")]
    public void LongTest2()
    {
        Assert.True(true); 
    }
}