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

        public MonoTestResult TestResult { get; private set; }

        public TestCaseElement(MonoTestCase testCase, TouchRunner runner)
            : base(runner)
        {
            TestCase = testCase;
            Caption = testCase.DisplayName;
            Value = "NotExecuted";
            TestResult = new MonoTestResult(testCase, null);
            Tapped += async delegate
            {
                if (!Runner.OpenWriter(TestCase.DisplayName))
                    return;

                await Run();

                if ( TestResult.Outcome == TestState.Failed)
                {
                    var root = new RootElement("Results")
                    {
                        new Section()
                        {
                            new TestResultElement(TestResult)
                        }
                    };
                    var dvc = new DialogViewController(root, true)
                    {
                        Autorotate = true
                    };
                    Runner.NavigationController.PushViewController(dvc, true);
                }

                Runner.CloseWriter();
            };
        }

        protected override void OptionsChanged()
        {
            Caption = TestCase.DisplayName;
            base.OptionsChanged();

        }

        public override TestState Result
        {
            get { return TestResult.Outcome; }
        }

        public void UpdateResult(MonoTestResult result)
        {
            TestResult = result;

            if (TestResult.Outcome == TestState.Skipped)
            {
                Value = ((ITestSkipped)TestResult.TestResultMessage).Reason;
                DetailColor = UIColor.Orange;
            }
            else if (TestResult.Outcome == TestState.Passed)
            {
                Value = String.Format("Success! {0} ms", TestResult.Duration.TotalMilliseconds);
                DetailColor = DarkGreen;
            }
            else if (TestResult.Outcome == TestState.Failed)
            {
                Value = TestResult.ErrorMessage;
                DetailColor = UIColor.Red;
            }
            else
            {
                // Assert.Ignore falls into this
                Value = TestResult.ErrorMessage;
            }

           

            Refresh();
        }
      
        public async Task Run()
        {
            if (TestResult.Outcome == TestState.NotRun)
            {
                await Runner.Run(TestCase);
            }
        }

      

    }
}