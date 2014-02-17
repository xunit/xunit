using System;
using System.Collections.Generic;
using System.Linq;

namespace Xunit
{
    public class XunitExecutionOptions : TestFrameworkOptions
    {
        public bool DisableParallelization
        {
            get { return GetValue<bool>(TestOptionsNames.Execution.DisableParallelization, false); }
            set { SetValue(TestOptionsNames.Execution.DisableParallelization, value); }
        }

        public int MaxParallelThreads
        {
            get { return GetValue<int>(TestOptionsNames.Execution.MaxParallelThreads, 0); }
            set { SetValue(TestOptionsNames.Execution.MaxParallelThreads, value); }
        }
    }
}
