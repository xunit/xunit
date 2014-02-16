using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Xunit.Abstractions;

namespace Xunit
{
    // TODO: Need to figure out what might go here
    [Serializable]
    public class XunitDiscoveryOptions : TestFrameworkOptions
    {
        public XunitDiscoveryOptions() { }

        /// <inheritdoc/>
        protected XunitDiscoveryOptions(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
