namespace Xunit.Installer
{
    public interface IApplication
    {
        bool Enableable { get; }
        bool Enabled { get; }
        string PreRequisites { get; }
        string XunitVersion { get; }

        string Disable();
        string Enable();
    }
}
