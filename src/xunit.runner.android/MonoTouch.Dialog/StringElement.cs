using System;

using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Renderscripts;
using Android.Views;
using Android.Widget;
using Android.Runtime;


namespace MonoDroid.Dialog
{
    internal class StringElement : Element
    {


		public int? FontSize {get;set;}
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_text != null) 
                    _text.Text = _value;

            }
        }
        private string _value;

        public object Alignment;

        public StringElement(string caption)
            : base(caption, (int)DroidResources.ElementLayout.dialog_labelfieldright)
        {
        }

        public StringElement(string caption, int layoutId)
            : base(caption, layoutId)
        {
        }

        public StringElement(string caption, string value)
            : base(caption, (int)DroidResources.ElementLayout.dialog_labelfieldright)
        {
            Value = value;
        }
		
		
        public StringElement(string caption, string value, EventHandler clicked)
            : base(caption, (int)DroidResources.ElementLayout.dialog_labelfieldright)
        {
            Value = value;
			this.Click = clicked;
        }

        public StringElement(string caption, string value, int layoutId)
            : base(caption, layoutId)
        {
            Value = value;
        }
		
		public StringElement(string caption, EventHandler clicked)
            : base(caption, (int)DroidResources.ElementLayout.dialog_labelfieldright)
        {
            Value = null;
			this.Click = clicked;
        }

        public override View GetView(Context context, View convertView, ViewGroup parent)
        {
            View view = DroidResources.LoadStringElementLayout(context, convertView, parent, LayoutId, out _caption, out _text);
            if (view != null)
            {
                _caption.Text = Caption;
                _text.Text = Value;

                if (FontSize.HasValue)
                {
                    _caption.TextSize = FontSize.Value;
                    _text.TextSize = FontSize.Value;    
                }

				if (Click != null)
					view.Click += Click;
            }
            return view;
        }

        public override string Summary()
        {
            return Value;
        }

        public override bool Matches(string text)
        {
            return (Value != null && Value.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) != -1) ||
                   base.Matches(text);
        }

        protected TextView _caption;
        protected TextView _text;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //_caption.Dispose();
                _caption = null;
                //_text.Dispose();
                _text = null;
            }
        }
    }
}