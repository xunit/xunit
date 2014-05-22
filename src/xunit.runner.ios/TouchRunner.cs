// TouchRunner.cs: MonoTouch.Dialog-based driver to run unit tests
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2013 Xamarin Inc.
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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MonoTouch;
using MonoTouch.Dialog;
using Xunit;
using Xunit.Runners;
using Xunit.Runners.Utilities;
using Xunit.Runners.Visitors;
#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
using UIKit;
using Constants = global::ObjCRuntime.Constants;
#else
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
#endif


namespace Xunit.Runners.UI
{
    public class TouchRunner : ITestListener
    {
        private readonly List<Assembly> assemblies = new List<Assembly>();
        private readonly Dictionary<MonoTestCase, TestCaseElement> case_elements = new Dictionary<MonoTestCase, TestCaseElement>();
        private readonly ManualResetEvent mre = new ManualResetEvent(false);
        private readonly Dictionary<string, TestSuiteElement> suite_elements = new Dictionary<string, TestSuiteElement>();
        private readonly Dictionary<string, TouchViewController> suites_dvc = new Dictionary<string, TouchViewController>();
        private readonly UIWindow window;
        private Assembly executionAssembly;
        private int failed;
        private int skipped;
        private int passed;
        private bool cancelled;
        private IEnumerable<IGrouping<string, MonoTestCase>> allTests;

        private readonly AsyncLock executionLock = new AsyncLock();

        [CLSCompliant(false)]
        public TouchRunner(UIWindow window)
        {
            if (window == null)
                throw new ArgumentNullException("window");

            this.window = window;
        }

        public bool AutoStart
        {
            get { return RunnerOptions.Current.AutoStart; }
            set { RunnerOptions.Current.AutoStart = value; }
        }

        public bool TerminateAfterExecution
        {
            get { return RunnerOptions.Current.TerminateAfterExecution; }
            set { RunnerOptions.Current.TerminateAfterExecution = value; }
        }

        [CLSCompliant(false)]
        public UINavigationController NavigationController
        {
            get { return (UINavigationController)window.RootViewController; }
        }

        private void OnTestRunCompleted()
        {
            window.BeginInvokeOnMainThread(
                () =>
                {
                    foreach(var ts in suite_elements.Values)
                    {
                        // Recalc the status
                        ts.Update();
                    }
                });
        }

        void ITestListener.RecordResult(MonoTestResult result)
        {
            window.BeginInvokeOnMainThread(
                () =>
                case_elements[result.TestCase].UpdateResult(result));


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
                var lines = stacktrace.Split(new char[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                    Writer.WriteLine("\t\t{0}", line);
            }
        }

        public void Add(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            assemblies.Add(assembly);
        }

        // This is here due to iOS AOT. We need the assm to be in the app dir
        public void AddExecutionAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            executionAssembly = assembly;
        }

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


        private static void TerminateWithSuccess()
        {
            var selector = new Selector("terminateWithSuccess");
            UIApplication.SharedApplication.PerformSelector(selector, UIApplication.SharedApplication, 0);
        }


        [CLSCompliant(false)]
        public UIViewController GetViewController()
        {
            var menu = new RootElement("Test Runner");

            var main = new Section("Loading test suites...");
            menu.Add(main);

            var options = new Section()
            {
                new StyledStringElement("Options", Options)
                {
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                },
                new StyledStringElement("Credits", Credits)
                {
                    Accessory = UITableViewCellAccessory.DisclosureIndicator
                }
            };
            menu.Add(options);

            // large unit tests applications can take more time to initialize
            // than what the iOS watchdog will allow them on devices
            ThreadPool.QueueUserWorkItem(delegate
            {
                allTests = DiscoverTestsInAssemblies();


                window.InvokeOnMainThread(
                    delegate
                    {
                        foreach (var group in allTests)
                        {
                            main.Add(SetupSource(group));
                        }


                        mre.Set();

                        main.Caption = null;
                        menu.Reload(main, UITableViewRowAnimation.Fade);

                        options.Insert(0, new StringElement("Run Everything", async () => await Run()));
                        menu.Reload(options, UITableViewRowAnimation.Fade);
                    });
                assemblies.Clear();
            });

            var dv = new DialogViewController(menu)
            {
                Autorotate = true
            };

            // AutoStart running the tests (with either the supplied 'writer' or the options)
            if (AutoStart)
            {
                ThreadPool.QueueUserWorkItem(delegate
                {
                    mre.WaitOne();
                    window.BeginInvokeOnMainThread(async delegate
                    {
                        await Run();
                        // optionally end the process, e.g. click "Touch.Unit" -> log tests results, return to springboard...
                        // http://stackoverflow.com/questions/1978695/uiapplication-sharedapplication-terminatewithsuccess-is-not-there
                        if (TerminateAfterExecution)
                            TerminateWithSuccess();
                    });
                });
            }
            return dv;
        }

