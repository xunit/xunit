// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET452 || NETCOREAPP1_0

namespace Internal.Microsoft.Extensions.DependencyModel
{
    internal interface IEnvironment
    {
        string GetEnvironmentVariable(string name);
    }
}

#endif
