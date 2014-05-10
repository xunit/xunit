// TestRocks.cs: Helpers
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


using Xunit.Abstractions;

namespace Xunit.Runners.UI {
	
	static class TestRock {
		
		const string XunitExceptionPrefix = "Xunit.";

		static public bool IsSkipped (this ITestResultMessage result)
		{
			return  result is ITestSkipped;
		}

        static public bool IsSuccess(this ITestResultMessage result)
		{
			return result is ITestPassed;
		}

        static public bool IsFailure(this ITestResultMessage result)
        {
            return result is ITestFailed;
        }

		// remove the nunit exception message from the "real" message
        static public string GetMessage(this ITestResultMessage result)
		{
			string m = result.Output;
			if (m == null)
				return "Unknown error";
            if (!m.StartsWith(XunitExceptionPrefix))
				return m;
			return m.Substring (m.IndexOf (" : ") + 3);
		}
	}
}