        private Task Run()
        {
            return Run(allTests.SelectMany(t => t), "Run Everything");
        }

        private void Options()
        {
            NavigationController.PushViewController(RunnerOptions.Current.GetViewController(), true);
        }

        private void Credits()
        {
            var title = new MultilineElement("xUnit MonoTouch Runner\nCopyright 2014 Outercurve Foundation\nAll rights reserved.");
            title.Alignment = UITextAlignment.Center;

            var root = new RootElement("Credits")
            {
                new Section()
                {
                    title
                },
                new Section()
                {
                    new HtmlElement("About Xamarin", "http://www.xamarin.com"),
                    new HtmlElement("About MonoTouch", "http://ios.xamarin.com"),
                    new HtmlElement("About MonoTouch.Dialog", "https://github.com/migueldeicaza/MonoTouch.Dialog"),
                    new HtmlElement("About xUnit", "https://github.com/xunit/xunit"),
                    new HtmlElement("About Font Awesome", "http://fortawesome.github.com/Font-Awesome")
                }
            };

            var dv = new DialogViewController(root, true)
            {
                Autorotate = true
            };
            NavigationController.PushViewController(dv, true);
        }

        public void Show(string suite)
        {
            NavigationController.PushViewController(suites_dvc[suite], true);
        }

        private TestSuiteElement SetupSource(IGrouping<string, MonoTestCase> testSource)
        {
            
            var root = new RootElement("Tests");

            var elements = new List<TestCaseElement>();

            var section = new Section(testSource.Key);
            foreach (var test in testSource)
            {
                var ele = Setup(test);
                elements.Add(ele);
                section.Add(ele);
            }

            var tse = new TestSuiteElement(testSource.Key, elements, this);
            suite_elements.Add(testSource.Key, tse);


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

                                               suites_dvc[testSource.Key].Filter();
                                           });
                var options = new Section()
                {
                  allbtn
                };

