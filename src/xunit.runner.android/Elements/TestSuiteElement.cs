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


namespace Xunit.Runners.UI
{
    internal class TestSuiteElement : TestElement
    {
        private readonly string sourceName;
        private readonly IEnumerable<TestCaseElement> testCases = Enumerable.Empty<TestCaseElement>();
        private TestState result = TestState.NotRun;

        public IEnumerable<TestCaseElement> TestCases { get { return testCases; } } 

        public TestSuiteElement(string sourceName, IEnumerable<TestCaseElement> testCases, AndroidRunner runner)
            : base(runner)
        {
            this.sourceName = sourceName;
            this.testCases = testCases;

            if (testCases.Any())
                Indicator = ">"; // hint there's more

            //SetCaption(sourceName);
        }


        protected override string GetCaption()
        {
            var count = testCases.Count();
            var caption = String.Format("<b>{0}</b><br>", sourceName);
            if (count == 0)
            {
                caption += "<font color='#ff7f00'>no test was found inside this suite</font>";
            }
            else if (Result == TestState.Passed)
            {
                caption += String.Format("<font color='green'><b>{0}</b> test case{1}, <i>{2}</i></font>",
                                         count, count == 1 ? String.Empty : "s", Result);
            }
            else
            {
                var outcomes = testCases.Select(t => t.TestResult)
                                        .GroupBy(r => r.Outcome);

                var results = outcomes.ToDictionary(k => k.Key, v => v.Count());

                int positive;
                results.TryGetValue(TestState.Passed, out positive);

                int failure;
                results.TryGetValue(TestState.Failed, out failure);

                int skipped;
                results.TryGetValue(TestState.Skipped, out skipped);

                if (failure == 0)
                {
                    caption += string.Format("<font color='green'><b>Success!</b> {0} test{1}</font>",
                                             positive, positive == 1 ? string.Empty : "s");

                    result = TestState.Passed;
                }
                else if (result != TestState.NotRun)
                {
                    caption += String.Format("<font color='green'>{0} success,</font> <font color='red'>{1} failure{2}, {3} skip{4}</font>",
                                             positive, failure, failure > 1 ? "s" : String.Empty,
                                             skipped, skipped > 1 ? "s" : String.Empty);

                    result = TestState.Failed;
                }
            }
            return caption;
        }

        public override View GetView(Context context, View convertView, ViewGroup parent)
        {
            var view = base.GetView(context, convertView, parent);
            // if there are test cases inside this suite then create an activity to show them
            if (testCases.Any())
            {
                view.Click += delegate
                {
                    var intent = new Intent(context, typeof(TestSuiteActivity));
                    intent.PutExtra("TestSuite", sourceName);
                    intent.AddFlags(ActivityFlags.NewTask);
                    context.StartActivity(intent);
                };
            }
            return view;
        }

        public override TestState Result
        {
            get { return result; }
        }
    }
}