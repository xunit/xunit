using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Xunit.Gui.Properties;

namespace Xunit.Gui
{
    public partial class RunnerForm : Form, ITestMethodRunnerCallback
    {
        XunitProject project = new XunitProject();
        MultiAssemblyTestEnvironment mate = new MultiAssemblyTestEnvironment();
        RecentlyUsedAssemblyList mruAssemblyList = new RecentlyUsedAssemblyList();
        RecentlyUsedProjectList mruProjectList = new RecentlyUsedProjectList();
        bool isCancelRequested = false;
        bool isCloseRequested = false;
        bool isRunning = false;
        Dictionary<TestMethod, ListViewItem> itemHash = new Dictionary<TestMethod, ListViewItem>();
        Dictionary<TestAssembly, FileWatcher> fileWatchers = new Dictionary<TestAssembly, FileWatcher>();
        List<TestAssembly> filterAssemblies = new List<TestAssembly>();
        MultiValueDictionary<string, string> filterTraits = new MultiValueDictionary<string, string>();
        List<TestAssembly> reloadRequested = new List<TestAssembly>();
        string filterSearchText;
        int methodCountPassed = 0;
        int methodCountFailed = 0;
        int methodCountSkipped = 0;
        int gapAssembly;
        int gapTrait;
        int gapTest;
        string windowTitle;
        string extendedWindowTitle;

        const int IMAGE_PASSED = 0;
        const int IMAGE_SKIPPED = 1;
        const int IMAGE_FAILED = 2;

        public RunnerForm()
        {
            InitializeComponent();

            windowTitle = Text;

            extendedWindowTitle = String.Format("{0} ({1}-bit .NET {2})",
                                                windowTitle,
                                                IntPtr.Size * 8,
                                                Environment.Version);

            statusIconList.Images.Add(Resources.Passed);
            statusIconList.Images.Add(Resources.Skipped);
            statusIconList.Images.Add(Resources.Failed);

            UpdateTestItemStatistics();
            UpdateAssemblyDynamicMenus();
            UpdateProjectDynamicMenus();

            passedToolStripButton.Image = Resources.Passed.ToBitmap();
            failedToolStripButton.Image = Resources.Failed.ToBitmap();
            skippedToolStripButton.Image = Resources.Skipped.ToBitmap();

            gapAssembly = listAssemblies.Width - columnAssembly.Width;
            gapTrait = listTraits.Width - columnTrait.Width;
            gapTest = listTests.Width - columnTest.Width;

            if (!DesignMode)
            {
                Preferences prefs = Preferences.Load();

                if (prefs != null)
                {
                    StartPosition = FormStartPosition.Manual;
                    Size = prefs.WindowSize;
                    Location = prefs.WindowLocation;

                    if (prefs.IsMaximized)
                        WindowState = FormWindowState.Maximized;

                    Show();

                    horizontalSplitter.SplitterDistance = prefs.HorizontalSplitterDistance;
                    verticalSplitter.SplitterDistance = prefs.VerticalSplitterDistance;
                }
            }
        }

        public RunnerForm(string projectFilename)
            : this()
        {
            LoadProject(projectFilename);
        }

        public RunnerForm(string[] assemblyFilenames)
            : this()
        {
            using (var splash = new LoaderForm())
            {
                splash.Show();
                splash.Update();

                foreach (string assemblyFilename in assemblyFilenames)
                    LoadAssembly(Path.GetFullPath(assemblyFilename), null);

                splash.Close();
            }
        }

        bool HasTests
        {
            get { return listTests.Items.Count > 0; }
        }

        void AddFileWatcher(TestAssembly testAssembly)
        {
            FileWatcher fileWatcher = new FileWatcher(testAssembly.AssemblyFilename);
            fileWatcher.Changed += (s, e) => ReloadAssembly(testAssembly);
            fileWatchers[testAssembly] = fileWatcher;
        }

