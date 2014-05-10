using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Xunit.Abstractions;

namespace Xunit.Runner.iOS
{
    class MonoTestCase
    {
        public string AssemblyFileName { get; private set; }
        public ITestCase TestCase { get; private set; }
        public bool ForceUniqueNames { get; private set; }

        public MonoTestCase(string assemblyFileName, ITestCase testCase, bool forceUniqueNames)
        {
            if (assemblyFileName == null) throw new ArgumentNullException("assemblyFileName");
            if (testCase == null) throw new ArgumentNullException("testCase");

            AssemblyFileName = assemblyFileName;
            TestCase = testCase;
            ForceUniqueNames = forceUniqueNames;
        }
    }
}