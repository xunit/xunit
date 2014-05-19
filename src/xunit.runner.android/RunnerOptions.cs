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
using Android.Content;

namespace Xunit.Runners
{
    public class RunnerOptions
    {
        public RunnerOptions()
        {
        }

        private static RunnerOptions current;

        public static RunnerOptions Current { get { return current; }}

        // Options are not as useful as under iOS since re-installing the
        // application deletes the file containing them.
        internal RunnerOptions(Activity activity)
        {
            var prefs = activity.GetSharedPreferences("options", FileCreationMode.Private);
            EnableNetwork = prefs.GetBoolean("remote", false);
            HostName = prefs.GetString("hostName", "0.0.0.0");
            HostPort = prefs.GetInt("hostPort", -1);
            NameDisplay = (NameDisplay)prefs.GetInt("nameDisplay", 1);
            ParallelizeAssemblies = prefs.GetBoolean("parallel", false);

            current = this;
        }

        public bool EnableNetwork { get; set; }

        public string HostName { get; set; }
        public bool ParallelizeAssemblies { get; set; }
        public int HostPort { get; set; }

        public bool ShowUseNetworkLogger
        {
            get { return (EnableNetwork && !String.IsNullOrWhiteSpace(HostName) && (HostPort > 0)); }
        }

        public void Save(Activity activity)
        {
            var prefs = activity.GetSharedPreferences("options", FileCreationMode.Private);
            var edit = prefs.Edit();
            edit.PutBoolean("remote", EnableNetwork);
            edit.PutString("hostName", HostName);
            edit.PutInt("hostPort", HostPort);
            edit.PutInt("nameDisplay", (int)NameDisplay);
            edit.PutBoolean("parallel", ParallelizeAssemblies);
            edit.Commit();
        }

        public static void Initialize(Activity activity)
        {
            // New it up to read the latest prefs
            current = new RunnerOptions(activity);
        }

        public static string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {

            if (current.NameDisplay == NameDisplay.Full)
                return displayName;
            if (displayName == fullyQualifiedMethodName || displayName.StartsWith(fullyQualifiedMethodName + "("))
                return shortMethodName + displayName.Substring(fullyQualifiedMethodName.Length);
            return displayName;
        }

        public NameDisplay NameDisplay { get; set; }
    }
}