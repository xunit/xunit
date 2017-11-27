// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if NET452 || NETCOREAPP1_0 || NETCOREAPP2_0

using System.Collections.Generic;

namespace Internal.Microsoft.Extensions.DependencyModel.Resolution
{
    internal interface ICompilationAssemblyResolver
    {
        bool TryResolveAssemblyPaths(CompilationLibrary library, List<string> assemblies);
    }
}

#endif
