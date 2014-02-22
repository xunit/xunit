using System;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    internal class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation
    {
        public string FileName { get; set; }

        public int? LineNumber { get; set; }
    }
}