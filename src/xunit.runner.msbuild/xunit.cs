using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Microsoft.Build.Framework;
using Xunit.Abstractions;
using MSBuildTask = Microsoft.Build.Utilities.Task;

namespace Xunit.Runner.MSBuild
{
    public class xunit : MSBuildTask, ICancelableTask
    {
        bool? appDomains;
        volatile bool cancel;
        readonly ConcurrentDictionary<string, ExecutionSummary> completionMessages = new ConcurrentDictionary<string, ExecutionSummary>();
        XunitFilters filters;
        IRunnerLogger logger;
        int? maxThreadCount;
        bool? parallelizeAssemblies;
        bool? parallelizeTestCollections;
        IMessageSink reporterMessageHandler;

        public xunit()
        {
            ShadowCopy = true;
        }

        public bool AppDomains { set { appDomains = value; } }

        [Required]
        public ITaskItem[] Assemblies { get; set; }

        public bool DiagnosticMessages { get; set; }

        public string ExcludeTraits { get; set; }

        [Output]
        public int ExitCode { get; protected set; }

        protected XunitFilters Filters
        {
            get
            {
                if (filters == null)
                {
                    var traitParser = new TraitParser(msg => Log.LogWarning(msg));
                    filters = new XunitFilters();
                    traitParser.Parse(IncludeTraits, filters.IncludedTraits);
                    traitParser.Parse(ExcludeTraits, filters.ExcludedTraits);
                }

                return filters;
            }
        }

        public ITaskItem Html { get; set; }

        public string IncludeTraits { get; set; }

        public string MaxParallelThreads { get; set; }

        protected bool NeedsXml
        {
            get { return Xml != null || XmlV1 != null || Html != null || NUnit != null; }
        }

        public bool NoLogo { get; set; }

        public ITaskItem NUnit { get; set; }

        public bool ParallelizeAssemblies { set { parallelizeAssemblies = value; } }

        public bool ParallelizeTestCollections { set { parallelizeTestCollections = value; } }

        public string Reporter { get; set; }

        // To be used by the xUnit.net team for diagnostic purposes only
        public bool SerializeTestCases { get; set; }

        public bool ShadowCopy { get; set; }

        // Obsolote; remove post 2.1 RTM
        public bool TeamCity { get; set; }

        // Obsolote; remove post 2.1 RTM
        public bool Verbose { get; set; }

        public string WorkingFolder { get; set; }

        public ITaskItem Xml { get; set; }

        public ITaskItem XmlV1 { get; set; }

        public void Cancel()
        {
            cancel = true;
        }

