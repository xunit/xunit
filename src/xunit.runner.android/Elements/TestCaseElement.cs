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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
using Xunit.Abstractions;


namespace Xunit.Runners.UI
{
    internal class TestCaseElement : TestElement
    {
        public TestCaseElement(MonoTestCase testCase, AndroidRunner runner)
            : base(runner)
        {
            if (testCase == null) throw new ArgumentNullException("testCase");

            TestCase = testCase;

            MonoTestResult result;
            Runner.Results.TryGetValue(testCase.UniqueName, out result);
            
            TestResult = result ?? new MonoTestResult(testCase, null);

            if (testCase.Result == TestState.NotRun)
                Indicator = "..."; // hint there's more

            Refresh();
        }


        public MonoTestCase TestCase { get; private set; }

        public MonoTestResult TestResult { get; private set; }


        public override TestState Result
        {
            get { return TestResult.Outcome; }
        }

        protected override string GetCaption()
        {
            if (TestResult == null)
                return "Initial";


            if (TestResult.Outcome == TestState.Skipped)
            {
                var val = ((ITestSkipped)TestResult.TestResultMessage).Reason;
                return string.Format("<b>{0}</b><br><font color='#FF7700'>{1}: {2}</font>",
                                     TestCase.DisplayName, TestState.Skipped, val);
            }
            else if (TestResult.Outcome == TestState.Passed)
            {
                Indicator = null;
                return string.Format("<b>{0}</b><br><font color='green'>Success! {1} ms</font>", TestCase.DisplayName, TestResult.Duration.TotalMilliseconds);
            }
            else if (TestResult.Outcome == TestState.Failed)
            {
                var val = TestResult.ErrorMessage;
                return string.Format("<b>{0}</b><br><font color='red'>{1}</font>", TestCase.DisplayName, val);
            }
            else
            {
                // Assert.Ignore falls into this
                var val = TestResult.ErrorMessage;
                return string.Format("<b>{0}</b><br><font color='grey'>{1}</font>", TestCase.DisplayName, val);
            }
        }

        public void UpdateResult(MonoTestResult result)
        {
            TestResult = result;


            Refresh();
        }

        public async Task Run()
        {
            if (TestResult.Outcome == TestState.NotRun)
            {
                await Runner.Run(TestCase);
            }
        }

        public override View GetView(Context context, View convertView, ViewGroup parent)
        {
            var view = base.GetView(context, convertView, parent);
            view.Click += async delegate
            {
                await Run();

                if (Result != TestState.Passed)
                {
                    var intent = new Intent(context, typeof(TestResultActivity));
                    intent.PutExtra("TestCase", TestCase.UniqueName);
                    intent.AddFlags(ActivityFlags.NewTask);
                    context.StartActivity(intent);
                }
            };
            return view;
        }
    }
}