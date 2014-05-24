//
// Copyright 2011-2012 Xamarin Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;

using Android.App;
using Android.OS;

using MonoDroid.Dialog;

namespace Xunit.Runners.UI
{
	
	[Activity (Label = "Credits")]
	internal class CreditsActivity : DialogActivity {
		
		const string notice = "<br><b>xUnit Android Runner</b><br>Copyright &copy; 2014<br>Outercurve Foundation<br>All rights reserved.<br><br>Author: Oren Novotny<br>";

		protected override void OnCreate (Bundle bundle)
		{
			Root = new RootElement (String.Empty) {
				new FormattedSection (notice) {
					new HtmlElement ("About Xamarin", "http://xamarin.com"),
					new HtmlElement ("About Mono for Android", "http://android.xamarin.com"),
					new HtmlElement ("About MonoDroid.Dialog", "https://github.com/spouliot/MonoDroid.Dialog"),
					new HtmlElement("About xUnit", "https://github.com/xunit/xunit"),
				}
			};
			
			base.OnCreate (bundle);
		}
	}
}