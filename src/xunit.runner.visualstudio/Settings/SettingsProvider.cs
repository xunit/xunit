using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;

namespace Xunit.Runner.VisualStudio
{
    public static class SettingsProvider
    {
        private const string REGVALUE_NameDisplay = "NameDisplay";
        private const string REGVALUE_ParallelizeAssemblies = "ParallelizeAssemblies";

        public static XunitVisualStudioSettings Load()
        {
            var result = new XunitVisualStudioSettings();

            using (var software = Registry.CurrentUser.OpenSubKey("Software", writable: true))
            using (var outercurve = software.CreateOrOpen("Outercurve Foundation"))
            using (var xunit = outercurve.CreateOrOpen("xUnit.net"))
            using (var vsrunner = xunit.CreateOrOpen("Visual Studio Test Plugin"))
            {
                result.NameDisplay = vsrunner.GetValue<string>(REGVALUE_NameDisplay, NameDisplay.Short.ToString()).ToEnum<NameDisplay>();
                result.ParallelizeAssemblies = vsrunner.GetValue<int>(REGVALUE_ParallelizeAssemblies) == 0 ? false : true;
            }

            return result;
        }

        public static void Save(XunitVisualStudioSettings settings)
        {
            using (var software = Registry.CurrentUser.OpenSubKey("Software", writable: true))
            using (var outercurve = software.CreateOrOpen("Outercurve Foundation"))
            using (var xunit = outercurve.CreateOrOpen("xUnit.net"))
            using (var vsrunner = xunit.CreateOrOpen("Visual Studio Test Plugin"))
            {
                vsrunner.SetValue(REGVALUE_NameDisplay, settings.NameDisplay.ToString());
                vsrunner.SetValue(REGVALUE_ParallelizeAssemblies, settings.ParallelizeAssemblies ? 1 : 0);
            }
        }

        static RegistryKey CreateOrOpen(this RegistryKey parent, string keyName)
        {
            return parent.OpenSubKey(keyName, writable: true) ?? parent.CreateSubKey(keyName);
        }

        static T GetValue<T>(this RegistryKey key, string name, T defaultValue = default(T))
        {
            return (T)key.GetValue(name, defaultValue);
        }

        static T ToEnum<T>(this string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}
