using System;
using System.Collections.Generic;
using System.Threading;
using Xunit.Abstractions;

namespace Xunit.Sdk2
{
    public interface ITestFramework
    {
        IObservable<ITestCase> Find(IAssemblyInfo assembly, CancellationToken token);
        IObservable<ITestCase> Find(IAssemblyInfo assembly, ITypeInfo type, CancellationToken token);
        IObservable<ITestCase> Find(IAssemblyInfo assembly, ITypeInfo type, IMethodInfo method, CancellationToken token);
        IObservable<ITestCaseResult> Run(IEnumerable<ITestCase> testMethods, CancellationToken token);

        // Not just an observable of results, we also need to think about running
        // events, like "started", "finished", "here's some output", etc.

        // Write an implementation of IObserver<ITestCaseResult> that does the
        // casting and converts it into testing-friendly events (OnPass, OnFail, etc.)
    }
}
