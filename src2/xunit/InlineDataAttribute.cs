using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit
{
    [CLSCompliant(false)]
    [DataDiscoverer(DiscovererType = typeof(InlineDataDiscoverer))]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class InlineDataAttribute : DataAttribute
    {
        public InlineDataAttribute(params object[] data) { }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            // This should never be called, because the discoverer can always find the data.
            throw new InvalidOperationException();
        }
    }

    //public abstract class DataAdapterDataAttribute : DataAttribute
    //{
    //    // Discovery: never?
    //    // GetData: get the data
    //}

    //public class OleDbDataAttribute : DataAdapterDataAttribute
    //{
    //}

    //public class ExcelDataAttribute : OleDbDataAttribute
    //{
    //}

    //public class SqlServerDataAttibute : OleDbDataAttribute
    //{
    //}
}