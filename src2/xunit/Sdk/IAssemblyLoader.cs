using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal interface, and is not intended to be called from end-user code.
    /// </summary>
    public interface IAssemblyLoader
    {
        /// <summary/>
        Assembly Load(string assemblyFileName);
    }
}
