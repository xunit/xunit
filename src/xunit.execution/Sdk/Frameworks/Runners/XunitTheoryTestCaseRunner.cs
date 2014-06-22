﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The test case runner for xUnit.net v2 theories (which could not be pre-enumerated;
    /// pre-enumerated test cases use <see cref="XunitTestCaseRunner"/>).
    /// </summary>
    public class XunitTheoryTestCaseRunner : XunitTestCaseRunner
    {
        static readonly object[] NoArguments = new object[0];

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTheoryTestCaseRunner"/> class.
        /// </summary>
        /// <param name="testCase">The test case to be run.</param>
        /// <param name="displayName">The display name of the test case.</param>
        /// <param name="skipReason">The skip reason, if the test is to be skipped.</param>
        /// <param name="constructorArguments">The arguments to be passed to the test class constructor.</param>
        /// <param name="messageBus">The message bus to report run status to.</param>
        /// <param name="aggregator">The exception aggregator used to run code and collect exceptions.</param>
        /// <param name="cancellationTokenSource">The task cancellation token source, used to cancel the test run.</param>
        public XunitTheoryTestCaseRunner(IXunitTestCase testCase,
                                         string displayName,
                                         string skipReason,
                                         object[] constructorArguments,
                                         IMessageBus messageBus,
                                         ExceptionAggregator aggregator,
                                         CancellationTokenSource cancellationTokenSource)
            : base(testCase, displayName, skipReason, constructorArguments, NoArguments, messageBus, aggregator, cancellationTokenSource) { }

        /// <inheritdoc/>
        protected override async Task<RunSummary> RunTestAsync()
        {
            var testRunners = new List<XunitTestRunner>();
            var toDispose = new List<IDisposable>();

            try
            {
                var dataAttributes = TestCase.TestMethod.Method.GetCustomAttributes(typeof(DataAttribute));

                foreach (var dataAttribute in dataAttributes)
                {
                    var discovererAttribute = dataAttribute.GetCustomAttributes(typeof(DataDiscovererAttribute)).First();
                    var args = discovererAttribute.GetConstructorArguments().Cast<string>().ToList();
                    var discovererType = Reflector.GetType(args[1], args[0]);
                    var discoverer = ExtensibilityPointFactory.GetDataDiscoverer(discovererType);

                    foreach (var dataRow in discoverer.GetData(dataAttribute, TestCase.TestMethod.Method))
                    {
                        toDispose.AddRange(dataRow.OfType<IDisposable>());

                        ITypeInfo[] resolvedTypes = null;
                        var methodToRun = TestMethod;

                        if (methodToRun.IsGenericMethodDefinition)
                        {
                            resolvedTypes = TypeUtility.ResolveGenericTypes(TestCase.TestMethod.Method, dataRow);
                            methodToRun = methodToRun.MakeGenericMethod(resolvedTypes.Select(t => ((IReflectionTypeInfo)t).Type).ToArray());
                        }

                        var parameterTypes = methodToRun.GetParameters().Select(p => p.ParameterType).ToArray();
                        var convertedDataRow = Reflector.ConvertArguments(dataRow, parameterTypes);
                        var theoryDisplayName = TypeUtility.GetDisplayNameWithArguments(TestCase.TestMethod.Method, DisplayName, convertedDataRow, resolvedTypes);

                        testRunners.Add(new XunitTestRunner(TestCase, MessageBus, TestClass, ConstructorArguments, methodToRun, convertedDataRow, theoryDisplayName, SkipReason, BeforeAfterAttributes, Aggregator, CancellationTokenSource));
                    }
                }
            }
            catch (Exception ex)
            {
                if (!MessageBus.QueueMessage(new TestStarting(TestCase, DisplayName)))
                    CancellationTokenSource.Cancel();
                else
                {
                    if (!MessageBus.QueueMessage(new TestFailed(TestCase, DisplayName, 0, null, ex.Unwrap())))
                        CancellationTokenSource.Cancel();
                }

                if (!MessageBus.QueueMessage(new TestFinished(TestCase, DisplayName, 0, null)))
                    CancellationTokenSource.Cancel();

                return new RunSummary { Total = 1, Failed = 1 };
            }

            var runSummary = new RunSummary();

            foreach (var testRunner in testRunners)
                runSummary.Aggregate(await testRunner.RunAsync());

            var timer = new ExecutionTimer();
            var aggregator = new ExceptionAggregator();
            // REVIEW: What should be done with these leftover errors?

            foreach (var disposable in toDispose)
                timer.Aggregate(() => aggregator.Run(() => disposable.Dispose()));

            runSummary.Time += timer.Total;

            return runSummary;
        }
    }
}
