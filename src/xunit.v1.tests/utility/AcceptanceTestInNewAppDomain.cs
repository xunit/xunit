using System.IO;
using System.Xml;

namespace TestUtility
{
    public abstract class AcceptanceTestInNewAppDomain
    {
        public XmlNode Execute(string code)
        {
            return Execute(code, null);
        }

        public XmlNode Execute(string code, string configFile, params string[] references)
        {
            using (MockAssembly mockAssembly = new MockAssembly())
            {
                mockAssembly.Compile(code, references);
                return mockAssembly.Run(configFile);
            }
        }

        public XmlNode ExecuteWithCustomAssemblyName(string code, string assemblyName)
        {
            using (MockAssembly mockAssembly = new MockAssembly(assemblyName))
            {
                string fullAssemblyName = Path.GetFullPath(assemblyName);
                string assemblyPath = Path.GetDirectoryName(fullAssemblyName);
                string xunitCopyFilename = Path.Combine(assemblyPath, "xunit.dll");
                File.Copy(mockAssembly.XunitDllFilename, xunitCopyFilename, true);

                try
                {
                    mockAssembly.Compile(code, null);
                    return mockAssembly.Run(null);
                }
                finally
                {
                    try
                    {
                        if (xunitCopyFilename != null && File.Exists(xunitCopyFilename))
                            File.Delete(xunitCopyFilename);
                    }
                    catch { } // Throws on Mono because of a lack of shadow copying
                }
            }
        }

        public XmlNode ExecuteWithReferences(string code, params string[] references)
        {
            return Execute(code, null, references);
        }
    }
}
