// Imported from https://github.com/aspnet/Common/blob/a7b9be2e5020a364765efe2a33f50fa237979980/shared/Microsoft.Extensions.CommandLineUtils.Sources/Utilities/DotNetMuxer.cs

using System;
using System.Diagnostics;
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

        var mainModule = Process.GetCurrentProcess().MainModule;
        if (!string.IsNullOrEmpty(mainModule?.FileName)
            && Path.GetFileName(mainModule.FileName).Equals(fileName, StringComparison.OrdinalIgnoreCase))
        {
            return mainModule.FileName;
        }

        // if Process.MainModule is not available or it does not equal "dotnet(.exe)?", fallback to navigating to the muxer
        // by using the location of the shared framework

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
    }
}
