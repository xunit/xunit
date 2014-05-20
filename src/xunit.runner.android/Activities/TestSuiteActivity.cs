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
using Android.App;
using Android.OS;
using Android.Widget;
using MonoDroid.Dialog;

namespace Xunit.Runners.UI
{
    [Activity(Label = "Tests")]
    public class TestSuiteActivity : Activity
    {
        private Section main;
        private string sourceName;
        private TestSuiteElement suiteElement;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            sourceName = Intent.GetStringExtra("TestSuite");
            suiteElement = AndroidRunner.Runner.Suites[sourceName];

            var menu = new RootElement(String.Empty);

            main = new Section(sourceName);
            foreach (var test in suiteElement.TestCases)
            {
                main.Add(test);
            }
            menu.Add(main);

            var options = new Section()
            {
                new ActionElement("Run all", Run),
            };
            menu.Add(options);

            var da = new DialogAdapter(this, menu);
            var lv = new ListView(this)
            {
                Adapter = da
            };
            SetContentView(lv);
        }

        private async void Run()
        {
            var runner = AndroidRunner.Runner;

            await runner.Run(suiteElement.TestCases.Select(tc => tc.TestCase));

            suiteElement.Refresh();
        }
    }
}