        public override bool Execute()
        {
            RemotingUtility.CleanUpRegisteredChannels();

            if (TeamCity)
            {
                Log.LogError("The 'TeamCity' property is deprecated. Please set the 'Reporter' property to 'teamcity' instead.");
                return false;
            }
            if (Verbose)
            {
                Log.LogError("The 'Verbose' property is deprecated. Please set the 'Reporter' property to 'verbose' instead.");
                return false;
            }

            XElement assembliesElement = null;
            var environment = $"{IntPtr.Size * 8}-bit .NET {Environment.Version}";

            if (NeedsXml)
                assembliesElement = new XElement("assemblies");

            switch (MaxParallelThreads)
            {
                case null:
                case "default":
                    break;

                case "unlimited":
                    maxThreadCount = -1;
                    break;

                default:
                    int threadValue;
                    if (!int.TryParse(MaxParallelThreads, out threadValue) || threadValue < 1)
                    {
                        Log.LogError("MaxParallelThreads value '{0}' is invalid: must be 'default', 'unlimited', or a positive number", MaxParallelThreads);
                        return false;
                    }

                    maxThreadCount = threadValue;
                    break;
            }

            var originalWorkingFolder = Directory.GetCurrentDirectory();

            using (AssemblyHelper.SubscribeResolve())
            {
                var reporters = GetAvailableRunnerReporters();
                var reporter = reporters.FirstOrDefault(r => r.IsEnvironmentallyEnabled);

                if (reporter == null && !string.IsNullOrWhiteSpace(Reporter))
                {
                    reporter = reporters.FirstOrDefault(r => string.Equals(r.RunnerSwitch, Reporter, StringComparison.OrdinalIgnoreCase));
                    if (reporter == null)
                    {
                        var switchableReporters = reporters.Where(r => !string.IsNullOrWhiteSpace(r.RunnerSwitch)).Select(r => r.RunnerSwitch.ToLowerInvariant()).OrderBy(x => x).ToList();
                        if (switchableReporters.Count == 0)
                            Log.LogError("Reporter value '{0}' is invalid. There are no available reporters.", Reporter);
                        else
                            Log.LogError("Reporter value '{0}' is invalid. Available reporters: {1}", Reporter, string.Join(", ", switchableReporters));

                        return false;
                    }
                }

                if (reporter == null)
                    reporter = new DefaultRunnerReporter();

                logger = new MSBuildLogger(Log);
                reporterMessageHandler = reporter.CreateMessageHandler(logger);

                if (!NoLogo)
                    Log.LogMessage(MessageImportance.High, "xUnit.net MSBuild Runner ({0})", environment);

                var project = new XunitProject();
                foreach (var assembly in Assemblies)
                {
                    var assemblyFileName = assembly.GetMetadata("FullPath");
                    var configFileName = assembly.GetMetadata("ConfigFile");
                    if (configFileName != null && configFileName.Length == 0)
                        configFileName = null;

                    project.Add(new XunitProjectAssembly { AssemblyFilename = assemblyFileName, ConfigFilename = configFileName, ShadowCopy = ShadowCopy });
                }

                if (WorkingFolder != null)
                    Directory.SetCurrentDirectory(WorkingFolder);

                var clockTime = Stopwatch.StartNew();

                if (!parallelizeAssemblies.HasValue)
                    parallelizeAssemblies = project.All(assembly => assembly.Configuration.ParallelizeAssemblyOrDefault);

                if (parallelizeAssemblies.GetValueOrDefault())
                {
                    var tasks = project.Assemblies.Select(assembly => Task.Run(() => ExecuteAssembly(assembly)));
                    var results = Task.WhenAll(tasks).GetAwaiter().GetResult();
                    foreach (var assemblyElement in results.Where(result => result != null))
                        assembliesElement.Add(assemblyElement);
                }
                else
                {
                    foreach (var assembly in project.Assemblies)
                    {
                        var assemblyElement = ExecuteAssembly(assembly);
                        if (assemblyElement != null)
                            assembliesElement.Add(assemblyElement);
                    }
                }

                clockTime.Stop();

                if (completionMessages.Count > 0)
                    reporterMessageHandler.OnMessage(new TestExecutionSummary(clockTime.Elapsed, completionMessages.OrderBy(kvp => kvp.Key).ToList()));
            }

            Directory.SetCurrentDirectory(WorkingFolder ?? originalWorkingFolder);

            if (NeedsXml)
            {
                if (Xml != null)
                    assembliesElement.Save(Xml.GetMetadata("FullPath"));

                if (XmlV1 != null)
                    Transform("xUnit1.xslt", assembliesElement, XmlV1);

                if (Html != null)
                    Transform("HTML.xslt", assembliesElement, Html);

                if (NUnit != null)
                    Transform("NUnitXml.xslt", assembliesElement, NUnit);
            }

            return ExitCode == 0;
        }

        List<IRunnerReporter> GetAvailableRunnerReporters()
        {
            var result = new List<IRunnerReporter>();
            var runnerPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetLocalCodeBase());

            foreach (var dllFile in Directory.GetFiles(runnerPath, "*.dll").Select(f => Path.Combine(runnerPath, f)))
            {
                Type[] types;

                try
                {
                    var assembly = Assembly.LoadFile(dllFile);
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || type == typeof(DefaultRunnerReporter) || !type.GetInterfaces().Any(t => t == typeof(IRunnerReporter)))
                        continue;
                    var ctor = type.GetConstructor(new Type[0]);
                    if (ctor == null)
                    {
                        Log.LogWarning("Type {0} in assembly {1} appears to be a runner reporter, but does not have an empty constructor.", type.FullName, dllFile);
                        continue;
                    }

                    result.Add((IRunnerReporter)ctor.Invoke(new object[0]));
                }
            }

