using System.CodeDom.Compiler;
using System.IO;

public class AcceptanceTestV1Assembly : AcceptanceTestAssembly
{
    private AcceptanceTestV1Assembly() { }

    protected override void AddStandardReferences(CompilerParameters parameters)
    {
        base.AddStandardReferences(parameters);

        parameters.ReferencedAssemblies.Add(Path.Combine(BasePath, "xunit.dll"));
    }

    public static AcceptanceTestV1Assembly Create(string code, params string[] references)
    {
        var assembly = new AcceptanceTestV1Assembly();
        assembly.Compile(code, references);
        return assembly;
    }
}