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
using Xunit.Runners;
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
            Tapped += async delegate
            {
               await Run();

                if (TestCase.Result == TestState.Failed)
                {
                    var root = new RootElement("Results")
                    {
                        new Section()
                        {
                            new TestResultElement(TestCase.TestResult)
                        }
                    };
                    var dvc = new DialogViewController(root, true)
                    {
                        Autorotate = true
                    };
                    Runner.NavigationController.PushViewController(dvc, true);
                }
            };

            testCase.TestCaseUpdated += OnTestCaseUpdated;
        }

        private void OnTestCaseUpdated(object sender, EventArgs e)
        {
            UpdateResult();
        }

        protected override void OptionsChanged()
        {
            Caption = TestCase.DisplayName;
            base.OptionsChanged();

        }

        public override TestState Result
        {
            get { return TestCase.Result; }
        }

        private void UpdateResult()
        {

            if (TestCase.Result == TestState.Skipped)
            {
                Value = ((ITestSkipped)TestCase.TestResult.TestResultMessage).Reason;
                DetailColor = UIColor.Orange;
            }
            else if (TestCase.Result == TestState.Passed)
            {
                Value = String.Format("Success! {0} ms", TestCase.TestResult.Duration.TotalMilliseconds);
                DetailColor = DarkGreen;
            }
            else if (TestCase.Result == TestState.Failed)
            {
                Value = TestCase.TestResult.ErrorMessage;
                DetailColor = UIColor.Red;
            }
            else
            {
                // Assert.Ignore falls into this
                Value = TestCase.TestResult.ErrorMessage;
            }

           

            Refresh();
        }
      
        private async Task Run()
        {
            if (TestCase.Result == TestState.NotRun)
            {
                await Runner.Run(TestCase);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TestCase.TestCaseUpdated -= OnTestCaseUpdated;
            }
            base.Dispose(disposing);
        }

    }
}