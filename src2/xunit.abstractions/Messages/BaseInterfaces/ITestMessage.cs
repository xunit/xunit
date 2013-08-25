using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xunit.Abstractions
{
    public interface ITestMessage : ITestCaseMessage
    {
        /// <summary>
        /// The display name of the test.
        /// </summary>
        string TestDisplayName { get; }
    }
}