        void AddTestAssembly(TestAssembly testAssembly)
        {
            ListViewItem lviAssembly = new ListViewItem
            {
                Text = Path.GetFileNameWithoutExtension(testAssembly.AssemblyFilename),
                ToolTipText = testAssembly.AssemblyFilename + Environment.NewLine +
                              "xunit.dll verison " + testAssembly.XunitVersion,
                Tag = testAssembly
            };

            AddFileWatcher(testAssembly);
            listAssemblies.Items.Add(lviAssembly);

            UpdateAssemblyFilter();
            UpdateTraitsList();
            UpdateTestList();
            UpdateAssemblyDynamicMenus();
        }

        void AddTestMethods(IEnumerable<TestMethod> testMethods)
        {
            listTests.BeginUpdate();

            foreach (TestMethod testMethod in testMethods)
            {
                ListViewItem lviMethod = new ListViewItem { Text = testMethod.DisplayName, Tag = testMethod };
                listTests.Items.Add(lviMethod);
                itemHash[testMethod] = lviMethod;
                UpdateTestItem(lviMethod, testMethod.RunStatus);
            }

            listTests.EndUpdate();

            UpdateMethodCount();
            UpdateRunState();
            UpdateTestItemStatistics();
        }

        ListViewItem FindTestAssemblyItem(TestAssembly testAssembly)
        {
            foreach (ListViewItem lvi in listAssemblies.Items)
                if (lvi.Tag == testAssembly)
                    return lvi;

            return null;
        }

        ListViewItem FindTestListItem(TestMethod testMethod)
        {
            ListViewItem item = null;
            itemHash.TryGetValue(testMethod, out item);
            return item;
        }

        string FormatOutput(string output)
        {
            string result = "";
            string trimmedOutput = output.Trim().Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");

            foreach (string line in trimmedOutput.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
                result += String.Format("  {0}", line) + Environment.NewLine;

            return result;
        }

        string GetRelativePath(string directory, string filename)
        {
            if (filename.StartsWith(directory, StringComparison.OrdinalIgnoreCase))
                return filename.Substring(directory.Length).TrimStart('\\');

            return filename;
        }

        void LoadAssembly(string assemblyFilename, string configFilename)
        {
            statusLabel.Text = "Loading " + assemblyFilename + " ...";

            passedToolStripButton.Checked = false;
            failedToolStripButton.Checked = false;
            skippedToolStripButton.Checked = false;

            try
            {
                TestAssembly testAssembly = mate.Load(assemblyFilename, configFilename, true);
                AddTestAssembly(testAssembly);
                mruAssemblyList.Add(assemblyFilename, configFilename);
                project.AddAssembly(
                    new XunitProjectAssembly
                    {
                        AssemblyFilename = assemblyFilename,
                        ConfigFilename = configFilename
                    });

                UpdateAssemblyDynamicMenus();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading assembly:\r\n\r\n" + ex.Message, windowTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                statusLabel.Text = "";
            }
        }

        void LoadProject(string filename)
        {
            var splash = new LoaderForm();

            try
            {
                splash.Show();
                splash.Update();

                UnloadAssemblies(listAssemblies.Items);
                listAssemblies.Update();

                project = XunitProject.Load(filename);
                mruProjectList.Add(filename);

                foreach (XunitProjectAssembly assembly in project.Assemblies)
                    AddTestAssembly(mate.Load(assembly.AssemblyFilename,
                                              assembly.ConfigFilename,
                                              assembly.ShadowCopy));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading project:\r\n\r\n" + ex.Message, windowTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                splash.Close();
                splash.Dispose();
            }

            UpdateAssemblyDynamicMenus();
            UpdateProjectDynamicMenus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (isRunning)
            {
                e.Cancel = true;
                isCloseRequested = true;
                isCancelRequested = true;
                UpdateRunState();
            }
            else
            {
                if (project.IsDirty)
                {
                    string message = project.Filename == null
                        ? "You have not yet saved a project file. Are you sure you want to exit?"
                        : "You have not saved the changes to your project file. Are you sure you want exit?";

                    if (MessageBox.Show(message, windowTitle,
                                        MessageBoxButtons.OKCancel,
                                        MessageBoxIcon.Warning,
                                        MessageBoxDefaultButton.Button2) != DialogResult.OK)
                        e.Cancel = true;
                }
            }

            base.OnClosing(e);
        }

        delegate void ReloadAssemblyDelegate(TestAssembly testAssembly);

        void ReloadAssembly(TestAssembly testAssembly)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new ReloadAssemblyDelegate(ReloadAssembly), testAssembly);
                return;
            }

