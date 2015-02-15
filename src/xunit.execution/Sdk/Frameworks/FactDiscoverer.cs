using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IXunitTestCaseDiscoverer"/> that supports finding test cases
    /// on methods decorated with <see cref="FactAttribute"/>.
    /// </summary>
    public class FactDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly IMessageSink diagnosticMessageSink;

        /// <summary>
        /// Initializes a new instance of the <see cref="FactDiscoverer"/> class.
        /// </summary>
        /// <param name="diagnosticMessageSink">The message sink used to send diagnostic messages</param>
        public FactDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this.diagnosticMessageSink = diagnosticMessageSink;
        }

        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();

            IXunitTestCase testCase;
            if (testMethod.Method.GetParameters().Any())
                testCase = new ExecutionErrorTestCase(diagnosticMessageSink, methodDisplay, testMethod, "[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?");
            else
                testCase = new XunitTestCase(diagnosticMessageSink, methodDisplay, testMethod);

            return new[] { testCase };
        }
    }
}