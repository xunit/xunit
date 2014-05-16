﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Webkit;
using Uri = Android.Net.Uri;

namespace MonoDroid.Dialog
{
    internal class HtmlElement : StringElement
    {
        // public string Value;
		
        public HtmlElement(string caption, string url)
            : base(caption)
        {
            Url = Uri.Parse(url);
        }

        public HtmlElement(string caption, Uri uri)
            : base(caption)
        {
            Url = uri;
        }

        public Uri Url { get; set; }
				
		void OpenUrl(Context context)
		{
			Intent intent = new Intent(context, typeof(HtmlActivity));
			intent.PutExtra("URL",this.Url.ToString());
			intent.PutExtra("Title",Caption);
			intent.AddFlags(ActivityFlags.NewTask);	
			context.StartActivity(intent);
		}

        public override View GetView(Context context, View convertView, ViewGroup parent)
		{
			View view = base.GetView (context, convertView, parent);
			
            this.Click = (o, e) => OpenUrl(context);

			return view;
		}
    }
	
	[Activity]
    internal class HtmlActivity : Activity
	{
		protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
			
			Intent i = this.Intent;
			string url = i.GetStringExtra("URL");
			this.Title = i.GetStringExtra("Title");
			
			WebView webview = new WebView(this);
			webview.Settings.BuiltInZoomControls = true;
			webview.Settings.JavaScriptEnabled = true;
 			SetContentView(webview);	
			webview.LoadUrl(url);
		}
	}
}