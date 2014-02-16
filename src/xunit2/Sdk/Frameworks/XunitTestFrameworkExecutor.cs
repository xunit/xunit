using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// The implementation of <see cref="ITestFrameworkExecutor"/> that supports execution
    /// of unit tests linked against xunit2.dll.
    /// </summary>
    public class XunitTestFrameworkExecutor : LongLivedMarshalByRefObject, ITestFrameworkExecutor
    {
        readonly string assemblyFileName;
        readonly IAssemblyInfo assemblyInfo;
        readonly bool disableParallelization;
        readonly string displayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
        /// </summary>
        /// <param name="assemblyFileName">Path of the test assembly.</param>
        public XunitTestFrameworkExecutor(string assemblyFileName)
        {
            this.assemblyFileName = assemblyFileName;

            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            assemblyInfo = Reflector.Wrap(assembly);

            var collectionBehaviorAttribute = assemblyInfo.GetCustomAttributes(typeof(CollectionBehaviorAttribute)).SingleOrDefault();
            if (collectionBehaviorAttribute != null)
                disableParallelization = collectionBehaviorAttribute.GetNamedArgument<bool>("DisableTestParallelization");

            var testCollectionFactory = XunitTestFrameworkDiscoverer.GetTestCollectionFactory(assemblyInfo, collectionBehaviorAttribute);
            displayName = String.Format("{0}-bit .NET {1} [{2}, {3}]",
                                        IntPtr.Size * 8,
                                        Environment.Version,
                                        testCollectionFactory.DisplayName,
                                        disableParallelization ? "non-parallel" : "parallel");
        }

        static void CreateFixture(Type interfaceType, ExceptionAggregator aggregator, Dictionary<Type, object> mappings)
        {
            var fixtureType = interfaceType.GetGenericArguments().Single();
            aggregator.Run(() => mappings[fixtureType] = Activator.CreateInstance(fixtureType));
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            return SerializationHelper.Deserialize<ITestCase>(value);
        }

        /// <inheritdoc/>
        public void Dispose() { }

        static ITestCaseOrderer GetTestCaseOrderer(IAttributeInfo ordererAttribute)
        {
            var args = ordererAttribute.GetConstructorArguments().Cast<string>().ToList();
            var ordererType = Reflector.GetType(args[1], args[0]);
            return (ITestCaseOrderer)Activator.CreateInstance(ordererType);
        }

        /// <inheritdoc/>
        public async void Run(IEnumerable<ITestCase> testCases, IMessageSink messageSink)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var totalSummary = new RunSummary();

            string currentDirectory = Directory.GetCurrentDirectory();

            var ordererAttribute = assemblyInfo.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            var orderer = ordererAttribute != null ? GetTestCaseOrderer(ordererAttribute) : new DefaultTestCaseOrderer();

            using (var messageBus = new MessageBus(messageSink))
            {
                try
                {
                    Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));

                    if (messageBus.QueueMessage(new TestAssemblyStarting(assemblyFileName, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, DateTime.Now,
                                                                         displayName, XunitTestFrameworkDiscoverer.DisplayName)))
                    {
                        IList<RunSummary> summaries;

                        // TODO: Contract for Run() states that null "testCases" means "run everything".

                        var masterStopwatch = Stopwatch.StartNew();

                        if (disableParallelization)
                        {
                            summaries = new List<RunSummary>();

                            foreach (var collectionGroup in testCases.Cast<XunitTestCase>().GroupBy(tc => tc.TestCollection))
                                summaries.Add(await RunTestCollectionAsync(messageBus, collectionGroup.Key, collectionGroup, orderer, cancellationTokenSource));
                        }
                        else
                        {
                            var tasks = testCases.Cast<XunitTestCase>()
                                                 .GroupBy(tc => tc.TestCollection)
                                                 .Select(collectionGroup => Task.Run(() => RunTestCollectionAsync(messageBus, collectionGroup.Key, collectionGroup, orderer, cancellationTokenSource)))
                                                 .ToArray();

                            summaries = await Task.WhenAll(tasks);
                        }

                        totalSummary.Time = (decimal)masterStopwatch.Elapsed.TotalSeconds;
                        totalSummary.Total = summaries.Sum(s => s.Total);
                        totalSummary.Failed = summaries.Sum(s => s.Failed);
                        totalSummary.Skipped = summaries.Sum(s => s.Skipped);
                    }
                }
                finally
                {
                    messageBus.QueueMessage(new TestAssemblyFinished(assemblyInfo, totalSummary.Time, totalSummary.Total, totalSummary.Failed, totalSummary.Skipped));
                    Directory.SetCurrentDirectory(currentDirectory);
                }
            }
        }

        private async Task<RunSummary> RunTestCollectionAsync(IMessageBus messageBus,
                                                              ITestCollection collection,
                                                              IEnumerable<XunitTestCase> testCases,
                                                              ITestCaseOrderer orderer,
                                                              CancellationTokenSource cancellationTokenSource)
        {
            var collectionSummary = new RunSummary();
            var collectionFixtureMappings = new Dictionary<Type, object>();
            var aggregator = new ExceptionAggregator();

            if (collection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)collection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateFixture(interfaceType, aggregator, collectionFixtureMappings);

                var ordererAttribute = collection.CollectionDefinition.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
                if (ordererAttribute != null)
                    orderer = GetTestCaseOrderer(ordererAttribute);
            }

            if (messageBus.QueueMessage(new TestCollectionStarting(collection)))
            {
                foreach (var testCasesByClass in testCases.GroupBy(tc => tc.Class))
                {
                    var classSummary = new RunSummary();

                    if (!messageBus.QueueMessage(new TestClassStarting(collection, testCasesByClass.Key.Name)))
                        cancellationTokenSource.Cancel();
                    else
                    {
                        await RunTestClassAsync(messageBus, collection, collectionFixtureMappings, (IReflectionTypeInfo)testCasesByClass.Key, testCasesByClass, orderer, classSummary, aggregator, cancellationTokenSource);
                        collectionSummary.Aggregate(classSummary);
                    }

                    if (!messageBus.QueueMessage(new TestClassFinished(collection, testCasesByClass.Key.Name, classSummary.Time, classSummary.Total, classSummary.Failed, classSummary.Skipped)))
                        cancellationTokenSource.Cancel();

                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                }
            }

            foreach (var fixture in collectionFixtureMappings.Values.OfType<IDisposable>())
            {
                try
                {
                    fixture.Dispose();
                }
                catch (Exception ex)
                {
                    if (!messageBus.QueueMessage(new ErrorMessage(ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }
            }

            messageBus.QueueMessage(new TestCollectionFinished(collection, collectionSummary.Time, collectionSummary.Total, collectionSummary.Failed, collectionSummary.Skipped));
            return collectionSummary;
        }

        private static async Task RunTestClassAsync(IMessageBus messageBus,
                                                    ITestCollection collection,
                                                    Dictionary<Type, object> collectionFixtureMappings,
                                                    IReflectionTypeInfo testClass,
                                                    IEnumerable<XunitTestCase> testCases,
                                                    ITestCaseOrderer orderer,
                                                    RunSummary classSummary,
                                                    ExceptionAggregator aggregator,
                                                    CancellationTokenSource cancellationTokenSource)
        {
            var testClassType = testClass.Type;
            var fixtureMappings = new Dictionary<Type, object>();
            var constructorArguments = new List<object>();

            var ordererAttribute = testClass.GetCustomAttributes(typeof(TestCaseOrdererAttribute)).SingleOrDefault();
            if (ordererAttribute != null)
                orderer = GetTestCaseOrderer(ordererAttribute);

            if (testClassType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                aggregator.Add(new TestClassException("A test class may not be decorated with ICollectionFixture<> (decorate the test collection class instead)."));

            foreach (var interfaceType in testClassType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                CreateFixture(interfaceType, aggregator, fixtureMappings);

            if (collection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)collection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IClassFixture<>)))
                    CreateFixture(interfaceType, aggregator, fixtureMappings);
            }

            var isStaticClass = testClassType.IsAbstract && testClassType.IsSealed;
            if (!isStaticClass)
            {
                var ctors = testClassType.GetConstructors();
                if (ctors.Length != 1)
                {
                    aggregator.Add(new TestClassException("A test class may only define a single public constructor."));
                }
                else
                {
                    var ctor = ctors.Single();
                    var unusedArguments = new List<string>();

                    foreach (var paramInfo in ctor.GetParameters())
                    {
                        object fixture;

                        if (fixtureMappings.TryGetValue(paramInfo.ParameterType, out fixture) || collectionFixtureMappings.TryGetValue(paramInfo.ParameterType, out fixture))
                            constructorArguments.Add(fixture);
                        else
                            unusedArguments.Add(String.Format("{0} {1}", paramInfo.ParameterType.Name, paramInfo.Name));
                    }

                    if (unusedArguments.Count > 0)
                        aggregator.Add(new TestClassException("The following constructor arguments did not have matching fixture data: " + String.Join(", ", unusedArguments)));
                }
            }

            var orderedTestCases = orderer.OrderTestCases(testCases);
            var methodGroups = orderedTestCases.GroupBy(tc => tc.Method);

            foreach (var method in methodGroups)
            {
                if (!messageBus.QueueMessage(new TestMethodStarting(collection, testClass.Name, method.Key.Name)))
                    cancellationTokenSource.Cancel();
                else
                    await RunTestMethodAsync(messageBus, constructorArguments.ToArray(), method, classSummary, aggregator, cancellationTokenSource);

                if (!messageBus.QueueMessage(new TestMethodFinished(collection, testClass.Name, method.Key.Name)))
                    cancellationTokenSource.Cancel();

                if (cancellationTokenSource.IsCancellationRequested)
                    break;
            }

            foreach (var fixture in fixtureMappings.Values.OfType<IDisposable>())
            {
                try
                {
                    fixture.Dispose();
                }
                catch (Exception ex)
                {
                    if (!messageBus.QueueMessage(new ErrorMessage(ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }
            }
        }

        private static async Task RunTestMethodAsync(IMessageBus messageBus,
                                                     object[] constructorArguments,
                                                     IEnumerable<XunitTestCase> testCases,
                                                     RunSummary classSummary,
                                                     ExceptionAggregator aggregator,
                                                     CancellationTokenSource cancellationTokenSource)
        {
            foreach (var testCase in testCases)
            {
                using (var delegatingBus = new DelegatingMessageBus<ITestCaseFinished>(messageBus))
                {
                    await testCase.RunAsync(delegatingBus, constructorArguments, aggregator, cancellationTokenSource);
                    delegatingBus.Finished.WaitOne();

                    classSummary.Total += delegatingBus.FinalMessage.TestsRun;
                    classSummary.Failed += delegatingBus.FinalMessage.TestsFailed;
                    classSummary.Skipped += delegatingBus.FinalMessage.TestsSkipped;
                    classSummary.Time += delegatingBus.FinalMessage.ExecutionTime;
                }

                if (cancellationTokenSource.IsCancellationRequested)
                    break;
            }
        }

        class RunSummary
        {
            public int Total;
            public int Failed;
            public int Skipped;
            public decimal Time;

            public void Aggregate(RunSummary other)
            {
                Total += other.Total;
                Failed += other.Failed;
                Skipped += other.Skipped;
                Time += other.Time;
            }
        }
    }
}