            ListViewItem lvi = FindTestAssemblyItem(testAssembly);

            if (lvi != null)
            {
                if (isRunning)
                {
                    isCancelRequested = true;
                    reloadRequested.Add(testAssembly);
                }
                else
                {
                    ReloadAssemblies(new[] { lvi });
                    statusLabel.Text = "Reloaded " + testAssembly.AssemblyFilename + " at " + DateTime.Now.ToShortTimeString();
                }
            }
        }

        void ReloadAssemblies(IEnumerable assemblyListViewItems)
        {
            foreach (ListViewItem assemblyListViewItem in assemblyListViewItems)
            {
                TestAssembly testAssembly = (TestAssembly)assemblyListViewItem.Tag;
                string assemblyFilename = testAssembly.AssemblyFilename;
                string configFilename = testAssembly.ConfigFilename;

                RemoveFileWatcher(testAssembly);
                mate.Unload(testAssembly);

                testAssembly = mate.Load(assemblyFilename, configFilename);
                assemblyListViewItem.Tag = testAssembly;
                AddFileWatcher(testAssembly);
            }

            UpdateAssemblyFilter();
            UpdateTestList();
            UpdateTraitsList();

            progress.Status = ProgressControl.ProgressStatus.Unknown;
        }

        private void RemoveFileWatcher(TestAssembly testAssembly)
        {
            FileWatcher fileWatcher = fileWatchers[testAssembly];

            fileWatcher.Dispose();
            fileWatchers.Remove(testAssembly);
        }

        void SaveProject(string filename)
        {
            try
            {
                project.SaveAs(filename);
                mruProjectList.Add(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading project:\r\n\r\n" + ex.Message, windowTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            UpdateProjectDynamicMenus();
            UpdateRunState();
        }

        bool TestFilter(TestMethod testMethod)
        {
            if (!String.IsNullOrEmpty(filterSearchText))
                if (testMethod.MethodName.IndexOf(filterSearchText, StringComparison.InvariantCultureIgnoreCase) < 0 &&
                    testMethod.TestClass.TypeName.IndexOf(filterSearchText, StringComparison.InvariantCultureIgnoreCase) < 0 &&
                    testMethod.DisplayName.IndexOf(filterSearchText, StringComparison.InvariantCultureIgnoreCase) < 0)
                    return false;

            if (!isRunning)
            {
                if (passedToolStripButton.Checked && testMethod.RunStatus != TestStatus.Passed)
                    return false;
                if (failedToolStripButton.Checked && testMethod.RunStatus != TestStatus.Failed)
                    return false;
                if (skippedToolStripButton.Checked && testMethod.RunStatus != TestStatus.Skipped)
                    return false;
            }

            if (!filterAssemblies.Contains(testMethod.TestClass.TestAssembly))
                return false;

            if (filterTraits.Count == 0)
                return true;

            foreach (string name in testMethod.Traits.Keys)
                foreach (string value in testMethod.Traits[name])
                    if (filterTraits.Contains(name, value))
                        return true;

            return false;
        }

        void UnloadAssemblies(IEnumerable assemblyListViewItems)
        {
            foreach (ListViewItem assemblyListViewItem in assemblyListViewItems)
            {
                TestAssembly testAssembly = (TestAssembly)assemblyListViewItem.Tag;

                RemoveFileWatcher(testAssembly);
                listAssemblies.Items.Remove(assemblyListViewItem);
                mate.Unload(testAssembly);

                XunitProjectAssembly toRemove = null;

                foreach (XunitProjectAssembly assembly in project.Assemblies)
                {
                    if (assembly.AssemblyFilename == testAssembly.AssemblyFilename &&
                        assembly.ConfigFilename == testAssembly.ConfigFilename)
                    {
                        toRemove = assembly;
                        break;
                    }
                }

                if (toRemove != null)
                    project.RemoveAssembly(toRemove);
            }

            UpdateAssemblyFilter();
            UpdateTestList();
            UpdateTraitsList();
            UpdateAssemblyDynamicMenus();
        }

        void UpdateAssemblyDynamicMenus()
        {
            menuAssemblyUnload.DropDownItems.Clear();
            menuAssemblyRecent.DropDownItems.Clear();
            menuAssemblyReload.DropDownItems.Clear();

            int index = 1;

            foreach (RecentlyUsedAssembly assembly in mruAssemblyList)
            {
                string displayName = String.Format("&{0} {1}", index++, assembly.AssemblyFilename);
                ToolStripMenuItem recentItem = new ToolStripMenuItem(displayName);
                recentItem.Tag = assembly;
                recentItem.Click += (sender, e) => menuAssemblyRecentItem_Click(recentItem);
                menuAssemblyRecent.DropDownItems.Add(recentItem);
            }

            foreach (ListViewItem lvi in listAssemblies.Items)
            {
                TestAssembly assembly = (TestAssembly)lvi.Tag;

                ToolStripMenuItem closeItem = new ToolStripMenuItem(assembly.AssemblyFilename);
                closeItem.Tag = lvi;
                closeItem.Click += (sender, e) => menuAssemblyUnloadItem_Click(closeItem);
                menuAssemblyUnload.DropDownItems.Add(closeItem);

                ToolStripMenuItem reloadItem = new ToolStripMenuItem(assembly.AssemblyFilename);
                reloadItem.Tag = lvi;
                reloadItem.Click += (sender, e) => menuAssemblyReloadItem_Click(reloadItem);
                menuAssemblyReload.DropDownItems.Add(reloadItem);
            }

            UpdateRunState();
        }

        void UpdateAssemblyFilter()
        {
            IEnumerable items;

            if (listAssemblies.SelectedIndices.Count > 0)
                items = listAssemblies.SelectedItems;
            else
                items = listAssemblies.Items;

            filterAssemblies.Clear();
            foreach (ListViewItem item in items)
                filterAssemblies.Add((TestAssembly)item.Tag);
        }

        void UpdateMethodCount()
        {
            if (listTests.SelectedIndices.Count > 0)
            {
                testsGroupBox.Text = String.Format("Methods ({0} of {1})", listTests.SelectedIndices.Count, listTests.Items.Count);
                buttonRun.Text = "&Run Selected";
            }
            else
            {
                testsGroupBox.Text = String.Format("Methods ({0})", listTests.Items.Count);
                buttonRun.Text = "&Run All";
            }
        }

        void UpdateProjectDynamicMenus()
        {
            menuProjectRecent.DropDownItems.Clear();

            int index = 1;

            foreach (string project in mruProjectList)
            {
                string displayName = String.Format("&{0} {1}", index++, project);
                ToolStripMenuItem recentItem = new ToolStripMenuItem(displayName);
                recentItem.Tag = project;
                recentItem.Click += (sender, e) => menuProjectRecentItem_Click(recentItem);
                menuProjectRecent.DropDownItems.Add(recentItem);
            }

            UpdateRunState();
        }

        void UpdateRunState()
        {
            string title = project.Filename == null ? "Untitled" : Path.GetFileNameWithoutExtension(project.Filename);
            if (project.IsDirty)
                title += " *";

            Text = title + " - " + extendedWindowTitle;

            buttonRun.Enabled = !isRunning && HasTests;
            buttonCancel.Enabled = isRunning && !isCancelRequested;

            menuFileExit.Enabled = !isRunning;

            menuAssemblyOpen.Enabled = !isRunning;
            menuAssemblyRecent.Enabled = !isRunning && menuAssemblyRecent.DropDownItems.Count > 0;
            menuAssemblyUnload.Enabled = !isRunning && menuAssemblyUnload.DropDownItems.Count > 0;
            menuAssemblyReload.Enabled = !isRunning && menuAssemblyReload.DropDownItems.Count > 0;

            menuProjectOpen.Enabled = !isRunning;
            menuProjectRecent.Enabled = !isRunning && menuProjectRecent.DropDownItems.Count > 0;
            menuProjectClose.Enabled = !isRunning && (project.Filename != null || project.IsDirty);
            menuProjectSave.Enabled = !isRunning && project.Filename != null && project.IsDirty;
            menuProjectSaveAs.Enabled = !isRunning && listAssemblies.Items.Count > 0;

            popupMenuAssemblyUnload.Enabled = !isRunning;
            popupMenuAssemblyReload.Enabled = !isRunning;
        }

        void UpdateTestItem(TestMethod testMethod, TestStatus runStatus)
        {
            ListViewItem lvi;

            if (itemHash.TryGetValue(testMethod, out lvi))
                UpdateTestItem(lvi, runStatus);
        }

        void UpdateTestItem(ListViewItem listViewItem, TestStatus runStatus)
        {
            switch (listViewItem.StateImageIndex)
            {
                case IMAGE_PASSED:
                    methodCountPassed--;
                    break;

                case IMAGE_FAILED:
                    methodCountFailed--;
                    break;

                case IMAGE_SKIPPED:
                    methodCountSkipped--;
                    break;
            }

            if (runStatus == TestStatus.Passed)
            {
                methodCountPassed++;
                listViewItem.StateImageIndex = IMAGE_PASSED;
            }
            else if (runStatus == TestStatus.Failed)
            {
                methodCountFailed++;
                listViewItem.StateImageIndex = IMAGE_FAILED;
            }
            else if (runStatus == TestStatus.Skipped)
            {
                methodCountSkipped++;
                listViewItem.StateImageIndex = IMAGE_SKIPPED;
                listViewItem.ForeColor = Color.Gray;
            }
        }

        void UpdateTestItemStatistics()
        {
            passedToolStripButton.Text = " " + methodCountPassed + " ";
            passedToolStripButton.ToolTipText = "Passed: " + methodCountPassed;

            failedToolStripButton.Text = " " + methodCountFailed + " ";
            failedToolStripButton.ToolTipText = "Failed: " + methodCountFailed;

            skippedToolStripButton.Text = " " + methodCountSkipped + " ";
            skippedToolStripButton.ToolTipText = "Skipped: " + methodCountSkipped;
        }

        void UpdateTestList()
        {
            methodCountPassed = 0;
            methodCountFailed = 0;
            methodCountSkipped = 0;

            listTests.Items.Clear();
            itemHash.Clear();

            AddTestMethods(mate.EnumerateTestMethods(TestFilter));
        }

        void UpdateTraitsFilter()
        {
            filterTraits.Clear();

            if (listTraits.SelectedIndices.Count > 0)
                foreach (ListViewItem item in listTraits.SelectedItems)
                    filterTraits.AddValue(item.Group.Name, item.Text);
        }

        void UpdateTraitsList()
        {
            MultiValueDictionary<string, string> traits = mate.EnumerateTraits();

            listTraits.BeginUpdate();
            listTraits.Items.Clear();
            listTraits.Groups.Clear();

            foreach (string name in traits.Keys)
            {
                ListViewGroup group = new ListViewGroup(name, HorizontalAlignment.Left);
                group.Name = name;
                listTraits.Groups.Add(group);

                foreach (string value in traits[name])
                    listTraits.Items.Add(new ListViewItem(value, group));
            }

            listTraits.EndUpdate();

            UpdateTraitsFilter();
        }

        // Event handlers

        void buttonCancel_Click(object sender, EventArgs e)
        {
            isCancelRequested = true;
            UpdateRunState();
        }

        void buttonRun_Click(object sender, EventArgs e)
        {
            IEnumerable items;

            if (listTests.SelectedIndices.Count > 0)
                items = listTests.SelectedItems;
            else
                items = listTests.Items;

            List<TestMethod> testMethods = new List<TestMethod>();

            foreach (ListViewItem item in items)
                testMethods.Add((TestMethod)item.Tag);

            progress.Value = 0;
            progress.Maximum = testMethods.Count;
            progress.Status = ProgressControl.ProgressStatus.Passing;
            reloadRequested.Clear();

            Thread thread = new Thread(() =>
            {
                RunStart();
                mate.Run(testMethods, this);
                RunFinished();
            });

            thread.Start();
        }

        void failedToolStripButton_Click(object sender, EventArgs e)
        {
            passedToolStripButton.Checked = false;
            skippedToolStripButton.Checked = false;

            UpdateTestList();
        }

        void listAssemblies_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem lvi in listAssemblies.Items)
                    lvi.Selected = true;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }

        void listAssemblies_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listAssemblies.SelectedItems.Count > 0)
                assemblyContextMenu.Show(listAssemblies, new Point(e.X, e.Y));
        }

        void listAssemblies_Resize(object sender, EventArgs e)
        {
            listAssemblies.BeginUpdate();
            columnAssembly.Width = listAssemblies.Width - gapAssembly;
            listAssemblies.EndUpdate();
        }

        void listAssemblies_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAssemblyFilter();
            UpdateTestList();
        }

        void listTests_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                foreach (ListViewItem lvi in listTests.Items)
                    lvi.Selected = true;

                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }

        void listTests_Resize(object sender, EventArgs e)
        {
            listTests.BeginUpdate();
            columnTest.Width = listTests.Width - gapTest;
            listTests.EndUpdate();
        }

        void listTests_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateMethodCount();
        }

        void listTraits_Resize(object sender, EventArgs e)
        {
            listTraits.BeginUpdate();
            columnTrait.Width = listTraits.Width - gapTrait;
            listTraits.EndUpdate();
        }

        void listTraits_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTraitsFilter();
            UpdateTestList();
        }

        void menuAssemblyOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select Test Assembly",
                Multiselect = false,
                Filter = "Libraries (*.dll)|*.dll|Executables (*.exe)|*.exe|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
                LoadAssembly(dlg.FileName, null);
        }

        void menuAssemblyRecentItem_Click(ToolStripMenuItem item)
        {
            RecentlyUsedAssembly assembly = (RecentlyUsedAssembly)item.Tag;
            LoadAssembly(assembly.AssemblyFilename, assembly.ConfigFilename);
        }

        void menuAssemblyReloadItem_Click(ToolStripMenuItem item)
        {
            ListViewItem assembly = (ListViewItem)item.Tag;
            ReloadAssemblies(new[] { assembly });
        }

        void menuAssemblyUnloadItem_Click(ToolStripMenuItem item)
        {
            ListViewItem assembly = (ListViewItem)item.Tag;
            UnloadAssemblies(new[] { assembly });
        }

        void menuFileExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        void menuProjectClose_Click(object sender, EventArgs e)
        {
            if (project.IsDirty)
                if (MessageBox.Show("Are you sure you want to close this project? You have unsaved changes.", windowTitle,
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                    return;

            UnloadAssemblies(listAssemblies.Items);

            project = new XunitProject();

            UpdateRunState();
        }

        void menuProjectOpen_Click(object sender, EventArgs e)
        {
            if (project.IsDirty)
                if (MessageBox.Show("Are you sure you want to open a new project? You have unsaved changes.", windowTitle,
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                    return;

            OpenFileDialog dlg = new OpenFileDialog
            {
                Title = "Select Test Project",
                Multiselect = false,
                Filter = "xUnit.net Project files (*.xunit)|*.xunit|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
                LoadProject(dlg.FileName);
        }

        void menuProjectRecentItem_Click(ToolStripMenuItem item)
        {
            if (project.IsDirty)
                if (MessageBox.Show("Are you sure you want to open a new project? You have unsaved changes.", windowTitle,
                                    MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.OK)
                    return;

            string projectFilename = (string)item.Tag;
            LoadProject(projectFilename);
        }

        void menuProjectSave_Click(object sender, EventArgs e)
        {
            SaveProject(project.Filename);
        }

        void menuProjectSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog
            {
                Title = "Save Test Project",
                Filter = "xUnit.net Project files (*.xunit)|*.xunit|All files (*.*)|*.*"
            };

            if (dlg.ShowDialog(this) == DialogResult.OK)
                SaveProject(dlg.FileName);
        }

        void passedToolStripButton_Click(object sender, EventArgs e)
        {
            failedToolStripButton.Checked = false;
            skippedToolStripButton.Checked = false;

            UpdateTestList();
        }

        void popupMenuAssemblyReloadItem_Click(object sender, EventArgs e)
        {
            ReloadAssemblies(listAssemblies.SelectedItems);
        }

        void popupMenuAssemblyUnloadItem_Click(object sender, EventArgs e)
        {
            UnloadAssemblies(listAssemblies.SelectedItems);
        }

        void RunnerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool normalWindowState = WindowState == FormWindowState.Normal;

            new Preferences
            {
                IsMaximized = WindowState == FormWindowState.Maximized,
                WindowSize = normalWindowState ? Size : RestoreBounds.Size,
                WindowLocation = normalWindowState ? Location : RestoreBounds.Location,
                HorizontalSplitterDistance = horizontalSplitter.SplitterDistance,
                VerticalSplitterDistance = verticalSplitter.SplitterDistance
            }.Save();
        }

        void skippedToolStripButton_Click(object sender, EventArgs e)
        {
            passedToolStripButton.Checked = false;
            failedToolStripButton.Checked = false;

            UpdateTestList();
        }

        void textSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                textSearch.SelectAll();
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
            else
                base.OnKeyDown(e);
        }

        void textSearch_TextChanged(object sender, EventArgs e)
        {
            filterSearchText = textSearch.Text;
            UpdateTestList();
        }

        // TODO: Extract into its own class?
        // ITestMethodRunnerCallback implementation

        int testsTotal = 0;
        int testsFailed = 0;
        int testsSkipped = 0;
        double testsDuration = 0.0;

        delegate void AssemblyFinishedDelegate(TestAssembly testAssembly, int total, int failed, int skipped, double time);

        public void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new AssemblyFinishedDelegate(AssemblyFinished), testAssembly, total, failed, skipped, time);
                return;
            }

            testsTotal += total;
            testsFailed += failed;
            testsSkipped += skipped;
            testsDuration += time;
        }

        public void AssemblyStart(TestAssembly testAssembly)
        {
        }

        public bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
        {
            return true;
        }

        delegate void ExceptionThrownDelegate(TestAssembly testAssembly, Exception exception);

        public void ExceptionThrown(TestAssembly testAssembly, Exception exception)
        {
            if (InvokeRequired)
            {
                Delegate del = new ExceptionThrownDelegate(ExceptionThrown);
                BeginInvoke(del, testAssembly, exception);
                return;
            }

            textResults.Text += exception.ToString() + Environment.NewLine + Environment.NewLine;
        }

        delegate void RunStartDelegate();

        public void RunStart()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RunStartDelegate(RunStart));
                return;
            }

            testsTotal = 0;
            testsFailed = 0;
            testsSkipped = 0;
            testsDuration = 0.0;

            textResults.Text = "";

            isRunning = true;
            isCancelRequested = false;
            UpdateRunState();
            UpdateTestList();

            passedToolStripButton.Enabled = false;
            failedToolStripButton.Enabled = false;
            skippedToolStripButton.Enabled = false;
        }

        delegate void RunFinishedDelegate();

        public void RunFinished()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new RunFinishedDelegate(RunFinished));
                return;
            }

            isRunning = false;
            UpdateTestList();
            UpdateRunState();

            passedToolStripButton.Enabled = true;
            failedToolStripButton.Enabled = true;
            skippedToolStripButton.Enabled = true;

            if (isCancelRequested)
                progress.Status = ProgressControl.ProgressStatus.Cancelled;
            else
                progress.Value = progress.Maximum;

            statusLabel.Text = string.Format("Total tests: {0}, Failures: {1}, Skipped: {2}, Time: {3} seconds",
                                             testsTotal, testsFailed, testsSkipped, testsDuration.ToString("0.000"));

            if (isCloseRequested)
                Close();
            else
                foreach (TestAssembly testAssembly in reloadRequested)
                    ReloadAssembly(testAssembly);
        }

        void TestFailed(TestFailedResult testResult)
        {
            progress.Status = ProgressControl.ProgressStatus.Failing;

            string result = testResult.DisplayName + " : " + testResult.ExceptionMessage + Environment.NewLine;

            if (!string.IsNullOrEmpty(testResult.ExceptionStackTrace))
                result += "Stack Trace:" + Environment.NewLine + testResult.ExceptionStackTrace + Environment.NewLine;

            if (!string.IsNullOrEmpty(testResult.Output))
                result += "Output:" + Environment.NewLine + FormatOutput(testResult.Output);

            textResults.Text += result + Environment.NewLine;
        }

        public bool TestFinished(TestMethod testMethod)
        {
            TestFinishedDispatch(testMethod, testMethod.RunStatus, testMethod.RunResults[testMethod.RunResults.Count - 1]);
            return !isCancelRequested;
        }

        delegate void TestFinishedDispatchDelegate(TestMethod testMethod, TestStatus runStatus, TestResult lastRunResult);

        void TestFinishedDispatch(TestMethod testMethod, TestStatus runStatus, TestResult lastRunResult)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TestFinishedDispatchDelegate(TestFinishedDispatch), testMethod, runStatus, lastRunResult);
                return;
            }

            if (lastRunResult is TestPassedResult)
                TestPassed(lastRunResult as TestPassedResult);
            else if (lastRunResult is TestFailedResult)
                TestFailed(lastRunResult as TestFailedResult);
            else
                TestSkipped(lastRunResult as TestSkippedResult);

            progress.Increment();

            UpdateTestItem(testMethod, runStatus);
            UpdateTestItemStatistics();
        }

        void TestPassed(TestPassedResult testResult)
        {
            if (!string.IsNullOrEmpty(testResult.Output))
                textResults.Text += "Output from " + testResult.DisplayName + ":" + Environment.NewLine
                                    + FormatOutput(testResult.Output) + Environment.NewLine;
        }

        void TestSkipped(TestSkippedResult testResult)
        {
            if (progress.Status != ProgressControl.ProgressStatus.Failing)
                progress.Status = ProgressControl.ProgressStatus.Skipping;

            textResults.Text += testResult.DisplayName + " : " + testResult.Reason + Environment.NewLine + Environment.NewLine;
        }

        delegate bool TestStartDelegate(TestMethod testMethod);

        public bool TestStart(TestMethod testMethod)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TestStartDelegate(TestStart), testMethod);
                return !isCancelRequested;
            }

            statusLabel.Text = "Running " + testMethod.DisplayName;
            return !isCancelRequested;
        }
    }
}