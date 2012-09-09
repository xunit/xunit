using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface ITestFramework
    {
        IEnumerable<ITestCase> Find(IAssemblyInfo assembly);
        IEnumerable<ITestCase> Find(ITypeInfo type);

        IObservable<ITestCaseResult> Run(IEnumerable<ITestCase> testMethods, CancellationToken token = default(CancellationToken));

        // Not just an observable of results, we also need to think about running
        // events, like "started", "finished", "here's some output", etc.

        // Write an implementation of IObserver<ITestCaseResult> that does the
        // casting and converts it into testing-friendly events (OnPass, OnFail, etc.)
    }
}
