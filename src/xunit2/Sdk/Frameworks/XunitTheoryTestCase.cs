using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
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
        protected override void RunTestsOnMethod(IMessageSink messageSink,
                                                 Type classUnderTest,
                                                 object[] constructorArguments,
                                                 MethodInfo methodUnderTest,
                                                 List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                 ExceptionAggregator aggregator,
                                                 CancellationTokenSource cancellationTokenSource,
                                                 ref decimal executionTime)
        {
            try
            {
                var testMethod = Reflector.Wrap(methodUnderTest);

                var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    var discovererType = Reflector.GetType(args[1], args[0]);
                    IDataDiscoverer discoverer = (IDataDiscoverer)Activator.CreateInstance(discovererType);

                    foreach (object[] dataRow in discoverer.GetData(dataAttribute, testMethod))
                    {
                        RunTestWithArguments(messageSink, classUnderTest, constructorArguments, methodUnderTest, dataRow, GetDisplayNameWithArguments(DisplayName, dataRow), beforeAfterAttributes, aggregator, cancellationTokenSource, ref executionTime);
                        if (cancellationTokenSource.IsCancellationRequested)
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!OnMessage(messageSink, new TestStarting(this, DisplayName)))
                    cancellationTokenSource.Cancel();
                else
                {
                    if (!OnMessage(messageSink, new TestFailed(this, DisplayName, executionTime, null, ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }

                if (!OnMessage(messageSink, new TestFinished(this, DisplayName, executionTime, null)))
                    cancellationTokenSource.Cancel();
            }
        }
    }
}