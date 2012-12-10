using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public class TestResultMessage : LongLivedMarshalByRefObject, ITestResultMessage
    {
        public string TestDisplayName { get; set; }
        public decimal ExecutionTime { get; set; }
        public ITestCase TestCase { get; set; }
    }
}
