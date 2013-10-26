using System;
using System.Collections.Generic;
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
        readonly bool disableParallelization = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitTestFrameworkExecutor"/> class.
        /// </summary>
        /// <param name="assemblyFileName">Path of the test assembly.</param>
        public XunitTestFrameworkExecutor(string assemblyFileName)
        {
            this.assemblyFileName = assemblyFileName;

            var assembly = Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
            assemblyInfo = Reflector.Wrap(assembly);

            var collectionBehavior = assembly.GetCustomAttribute<CollectionBehaviorAttribute>();
            if (collectionBehavior != null)
                disableParallelization = collectionBehavior.DisableTestParallelization;
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

        /// <inheritdoc/>
        public async void Run(IEnumerable<ITestCase> testCases, IMessageSink messageSink)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var totalSummary = new RunSummary();

            string currentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyInfo.AssemblyPath));

                if (messageSink.OnMessage(new TestAssemblyStarting(assemblyFileName, AppDomain.CurrentDomain.SetupInformation.ConfigurationFile, DateTime.Now,
                                                                   String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version),
                                                                   XunitTestFrameworkDiscoverer.DisplayName)))
                {
                    IList<RunSummary> summaries;

                    // TODO: Contract for Run() states that null "testCases" means "run everything".

                    if (disableParallelization)
                    {
                        summaries = new List<RunSummary>();

                        foreach (var collectionGroup in testCases.Cast<XunitTestCase>().GroupBy(tc => tc.TestCollection))
                            summaries.Add(RunTestCollection(messageSink, collectionGroup.Key, collectionGroup, cancellationTokenSource));
                    }
                    else
                    {
                        var tasks = testCases.Cast<XunitTestCase>()
                                             .GroupBy(tc => tc.TestCollection)
                                             .Select(collectionGroup => Task.Run(() => RunTestCollection(messageSink, collectionGroup.Key, collectionGroup, cancellationTokenSource)))
                                             .ToArray();

                        summaries = await Task.WhenAll(tasks);
                    }

                    totalSummary.Time = summaries.Sum(s => s.Time);
                    totalSummary.Total = summaries.Sum(s => s.Total);
                    totalSummary.Failed = summaries.Sum(s => s.Failed);
                    totalSummary.Skipped = summaries.Sum(s => s.Skipped);
                }

                messageSink.OnMessage(new TestAssemblyFinished(assemblyInfo, totalSummary.Time, totalSummary.Total, totalSummary.Failed, totalSummary.Skipped));
            }
            finally
            {
                Directory.SetCurrentDirectory(currentDirectory);
            }
        }

        private RunSummary RunTestCollection(IMessageSink messageSink, ITestCollection collection, IEnumerable<XunitTestCase> testCases, CancellationTokenSource cancellationTokenSource)
        {
            var collectionSummary = new RunSummary();
            var collectionFixtureMappings = new Dictionary<Type, object>();
            var aggregator = new ExceptionAggregator();

            if (collection.CollectionDefinition != null)
            {
                var declarationType = ((IReflectionTypeInfo)collection.CollectionDefinition).Type;
                foreach (var interfaceType in declarationType.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollectionFixture<>)))
                    CreateFixture(interfaceType, aggregator, collectionFixtureMappings);
            }

            if (messageSink.OnMessage(new TestCollectionStarting(collection)))
            {
                foreach (var testCasesByClass in testCases.GroupBy(tc => tc.Class))
                {
                    var classSummary = new RunSummary();

                    if (!messageSink.OnMessage(new TestClassStarting(collection, testCasesByClass.Key.Name)))
                        cancellationTokenSource.Cancel();
                    else
                    {
                        RunTestClass(messageSink, collection, collectionFixtureMappings, (IReflectionTypeInfo)testCasesByClass.Key, testCasesByClass, classSummary, aggregator, cancellationTokenSource);
                        collectionSummary.Aggregate(classSummary);
                    }

                    if (!messageSink.OnMessage(new TestClassFinished(collection, testCasesByClass.Key.Name, classSummary.Time, classSummary.Total, classSummary.Failed, classSummary.Skipped)))
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
                    if (!messageSink.OnMessage(new ErrorMessage(ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }
            }

            messageSink.OnMessage(new TestCollectionFinished(collection, collectionSummary.Time, collectionSummary.Total, collectionSummary.Failed, collectionSummary.Skipped));
            return collectionSummary;
        }

        private static void RunTestClass(IMessageSink messageSink,
                                         ITestCollection collection,
                                         Dictionary<Type, object> collectionFixtureMappings,
                                         IReflectionTypeInfo testClass,
                                         IEnumerable<XunitTestCase> testCases,
                                         RunSummary classSummary,
                                         ExceptionAggregator aggregator,
                                         CancellationTokenSource cancellationTokenSource)
        {
            var testClassType = testClass.Type;
            var fixtureMappings = new Dictionary<Type, object>();
            var constructorArguments = new List<object>();

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
                            unusedArguments.Add(paramInfo.ParameterType.Name + " " + paramInfo.Name);
                    }

                    if (unusedArguments.Count > 0)
                        aggregator.Add(new TestClassException("The following constructor arguments did not have matching fixture data: " + String.Join(", ", unusedArguments)));
                }
            }

            var methodGroups = testCases.GroupBy(tc => tc.Method);

            foreach (var method in methodGroups)
            {
                if (!messageSink.OnMessage(new TestMethodStarting(collection, testClass.Name, method.Key.Name)))
                    cancellationTokenSource.Cancel();
                else
                    RunTestMethod(messageSink, constructorArguments.ToArray(), method, classSummary, aggregator, cancellationTokenSource);

                if (!messageSink.OnMessage(new TestMethodFinished(collection, testClass.Name, method.Key.Name)))
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
                    if (!messageSink.OnMessage(new ErrorMessage(ex.Unwrap())))
                        cancellationTokenSource.Cancel();
                }
            }
        }

        private static void RunTestMethod(IMessageSink messageSink,
                                          object[] constructorArguments,
                                          IEnumerable<XunitTestCase> testCases,
                                          RunSummary classSummary,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource)
        {
            foreach (XunitTestCase testCase in testCases)
            {
                var delegatingSink = new DelegatingMessageSink<ITestCaseFinished>(messageSink);

                testCase.Run(delegatingSink, constructorArguments, aggregator, cancellationTokenSource);
                delegatingSink.Finished.WaitOne();

                classSummary.Total += delegatingSink.FinalMessage.TestsRun;
                classSummary.Failed += delegatingSink.FinalMessage.TestsFailed;
                classSummary.Skipped += delegatingSink.FinalMessage.TestsSkipped;
                classSummary.Time += delegatingSink.FinalMessage.ExecutionTime;

                if (cancellationTokenSource.IsCancellationRequested)
                    break;
            }
        }

        class RunSummary
        {
            public int Total = 0;
            public int Failed = 0;
            public int Skipped = 0;
            public decimal Time = 0M;

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