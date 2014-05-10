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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MonoTouch.Dialog;
using Xunit.Abstractions;
using Xunit.Runner.iOS;
#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

namespace Xunit.Runners.UI
{
    internal class TestCaseElement : TestElement
    {
        public MonoTestCase TestCase { get; private set; }

        public TestCaseElement(MonoTestCase testCase, TouchRunner runner)
            : base(runner)
        {
            TestCase = testCase;
            Caption = testCase.DisplayName;
            Value = "NotExecuted";
            //Tapped += delegate
            //{
            //    if (!Runner.OpenWriter(Test.DisplayName))
            //        return;

            //    Run();
                
            //    Runner.CloseWriter();
            //    // display more details on (any) failure (but not when ignored)
            //    if ((TestCase.SkipReason != null) && !Result.IsSuccess())
            //    {
            //        var root = new RootElement("Results")
            //        {
            //            new Section()
            //            {
            //                new TestResultElement(Result)
            //            }
            //        };
            //        var dvc = new DialogViewController(root, true)
            //        {
            //            Autorotate = true
            //        };
            //        runner.NavigationController.PushViewController(dvc, true);
            //    }
            //    else if (GetContainerTableView() != null)
            //    {
            //        var root = GetImmediateRootElement();
            //        root.Reload(this, UITableViewRowAnimation.Fade);
            //    }
            //};
        }

        protected override void OptionsChanged()
        {
            Caption = TestCase.DisplayName;
            base.OptionsChanged();

        }

        //public ITestCase TestCase
        //{
        //    get { return Test; }
        //}

        //public void Run()
        //{
        //    Update(Runner.Run(TestCase));
        //}

        //public override void Update()
        //{
        //    if (Result.IsSkipped())
        //    {
        //        Value = Result.GetMessage();
        //        DetailColor = UIColor.Orange;
        //    }
        //    else if (Result.IsSuccess() || Result.IsInconclusive())
        //    {
        //        int counter = Result.AssertCount;
        //        Value = String.Format("{0} {1} ms for {2} assertion{3}",
        //                              Result.IsInconclusive() ? "Inconclusive." : "Success!",
        //                              Result.ExecutionTime*1000, counter,
        //                              counter == 1 ? String.Empty : "s");
        //        DetailColor = DarkGreen;
        //    }
        //    else if (Result.IsFailure())
        //    {
        //        Value = Result.GetMessage();
        //        DetailColor = UIColor.Red;
        //    }
        //    else
        //    {
        //        // Assert.Ignore falls into this
        //        Value = Result.GetMessage();
        //    }
        //}

        public override TestState Result
        {
            get { return TestCase.Result; }
        }
    }
}