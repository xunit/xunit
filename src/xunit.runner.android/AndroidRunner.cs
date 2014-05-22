//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Security.Acl;
using MonoDroid.Dialog;
using Xunit.Runners.UI;
using Xunit.Runners.Utilities;
using Xunit.Runners.Visitors;
using Debug = System.Diagnostics.Debug;


namespace Xunit.Runners
{
    public class AndroidRunner : ITestListener
    {
        private static readonly AndroidRunner runner = new AndroidRunner();
        
        private readonly Dictionary<string, TestSuiteElement> suite_elements = new Dictionary<string, TestSuiteElement>();
        private readonly Dictionary<string, MonoTestResult> results = new Dictionary<string, MonoTestResult>();
        

        private static readonly List<Assembly> assemblies = new List<Assembly>();
        private readonly AsyncLock executionLock = new AsyncLock();
        private readonly Stack<DateTime> time = new Stack<DateTime>();
        private bool cancelled;
        readonly ManualResetEvent mre = new ManualResetEvent(false);
        private RunnerOptions options;

        private Action refreshViews;

        private Dictionary<string, IEnumerable<MonoTestCase>> testCasesByAssembly = new Dictionary<string, IEnumerable<MonoTestCase>>();
        private int passed;
        private int failed;
        private int skipped;

        private AndroidRunner()
        {
        }

        public bool AutoStart { get; set; }

        public bool TerminateAfterExecution { get; set; }

        public RunnerOptions Options
        {
            get
            {
                if (options == null)
                    options = new RunnerOptions();
                return options;
            }
            set { options = value; }
        }

    
        public static AndroidRunner Runner
        {
            get { return runner; }
        }

        public IDictionary<string, MonoTestResult> Results
        {
            get { return results; }
        }

        #region writer

        public TextWriter Writer { get; set; }

        private bool OpenWriter(string message)
        {
            var now = DateTime.Now;
            // let the application provide it's own TextWriter to ease automation with AutoStart property
            if (Writer == null)
            {
                if (Options.ShowUseNetworkLogger)
                {
                    Console.WriteLine("[{0}] Sending '{1}' results to {2}:{3}", now, message, Options.HostName, Options.HostPort);
                    try
                    {
                        Writer = new TcpTextWriter(Options.HostName, Options.HostPort);
                    }
                    catch (SocketException)
                    {
                        var msg = String.Format("Cannot connect to {0}:{1}. Start network service or disable network option", options.HostName, options.HostPort);
                        Toast.MakeText(Application.Context, msg, ToastLength.Long)
                             .Show();
                        return false;
                    }
                }
                else
                {
                    Writer = Console.Out;
                }
            }

            Writer.WriteLine("[Runner executing:\t{0}]", message);
            // FIXME
            Writer.WriteLine("[M4A Version:\t{0}]", "???");

            Writer.WriteLine("[Board:\t\t{0}]", Build.Board);
            Writer.WriteLine("[Bootloader:\t{0}]", Build.Bootloader);
            Writer.WriteLine("[Brand:\t\t{0}]", Build.Brand);
            Writer.WriteLine("[CpuAbi:\t{0} {1}]", Build.CpuAbi, Build.CpuAbi2);
            Writer.WriteLine("[Device:\t{0}]", Build.Device);
            Writer.WriteLine("[Display:\t{0}]", Build.Display);
            Writer.WriteLine("[Fingerprint:\t{0}]", Build.Fingerprint);
            Writer.WriteLine("[Hardware:\t{0}]", Build.Hardware);
            Writer.WriteLine("[Host:\t\t{0}]", Build.Host);
            Writer.WriteLine("[Id:\t\t{0}]", Build.Id);
            Writer.WriteLine("[Manufacturer:\t{0}]", Build.Manufacturer);
            Writer.WriteLine("[Model:\t\t{0}]", Build.Model);
            Writer.WriteLine("[Product:\t{0}]", Build.Product);
            Writer.WriteLine("[Radio:\t\t{0}]", Build.Radio);
            Writer.WriteLine("[Tags:\t\t{0}]", Build.Tags);
            Writer.WriteLine("[Time:\t\t{0}]", Build.Time);
            Writer.WriteLine("[Type:\t\t{0}]", Build.Type);
            Writer.WriteLine("[User:\t\t{0}]", Build.User);
            Writer.WriteLine("[VERSION.Codename:\t{0}]", Build.VERSION.Codename);
            Writer.WriteLine("[VERSION.Incremental:\t{0}]", Build.VERSION.Incremental);
            Writer.WriteLine("[VERSION.Release:\t{0}]", Build.VERSION.Release);
            Writer.WriteLine("[VERSION.Sdk:\t\t{0}]", Build.VERSION.Sdk);
            Writer.WriteLine("[VERSION.SdkInt:\t{0}]", Build.VERSION.SdkInt);
            Writer.WriteLine("[Device Date/Time:\t{0}]", now); // to match earlier C.WL output

            // FIXME: add data about how the app was compiled (e.g. ARMvX, LLVM, Linker options)

            return true;
        }

