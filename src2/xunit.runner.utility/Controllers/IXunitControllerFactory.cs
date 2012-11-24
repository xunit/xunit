namespace Xunit
{
    public interface IXunitControllerFactory
    {
        IXunitController Create(string assemblyFileName, string configFileName, bool shadowCopy);
    }
}
