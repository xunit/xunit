using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;

namespace Xunit.Gui
{
    public class RecentlyUsedProjectList : IEnumerable<string>
    {
        const string RECENT_PROJECTS_KEY_NAME = "RecentProjects";

        List<string> projects;
        int maxItems;

        public RecentlyUsedProjectList() : this(5) { }

        public RecentlyUsedProjectList(int maxItems)
        {
            this.maxItems = maxItems;
            projects = LoadProjectList();
        }

        public void Add(string projectFilename)
        {
            // Insert at the front
            projects.Insert(0, projectFilename);

            // Delete if you find a duplicate
            for (int index = 1; index < projects.Count; ++index)
                if (projects[index].Equals(projectFilename))
                    projects.RemoveAt(index);

            // Delete off the end if you have too many
            if (projects.Count > maxItems)
                projects.RemoveAt(projects.Count - 1);

            // Save
            SaveProjectList(projects);
        }

        public static void ClearProjectList()
        {
            try
            {
                using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
                    xunitKey.DeleteSubKeyTree(RECENT_PROJECTS_KEY_NAME);
            }
            catch (ArgumentException) { }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return projects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static List<string> LoadProjectList()
        {
            var result = new List<string>();

            using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
            using (var recentKey = xunitKey.CreateSubKey(RECENT_PROJECTS_KEY_NAME))
                for (int index = 0; ; ++index)
                {
                    string projectFilename = (string)recentKey.GetValue(index.ToString());
                    if (projectFilename == null)
                        break;

                    result.Add(projectFilename);
                }

            return result;
        }

        public static void SaveProjectList(List<string> projects)
        {
            using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
            using (var recentKey = xunitKey.CreateSubKey(RECENT_PROJECTS_KEY_NAME))
                for (int index = 0; index < projects.Count; ++index)
                    recentKey.SetValue(index.ToString(), projects[index]);
        }
    }
}