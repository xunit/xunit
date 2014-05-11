// TestElement.cs: MonoTouch.Dialog element for ITest, i.e. TestSuite and TestCase
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
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
using Xunit.Runner.iOS;
#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;


namespace Xunit.Runners.UI {

	abstract class TestElement : StyledMultilineElement {
		
		static internal UIColor DarkGreen = UIColor.FromRGB (0x00, 0x77, 0x00);

		public TestElement ( TouchRunner runner)
			: base ("?", "?", UITableViewCellStyle.Subtitle)
		{
			if (runner == null)
				throw new ArgumentNullException ("runner");
		
			Runner = runner;

            // Normally this would be a bad thing, an event on a static class
            // given the lifespan of these elements, it doesn't matter.
		    TouchOptions.Current.OptionsChanged +=
		        (sender, args) =>
		        {
		            OptionsChanged();

		            Refresh();
		        };
        }

	    protected virtual void OptionsChanged()
        {
	        
	    }

	    protected void Refresh()
	    {
            if (GetContainerTableView() != null)
            {
                var root = GetImmediateRootElement();
                root.Reload(this, UITableViewRowAnimation.None);
            }
	    }

        public abstract TestState Result { get; }

		protected TouchRunner Runner { get; private set; }

	}
}
