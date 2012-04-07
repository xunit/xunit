using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Represents an xUnit Test Project file (.xunit file)
    /// </summary>
    public class XunitProject
    {
        List<XunitProjectAssembly> assemblies;
        string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="XunitProject"/> class.
        /// </summary>
        public XunitProject()
        {
            assemblies = new List<XunitProjectAssembly>();
            Filters = new XunitFilters();
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
            XmlDocument doc = new XmlDocument();
            XunitProject result = new XunitProject { Filename = filename };

            try
            {
                doc.Load(filename);
            }
            catch (XmlException)
            {
                throw new ArgumentException("The xUnit.net project file appears to be malformed.", "filename");
            }

            foreach (XmlNode assemblyNode in doc.SelectNodes("xunit/assemblies/assembly"))
            {
                XunitProjectAssembly assembly = new XunitProjectAssembly
                {
                    AssemblyFilename = Path.GetFullPath(Path.Combine(directory, assemblyNode.Attributes["filename"].Value))
                };

                if (assemblyNode.Attributes["config-filename"] != null)
                    assembly.ConfigFilename = Path.GetFullPath(Path.Combine(directory, assemblyNode.Attributes["config-filename"].Value));
                if (assemblyNode.Attributes["shadow-copy"] != null)
                    assembly.ShadowCopy = Boolean.Parse(assemblyNode.Attributes["shadow-copy"].Value);

                foreach (XmlNode outputNode in assemblyNode.SelectNodes("output"))
                    assembly.Output.Add(outputNode.Attributes["type"].Value,
                                        Path.GetFullPath(Path.Combine(directory, outputNode.Attributes["filename"].Value)));

                result.assemblies.Add(assembly);
            }

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
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version='1.0' encoding='utf-8'?><xunit><assemblies/></xunit>");
            XmlNode assembliesNode = doc.SelectSingleNode("xunit/assemblies");

            foreach (XunitProjectAssembly assembly in Assemblies)
            {
                XmlElement assemblyNode = doc.CreateElement("assembly");

                XmlAttribute filenameAttribute = doc.CreateAttribute("filename");
                filenameAttribute.Value = GetRelativePath(directory, assembly.AssemblyFilename);
                assemblyNode.Attributes.Append(filenameAttribute);

                if (!String.IsNullOrEmpty(assembly.ConfigFilename))
                {
                    XmlAttribute configFilenameAttribute = doc.CreateAttribute("config-filename");
                    configFilenameAttribute.Value = GetRelativePath(directory, assembly.ConfigFilename);
                    assemblyNode.Attributes.Append(configFilenameAttribute);
                }

                XmlAttribute shadowCopyAttribute = doc.CreateAttribute("shadow-copy");
                shadowCopyAttribute.Value = assembly.ShadowCopy.ToString().ToLowerInvariant();
                assemblyNode.Attributes.Append(shadowCopyAttribute);

                foreach (KeyValuePair<string, string> kvp in assembly.Output)
                {
                    XmlElement outputElement = doc.CreateElement("output");
                    assemblyNode.AppendChild(outputElement);

                    XmlAttribute outputTypeAttribute = doc.CreateAttribute("type");
                    outputTypeAttribute.Value = kvp.Key;
                    outputElement.Attributes.Append(outputTypeAttribute);

                    XmlAttribute outputFilenameAttribute = doc.CreateAttribute("filename");
                    outputFilenameAttribute.Value = GetRelativePath(directory, kvp.Value);
                    outputElement.Attributes.Append(outputFilenameAttribute);
                }

                assembliesNode.AppendChild(assemblyNode);
            }

            doc.Save(filename);
            Filename = filename;
            IsDirty = false;
        }
    }
}