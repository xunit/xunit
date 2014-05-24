using System;

using Android.Content;
using Android.Views;
using Android.Widget;

namespace MonoDroid.Dialog
{
    internal class ButtonElement : StringElement
	{
		public ButtonElement (string caption, EventHandler tapped)
            : base(caption, (int)DroidResources.ElementLayout.dialog_button)
		{
			this.Click = tapped;
		}

		public override View GetView (Context context, View convertView, ViewGroup parent)
		{
			Button button;
			var view = DroidResources.LoadButtonLayout (context, convertView, parent, LayoutId, out button);
			if (view != null) {
				button.Text = Caption;
				if (Click != null)
					button.Click += Click;
			}
			
			return view;
		}

		public override string Summary ()
		{
			return Caption;
		}
	}
}