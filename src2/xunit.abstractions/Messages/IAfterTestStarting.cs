namespace Xunit.Abstractions
{
    /// <summary>
    /// The IAfterTestStarting message is sent during execution of a test 
    /// to indicate the After of a Before/After attribute has started executing. 
    /// </summary>
    public interface IAfterTestStarting : ITestMessage
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
