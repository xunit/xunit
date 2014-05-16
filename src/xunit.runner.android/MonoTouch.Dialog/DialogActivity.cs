using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;

namespace MonoDroid.Dialog
{
    internal class DialogInstanceData : Java.Lang.Object
    {
        public DialogInstanceData()
        {
            _dialogState = new Dictionary<string, string>();
        }

        private Dictionary<String, String> _dialogState;
    }

    internal class DialogActivity : ListActivity
	{
		public RootElement Root { get; set; }
        private DialogHelper Dialog { get; set; }
		
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
            Dialog = new DialogHelper(this, this.ListView, this.Root);

            if (this.LastNonConfigurationInstance != null)
            {
                // apply value changes that are saved
            }
        }

        public override Java.Lang.Object OnRetainNonConfigurationInstance()
        {
            return null;
        }
	}
}