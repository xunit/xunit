namespace Xunit.Abstractions
{
    /// <summary>
    /// The IAfterTestFinished message is sent during execution to indicate the 
    /// the After of a Before/After attribute has completed executing. 
    /// </summary>
    public interface IAfterTestFinished : ITestMessage
    {
        /// <summary>
        /// AttributeName is the name of the Before/After Attribute
        /// </summary>
        string AttributeName { get; }

        /// <summary>
        /// The TestCase that this message applies too. 
        /// </summary>
        ITestCase TestCase { get; }
       
        /// <summary>
        /// The display name to be used for this test. 
        /// </summary>
        string TestDisplayName { get; }
    }
}
