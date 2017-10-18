#if NET452 || NETCOREAPP1_0

using System;
using System.IO;

namespace Internal.Microsoft.Extensions.DependencyModel
{
    internal interface IDependencyContextReader: IDisposable
    {
        DependencyContext Read(Stream stream);
    }
}

#endif
