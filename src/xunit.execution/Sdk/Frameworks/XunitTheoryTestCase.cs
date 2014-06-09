using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a test case which runs multiple tests for theory data, either because the
    /// data was not enumerable or because the data was not serializable.
    /// </summary>
    [Serializable]
    public class XunitTheoryTestCase : XunitTestCase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTheoryTestCase"/> class.
        /// </summary>
        /// <param name="testCollection">The test collection this theory belongs to.</param>
        /// <param name="assembly">The test assembly.</param>
        /// <param name="type">The type under test.</param>
        /// <param name="method">The method under test.</param>
        /// <param name="theoryAttribute">The theory attribute.</param>
        public XunitTheoryTestCase(ITestCollection testCollection, IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, IAttributeInfo theoryAttribute)
            : base(testCollection, assembly, type, method, theoryAttribute) { }

        /// <inheritdoc />
        protected XunitTheoryTestCase(SerializationInfo info, StreamingContext context) : base(info, context) { }

        /// <inheritdoc />
        public override Task<RunSummary> RunAsync(IMessageBus messageBus, object[] constructorArguments, ExceptionAggregator aggregator, CancellationTokenSource cancellationTokenSource)
        {
            return new XunitTheoryTestCaseRunner(this, DisplayName, SkipReason, constructorArguments, messageBus, aggregator, cancellationTokenSource).RunAsync();
        }
    }
}