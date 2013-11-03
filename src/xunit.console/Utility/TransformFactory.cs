//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.IO;
//using System.Reflection;

//namespace Xunit.ConsoleClient
//{
//    public class TransformFactory
//    {
//        string executablePath;
//        static TransformFactory instance = new TransformFactory();

//        protected TransformFactory()
//        {
//            string assemblyCodeBase = Assembly.GetExecutingAssembly().CodeBase;
//            executablePath = Path.GetDirectoryName(new Uri(assemblyCodeBase).LocalPath);
//            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
//            Config = (XunitConsoleConfigurationSection)config.GetSection("xunit") ?? new XunitConsoleConfigurationSection();
//        }

//        protected XunitConsoleConfigurationSection Config { get; set; }

//        protected virtual bool FileExists(string filename)
//        {
//            return File.Exists(filename);
//        }

//        public static List<IResultXmlTransform> GetAssemblyTransforms(XunitProjectAssembly assembly)
//        {
//            return instance.GetTransforms(assembly);
//        }

//        public List<IResultXmlTransform> GetTransforms(XunitProjectAssembly assembly)
//        {
//            List<IResultXmlTransform> results = new List<IResultXmlTransform>();

//            foreach (var output in assembly.Output)
//            {
//                if (output.Key == "xml")
//                    results.Add(new NullTransformer(output.Value));
//                else
//                {
//                    TransformConfigurationElement configItem = GetTransformConfigItem(output.Key);
//                    if (configItem == null)
//                        throw new ArgumentException(String.Format("unknown output transform: {0}", output.Key));

//                    string xslFilename = Path.Combine(executablePath, configItem.XslFile);
//                    if (!FileExists(xslFilename))
//                        throw new ArgumentException(String.Format("cannot find transform XSL file '{0}' for transform '{1}'", xslFilename, output.Key));

//                    results.Add(new XslStreamTransformer(xslFilename, output.Value));
//                }
//            }

//            return results;
//        }

//        TransformConfigurationElement GetTransformConfigItem(string optionName)
//        {
//            foreach (TransformConfigurationElement transform in Config.Transforms)
//                if (transform.CommandLine.ToLowerInvariant() == optionName)
//                    return transform;

//            return null;
//        }

//        public static IEnumerable<TransformConfigurationElement> GetInstalledTransforms()
//        {
//            foreach (TransformConfigurationElement element in instance.Config.Transforms)
//                yield return element;
//        }
//    }
//}