        private void CloseWriter()
        {
            Writer.Close();
            Writer = null;
        }

        #endregion

        private IEnumerable<IGrouping<string, MonoTestCase>> DiscoverTestsInAssemblies()
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new List<IGrouping<string, MonoTestCase>>();

            try
            {
                using (AssemblyHelper.SubscribeResolve())
                {
                    foreach (var assm in assemblies)
                    {
                        // Xunit needs the file name
                        var fileName = Path.GetFileName(assm.Location);

                        try
                        {
                            using (var framework = new XunitFrontController(fileName, configFileName: null, shadowCopy: true))
                            using (var sink = new TestDiscoveryVisitor())
                            {
                                framework.Find(includeSourceInformation: true, messageSink: sink, options: new TestFrameworkOptions());
                                sink.Finished.WaitOne();

                                result.Add(
                                    new Grouping<string, MonoTestCase>(
                                        fileName,
                                        sink.TestCases
                                            .GroupBy(tc => String.Format("{0}.{1}", tc.Class.Name, tc.Method.Name))
                                            .SelectMany(group =>
                                                        group.Select(testCase =>
                                                                     new MonoTestCase(fileName, testCase, forceUniqueNames: group.Count() > 1)))
                                            .ToList()
                                        )
                                    );
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            stopwatch.Stop();

            return result;
        }

        internal View GetView(Activity activity)
        {
            
            if (Options == null)
                Options = new RunnerOptions(activity);

            RunnerOptions.Initialize(activity);

            Results.Clear();
            suite_elements.Clear();

            var menu = new RootElement("Test Runner");

            var main = new Section("Loading test assemblies...");

            var optSect = new Section()
            {
                new ActivityElement("Options", typeof(OptionsActivity)),
                new ActivityElement("Credits", typeof(CreditsActivity))
            };

            menu.Add(main);
            menu.Add(optSect);

            var a = new DialogAdapter(activity, menu);
            var lv = new ListView(activity)
            {
                Adapter = a
            };

            //refreshViews = () =>
            //{
            //    a.NotifyDataSetChanged();
            //    optSect.Adapter.NotifyDataSetChanged();
            //};

     //       optSect.Adapter.NotifyDataSetChanged();

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var allTests = DiscoverTestsInAssemblies();
                testCasesByAssembly = allTests.ToDictionary(cases => cases.Key, cases => cases as IEnumerable<MonoTestCase>);


                activity.RunOnUiThread(() =>
                {
                    foreach (var kvp in testCasesByAssembly)
                    {
                        main.Add(SetupSource(kvp.Key, kvp.Value));
                    }


                    mre.Set();
                    main.Caption = null;

                   optSect.Insert(0, new ActionElement("Run Everything", async () => await Run()));
                  // optSect.Adapter.NotifyDataSetChanged();
                    
     //               a.NotifyDataSetChanged();
                });

                assemblies.Clear();
            });


            //// AutoStart running the tests (with either the supplied 'writer' or the options)
            //if (AutoStart)
            //{
            //    ThreadPool.QueueUserWorkItem(delegate
            //    {
            //        mre.WaitOne();
            //        activity.RunOnUiThread(async () =>
            //        {
            //            await Run();

            //            // optionally end the process, 
            //            if (TerminateAfterExecution)
            //                activity.Finish();
            //        });
            //    });
            //}

            return lv;
        }

        void ITestListener.RecordResult(MonoTestResult result)
        {
            Application.SynchronizationContext.Post(_ =>
            {
                Results[result.TestCase.UniqueName] = result;

                result.RaiseTestUpdated();
            }, null);

            if (result.TestCase.Result == TestState.Passed)
            {
                Writer.Write("\t[PASS] ");
                passed++;
            }
            else if (result.TestCase.Result == TestState.Skipped)
            {
                Writer.Write("\t[SKIPPED] ");
                skipped++;
            }
            else if (result.TestCase.Result == TestState.Failed)
            {
                Writer.Write("\t[FAIL] ");
                failed++;
            }
            else
            {
                Writer.Write("\t[INFO] ");
            }
            Writer.Write(result.TestCase.DisplayName);

            var message = result.ErrorMessage;
            if (!String.IsNullOrEmpty(message))
            {
                Writer.Write(" : {0}", message.Replace("\r\n", "\\r\\n"));
            }
            Writer.WriteLine();

            var stacktrace = result.ErrorStackTrace;
            if (!String.IsNullOrEmpty(result.ErrorStackTrace))
            {
                var lines = stacktrace.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                    Writer.WriteLine("\t\t{0}", line);
            }
        }

        internal IDictionary<string, TestSuiteElement> Suites
        {
            get { return suite_elements; }
        }

        internal static void AddAssembly(Assembly testAssm)
        {
            assemblies.Add(testAssm);
        }

        internal Task Run()
        {

            return Run(testCasesByAssembly.Values.SelectMany(v => v), "Run Everything");
        }

        internal Task Run(MonoTestCase test)
        {
            return Run(new[] {test});
        }

        internal async Task Run(IEnumerable<MonoTestCase> tests, string message = null)
        {
            var stopWatch = Stopwatch.StartNew();

            var groups = tests.GroupBy(t => t.AssemblyFileName);

            using (await executionLock.LockAsync())
            {

                if (message == null)
                    message = tests.Count() > 1 ? "Run Multiple Tests" : tests.First()
                                                                              .DisplayName;
                if (!OpenWriter(message))
                    return;
                try
                {
                    await RunTests(groups, stopWatch);
                }
                finally
                {
                    CloseWriter();
                }
            }
        }

        private TestSuiteElement SetupSource(string sourceName, IEnumerable<MonoTestCase> testSource)
        {

            var root = new RootElement("Tests");

            var elements = new List<TestCaseElement>();

            var section = new Section(sourceName);
            foreach (var test in testSource)
            {
                var ele = new TestCaseElement(test, this);
                elements.Add(ele);
                section.Add(ele);
            }

            var tse = new TestSuiteElement(sourceName, elements, this);
            suite_elements[sourceName] = tse;


            root.Add(section);

            if (section.Count > 1)
            {
                StringElement allbtn = null;
                allbtn = new StringElement("Run all",
                                           async delegate
                                           {
                                               
                                               //var table = allbtn.GetContainerTableView();
                                               //var cell = allbtn.GetCell(table);
                                               //cell.UserInteractionEnabled = false;
                                               //cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                                               //cell.
                                               await Run(testSource);

                                               //cell.UserInteractionEnabled = true;
                                               //cell.SelectionStyle = UITableViewCellSelectionStyle.Default;

                                           //    suites_dvc[testSource.Key].Filter();
                                           });
                var options = new Section()
                {
                  allbtn
                };

                root.Add(options);
            }

           // suites_dvc.Add(testSource.Key, new TouchViewController(root));
            return tse;
        }


        private Task RunTests(IEnumerable<IGrouping<string, MonoTestCase>> testCaseAccessor, Stopwatch stopwatch)
        {
            var tcs = new TaskCompletionSource<object>(null);

            ThreadPool.QueueUserWorkItem(state =>
            {
                var toDispose = new List<IDisposable>();

                try
                {
                    cancelled = false;

                    using (AssemblyHelper.SubscribeResolve())
                        if (RunnerOptions.Current.ParallelizeAssemblies)
                            testCaseAccessor
                                .Select(testCaseGroup => RunTestsInAssemblyAsync(toDispose, testCaseGroup.Key, testCaseGroup, stopwatch))
                                .ToList()
                                .ForEach(@event => @event.WaitOne());
                        else
                            testCaseAccessor
                                .ToList()
                                .ForEach(testCaseGroup => RunTestsInAssembly(toDispose, testCaseGroup.Key, testCaseGroup, stopwatch));
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
                finally
                {
                    toDispose.ForEach(disposable => disposable.Dispose());
                    OnTestRunCompleted();
                    tcs.SetResult(null);
                }
            });

            return tcs.Task;
        }

        private ManualResetEvent RunTestsInAssemblyAsync(List<IDisposable> toDispose,
                                                         string assemblyFileName,
                                                         IEnumerable<MonoTestCase> testCases,
                                                         Stopwatch stopwatch)
        {
            var @event = new ManualResetEvent(initialState: false);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    RunTestsInAssembly(toDispose, assemblyFileName, testCases, stopwatch);
                }
                finally
                {
                    @event.Set();
                }
            });

            return @event;
        }

