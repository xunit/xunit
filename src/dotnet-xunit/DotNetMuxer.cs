// Imported from https://github.com/aspnet/Common/blob/bc56611fd65f1dfc7bc0ab88970bed07b29f30dc/shared/Microsoft.Extensions.CommandLineUtils.Sources/Utilities/DotNetMuxer.cs

using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Utilities for finding the "dotnet.exe" file from the currently running .NET Core application
/// </summary>
internal static class DotNetMuxer
{
    private const string MuxerName = "dotnet";

    static DotNetMuxer()
    {
        MuxerPath = TryFindMuxerPath();
    }

    /// <summary>
    /// The full filepath to the .NET Core muxer.
    /// </summary>
    public static string MuxerPath { get; }

    /// <summary>
    /// Finds the full filepath to the .NET Core muxer,
    /// or returns a string containing the default name of the .NET Core muxer ('dotnet').
    /// </summary>
    /// <returns>The path or a string named 'dotnet'</returns>
    public static string MuxerPathOrDefault()
        => MuxerPath ?? MuxerName;

    private static string TryFindMuxerPath()
    {
        var fileName = MuxerName;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            fileName += ".exe";
        }

#if true
        return fileName;
#else
        // This is broken right now for anybody who has an incomplete set of SDKs in ~\.dotnet, which
        // will commonly happen for users who are running KoreBuild (among other things).

        var fxDepsFile = AppContext.GetData("FX_DEPS_FILE") as string;

        if (string.IsNullOrEmpty(fxDepsFile))
        {
            return null;
        }

        var muxerDir = new FileInfo(fxDepsFile) // Microsoft.NETCore.App.deps.json
            .Directory? // (version)
            .Parent? // Microsoft.NETCore.App
            .Parent? // shared
            .Parent; // DOTNET_HOME

        if (muxerDir == null)
        {
            return null;
        }

        var muxer = Path.Combine(muxerDir.FullName, fileName);
        return File.Exists(muxer)
            ? muxer
            : null;
#endif
    }
}
