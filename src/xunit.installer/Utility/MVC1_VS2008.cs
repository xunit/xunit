using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public class MVC1_VS2008 : IApplication
    {
        const string TEMPLATE_NAME_CSHARP = "XunitMvcTestProjectTemplate.cs.zip";
        const string TEMPLATE_NAME_VB = "XunitMvcTestProjectTemplate.vb.zip";

        public bool Enableable
        {
            get
            {
                using (RegistryKey key = OpenMvcKey())
                    return key != null;
            }
        }

        public bool Enabled
        {
            get
            {
                using (RegistryKey key = OpenMvcKey())
                {
                    if (key == null)
                        return false;

                    using (RegistryKey templateKey = key.OpenSubKey("xUnit.net"))
                        return templateKey != null;
                }
            }
        }

        public string PreRequisites
        {
            get { return "Visual Studio 2008 SP1\r\nASP.NET MVC 1.0"; }
        }

        public string XunitVersion
        {
            get
            {
                using (RegistryKey key = OpenMvcKey())
                using (RegistryKey csharpKey = key.OpenSubKey(@"xUnit.net\C#"))
                    return (string)csharpKey.GetValue("TestFrameworkName");
            }
        }

        public string Disable()
        {
            using (RegistryKey key = OpenMvcKey())
                key.DeleteSubKeyTree("xUnit.net");

            UninstallZipFiles(VisualStudio2008.GetTestProjectTemplatePath("CSharp"));
            UninstallZipFiles(VisualStudio2008.GetTestProjectTemplatePath("VisualBasic"));

            return null;
        }

        public string Enable()
        {
            string CSharpPath = VisualStudio2008.GetTestProjectTemplatePath("CSharp");
            if (!Directory.Exists(CSharpPath))
                Directory.CreateDirectory(CSharpPath);

            ResourceHelper.WriteResourceToFile("Xunit.Installer.Templates.MVC1-CS-VS2008.zip",
                                               Path.Combine(CSharpPath, TEMPLATE_NAME_CSHARP));

            string VBPath = VisualStudio2008.GetTestProjectTemplatePath("VisualBasic");
            if (!Directory.Exists(VBPath))
                Directory.CreateDirectory(VBPath);

            ResourceHelper.WriteResourceToFile("Xunit.Installer.Templates.MVC1-VB-VS2008.zip",
                                               Path.Combine(VBPath, TEMPLATE_NAME_VB));

            using (RegistryKey key = OpenMvcKey())
            using (RegistryKey templateKey = key.CreateSubKey("xUnit.net"))
            {
                using (RegistryKey csharpKey = templateKey.CreateSubKey("C#"))
                {
                    csharpKey.SetValue("AdditionalInfo", "http://xunit.codeplex.com/");
                    csharpKey.SetValue("Package", "");
                    csharpKey.SetValue("Path", @"CSharp\Test");
                    csharpKey.SetValue("Template", TEMPLATE_NAME_CSHARP);
                    csharpKey.SetValue("TestFrameworkName", "xUnit.net build " + Assembly.GetExecutingAssembly().GetName().Version);
                }
                using (RegistryKey vbKey = templateKey.CreateSubKey("VB"))
                {
                    vbKey.SetValue("AdditionalInfo", "http://xunit.codeplex.com/");
                    vbKey.SetValue("Package", "");
                    vbKey.SetValue("Path", @"VisualBasic\Test");
                    vbKey.SetValue("Template", TEMPLATE_NAME_VB);
                    vbKey.SetValue("TestFrameworkName", "xUnit.net build " + Assembly.GetExecutingAssembly().GetName().Version);
                }
            }

            return null;
        }

        static RegistryKey OpenMvcKey()
        {
            return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\9.0\MVC\TestProjectTemplates", true);
        }

        static void UninstallZipFiles(string templatePath)
        {
            foreach (string file in Directory.GetFiles(templatePath, "XunitMvcTestProjectTemplate.*"))
                File.Delete(Path.Combine(templatePath, file));

            foreach (string directory in Directory.GetDirectories(templatePath))
                UninstallZipFiles(Path.Combine(templatePath, directory));
        }
    }
}