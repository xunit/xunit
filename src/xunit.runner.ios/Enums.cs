using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Xunit.Runner.iOS
{
    enum TestState
    {
        NotRun,
        Passed,
        Failed,
        Skipped
    }

    enum TestRunState
    {
        Success,
        Failure,
        Error,
        NoTests,
    }
}