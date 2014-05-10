using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Xunit.Runner.iOS
{
    interface ITestListener
    {
        void RecordResult(MonoTestResult result);
    }
}