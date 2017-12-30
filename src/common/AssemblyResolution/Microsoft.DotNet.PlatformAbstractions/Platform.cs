// Copyright(c) .NET Foundation and contributors.All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET452 || NETSTANDARD1_5 || NETCOREAPP1_0 || NETCOREAPP2_0

namespace Internal.Microsoft.DotNet.PlatformAbstractions
{
    internal enum Platform
    {
        Unknown = 0,
        Windows = 1,
        Linux = 2,
        Darwin = 3
    }
}

#endif
