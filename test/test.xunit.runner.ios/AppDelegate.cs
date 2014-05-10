using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xunit.Runners.UI;
using Xunit.Sdk;

namespace test.xunit.runner.ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations
        UIWindow window;
        TouchRunner runner;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);
            runner = new TouchRunner(window);

            // We need this to ensure the execution assembly is part of the app bundle
            runner.AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            
            // tests can be inside the main assembly
            runner.Add(Assembly.GetExecutingAssembly());
            // otherwise you need to ensure that the test assemblies will 
            // become part of the app bundle
       //     runner.Add(typeof(MonoTouchFixtures.Test.Test).Assembly);
#if false
			// you can use the default or set your own custom writer (e.g. save to web site and tweet it ;-)
			runner.Writer = new TcpTextWriter ("10.0.1.2", 16384);
			// start running the test suites as soon as the application is loaded
			runner.AutoStart = true;
			// crash the application (to ensure it's ended) and return to springboard
			runner.TerminateAfterExecution = true;
#endif
#if false
			// you can get NUnit[2-3]-style XML reports to the console or server like this
			// replace `null` (default to Console.Out) to a TcpTextWriter to send data to a socket server
			// replace `NUnit2XmlOutputWriter` with `NUnit3XmlOutputWriter` for NUnit3 format
			runner.Writer = new NUnitOutputTextWriter (runner, null, new NUnitLite.Runner.NUnit2XmlOutputWriter ());
			// the same AutoStart and TerminateAfterExecution can be used for build automation
#endif
            window.RootViewController = new UINavigationController(runner.GetViewController());

            // make the window visible
            window.MakeKeyAndVisible();

            return true;
        }
    }
}