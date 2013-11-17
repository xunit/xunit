using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Xunit.Runner.VisualStudio
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid(Guids.PropertyPage)]
    public class XunitVisualStudioSettings : DialogPage, INotifyPropertyChanged
    {
        NameDisplay nameDisplay = NameDisplay.Short;
        bool parallelizeAssemblies;
        //bool parallelizeCollections = true;

        public event PropertyChangedEventHandler PropertyChanged;

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

        public override void ResetSettings()
        {
            NameDisplay = NameDisplay.Short;
            ParallelizeAssemblies = false;
            //ParallelizeCollections = true;

            base.ResetSettings();
        }
    }
}