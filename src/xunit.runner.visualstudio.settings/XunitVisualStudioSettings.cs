using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Xunit.Runner.VisualStudio.Settings
{
    public enum MessageDisplay
    {
        None = 1,
        Minimal = 2,
        Diagnostic = 3
    }

    public enum NameDisplay
    {
        Short = 1,
        Full = 2
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(Guids.PropertyPage)]
    public class XunitVisualStudioSettings : DialogPage, INotifyPropertyChanged
    {
        MessageDisplay messageDisplay;
        NameDisplay nameDisplay;
        bool parallelizeAssemblies;
        //bool parallelizeCollections;
        bool shutdownAfterRun;

        public XunitVisualStudioSettings()
        {
            Reset();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [Browsable(true)]
        [Category("General")]
        [DisplayName("Message display")]
        [Description("Determines how much information to print in the Tests output window")]
        public MessageDisplay MessageDisplay
        {
            get { return messageDisplay; }
            set
            {
                messageDisplay = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("MessageDisplay"));
            }
        }

        [Browsable(true)]
        [Category("General")]
        [DisplayName("Name display")]
        [Description("Determines whether to use short or full names in the Test Explorer window")]
        public NameDisplay NameDisplay
        {
            get { return nameDisplay; }
            set
            {
                nameDisplay = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("NameDisplay"));
            }
        }

        [Browsable(true)]
        [Category("General")]
        [DisplayName("Parallelize Assemblies")]
        [Description("Enables or disables running test assemblies in parallel")]
        public bool ParallelizeAssemblies
        {
            get { return parallelizeAssemblies; }
            set
            {
                parallelizeAssemblies = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ParallelizeAssemblies"));
            }
        }

        //[Browsable(true)]
        //[Category("General")]
        //[DisplayName("Parallelize Collections")]
        //[Description("Enables or disables running test collections in parallel")]
        //public bool ParallelizeCollections
        //{
        //    get { return parallelizeCollections; }
        //    set
        //    {
        //        parallelizeCollections = value;
        //        if (PropertyChanged != null)
        //            PropertyChanged(this, new PropertyChangedEventArgs("ParallelizeCollections"));
        //    }
        //}

        [Browsable(true)]
        [Category("General")]
        [DisplayName("Shutdown After Run")]
        [Description("Enables or disables shutting down the test execution engine after tests have run. Users with low RAM or resource locking issues should enable this option.")]
        public bool ShutdownAfterRun
        {
            get { return shutdownAfterRun; }
            set
            {
                shutdownAfterRun = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ShutdownAfterRun"));
            }
        }

        public void Reset()
        {
            MessageDisplay = MessageDisplay.None;
            NameDisplay = NameDisplay.Short;
            ParallelizeAssemblies = false;
            //ParallelizeCollections = false;
            ShutdownAfterRun = false;
        }

        public override void ResetSettings()
        {
            Reset();
            base.ResetSettings();
        }
    }
}