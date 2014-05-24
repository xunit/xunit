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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using MonoDroid.Dialog;

namespace Xunit.Runners.UI
{
    public class RunnerActivity : Activity
    {
        private Assembly assembly;

        public RunnerActivity()
        {
            //Initialized = (AndroidRunner.AssemblyLevel.Count > 0);
        }

        public bool Initialized { get; private set; }

        public AndroidRunner Runner
        {
            get { return AndroidRunner.Runner; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var view = Runner.GetView(this);

            Initialized = true;

            SetContentView(view);
        }

        public void Add(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            // this can be called many times but we only want to load them
            // once since we need to share them across most activities
            if (!Initialized)
            {
                AndroidRunner.AddAssembly(assembly);
            }
        }

        public void AddExecutionAssembly(Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException("assembly");
            this.assembly = assembly;
        }
    }
}