using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Default implementation of <see cref="ITestClassStarting"/>.
    /// </summary>
    public class TestClassStarting : LongLivedMarshalByRefObject, ITestClassStarting
    {
        /// <inheritdoc/>
        public string ClassName { get; set; }
    }
}