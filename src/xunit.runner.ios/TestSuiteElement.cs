// TestSuiteElement.cs: MonoTouch.Dialog element for TestSuite
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2011-2013 Xamarin Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit.Abstractions;
using Xunit.Runner.iOS;
#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;


namespace Xunit.Runners.UI {

	class TestSuiteElement : TestElement {
	    private readonly IEnumerable<ITestCase> testCases;

	    public TestSuiteElement (string sourceName, IEnumerable<ITestCase> testCases, TouchRunner runner)
			: base (runner)
		{
		    this.testCases = testCases;
		    Caption = sourceName;
            int count = testCases.Count();
			if (count > 0) {
				Accessory = UITableViewCellAccessory.DisclosureIndicator;
				DetailColor = DarkGreen;
				//Value = String.Format ("{0} test case{1}, {2}", count, count == 1 ? String.Empty : "s", Suite.RunState);
				Value = String.Format ("{0} test case{1}", count, count == 1 ? String.Empty : "s");
				Tapped += delegate {
					runner.Show (sourceName);
				};
			} else {
				DetailColor = UIColor.Orange;
				Value = "No test was found inside this suite";
			}
		}
		
        //public TestSuite Suite {
        //    get { return Test as TestSuite; }
        //}
		
        //public void Run ()
        //{
        //    Result = Runner.Run (Suite);
        //}
		
        //public override void Update ()
        //{
        //    int positive = Result.PassCount + Result.InconclusiveCount;
        //    int failure = Result.FailCount;
        //    int skipped = Result.SkipCount;

        //    StringBuilder sb = new StringBuilder ();
        //    if (failure == 0) {
        //        DetailColor = DarkGreen;
        //        sb.Append ("Success! ").Append (Result.Duration.TotalMilliseconds).Append (" ms for ").Append (positive).Append (" test");
        //        if (positive > 1)
        //            sb.Append ('s');
        //    } else {
        //        DetailColor = UIColor.Red;
        //        if (positive > 0)
        //            sb.Append (positive).Append (" success");
        //        if (sb.Length > 0)
        //            sb.Append (", ");
        //        sb.Append (failure).Append (" failure");
        //        if (failure > 1)
        //            sb.Append ('s');
        //        if (skipped > 0)
        //            sb.Append (", ").Append (skipped).Append (" ignored");
        //    }
        //    Value = sb.ToString ();

        //    if (GetContainerTableView () != null) {
        //        var root = GetImmediateRootElement ();
        //        root.Reload (this, UITableViewRowAnimation.Fade);
        //    }
        //}
	    public override TestState Result
	    {
	        get { return TestState.NotRun; }
	    }
	}
}
