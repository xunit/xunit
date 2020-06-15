using System;
using System.Collections.Generic;
using System.Threading;

namespace Xunit.Runner.Common
{
    /// <summary>
    /// A test message sink which supports mapping test collection names to flow IDs.
    /// </summary>
    public class FlowMappedTestMessageSink : TestMessageSink
    {
        readonly Func<string, string> flowIdMapper;
        readonly Dictionary<string, string> flowMappings = new Dictionary<string, string>();
        readonly ReaderWriterLockSlim flowMappingsLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Initializes a new instance of the <see cref="FlowMappedTestMessageSink" /> class.
        /// </summary>
        /// <param name="flowIdMapper">Optional code which maps a test collection name to a flow ID
        /// (the default behavior generates a new GUID for each test collection)</param>
        public FlowMappedTestMessageSink(Func<string, string> flowIdMapper = null)
        {
            this.flowIdMapper = flowIdMapper ?? (_ => Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Gets the flow ID for a given test collection name.
        /// </summary>
        /// <param name="testCollectionName">The test collection name</param>
        /// <returns>The flow ID for the given test collection name</returns>
        protected string ToFlowId(string testCollectionName)
        {
            string result;

            flowMappingsLock.EnterReadLock();

            try
            {
                if (flowMappings.TryGetValue(testCollectionName, out result))
                    return result;
            }
            finally
            {
                flowMappingsLock.ExitReadLock();
            }

            flowMappingsLock.EnterWriteLock();

            try
            {
                if (!flowMappings.TryGetValue(testCollectionName, out result))
                {
                    result = flowIdMapper(testCollectionName);
                    flowMappings[testCollectionName] = result;
                }

                return result;
            }
            finally
            {
                flowMappingsLock.ExitWriteLock();
            }
        }
    }
}
