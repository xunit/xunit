// TestCaseElement.cs: MonoTouch.Dialog element for TestCase
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
using System.Reflection;
using Xunit.Abstractions;
#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;

using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Api;

namespace Xunit.Runners.UI {
	
	class TestCaseElement : TestElement {
		
		public TestCaseElement (ITestCase testCase, TouchRunner runner)
			: base (testCase, runner)
		{
			Caption = testCase.DisplayName;
			Value = "NotExecuted";
			this.Tapped += delegate {
				if (!Runner.OpenWriter (Test.FullName))
					return;

				var suite = testCase.TestCollection;
				var context = TestExecutionContext.CurrentContext;
				context.TestObject = Reflect.Construct (testCase.Method.ReflectedType, null);

				suite.GetOneTimeSetUpCommand ().Execute (context);
				Run ();
				suite.GetOneTimeTearDownCommand ().Execute (context);

				Runner.CloseWriter ();
				// display more details on (any) failure (but not when ignored)
				if ((TestCase.RunState == RunState.Runnable) && !Result.IsSuccess ()) {
					var root = new RootElement ("Results") {
						new Section () {
							new TestResultElement (Result)
						}
					};
					var dvc = new DialogViewController (root, true) { Autorotate = true };
					runner.NavigationController.PushViewController (dvc, true);
				} else if (GetContainerTableView () != null) {
					var root = GetImmediateRootElement ();
					root.Reload (this, UITableViewRowAnimation.Fade);
				}
			};
		}
		
		public ITestCase TestCase {
			get { return Test; }
		}
		
		public void Run ()
		{
			Update (Runner.Run (TestCase));
		}
		
		public override void Update ()
		{
			if (Result.IsIgnored ()) {
				Value = Result.GetMessage ();
				DetailColor = UIColor.Orange;
			} else if (Result.IsSuccess () || Result.IsInconclusive ()) {
				int counter = Result.AssertCount;
				Value = String.Format ("{0} {1} ms for {2} assertion{3}",
					Result.IsInconclusive () ? "Inconclusive." : "Success!",
					Result.ExecutionTime * 1000, counter,
					counter == 1 ? String.Empty : "s");
				DetailColor = DarkGreen;
			} else if (Result.IsFailure ()) {
				Value = Result.GetMessage ();
				DetailColor = UIColor.Red;
			} else {
				// Assert.Ignore falls into this
				Value = Result.GetMessage ();
			}
		}
	}
}
