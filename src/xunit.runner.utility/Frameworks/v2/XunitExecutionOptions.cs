using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit.Abstractions;

namespace Xunit
{
    [Serializable]
    public class XunitExecutionOptions : TestFrameworkOptions
    {
        public XunitExecutionOptions() { }

        /// <inheritdoc/>
        protected XunitExecutionOptions(SerializationInfo info, StreamingContext context) : base(info, context) { }

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