        private void RunTestsInAssembly(List<IDisposable> toDispose,
                                        string assemblyFileName,
                                        IEnumerable<MonoTestCase> testCases,
                                        Stopwatch stopwatch)
        {
            if (cancelled)
                return;

            var controller = new XunitFrontController(assemblyFileName, configFileName: null, shadowCopy: true);

            lock (toDispose)
                toDispose.Add(controller);

            var xunitTestCases = testCases.ToDictionary(tc => tc.TestCase);

            using (var executionVisitor = new MonoTestExecutionVisitor(xunitTestCases, this, () => cancelled))
            {
                var executionOptions = new XunitExecutionOptions
                {
                    //DisableParallelization = !settings.ParallelizeTestCollections,
                    //MaxParallelThreads = settings.MaxParallelThreads
                };

                controller.RunTests(xunitTestCases.Keys.ToList(), executionVisitor, executionOptions);
                executionVisitor.Finished.WaitOne();
            }
        }

        private void OnTestRunCompleted()
        {
            Application.SynchronizationContext.Post(_ =>
                {
                    foreach (var ts in suite_elements.Values)
                    {
                        // Recalc the status
                        ts.Refresh();
                    }
                    if (refreshViews != null)
                        refreshViews();
                }, null);
        }

        private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly IEnumerable<TElement> elements;

            public Grouping(TKey key, IEnumerable<TElement> elements)
            {
                Key = key;
                this.elements = elements;
            }

            public TKey Key { get; private set; }

            public IEnumerator<TElement> GetEnumerator()
            {
                return elements.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return elements.GetEnumerator();
            }
        }
    }
}