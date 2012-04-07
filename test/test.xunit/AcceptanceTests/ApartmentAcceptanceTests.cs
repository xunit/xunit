using System.Threading;
using Xunit;

public class ApartmentAcceptanceTests
{
    [Fact]
    public void TestsRunsInTheSingleThreadedApartment()
    {
        Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
    }
}