                root.Add(options);
            }

            suites_dvc.Add(testSource.Key, new TouchViewController(root));
            return tse;
        }

        private TestCaseElement Setup(MonoTestCase test)
        {
            var tce = new TestCaseElement(test, this);
            case_elements.Add(test, tce);
            return tce;
        }

        Task RunTests(IEnumerable<IGrouping<string, MonoTestCase>> testCaseAccessor, Stopwatch stopwatch)
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

        ManualResetEvent RunTestsInAssemblyAsync(List<IDisposable> toDispose,
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

        void RunTestsInAssembly(List<IDisposable> toDispose,
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

        //public void TestStarted (ITestCase test)
        //{
        //    if (test is TestSuite) {
        //        Writer.WriteLine ();
        //        Writer.WriteLine (test.Name);
        //    }
        //}


        internal Task Run(MonoTestCase test)
        {
            return Run(new[] { test });
        }

        internal async Task Run(IEnumerable<MonoTestCase> tests, string message = null)
        {

            var stopWatch = Stopwatch.StartNew();

            var groups = tests.GroupBy(t => t.AssemblyFileName);
            using (await executionLock.LockAsync())
            {
                if(message == null)
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

            stopWatch.Stop();
        }

        #region writer

        public TextWriter Writer { get; set; }

        private static string UniqueIdentifier
        {
            get
            {
                var handle = UIDevice.CurrentDevice.Handle;
                if (UIDevice.CurrentDevice.RespondsToSelector(new Selector("uniqueIdentifier")))
                    return NSString.FromHandle(objc_msgSend(handle, Selector.GetHandle("uniqueIdentifier")));
                return "unknown";
            }
        }

        private static string SelectHostName(string[] names, int port)
        {
            if (names.Length == 0)
                return null;

            if (names.Length == 1)
                return names[0];

            var lock_obj = new object();
            string result = null;
            var failures = 0;

            using (var evt = new ManualResetEvent(false))
            {
                for (var i = names.Length - 1; i >= 0; i--)
                {
                    var name = names[i];
                    ThreadPool.QueueUserWorkItem((v) =>
                    {
                        try
                        {
                            var client = new TcpClient(name, port);
                            using (var writer = new StreamWriter(client.GetStream()))
                            {
                                writer.WriteLine("ping");
                            }
                            lock (lock_obj)
                            {
                                if (result == null)
                                    result = name;
                            }
                            evt.Set();
                        }
                        catch (Exception)
                        {
                            lock (lock_obj)
                            {
                                failures++;
                                if (failures == names.Length)
                                    evt.Set();
                            }
                        }
                    });
                }

                // Wait for 1 success or all failures
                evt.WaitOne();
            }

            return result;
        }

        public bool OpenWriter(string message)
        {
            RunnerOptions options = RunnerOptions.Current;
            DateTime now = DateTime.Now;
            // let the application provide it's own TextWriter to ease automation with AutoStart property
            if (Writer == null)
            {
                if (options.ShowUseNetworkLogger)
                {
                    var hostname = SelectHostName(options.HostName.Split(','), options.HostPort);

                    if (hostname != null)
                    {
                        Console.WriteLine("[{0}] Sending '{1}' results to {2}:{3}", now, message, hostname, options.HostPort);
                        try
                        {
                            Writer = new TcpTextWriter(hostname, options.HostPort);
                        }
                        catch (SocketException)
                        {
                            UIAlertView alert = new UIAlertView("Network Error",
                                String.Format("Cannot connect to {0}:{1}. Continue on console ?", hostname, options.HostPort),
                                null, "Cancel", "Continue");
                            int button = -1;
                            alert.Clicked += delegate(object sender, UIButtonEventArgs e)
                            {
                                button = (int)e.ButtonIndex;
                            };
                            alert.Show();
                            while (button == -1)
                                NSRunLoop.Current.RunUntil(NSDate.FromTimeIntervalSinceNow(0.5));
                            Console.WriteLine(button);
                            Console.WriteLine("[Host unreachable: {0}]", button == 0 ? "Execution cancelled" : "Switching to console output");
                            if (button == 0)
                                return false;
                            else
                                Writer = Console.Out;
                        }
                    }
                }
                else
                {
                    Writer = Console.Out;
                }
            }

            Writer.WriteLine("[Runner executing:\t{0}]", message);
            Writer.WriteLine("[MonoTouch Version:\t{0}]", Constants.Version);
            Writer.WriteLine("[GC:\t{0}{1}]", GC.MaxGeneration == 0 ? "Boehm" : "sgen",
                NSObject.IsNewRefcountEnabled() ? "+NewRefCount" : String.Empty);
            UIDevice device = UIDevice.CurrentDevice;
            Writer.WriteLine("[{0}:\t{1} v{2}]", device.Model, device.SystemName, device.SystemVersion);
            Writer.WriteLine("[Device Name:\t{0}]", device.Name);
            Writer.WriteLine("[Device UDID:\t{0}]", UniqueIdentifier);
            Writer.WriteLine("[Device Locale:\t{0}]", NSLocale.CurrentLocale.Identifier);
            Writer.WriteLine("[Device Date/Time:\t{0}]", now); // to match earlier C.WL output

            Writer.WriteLine("[Bundle:\t{0}]", NSBundle.MainBundle.BundleIdentifier);
            // FIXME: add data about how the app was compiled (e.g. ARMvX, LLVM, GC and Linker options)
            passed = 0;
            skipped = 0;
            failed = 0;
            return true;
        }

        [DllImport("/usr/lib/libobjc.dylib")]
        private static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        // Apple blacklisted `uniqueIdentifier` (for the appstore) but it's still 
        // something useful to have inside the test logs

        public void CloseWriter()
        {
            var total = passed + failed; // ignored are *not* run
            Writer.WriteLine("Tests run: {0} Passed: {1} Failed: {2} Skipped: {3}", total, passed, failed, skipped);

            Writer.Close();
            Writer = null;
        }

        #endregion

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