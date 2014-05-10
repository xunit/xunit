// TestResultElement.cs: MonoTouch.Dialog element for TestResult
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
using Xunit.Abstractions;
using Xunit.Runner.iOS;
#if XAMCORE_2_0
using UIKit;
#else
using MonoTouch.UIKit;
#endif

using MonoTouch.Dialog;


namespace Xunit.Runners.UI {
	
	class TestResultElement : StyledMultilineElement {

        public TestResultElement(MonoTestCase result) : 
			base (result.Message ?? "Unknown error", result.StackTrace, UITableViewCellStyle.Subtitle)
		{
		}
	}
}
