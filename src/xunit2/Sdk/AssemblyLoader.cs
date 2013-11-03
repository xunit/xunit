using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// This is an internal class, and is not intended to be called from end-user code.
    /// </summary>
    public class AssemblyLoader : IAssemblyLoader
    {
        /// <inheritdoc/>
        public Assembly Load(string assemblyFileName)
        {
            return Assembly.Load(AssemblyName.GetAssemblyName(assemblyFileName));
        }
    }
}
