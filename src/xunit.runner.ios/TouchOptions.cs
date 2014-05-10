// TouchOptions.cs: MonoTouch.Dialog-based options
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
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
using Xunit.Runner.iOS;
#if XAMCORE_2_0
using Foundation;
using UIKit;
#else
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;

using Mono.Options;

namespace Xunit.Runners.UI {
	
	public class TouchOptions {

		static public TouchOptions Current = new TouchOptions ();

        // Normally this would be a bad thing, an event on a static class
        // given the lifespan of these elements, it doesn't matter.
	    public event EventHandler OptionsChanged;
		
		public TouchOptions ()
		{
			var defaults = NSUserDefaults.StandardUserDefaults;
            //EnableNetwork = defaults.BoolForKey ("network.enabled");
            //HostName = defaults.StringForKey ("network.host.name");
            //HostPort = (int)defaults.IntForKey ("network.host.port");
			SortNames = defaults.BoolForKey ("display.sort");
		    ParallelizeAssemblies = defaults.BoolForKey("exec.parallel");
		    var dnKey = defaults.IntForKey("display.NameDisplay");
            if(dnKey == 0)
                NameDisplay = NameDisplay.Short;
            else
            {
                NameDisplay = (NameDisplay)dnKey;
            }

			
			var os = new OptionSet () {
				{ "autoexit", "If the app should exit once the test run has completed.", v => TerminateAfterExecution = true },
				{ "autostart", "If the app should automatically start running the tests.", v => AutoStart = true },
                //{ "hostname=", "Comma-separated list of host names or IP address to (try to) connect to", v => HostName = v },
                //{ "hostport=", "TCP port to connect to.", v => HostPort = int.Parse (v) },
                //{ "enablenetwork", "Enable the network reporter.", v => EnableNetwork = true },
			};
			
			try {
				os.Parse (Environment.GetCommandLineArgs ());
			} catch (OptionException oe) {
				Console.WriteLine ("{0} for options '{1}'", oe.Message, oe.OptionName);
			}
		}
		
        //private bool EnableNetwork { get; set; }
		
        //public string HostName { get; private set; }
		
        //public int HostPort { get; private set; }
		
		public bool AutoStart { get; set; }
		
		public bool TerminateAfterExecution { get; set; }
		
        //public bool ShowUseNetworkLogger {
        //    get { return (EnableNetwork && !String.IsNullOrWhiteSpace (HostName) && (HostPort > 0)); }
        //}

		public bool SortNames { get; set; }
		public NameDisplay NameDisplay { get; set; }

        public bool ParallelizeAssemblies { get; set; }
		
		[CLSCompliant (false)]
		public UIViewController GetViewController ()
		{
            //var network = new BooleanElement ("Enable", EnableNetwork);

            //var host = new EntryElement ("Host Name", "name", HostName);
            //host.KeyboardType = UIKeyboardType.ASCIICapable;
			
            //var port = new EntryElement ("Port", "name", HostPort.ToString ());
            //port.KeyboardType = UIKeyboardType.NumberPad;
			
			var sort = new BooleanElement ("Sort Names", SortNames);
            var nameDisplayFull = new RadioElement("Full", "nameDisplay");
            var nameDisplayShort = new RadioElement("Short", "nameDisplay");

		    var nameDisplayGroup = new RadioGroup("nameDisplay", NameDisplay == NameDisplay.Short ? 1 : 0);

		    var par = new BooleanElement("Parallelize Assemblies", ParallelizeAssemblies);

			var root = new RootElement ("Options") {
			//	new Section ("Remote Server") { network, host, port },
                new Section("Execution") { par },

				new Section ("Display")
				{
				    sort,
                    new RootElement("Name Display", nameDisplayGroup)
                    {
                        new Section()
                        {
                            nameDisplayFull, 
                            nameDisplayShort
                        }
                    }
				}
			};
				
			var dv = new DialogViewController (root, true) { Autorotate = true };
		    dv.ViewDisappearing
		        += delegate
		        {
		            //EnableNetwork = network.Value;
		            //HostName = host.Value;
		            ushort p;
		            //if (UInt16.TryParse (port.Value, out p))
		            //    HostPort = p;
		            //else
		            //    HostPort = -1;
		            SortNames = sort.Value;
		            ParallelizeAssemblies = par.Value;
		            NameDisplay = nameDisplayGroup.Selected == 1 ? NameDisplay.Short : NameDisplay.Full;

		            var defaults = NSUserDefaults.StandardUserDefaults;
		            //	defaults.SetBool (EnableNetwork, "network.enabled");
		            //	defaults.SetString (HostName ?? String.Empty, "network.host.name");
		            //defaults.SetInt (HostPort, "network.host.port");

		            defaults.SetInt((int)NameDisplay, "display.nameDisplay");
		            defaults.SetBool(SortNames, "display.sort");
		            defaults.SetBool(ParallelizeAssemblies, "exec.parallel");

		            if (OptionsChanged != null)
		                OptionsChanged(this, EventArgs.Empty);
		        };
			
			return dv;
		}
        public string GetDisplayName(string displayName, string shortMethodName, string fullyQualifiedMethodName)
        {
            if (NameDisplay == NameDisplay.Full)
                return displayName;
            if (displayName == fullyQualifiedMethodName || displayName.StartsWith(fullyQualifiedMethodName + "("))
                return shortMethodName + displayName.Substring(fullyQualifiedMethodName.Length);
            return displayName;
        }
	}
}