            return result;
        }

        protected virtual XElement ExecuteAssembly(XunitProjectAssembly assembly)
        {
            if (cancel)
                return null;

            var assemblyElement = NeedsXml ? new XElement("assembly") : null;

            try
            {
                // Turn off pre-enumeration of theories, since there is no theory selection UI in this runner
                assembly.Configuration.PreEnumerateTheories = false;
                assembly.Configuration.DiagnosticMessages |= DiagnosticMessages;

                if (appDomains.HasValue)
                    assembly.Configuration.UseAppDomain = appDomains.GetValueOrDefault();

                // Setup discovery and execution options with command-line overrides
                var discoveryOptions = TestFrameworkOptions.ForDiscovery(assembly.Configuration);
                var executionOptions = TestFrameworkOptions.ForExecution(assembly.Configuration);
                if (maxThreadCount.HasValue && maxThreadCount.Value > -1)
                    executionOptions.SetMaxParallelThreads(maxThreadCount);
                if (parallelizeTestCollections.HasValue)
                    executionOptions.SetDisableParallelization(!parallelizeTestCollections);

                var assemblyDisplayName = Path.GetFileNameWithoutExtension(assembly.AssemblyFilename);
                var diagnosticMessageVisitor = new DiagnosticMessageVisitor(Log, assemblyDisplayName, assembly.Configuration.DiagnosticMessagesOrDefault);
                var useAppDomain = assembly.Configuration.UseAppDomainOrDefault;

                using (var controller = new XunitFrontController(useAppDomain, assembly.AssemblyFilename, assembly.ConfigFilename, assembly.ShadowCopy, diagnosticMessageSink: diagnosticMessageVisitor))
                using (var discoveryVisitor = new TestDiscoveryVisitor())
                {
                    // Discover & filter the tests
                    reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryStarting(assembly, useAppDomain, discoveryOptions));

                    controller.Find(false, discoveryVisitor, discoveryOptions);
                    discoveryVisitor.Finished.WaitOne();

                    var testCasesDiscovered = discoveryVisitor.TestCases.Count;
                    var filteredTestCases = discoveryVisitor.TestCases.Where(Filters.Filter).ToList();
                    var testCasesToRun = filteredTestCases.Count;

                    reporterMessageHandler.OnMessage(new TestAssemblyDiscoveryFinished(assembly, discoveryOptions, testCasesDiscovered, testCasesToRun));

                    // Run the filtered tests
                    if (testCasesToRun == 0)
                        completionMessages.TryAdd(Path.GetFileName(assembly.AssemblyFilename), new ExecutionSummary());
                    else
                    {
                        if (SerializeTestCases)
                            filteredTestCases = filteredTestCases.Select(controller.Serialize).Select(controller.Deserialize).ToList();

                        var resultsVisitor = new XmlAggregateVisitor(reporterMessageHandler, completionMessages, assemblyElement, () => cancel);

                        reporterMessageHandler.OnMessage(new TestAssemblyExecutionStarting(assembly, executionOptions));

                        controller.RunTests(filteredTestCases, resultsVisitor, executionOptions);
                        resultsVisitor.Finished.WaitOne();

                        reporterMessageHandler.OnMessage(new TestAssemblyExecutionFinished(assembly, executionOptions, resultsVisitor.ExecutionSummary));

                        if (resultsVisitor.Failed != 0)
                            ExitCode = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                var e = ex;

                while (e != null)
                {
                    Log.LogError("{0}: {1}", e.GetType().FullName, e.Message);

                    foreach (var stackLine in e.StackTrace.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                        Log.LogError(stackLine);

                    e = e.InnerException;
                }

                ExitCode = -1;
            }

            return assemblyElement;
        }

        static void Transform(string resourceName, XNode xml, ITaskItem outputFile)
        {
            var xmlTransform = new XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFile.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
            using (var xsltReader = XmlReader.Create(typeof(xunit).Assembly.GetManifestResourceStream("Xunit.Runner.MSBuild." + resourceName)))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }
    }
}
