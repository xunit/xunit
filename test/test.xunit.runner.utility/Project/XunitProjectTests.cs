using System;
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
            var project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.AddAssembly(null));
        }

        [Fact]
        public void AddedAssemblyIsPartOfAssemblyList()
        {
            var project = new XunitProject();
            var assembly = new XunitProjectAssembly();

            project.AddAssembly(assembly);

            Assert.Contains(assembly, project.Assemblies);
        }
    }

    public class Filename
    {
        [Fact]
        public void NullFilenameThrows()
        {
            var project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.Filename = null);
        }
    }

    public class IsDirty
    {
        [Fact]
        public void ProjectIsNotDirtyToStart()
        {
            var project = new XunitProject();

            Assert.False(project.IsDirty);
        }

        [Fact]
        public void ProjectIsDirtyWhenAddingAssembly()
        {
            var project = new XunitProject();
            var assembly = new XunitProjectAssembly();

            project.AddAssembly(assembly);

            Assert.True(project.IsDirty);
        }

        [Fact]
        public void ProjectIsDirtyWhenRemovingAssembly()
        {
            var project = new XunitProject();
            var assembly = new XunitProjectAssembly();
            project.AddAssembly(assembly);
            project.IsDirty = false;

            project.RemoveAssembly(assembly);

            Assert.True(project.IsDirty);
        }

        [Fact]
        public void ProjectIsMarkedCleanWhenSaved()
        {
            using (var tempFile = new TempFile())
            {
                var project = new XunitProject();
                var assembly = new XunitProjectAssembly { AssemblyFilename = @"C:\FooBar" };
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
            string xml = @"<xunit2>" + Environment.NewLine +
                         @"    <assembly filename='C:\AssemblyFilename' />" + Environment.NewLine +
                         @"</xunit2>";

            using (TempFile tempFile = new TempFile(xml))
            {
                var project = XunitProject.Load(tempFile.Filename);
                Assert.Equal(0, project.Output.Count);

                Assert.Equal(tempFile.Filename, project.Filename);
                var assembly = Assert.Single(project.Assemblies);
                Assert.Equal(@"C:\AssemblyFilename", assembly.AssemblyFilename);
                Assert.Null(assembly.ConfigFilename);
                Assert.True(assembly.ShadowCopy);
            }
        }

        [Fact]
        public void FilenamesAreRelativeToTheProjectLocation()
        {
            string xml = @"<xunit2>" + Environment.NewLine +
                         @"    <assembly filename='AssemblyFilename' config-filename='ConfigFilename' />" + Environment.NewLine +
                         @"    <output type='xml' filename='XmlFilename' />" + Environment.NewLine +
                         @"</xunit2>";

            XunitProject project;
            string directory;

            using (var tempFile = new TempFile(xml))
            {
                directory = Path.GetDirectoryName(tempFile.Filename);
                project = XunitProject.Load(tempFile.Filename);
            }

            var assembly = Assert.Single(project.Assemblies);
            Assert.Equal(Path.Combine(directory, "AssemblyFilename"), assembly.AssemblyFilename);
            Assert.Equal(Path.Combine(directory, "ConfigFilename"), assembly.ConfigFilename);
            var output = Assert.Single(project.Output);
            Assert.Equal(Path.Combine(directory, "XmlFilename"), output.Value);
        }

        [Fact]
        public void LoadAcceptanceTest()
        {
            string xml = @"<xunit2>" + Environment.NewLine +
                         @"    <assembly filename='C:\AssemblyFilename' config-filename='C:\ConfigFilename' shadow-copy='false' />" + Environment.NewLine +
                         @"    <assembly filename='C:\AssemblyFilename2' config-filename='C:\ConfigFilename2' shadow-copy='true' />" + Environment.NewLine +
                         @"    <output type='xml' filename='C:\XmlFilename' />" + Environment.NewLine +
                         @"    <output type='xmlv1' filename='C:\Xmlv1Filename' />" + Environment.NewLine +
                         @"    <output type='html' filename='C:\HtmlFilename' />" + Environment.NewLine +
                         @"</xunit2>";

            XunitProject project;

            using (var tempFile = new TempFile(xml))
                project = XunitProject.Load(tempFile.Filename);

            Assert.Collection(project.Assemblies,
                assembly =>
                {
                    Assert.Equal(@"C:\AssemblyFilename", assembly.AssemblyFilename);
                    Assert.Equal(@"C:\ConfigFilename", assembly.ConfigFilename);
                    Assert.False(assembly.ShadowCopy);
                },
                assembly =>
                {
                    Assert.Equal(@"C:\AssemblyFilename2", assembly.AssemblyFilename);
                    Assert.Equal(@"C:\ConfigFilename2", assembly.ConfigFilename);
                    Assert.True(assembly.ShadowCopy);
                }
            );
            Assert.Equal(@"C:\XmlFilename", project.Output["xml"]);
            Assert.Equal(@"C:\Xmlv1Filename", project.Output["xmlv1"]);
            Assert.Equal(@"C:\HtmlFilename", project.Output["html"]);
        }
    }

    public class RemoveAssembly
    {
        [Fact]
        public void NullAssemblyThrows()
        {
            var project = new XunitProject();

            Assert.Throws<ArgumentNullException>(() => project.RemoveAssembly(null));
        }

        [Fact]
        public void RemovesAssemblyFromAssembliesList()
        {
            var project = new XunitProject();
            var assembly = new XunitProjectAssembly();
            project.AddAssembly(assembly);

            project.RemoveAssembly(assembly);

            Assert.DoesNotContain(assembly, project.Assemblies);
        }

        [Fact]
        public void UnknownAssemblyDoesNotThrow()
        {
            var project = new XunitProject();
            var assembly = new XunitProjectAssembly();

            Assert.DoesNotThrow(() => project.RemoveAssembly(assembly));
        }
    }

    public class Save
    {
        [Fact]
        public void InvalidFilenameThrows()
        {
            var project = new XunitProject { Filename = @"C:\\" + Guid.NewGuid() + "\\" + Guid.NewGuid() };
            project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = "foo" });

            Assert.Throws<DirectoryNotFoundException>(() => project.Save());
        }

        [Fact]
        public void AssemblyFilenameOnly()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit2>" + Environment.NewLine +
                                 @"  <assembly filename=""C:\AssemblyFilename"" shadow-copy=""true"" />" + Environment.NewLine +
                                 @"</xunit2>";

            using (var tempFile = new TempFile())
            {
                var project = new XunitProject { Filename = tempFile.Filename };
                project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = @"C:\AssemblyFilename" });

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void FilenamesAreRelativeToTheProjectLocation()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit2>" + Environment.NewLine +
                                 @"  <assembly filename=""C:\AssemblyFilename"" config-filename=""ConfigFilename"" shadow-copy=""true"" />" + Environment.NewLine +
                                 @"</xunit2>";

            using (var tempFile = new TempFile())
            {
                var directory = Path.GetDirectoryName(tempFile.Filename);
                var project = new XunitProject { Filename = tempFile.Filename };
                project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = @"C:\AssemblyFilename", ConfigFilename = Path.Combine(directory, "ConfigFilename") });

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void SaveAcceptanceTest()
        {
            string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>" + Environment.NewLine +
                                 @"<xunit2>" + Environment.NewLine +
                                 @"  <assembly filename=""C:\AssemblyFilename"" config-filename=""C:\ConfigFilename"" shadow-copy=""true"" />" + Environment.NewLine +
                                 @"  <assembly filename=""C:\AssemblyFilename2"" config-filename=""C:\ConfigFilename2"" shadow-copy=""false"" />" + Environment.NewLine +
                                 @"  <output type=""xml"" filename=""C:\XmlFilename"" />" + Environment.NewLine +
                                 @"  <output type=""html"" filename=""C:\HtmlFilename"" />" + Environment.NewLine +
                                 @"</xunit2>";

            using (var tempFile = new TempFile())
            {
                var project = new XunitProject { Filename = tempFile.Filename };
                project.Output.Add("xml", @"C:\XmlFilename");
                project.Output.Add("html", @"C:\HtmlFilename");
                project.AddAssembly(new XunitProjectAssembly() { AssemblyFilename = @"C:\AssemblyFilename", ConfigFilename = @"C:\ConfigFilename", ShadowCopy = true });
                project.AddAssembly(new XunitProjectAssembly() { AssemblyFilename = @"C:\AssemblyFilename2", ConfigFilename = @"C:\ConfigFilename2", ShadowCopy = false });

                project.Save();

                Assert.Equal(expectedXml, File.ReadAllText(tempFile.Filename));
            }
        }

        [Fact]
        public void EmptyProjectThrows()
        {
            using (var tempFile = new TempFile())
            {
                var project = new XunitProject { Filename = tempFile.Filename };
                Assert.Throws<InvalidOperationException>(() => project.Save());
            }
        }
    }

    public class SaveAs
    {
        [Fact]
        public void NullFilenameThrows()
        {
            var project = new XunitProject();
            project.AddAssembly(new XunitProjectAssembly { AssemblyFilename = "foo" });

            Assert.Throws<ArgumentNullException>(() => project.SaveAs(null));
        }

        [Fact]
        public void FilenameIsUpdated()
        {
            using (var tempFile = new TempFile())
            {
                var project = new XunitProject();
                var assembly = new XunitProjectAssembly { AssemblyFilename = @"C:\FooBar" };
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
