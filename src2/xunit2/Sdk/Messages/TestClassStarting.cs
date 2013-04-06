using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestClassStarting : LongLivedMarshalByRefObject, ITestClassStarting
    {
        public string ClassName { get; set; }
    }
}
