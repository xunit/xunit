using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xunit.Runners
{
    public enum TestState
    {
        NotRun,
        Passed,
        Failed,
        Skipped
    }


    public enum NameDisplay
    {
        Short = 1,
        Full = 2,
    }
}