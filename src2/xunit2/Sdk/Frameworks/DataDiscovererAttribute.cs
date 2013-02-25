using System;

namespace Xunit.Sdk
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DataDiscovererAttribute : AttributeBase
    {
        public Type DiscovererType { get; set; }
    }
}