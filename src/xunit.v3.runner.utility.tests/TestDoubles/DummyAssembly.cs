using System.Reflection;

class DummyAssembly : Assembly
{
    readonly string location;

    public DummyAssembly(string location)
        => this.location = location;

    public override string Location => location;
}
