using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        protected override async Task RunTestsOnMethodAsync(IMessageBus messageBus,
                                                            Type classUnderTest,
                                                            object[] constructorArguments,
                                                            MethodInfo methodUnderTest,
                                                            List<BeforeAfterTestAttribute> beforeAfterAttributes,
                                                            ExceptionAggregator aggregator,
                                                            CancellationTokenSource cancellationTokenSource)
        {
            var executionTime = 0M;

            try
            {
                var testMethod = Reflector.Wrap(methodUnderTest);

                var dataAttributes = testMethod.GetCustomAttributes(typeof(DataAttribute));
                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    var discovererType = Reflector.GetType(args[1], args[0]);
                    var discoverer = (IDataDiscoverer)Activator.CreateInstance(discovererType);

                    foreach (object[] dataRow in discoverer.GetData(dataAttribute, testMethod))
                    {
                        var methodToRun = methodUnderTest;
                        ITypeInfo[] resolvedTypes = null;

                        if (methodToRun.IsGenericMethodDefinition)
                        {
                            resolvedTypes = ResolveGenericTypes(testMethod, dataRow);
                            methodToRun = methodToRun.MakeGenericMethod(resolvedTypes.Select(t => ((IReflectionTypeInfo)t).Type).ToArray());
                        }

                        executionTime +=
                            await RunTestWithArgumentsAsync(messageBus,
                                                            classUnderTest,
                                                            constructorArguments,
                                                            methodToRun,
                                                            dataRow,
                                                            GetDisplayNameWithArguments(DisplayName, dataRow, resolvedTypes),
                                                            beforeAfterAttributes,
                                                            aggregator,
                                                            cancellationTokenSource);

                        if (cancellationTokenSource.IsCancellationRequested)
                            return;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!messageBus.QueueMessage(new TestStarting(this, DisplayName)))
                    cancellationTokenSource.Cancel();
                else
                {
                    if (!messageBus.QueueMessage(new TestFailed(this, DisplayName, executionTime, null, ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }

                if (!messageBus.QueueMessage(new TestFinished(this, DisplayName, executionTime, null)))
                    cancellationTokenSource.Cancel();
            }
        }
    }
}