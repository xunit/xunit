// NUnitOutputTextWriter.cs
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2013 Xamarin Inc.
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
using System.IO;
using System.Text;
using System.Xml;
using Xunit.Runners.UI;

namespace Xunit.Runners.UI {

	public class NUnitOutputTextWriter : TextWriter {

		bool real_time_reporting;

        public NUnitOutputTextWriter(TouchRunner runner, TextWriter baseWriter, XmlWriter xmlWriter)
		{
			Runner = runner;
			BaseWriter = baseWriter ?? Console.Out;
			XmlOutputWriter = xmlWriter;
			// do not send real-time test results on the writer sif XML reports are enabled
			real_time_reporting = (xmlWriter == null);
		}

		public override Encoding Encoding {
			get { return Encoding.UTF8; }
		}

		public TextWriter BaseWriter { get; private set; }

		public TouchRunner Runner { get; private set; }

		public XmlWriter XmlOutputWriter { get; private set; }

		public override void Write (char value)
		{
			if (real_time_reporting)
				BaseWriter.Write (value);
		}

		public override void Write (string value)
		{
			if (real_time_reporting)
				BaseWriter.Write (value);
		}

		public override void Close ()
		{
			if (XmlOutputWriter != null) {
				// now we want the XML report to write
				real_time_reporting = true;
				// TODO
                //XmlOutputWriter.WriteResultFile (Runner.Result, BaseWriter);
				real_time_reporting = false;
			}
			base.Close ();
		}
	}
}