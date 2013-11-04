using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Represents an xUnit.net Test Project file (.xunit file)
    /// </summary>
    public class XunitProject
    {
        readonly List<XunitProjectAssembly> assemblies;
        string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitProject"/> class.
        /// </summary>
        public XunitProject()
        {
            assemblies = new List<XunitProjectAssembly>();
            Filters = new XunitFilters();
            Output = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the assemblies in the project.
        /// </summary>
        public IEnumerable<XunitProjectAssembly> Assemblies
        {
            get { return assemblies; }
        }

        /// <summary>
        /// Gets or set the filename of the project.
        /// </summary>
        public string Filename
        {
            get { return filename; }
            set { filename = Path.GetFullPath(value); }
        }

        /// <summary>
        /// Gets the filters applied to this project.
        /// </summary>
        public XunitFilters Filters { get; private set; }

        /// <summary>
        /// Gets or sets a flag which indicates if this project has been modified since
        /// the last time it was loaded or saved.
        /// </summary>
        public bool IsDirty { get; set; }

        /// <summary>
        /// Gets or sets the output filenames. The dictionary key is the type
        /// of the file to be output; the dictionary value is the filename to
        /// write the output to. Supported output types vary by runner (console
        /// output types can be seen by getting help from the console runner;
        /// MSBuild runner only supports 'html', 'xml', and 'xmlv1').
        /// </summary>
        public Dictionary<string, string> Output { get; set; }

        /// <summary>
        /// Adds an assembly to the project
        /// </summary>
        /// <param name="assembly">The assembly to be added</param>
        public void AddAssembly(XunitProjectAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            assemblies.Add(assembly);
            IsDirty = true;
        }

        static string GetRelativePath(string directory, string filename)
        {
            if (filename.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                return filename.Substring(directory.Length).TrimStart('\\');

            return filename;
        }

        /// <summary>
        /// Loads an xUnit.net Test Project file from disk.
        /// </summary>
        /// <param name="filename">The test project filename</param>
        public static XunitProject Load(string filename)
        {
            filename = Path.GetFullPath(filename);
            string directory = Path.GetDirectoryName(filename);
            var doc = new XmlDocument();
            var result = new XunitProject { Filename = filename };

            try
            {
                doc.Load(filename);
            }
            catch (XmlException)
            {
                throw new ArgumentException("The xUnit.net project file appears to be malformed.", "filename");
            }

            foreach (XmlNode assemblyNode in doc.SelectNodes("xunit2/assembly"))
            {
                var assembly = new XunitProjectAssembly
                {
                    AssemblyFilename = Path.GetFullPath(Path.Combine(directory, assemblyNode.Attributes["filename"].Value))
                };

                if (assemblyNode.Attributes["config-filename"] != null)
                    assembly.ConfigFilename = Path.GetFullPath(Path.Combine(directory, assemblyNode.Attributes["config-filename"].Value));
                if (assemblyNode.Attributes["shadow-copy"] != null)
                    assembly.ShadowCopy = Boolean.Parse(assemblyNode.Attributes["shadow-copy"].Value);

                result.assemblies.Add(assembly);
            }

            foreach (XmlNode outputNode in doc.SelectNodes("xunit2/output"))
                result.Output.Add(outputNode.Attributes["type"].Value,
                                  Path.GetFullPath(Path.Combine(directory, outputNode.Attributes["filename"].Value)));

            if (result.assemblies.Count == 0)
                throw new ArgumentException("The xUnit.net project file has no assemblies.", "filename");

            return result;
        }

        /// <summary>
        /// Removes assembly from the assembly list
        /// </summary>
        /// <param name="assembly">The assembly to be removed</param>
        public void RemoveAssembly(XunitProjectAssembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            assemblies.Remove(assembly);
            IsDirty = true;
        }

        /// <summary>
        /// Saves the xUnit.net Test Project file to disk using the project's filename.
        /// </summary>
        public void Save()
        {
            SaveAs(Filename);
        }

        /// <summary>
        /// Saves the xUnit.net Test Project file to disk using the provided filename.
        /// The projects filename is updated to match this new name.
        /// </summary>
        /// <param name="filename">The test project filename</param>
        public void SaveAs(string filename)
        {
            if (assemblies.Count == 0)
                throw new InvalidOperationException("Cannot save an empty project");

            filename = Path.GetFullPath(filename);
            string directory = Path.GetDirectoryName(filename);
            var doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?><xunit2></xunit2>");
            var xunit2Node = doc.SelectSingleNode("xunit2");

            foreach (var assembly in Assemblies)
            {
                var assemblyNode = doc.CreateElement("assembly");

                var filenameAttribute = doc.CreateAttribute("filename");
                filenameAttribute.Value = GetRelativePath(directory, assembly.AssemblyFilename);
                assemblyNode.Attributes.Append(filenameAttribute);

                if (!String.IsNullOrEmpty(assembly.ConfigFilename))
                {
                    var configFilenameAttribute = doc.CreateAttribute("config-filename");
                    configFilenameAttribute.Value = GetRelativePath(directory, assembly.ConfigFilename);
                    assemblyNode.Attributes.Append(configFilenameAttribute);
                }

                var shadowCopyAttribute = doc.CreateAttribute("shadow-copy");
                shadowCopyAttribute.Value = assembly.ShadowCopy.ToString().ToLowerInvariant();
                assemblyNode.Attributes.Append(shadowCopyAttribute);

                xunit2Node.AppendChild(assemblyNode);
            }

            foreach (var kvp in Output)
            {
                var outputElement = doc.CreateElement("output");

                var outputTypeAttribute = doc.CreateAttribute("type");
                outputTypeAttribute.Value = kvp.Key;
                outputElement.Attributes.Append(outputTypeAttribute);

                var outputFilenameAttribute = doc.CreateAttribute("filename");
                outputFilenameAttribute.Value = GetRelativePath(directory, kvp.Value);
                outputElement.Attributes.Append(outputFilenameAttribute);

                xunit2Node.AppendChild(outputElement);
            }

            doc.Save(filename);
            Filename = filename;
            IsDirty = false;
        }
    }
}
