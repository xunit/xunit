using System;
using System.Linq;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    internal class TestClassMessage : TestCollectionMessage, ITestClassMessage
    {
        public TestClassMessage(ITestCollection testCollection, string className)
            : base(testCollection)
        {
            ClassName = className;
        }

        /// <inheritdoc/>
        public string ClassName { get; private set; }
    }
}
