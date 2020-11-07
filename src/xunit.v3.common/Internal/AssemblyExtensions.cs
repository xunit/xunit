#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace Xunit.Internal
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public static class AssemblyExtensions
	{
		/// <summary/>
		[return: NotNullIfNotNull("assembly")]
		public static string? GetLocalCodeBase(this Assembly? assembly) =>
			GetLocalCodeBase(assembly?.CodeBase, Path.DirectorySeparatorChar);

		/// <summary/>
		[return: NotNullIfNotNull("codeBase")]
		public static string? GetLocalCodeBase(
			string? codeBase,
			char directorySeparator)
		{
			if (codeBase == null)
				return null;

			if (!codeBase.StartsWith("file://", StringComparison.Ordinal))
				throw new ArgumentException($"Codebase '{codeBase}' is unsupported; must start with 'file://'.", nameof(codeBase));

			// "file:///path" is a local path; "file://machine/path" is a UNC
			var localFile = codeBase.Length > 7 && codeBase[7] == '/';

			// POSIX-style directories
			if (directorySeparator == '/')
			{
				if (localFile)
					return codeBase.Substring(7);

				throw new ArgumentException($"UNC-style codebase '{codeBase}' is not supported on POSIX-style file systems.", nameof(codeBase));
			}

			// Windows-style directories
			if (directorySeparator == '\\')
			{
				codeBase = codeBase.Replace('/', '\\');

				if (localFile)
					return codeBase.Substring(8);

				return codeBase.Substring(5);
			}

			throw new ArgumentException($"Unknown directory separator '{directorySeparator}'; must be one of '/' or '\\'.", nameof(directorySeparator));
		}
	}
}
