using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Framework;

namespace Xunit.Runner.MSBuild.Xslt
{
    public class CrossPlatform
    {
#if NET452
        public static void Transform(string resourceName, XNode xml, ITaskItem outputFile)
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
        {
            return Assembly.LoadFile(dllFile);
        }

        public static string Version => Environment.Version.ToString();
#endif

#if NETSTANDARD1_5
        public static void Transform(string resourceName, XNode xml, ITaskItem outputFile)
        {
            throw new Exception("Only xunit v2 is supported output type");
        }

        public static Assembly LoadAssembly(string dllFile)
        {
            return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(dllFile);
        }
        public static string Version => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
#endif
    }
}