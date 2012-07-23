using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class XunitProjectTests
{
    public class AddAssembly
    {
        [Fact]
        public void NullAssemblyThrows()
        {
            XunitProject project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.AddAssembly(null));
        }

        [Fact]
        public void AddedAssemblyIsPartOfAssemblyList()
        {
            XunitProject project = new XunitProject();
            XunitProjectAssembly assembly = new XunitProjectAssembly();

            project.AddAssembly(assembly);

            Assert.Contains(assembly, project.Assemblies);
        }
    }

    public class Filename
    {
        [Fact]
        public void NullFilenameThrows()
        {
            XunitProject project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.Filename = null);
        }
    }

    public class IsDirty
    {
        [Fact]
        public void ProjectIsNotDirtyToStart()
        {
            XunitProject project = new XunitProject();

            Assert.False(project.IsDirty);
        }

        [Fact]
        public void ProjectIsDirtyWhenAddingAssembly()
        {
            XunitProject project = new XunitProject();
            XunitProjectAssembly assembly = new XunitProjectAssembly();

            project.AddAssembly(assembly);

            Assert.True(project.IsDirty);
        }

        [Fact]
        public void ProjectIsDirtyWhenRemovingAssembly()
        {
            XunitProject project = new XunitProject();
            XunitProjectAssembly assembly = new XunitProjectAssembly();
            project.AddAssembly(assembly);
            project.IsDirty = false;

            project.RemoveAssembly(assembly);

            Assert.True(project.IsDirty);
        }

        [Fact]
        public void ProjectIsMarkedCleanWhenSaved()
        {
            using (TempFile tempFile = new TempFile())
            {
                XunitProject project = new XunitProject();
                XunitProjectAssembly assembly = new XunitProjectAssembly { AssemblyFilename = @"C:\FooBar" };
                project.AddAssembly(assembly);

                project.SaveAs(tempFile.Filename);

                Assert.False(project.IsDirty);
            }
        }
    }

    public class Load
    {
        [Fact]
        public void NullFilenameThrows()
        {
            Assert.Throws<ArgumentNullException>(() => XunitProject.Load(null));
        }

        [Fact]
        public void InvalidFilenameThrows()
        {
            Assert.Throws<FileNotFoundException>(
                () => XunitProject.Load(Guid.NewGuid().ToString()));
        }

        [Fact]
        public void IllegalXmlFileThrows()
        {
            using (TempFile tempFile = new TempFile("<invalid>"))
                Assert.Throws<ArgumentException>(() => XunitProject.Load(tempFile.Filename));
        }

        [Fact]
        public void InvalidXmlFormatThrows()
        {
            using (TempFile tempFile = new TempFile("<invalid />"))
                Assert.Throws<ArgumentException>(() => XunitProject.Load(tempFile.Filename));
        }

        [Fact]
        public void AssemblyFilenameOnly()
        {
            string xml = @"<xunit>" + Environment.NewLine +
                         @"    <assemblies>" + Environment.NewLine +
                         @"        <assembly filename='C:\AssemblyFilename' />" + Environment.NewLine +
                         @"    </assemblies>" + Environment.NewLine +
                         @"</xunit>";

            using (TempFile tempFile = new TempFile(xml))
            {
                XunitProject project = XunitProject.Load(tempFile.Filename);

                Assert.Equal(tempFile.Filename, project.Filename);
                XunitProjectAssembly assembly = Assert.Single(project.Assemblies);
                Assert.Equal(@"C:\AssemblyFilename", assembly.AssemblyFilename);
                Assert.Null(assembly.ConfigFilename);
                Assert.True(assembly.ShadowCopy);
                Assert.Equal(0, assembly.Output.Count);
            }
        }

        [Fact]
        public void FilenamesAreRelativeToTheProjectLocation()
        {
            string xml = @"<xunit>" + Environment.NewLine +
                         @"    <assemblies>" + Environment.NewLine +
                         @"        <assembly filename='AssemblyFilename' config-filename='ConfigFilename'>" + Environment.NewLine +
                         @"            <output type='xml' filename='XmlFilename' />" + Environment.NewLine +
                         @"        </assembly>" + Environment.NewLine +
                         @"    </assemblies>" + Environment.NewLine +
                         @"</xunit>";

            XunitProject project;
            string directory;

            using (TempFile tempFile = new TempFile(xml))
            {
                directory = Path.GetDirectoryName(tempFile.Filename);
                project = XunitProject.Load(tempFile.Filename);
            }

            XunitProjectAssembly assembly = Assert.Single(project.Assemblies);
            Assert.Equal(Path.Combine(directory, "AssemblyFilename"), assembly.AssemblyFilename);
            Assert.Equal(Path.Combine(directory, "ConfigFilename"), assembly.ConfigFilename);
            KeyValuePair<string, string> output = Assert.Single(assembly.Output);
            Assert.Equal(Path.Combine(directory, "XmlFilename"), output.Value);
        }

        [Fact]
        public void LoadAcceptanceTest()
        {
            string xml = @"<xunit>" + Environment.NewLine +
                         @"    <assemblies>" + Environment.NewLine +
                         @"        <assembly filename='C:\AssemblyFilename' config-filename='C:\ConfigFilename' shadow-copy='false'>" + Environment.NewLine +
                         @"            <output type='xml' filename='C:\XmlFilename' />" + Environment.NewLine +
                         @"            <output type='nunit' filename='C:\NunitFilename' />" + Environment.NewLine +
                         @"            <output type='html' filename='C:\HtmlFilename' />" + Environment.NewLine +
                         @"        </assembly>" + Environment.NewLine +
                         @"        <assembly filename='C:\AssemblyFilename2' config-filename='C:\ConfigFilename2' shadow-copy='true'>" + Environment.NewLine +
                         @"            <output type='xml' filename='C:\XmlFilename2' />" + Environment.NewLine +
                         @"            <output type='nunit' filename='C:\NunitFilename2' />" + Environment.NewLine +
                         @"            <output type='html' filename='C:\HtmlFilename2' />" + Environment.NewLine +
                         @"        </assembly>" + Environment.NewLine +
                         @"    </assemblies>" + Environment.NewLine +
                         @"</xunit>";

            XunitProject project;

            using (TempFile tempFile = new TempFile(xml))
                project = XunitProject.Load(tempFile.Filename);

            List<XunitProjectAssembly> assemblies = new List<XunitProjectAssembly>(project.Assemblies);

            Assert.Equal(2, assemblies.Count());

            XunitProjectAssembly assembly1 = assemblies[0];
            Assert.Equal(@"C:\AssemblyFilename", assembly1.AssemblyFilename);
            Assert.Equal(@"C:\ConfigFilename", assembly1.ConfigFilename);
            Assert.False(assembly1.ShadowCopy);
            Assert.Equal(3, assembly1.Output.Count);
            Assert.Equal(@"C:\XmlFilename", assembly1.Output["xml"]);
            Assert.Equal(@"C:\NunitFilename", assembly1.Output["nunit"]);
            Assert.Equal(@"C:\HtmlFilename", assembly1.Output["html"]);

            XunitProjectAssembly assembly2 = assemblies[1];
            Assert.Equal(@"C:\AssemblyFilename2", assembly2.AssemblyFilename);
            Assert.Equal(@"C:\ConfigFilename2", assembly2.ConfigFilename);
            Assert.True(assembly2.ShadowCopy);
            Assert.Equal(3, assembly2.Output.Count);
            Assert.Equal(@"C:\XmlFilename2", assembly2.Output["xml"]);
            Assert.Equal(@"C:\NunitFilename2", assembly2.Output["nunit"]);
            Assert.Equal(@"C:\HtmlFilename2", assembly2.Output["html"]);
        }
    }

    public class RemoveAssembly
    {
        [Fact]
        public void NullAssemblyThrows()
        {
            XunitProject project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.RemoveAssembly(null));
        }

        [Fact]
        public void RemovesAssemblyFromAssembliesList()
        {
            XunitProject project = new XunitProject();
            XunitProjectAssembly assembly = new XunitProjectAssembly();
            project.AddAssembly(assembly);

            project.RemoveAssembly(assembly);

            Assert.DoesNotContain(assembly, project.Assemblies);
        }

        [Fact]
        public void UnknownAssemblyDoesNotThrow()
        {
            XunitProject project = new XunitProject();
            XunitProjectAssembly assembly = new XunitProjectAssembly();

            Assert.DoesNotThrow(() => project.RemoveAssembly(assembly));
        }
    }

    public class Save
    {
        [Fact]
        public void InvalidFilenameThrows()
        {
            XunitProject project = new XunitProject { Filename = @"C:\\" + Guid.NewGuid() + "\\" + Guid.NewGuid() };
            project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = "foo" });

            Assert.Throws<DirectoryNotFoundException>(() => project.Save());
        }

        [Fact]
        public void AssemblyFilenameOnly()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit>" + Environment.NewLine +
                                 @"  <assemblies>" + Environment.NewLine +
                                 @"    <assembly filename=""C:\AssemblyFilename"" shadow-copy=""true"" />" + Environment.NewLine +
                                 @"  </assemblies>" + Environment.NewLine +
                                 @"</xunit>";

            using (TempFile tempFile = new TempFile())
            {
                XunitProject project = new XunitProject { Filename = tempFile.Filename };
                project.AddAssembly(
                    new XunitProjectAssembly
                    {
                        AssemblyFilename = @"C:\AssemblyFilename"
                    });

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void FilenamesAreRelativeToTheProjectLocation()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit>" + Environment.NewLine +
                                 @"  <assemblies>" + Environment.NewLine +
                                 @"    <assembly filename=""C:\AssemblyFilename"" config-filename=""ConfigFilename"" shadow-copy=""true"" />" + Environment.NewLine +
                                 @"  </assemblies>" + Environment.NewLine +
                                 @"</xunit>";

            using (TempFile tempFile = new TempFile())
            {
                string directory = Path.GetDirectoryName(tempFile.Filename);
                XunitProject project = new XunitProject { Filename = tempFile.Filename };
                project.AddAssembly(
                    new XunitProjectAssembly
                    {
                        AssemblyFilename = @"C:\AssemblyFilename",
                        ConfigFilename = Path.Combine(directory, "ConfigFilename")
                    });

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void SaveAcceptanceTest()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit>" + Environment.NewLine +
                                 @"  <assemblies>" + Environment.NewLine +
                                 @"    <assembly filename=""C:\AssemblyFilename"" config-filename=""C:\ConfigFilename"" shadow-copy=""true"">" + Environment.NewLine +
                                 @"      <output type=""xml"" filename=""C:\XmlFilename"" />" + Environment.NewLine +
                                 @"      <output type=""html"" filename=""C:\HtmlFilename"" />" + Environment.NewLine +
                                 @"    </assembly>" + Environment.NewLine +
                                 @"    <assembly filename=""C:\AssemblyFilename2"" config-filename=""C:\ConfigFilename2"" shadow-copy=""false"">" + Environment.NewLine +
                                 @"      <output type=""xml"" filename=""C:\XmlFilename2"" />" + Environment.NewLine +
                                 @"      <output type=""html"" filename=""C:\HtmlFilename2"" />" + Environment.NewLine +
                                 @"    </assembly>" + Environment.NewLine +
                                 @"  </assemblies>" + Environment.NewLine +
                                 @"</xunit>";

            using (TempFile tempFile = new TempFile())
            {
                XunitProject project = new XunitProject { Filename = tempFile.Filename };
                XunitProjectAssembly assembly1 = new XunitProjectAssembly();
                assembly1.AssemblyFilename = @"C:\AssemblyFilename";
                assembly1.ConfigFilename = @"C:\ConfigFilename";
                assembly1.ShadowCopy = true;
                assembly1.Output.Add("xml", @"C:\XmlFilename");
                assembly1.Output.Add("html", @"C:\HtmlFilename");
                project.AddAssembly(assembly1);
                XunitProjectAssembly assembly2 = new XunitProjectAssembly();
                assembly2.AssemblyFilename = @"C:\AssemblyFilename2";
                assembly2.ConfigFilename = @"C:\ConfigFilename2";
                assembly2.ShadowCopy = false;
                assembly2.Output.Add("xml", @"C:\XmlFilename2");
                assembly2.Output.Add("html", @"C:\HtmlFilename2");
                project.AddAssembly(assembly2);

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void EmptyProjectThrows()
        {
            using (TempFile tempFile = new TempFile())
            {
                XunitProject project = new XunitProject { Filename = tempFile.Filename };
                Assert.Throws<InvalidOperationException>(() => project.Save());
            }
        }
    }

    public class SaveAs
    {
        [Fact]
        public void NullFilenameThrows()
        {
            XunitProject project = new XunitProject();
            project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = "foo" });

            Assert.Throws<ArgumentNullException>(() => project.SaveAs(null));
        }

        [Fact]
        public void FilenameIsUpdated()
        {
            using (TempFile tempFile = new TempFile())
            {
                XunitProject project = new XunitProject();
                XunitProjectAssembly assembly = new XunitProjectAssembly { AssemblyFilename = @"C:\FooBar" };
                project.AddAssembly(assembly);

                project.SaveAs(tempFile.Filename);

                Assert.Equal(tempFile.Filename, project.Filename);
            }
        }
    }

    class TempFile : IDisposable
    {
        public TempFile()
        {
            Filename = Path.GetTempFileName();
        }

        public TempFile(string contents)
            : this()
        {
            File.WriteAllText(Filename, contents);
        }

        public string Filename { get; private set; }

        public void Dispose()
        {
            File.Delete(Filename);
        }
    }
}