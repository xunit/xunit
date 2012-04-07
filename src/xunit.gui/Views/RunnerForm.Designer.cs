namespace Xunit.Gui
{
    partial class RunnerForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RunnerForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuFileExit = new System.Windows.Forms.ToolStripMenuItem();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAssemblyOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAssemblyRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuAssemblyUnload = new System.Windows.Forms.ToolStripMenuItem();
            this.menuAssemblyReload = new System.Windows.Forms.ToolStripMenuItem();
            this.projectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProjectOpen = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProjectRecent = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuProjectClose = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.menuProjectSave = new System.Windows.Forms.ToolStripMenuItem();
            this.menuProjectSaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.statusIconList = new System.Windows.Forms.ImageList(this.components);
            this.assemblyContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.popupMenuAssemblyUnload = new System.Windows.Forms.ToolStripMenuItem();
            this.popupMenuAssemblyReload = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.horizontalSplitter = new System.Windows.Forms.SplitContainer();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.listTraits = new System.Windows.Forms.ListView();
            this.columnTrait = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.listAssemblies = new System.Windows.Forms.ListView();
            this.columnAssembly = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.textSearch = new System.Windows.Forms.TextBox();
            this.verticalSplitter = new System.Windows.Forms.SplitContainer();
            this.testsGroupBox = new System.Windows.Forms.GroupBox();
            this.listTests = new System.Windows.Forms.ListView();
            this.columnTest = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusToolStrip = new System.Windows.Forms.ToolStrip();
            this.passedToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.failedToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.skippedToolStripButton = new System.Windows.Forms.ToolStripButton();
            this.progress = new Xunit.Gui.ProgressControl();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.textResults = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.assemblyContextMenu.SuspendLayout();
            this.statusBar.SuspendLayout();
            this.horizontalSplitter.Panel1.SuspendLayout();
            this.horizontalSplitter.Panel2.SuspendLayout();
            this.horizontalSplitter.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.verticalSplitter.Panel1.SuspendLayout();
            this.verticalSplitter.Panel2.SuspendLayout();
            this.verticalSplitter.SuspendLayout();
            this.testsGroupBox.SuspendLayout();
            this.statusToolStrip.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem1,
            this.fileToolStripMenuItem,
            this.projectToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(786, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem1
            // 
            this.fileToolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFileExit});
            this.fileToolStripMenuItem1.Name = "fileToolStripMenuItem1";
            this.fileToolStripMenuItem1.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem1.Text = "&File";
            // 
            // menuFileExit
            // 
            this.menuFileExit.Name = "menuFileExit";
            this.menuFileExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.menuFileExit.Size = new System.Drawing.Size(134, 22);
            this.menuFileExit.Text = "E&xit";
            this.menuFileExit.Click += new System.EventHandler(this.menuFileExit_Click);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuAssemblyOpen,
            this.menuAssemblyRecent,
            this.toolStripSeparator1,
            this.menuAssemblyUnload,
            this.menuAssemblyReload});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.fileToolStripMenuItem.Text = "&Assembly";
            // 
            // menuAssemblyOpen
            // 
            this.menuAssemblyOpen.Name = "menuAssemblyOpen";
            this.menuAssemblyOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuAssemblyOpen.Size = new System.Drawing.Size(155, 22);
            this.menuAssemblyOpen.Text = "&Open...";
            this.menuAssemblyOpen.Click += new System.EventHandler(this.menuAssemblyOpen_Click);
            // 
            // menuAssemblyRecent
            // 
            this.menuAssemblyRecent.Name = "menuAssemblyRecent";
            this.menuAssemblyRecent.Size = new System.Drawing.Size(155, 22);
            this.menuAssemblyRecent.Text = "R&ecent";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(152, 6);
            // 
            // menuAssemblyUnload
            // 
            this.menuAssemblyUnload.Name = "menuAssemblyUnload";
            this.menuAssemblyUnload.Size = new System.Drawing.Size(155, 22);
            this.menuAssemblyUnload.Text = "&Unload";
            // 
            // menuAssemblyReload
            // 
            this.menuAssemblyReload.Name = "menuAssemblyReload";
            this.menuAssemblyReload.Size = new System.Drawing.Size(155, 22);
            this.menuAssemblyReload.Text = "&Reload";
            // 
            // projectToolStripMenuItem
            // 
            this.projectToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuProjectOpen,
            this.menuProjectRecent,
            this.toolStripSeparator2,
            this.menuProjectClose,
            this.toolStripSeparator3,
            this.menuProjectSave,
            this.menuProjectSaveAs});
            this.projectToolStripMenuItem.Name = "projectToolStripMenuItem";
            this.projectToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.projectToolStripMenuItem.Text = "&Project";
            // 
            // menuProjectOpen
            // 
            this.menuProjectOpen.Name = "menuProjectOpen";
            this.menuProjectOpen.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.O)));
            this.menuProjectOpen.Size = new System.Drawing.Size(187, 22);
            this.menuProjectOpen.Text = "&Open...";
            this.menuProjectOpen.Click += new System.EventHandler(this.menuProjectOpen_Click);
            // 
            // menuProjectRecent
            // 
            this.menuProjectRecent.Name = "menuProjectRecent";
            this.menuProjectRecent.Size = new System.Drawing.Size(187, 22);
            this.menuProjectRecent.Text = "&Recent";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
            // 
            // menuProjectClose
            // 
            this.menuProjectClose.Name = "menuProjectClose";
            this.menuProjectClose.Size = new System.Drawing.Size(187, 22);
            this.menuProjectClose.Text = "&Close";
            this.menuProjectClose.Click += new System.EventHandler(this.menuProjectClose_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(184, 6);
            // 
            // menuProjectSave
            // 
            this.menuProjectSave.Name = "menuProjectSave";
            this.menuProjectSave.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift)
                        | System.Windows.Forms.Keys.S)));
            this.menuProjectSave.Size = new System.Drawing.Size(187, 22);
            this.menuProjectSave.Text = "&Save";
            this.menuProjectSave.Click += new System.EventHandler(this.menuProjectSave_Click);
            // 
            // menuProjectSaveAs
            // 
            this.menuProjectSaveAs.Name = "menuProjectSaveAs";
            this.menuProjectSaveAs.Size = new System.Drawing.Size(187, 22);
            this.menuProjectSaveAs.Text = "Save &As...";
            this.menuProjectSaveAs.Click += new System.EventHandler(this.menuProjectSaveAs_Click);
            // 
            // statusIconList
            // 
            this.statusIconList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.statusIconList.ImageSize = new System.Drawing.Size(16, 16);
            this.statusIconList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // assemblyContextMenu
            // 
            this.assemblyContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.popupMenuAssemblyUnload,
            this.popupMenuAssemblyReload});
            this.assemblyContextMenu.Name = "assemblyContextMenu";
            this.assemblyContextMenu.Size = new System.Drawing.Size(119, 48);
            // 
            // popupMenuAssemblyUnload
            // 
            this.popupMenuAssemblyUnload.Name = "popupMenuAssemblyUnload";
            this.popupMenuAssemblyUnload.Size = new System.Drawing.Size(118, 22);
            this.popupMenuAssemblyUnload.Text = "&Unload";
            this.popupMenuAssemblyUnload.Click += new System.EventHandler(this.popupMenuAssemblyUnloadItem_Click);
            // 
            // popupMenuAssemblyReload
            // 
            this.popupMenuAssemblyReload.Name = "popupMenuAssemblyReload";
            this.popupMenuAssemblyReload.Size = new System.Drawing.Size(118, 22);
            this.popupMenuAssemblyReload.Text = "&Reload";
            this.popupMenuAssemblyReload.Click += new System.EventHandler(this.popupMenuAssemblyReloadItem_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusBar.Location = new System.Drawing.Point(0, 594);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(786, 22);
            this.statusBar.TabIndex = 1;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // horizontalSplitter
            // 
            this.horizontalSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.horizontalSplitter.Location = new System.Drawing.Point(0, 24);
            this.horizontalSplitter.Margin = new System.Windows.Forms.Padding(0);
            this.horizontalSplitter.Name = "horizontalSplitter";
            // 
            // horizontalSplitter.Panel1
            // 
            this.horizontalSplitter.Panel1.Controls.Add(this.buttonCancel);
            this.horizontalSplitter.Panel1.Controls.Add(this.buttonRun);
            this.horizontalSplitter.Panel1.Controls.Add(this.groupBox1);
            this.horizontalSplitter.Panel1.Padding = new System.Windows.Forms.Padding(3, 0, 0, 3);
            this.horizontalSplitter.Panel1MinSize = 200;
            // 
            // horizontalSplitter.Panel2
            // 
            this.horizontalSplitter.Panel2.Controls.Add(this.verticalSplitter);
            this.horizontalSplitter.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 3, 3);
            this.horizontalSplitter.Size = new System.Drawing.Size(786, 570);
            this.horizontalSplitter.SplitterDistance = 200;
            this.horizontalSplitter.TabIndex = 2;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(123, 541);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(71, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "&Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonRun.Location = new System.Drawing.Point(6, 541);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(111, 23);
            this.buttonRun.TabIndex = 11;
            this.buttonRun.Text = "&Run Selected";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.listTraits);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.listAssemblies);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textSearch);
            this.groupBox1.Location = new System.Drawing.Point(6, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(191, 532);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Refinements";
            // 
            // listTraits
            // 
            this.listTraits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listTraits.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTrait});
            this.listTraits.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listTraits.HideSelection = false;
            this.listTraits.Location = new System.Drawing.Point(6, 273);
            this.listTraits.Name = "listTraits";
            this.listTraits.Size = new System.Drawing.Size(179, 253);
            this.listTraits.TabIndex = 9;
            this.listTraits.UseCompatibleStateImageBehavior = false;
            this.listTraits.View = System.Windows.Forms.View.Details;
            this.listTraits.SelectedIndexChanged += new System.EventHandler(this.listTraits_SelectedIndexChanged);
            this.listTraits.Resize += new System.EventHandler(this.listTraits_Resize);
            // 
            // columnTrait
            // 
            this.columnTrait.Text = "Traits";
            this.columnTrait.Width = 158;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 255);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 14);
            this.label2.TabIndex = 8;
            this.label2.Text = "Traits:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 60);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 14);
            this.label4.TabIndex = 6;
            this.label4.Text = "Assemblies:";
            // 
            // listAssemblies
            // 
            this.listAssemblies.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listAssemblies.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnAssembly});
            this.listAssemblies.ContextMenuStrip = this.assemblyContextMenu;
            this.listAssemblies.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listAssemblies.HideSelection = false;
            this.listAssemblies.Location = new System.Drawing.Point(6, 77);
            this.listAssemblies.Name = "listAssemblies";
            this.listAssemblies.ShowItemToolTips = true;
            this.listAssemblies.Size = new System.Drawing.Size(179, 175);
            this.listAssemblies.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listAssemblies.TabIndex = 7;
            this.listAssemblies.UseCompatibleStateImageBehavior = false;
            this.listAssemblies.View = System.Windows.Forms.View.Details;
            this.listAssemblies.SelectedIndexChanged += new System.EventHandler(this.listAssemblies_SelectedIndexChanged);
            this.listAssemblies.Resize += new System.EventHandler(this.listAssemblies_Resize);
            // 
            // columnAssembly
            // 
            this.columnAssembly.Text = "Assembly";
            this.columnAssembly.Width = 158;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 14);
            this.label1.TabIndex = 4;
            this.label1.Text = "Search:";
            // 
            // textSearch
            // 
            this.textSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textSearch.Location = new System.Drawing.Point(7, 35);
            this.textSearch.Name = "textSearch";
            this.textSearch.Size = new System.Drawing.Size(178, 22);
            this.textSearch.TabIndex = 5;
            this.textSearch.TextChanged += new System.EventHandler(this.textSearch_TextChanged);
            // 
            // verticalSplitter
            // 
            this.verticalSplitter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.verticalSplitter.Location = new System.Drawing.Point(0, 0);
            this.verticalSplitter.Name = "verticalSplitter";
            this.verticalSplitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // verticalSplitter.Panel1
            // 
            this.verticalSplitter.Panel1.Controls.Add(this.testsGroupBox);
            // 
            // verticalSplitter.Panel2
            // 
            this.verticalSplitter.Panel2.Controls.Add(this.progress);
            this.verticalSplitter.Panel2.Controls.Add(this.groupBox2);
            this.verticalSplitter.Panel2MinSize = 100;
            this.verticalSplitter.Size = new System.Drawing.Size(579, 567);
            this.verticalSplitter.SplitterDistance = 305;
            this.verticalSplitter.TabIndex = 0;
            // 
            // testsGroupBox
            // 
            this.testsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.testsGroupBox.Controls.Add(this.listTests);
            this.testsGroupBox.Controls.Add(this.statusToolStrip);
            this.testsGroupBox.Location = new System.Drawing.Point(5, 3);
            this.testsGroupBox.Name = "testsGroupBox";
            this.testsGroupBox.Padding = new System.Windows.Forms.Padding(6);
            this.testsGroupBox.Size = new System.Drawing.Size(571, 299);
            this.testsGroupBox.TabIndex = 8;
            this.testsGroupBox.TabStop = false;
            this.testsGroupBox.Text = "Methods (0)";
            // 
            // listTests
            // 
            this.listTests.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listTests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnTest});
            this.listTests.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listTests.HideSelection = false;
            this.listTests.Location = new System.Drawing.Point(6, 49);
            this.listTests.Name = "listTests";
            this.listTests.Size = new System.Drawing.Size(557, 241);
            this.listTests.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.listTests.StateImageList = this.statusIconList;
            this.listTests.TabIndex = 2;
            this.listTests.UseCompatibleStateImageBehavior = false;
            this.listTests.View = System.Windows.Forms.View.Details;
            this.listTests.SelectedIndexChanged += new System.EventHandler(this.listTests_SelectedIndexChanged);
            this.listTests.Resize += new System.EventHandler(this.listTests_Resize);
            // 
            // columnTest
            // 
            this.columnTest.Text = "Test";
            this.columnTest.Width = 538;
            // 
            // statusToolStrip
            // 
            this.statusToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.statusToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.passedToolStripButton,
            this.failedToolStripButton,
            this.skippedToolStripButton});
            this.statusToolStrip.Location = new System.Drawing.Point(6, 21);
            this.statusToolStrip.Name = "statusToolStrip";
            this.statusToolStrip.Padding = new System.Windows.Forms.Padding(5, 0, 1, 0);
            this.statusToolStrip.Size = new System.Drawing.Size(559, 25);
            this.statusToolStrip.TabIndex = 1;
            // 
            // passedToolStripButton
            // 
            this.passedToolStripButton.CheckOnClick = true;
            this.passedToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("passedToolStripButton.Image")));
            this.passedToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.passedToolStripButton.Name = "passedToolStripButton";
            this.passedToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.passedToolStripButton.Text = "0";
            this.passedToolStripButton.Click += new System.EventHandler(this.passedToolStripButton_Click);
            // 
            // failedToolStripButton
            // 
            this.failedToolStripButton.CheckOnClick = true;
            this.failedToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("failedToolStripButton.Image")));
            this.failedToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.failedToolStripButton.Name = "failedToolStripButton";
            this.failedToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.failedToolStripButton.Text = "0";
            this.failedToolStripButton.Click += new System.EventHandler(this.failedToolStripButton_Click);
            // 
            // skippedToolStripButton
            // 
            this.skippedToolStripButton.CheckOnClick = true;
            this.skippedToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("skippedToolStripButton.Image")));
            this.skippedToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.skippedToolStripButton.Name = "skippedToolStripButton";
            this.skippedToolStripButton.Size = new System.Drawing.Size(33, 22);
            this.skippedToolStripButton.Text = "0";
            this.skippedToolStripButton.Click += new System.EventHandler(this.skippedToolStripButton_Click);
            // 
            // progress
            // 
            this.progress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.progress.Location = new System.Drawing.Point(5, 231);
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(571, 23);
            this.progress.Status = Xunit.Gui.ProgressControl.ProgressStatus.Unknown;
            this.progress.TabIndex = 9;
            this.progress.Value = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.textResults);
            this.groupBox2.Location = new System.Drawing.Point(5, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(571, 222);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Output";
            // 
            // textResults
            // 
            this.textResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textResults.BackColor = System.Drawing.Color.White;
            this.textResults.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textResults.Location = new System.Drawing.Point(6, 21);
            this.textResults.Multiline = true;
            this.textResults.Name = "textResults";
            this.textResults.ReadOnly = true;
            this.textResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textResults.Size = new System.Drawing.Size(556, 195);
            this.textResults.TabIndex = 0;
            // 
            // RunnerForm
            // 
            this.AcceptButton = this.buttonRun;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(786, 616);
            this.Controls.Add(this.horizontalSplitter);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(794, 646);
            this.Name = "RunnerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xUnit.net Test Runner";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RunnerForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.assemblyContextMenu.ResumeLayout(false);
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.horizontalSplitter.Panel1.ResumeLayout(false);
            this.horizontalSplitter.Panel2.ResumeLayout(false);
            this.horizontalSplitter.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.verticalSplitter.Panel1.ResumeLayout(false);
            this.verticalSplitter.Panel2.ResumeLayout(false);
            this.verticalSplitter.ResumeLayout(false);
            this.testsGroupBox.ResumeLayout(false);
            this.testsGroupBox.PerformLayout();
            this.statusToolStrip.ResumeLayout(false);
            this.statusToolStrip.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripMenuItem menuAssemblyOpen;
        private System.Windows.Forms.ToolStripMenuItem menuAssemblyRecent;
        private System.Windows.Forms.ImageList statusIconList;
        private System.Windows.Forms.ContextMenuStrip assemblyContextMenu;
        private System.Windows.Forms.ToolStripMenuItem popupMenuAssemblyUnload;
        private System.Windows.Forms.ToolStripMenuItem popupMenuAssemblyReload;
        private System.Windows.Forms.SplitContainer horizontalSplitter;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listAssemblies;
        private System.Windows.Forms.ColumnHeader columnAssembly;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textSearch;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.ListView listTraits;
        private System.Windows.Forms.ColumnHeader columnTrait;
        private System.Windows.Forms.ToolStripMenuItem menuAssemblyReload;
        private System.Windows.Forms.ToolStripMenuItem menuAssemblyUnload;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem menuFileExit;
        private System.Windows.Forms.ToolStripMenuItem projectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuProjectOpen;
        private System.Windows.Forms.ToolStripMenuItem menuProjectRecent;
        private System.Windows.Forms.ToolStripMenuItem menuProjectClose;
        private System.Windows.Forms.ToolStripMenuItem menuProjectSave;
        private System.Windows.Forms.ToolStripMenuItem menuProjectSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.SplitContainer verticalSplitter;
        private System.Windows.Forms.GroupBox testsGroupBox;
        private System.Windows.Forms.ListView listTests;
        private System.Windows.Forms.ColumnHeader columnTest;
        private System.Windows.Forms.ToolStrip statusToolStrip;
        private ProgressControl progress;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textResults;
        private System.Windows.Forms.ToolStripButton passedToolStripButton;
        private System.Windows.Forms.ToolStripButton failedToolStripButton;
        private System.Windows.Forms.ToolStripButton skippedToolStripButton;

    }
}