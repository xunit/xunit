#if NETCOREAPP

using System.Collections.Generic;
using System.Linq;
using Internal.Microsoft.Extensions.DependencyModel;

class TestableDependencyContext : DependencyContext
{
    static readonly TargetInfo DummyTargetInfo = new TargetInfo("framework", "runtime", "runtime-signature", isPortable: true);
    static readonly IEnumerable<CompilationLibrary> EmptyCompilationLibraries = Enumerable.Empty<CompilationLibrary>();

    public List<RuntimeFallbacks> InnerRuntimeGraph;
    public List<RuntimeLibrary> InnerRuntimeLibraries;

    public TestableDependencyContext(List<RuntimeLibrary> runtimeLibraries, List<RuntimeFallbacks> runtimeGraph)
        : base(DummyTargetInfo, CompilationOptions.Default, EmptyCompilationLibraries, runtimeLibraries, runtimeGraph)
    {
        InnerRuntimeLibraries = runtimeLibraries;
        InnerRuntimeGraph = runtimeGraph;
    }
}

#endif
