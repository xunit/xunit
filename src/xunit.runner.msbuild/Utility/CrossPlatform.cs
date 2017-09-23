using System;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Xunit.Runner.MSBuild
{
    public class CrossPlatform
    {
#if NET452
        public static void Transform(IRunnerLogger logger, string outputDisplayName, string resourceName, XNode xml, ITaskItem outputFile)
        {
            var xmlTransform = new System.Xml.Xsl.XslCompiledTransform();

            using (var writer = XmlWriter.Create(outputFile.GetMetadata("FullPath"), new XmlWriterSettings { Indent = true }))
            using (var xsltReader = XmlReader.Create(typeof(xunit).Assembly.GetManifestResourceStream("xunit.runner.msbuild." + resourceName)))
            using (var xmlReader = xml.CreateReader())
            {
                xmlTransform.Load(xsltReader);
                xmlTransform.Transform(xmlReader, writer);
            }
        }

        public static Assembly LoadAssembly(string dllFile)
            => Assembly.LoadFile(dllFile);

        public static string Version => $"Desktop .NET {Environment.Version}";
#endif

#if NETCOREAPP1_0
        public static void Transform(IRunnerLogger logger, string outputDisplayName, string resourceName, XNode xml, ITaskItem outputFile)
            => logger.LogWarning($"Skipping '{outputDisplayName}=\"{outputFile.ItemSpec}\"' because XSL-T is not supported on .NET Core");

        public static Assembly LoadAssembly(string dllFile)
            => System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);

        public static string Version => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
#endif
    }
}
