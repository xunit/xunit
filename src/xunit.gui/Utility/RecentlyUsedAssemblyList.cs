using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Xunit.Gui
{
    public class RecentlyUsedAssemblyList : IEnumerable<RecentlyUsedAssembly>
    {
        const string REGISTRY_SUBKEY_RECENTASSEMBLIES = "RecentAssemblies";
        const string REGISTRY_VALUE_ASSEMBLYFILENAME = "AssemblyFilename";
        const string REGISTRY_VALUE_CONFIGFILENAME = "ConfigFilename";

        List<RecentlyUsedAssembly> assemblies;
        int maxItems;

        public RecentlyUsedAssemblyList() : this(5) { }

        public RecentlyUsedAssemblyList(int maxItems)
        {
            this.maxItems = maxItems;
            assemblies = LoadAssemblyList();
        }

        public void Add(string assemblyFilename, string configFilename)
        {
            RecentlyUsedAssembly assembly = new RecentlyUsedAssembly
            {
                AssemblyFilename = assemblyFilename,
                ConfigFilename = configFilename
            };

            // Insert at the front
            assemblies.Insert(0, assembly);

            // Delete if you find a duplicate
            for (int index = 1; index < assemblies.Count; ++index)
                if (assemblies[index].Equals(assembly))
                    assemblies.RemoveAt(index);

            // Delete off the end if you have too many
            if (assemblies.Count > maxItems)
                assemblies.RemoveAt(assemblies.Count - 1);

            // Save
            SaveAssemblyList(assemblies);
        }

        public static void ClearAssemblyList()
        {
            try
            {
                using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
                    xunitKey.DeleteSubKeyTree(REGISTRY_SUBKEY_RECENTASSEMBLIES);
            }
            catch (ArgumentException) { }
        }

        public IEnumerator<RecentlyUsedAssembly> GetEnumerator()
        {
            return assemblies.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static List<RecentlyUsedAssembly> LoadAssemblyList()
        {
            var items = new List<RecentlyUsedAssembly>();

            using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
            using (var recentKey = xunitKey.CreateSubKey(REGISTRY_SUBKEY_RECENTASSEMBLIES))
                for (int index = 0; ; ++index)
                    using (var itemKey = recentKey.OpenSubKey(index.ToString()))
                    {
                        if (itemKey == null)
                            break;

                        if (itemKey != null)
                        {
                            string assemblyFilename = (string)itemKey.GetValue(REGISTRY_VALUE_ASSEMBLYFILENAME);
                            string configFilename = (string)itemKey.GetValue(REGISTRY_VALUE_CONFIGFILENAME);

                            if (assemblyFilename != null)
                                items.Add(new RecentlyUsedAssembly
                                {
                                    AssemblyFilename = assemblyFilename,
                                    ConfigFilename = configFilename
                                });
                        }
                    }

            return items;
        }

        public static void SaveAssemblyList(List<RecentlyUsedAssembly> items)
        {
            ClearAssemblyList();

            using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
            using (var recentKey = xunitKey.CreateSubKey(REGISTRY_SUBKEY_RECENTASSEMBLIES))
                for (int index = 0; index < items.Count; ++index)
                    using (var itemKey = recentKey.CreateSubKey(index.ToString()))
                    {
                        RecentlyUsedAssembly item = items[index];

                        itemKey.SetValue(REGISTRY_VALUE_ASSEMBLYFILENAME, item.AssemblyFilename);

                        if (!String.IsNullOrEmpty(item.ConfigFilename))
                            itemKey.SetValue(REGISTRY_VALUE_CONFIGFILENAME, item.ConfigFilename);
                        else if (itemKey.GetValue(REGISTRY_VALUE_CONFIGFILENAME) != null)
                            itemKey.DeleteValue(REGISTRY_VALUE_CONFIGFILENAME);
                    }
        }
    }
}