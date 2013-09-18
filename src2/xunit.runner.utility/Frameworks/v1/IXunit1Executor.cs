using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

namespace Xunit
{
    public interface IXunit1Executor : IDisposable
    {
        string TestFrameworkDisplayName { get; }

        void EnumerateTests(ICallbackEventHandler handler);
        void RunTests(string type, List<string> methods, ICallbackEventHandler handler);
    }
}