using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Xunit.Runners
{
    interface ITestListener
    {
        void RecordResult(MonoTestResult result);
    }
}