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
        /// <inheritdoc/>
        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var methodDisplay = discoveryOptions.MethodDisplayOrDefault();

            IXunitTestCase testCase;
            if (testMethod.Method.GetParameters().Any())
                testCase = new LambdaTestCase(methodDisplay, testMethod, () => { throw new InvalidOperationException("[Fact] methods are not allowed to have parameters. Did you mean to use [Theory]?"); });
            else
                testCase = new XunitTestCase(methodDisplay, testMethod);

            return new[] { testCase };
        }
    }
}