using System.Threading;
using Xunit;

namespace Xunit1
{
    public class ApartmentAcceptanceTests
    {
        [Fact]
        public void TestsRunsInTheSingleThreadedApartment()
        {
            Assert.Equal(ApartmentState.STA, Thread.CurrentThread.GetApartmentState());
        }
    }
}
