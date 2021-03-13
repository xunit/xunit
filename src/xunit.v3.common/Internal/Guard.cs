#nullable enable  // This file is temporarily shared with xunit.v1.tests and xunit.v2.tests, which are not nullable-enabled

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Xunit.Internal
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public static class Guard
	{
		/// <summary/>
		public static void ArgumentNotNull(string argName, [NotNull] object? argValue)
		{
			if (argValue == null)
				throw new ArgumentNullException(argName);
		}

		/// <summary/>
		public static T ArgumentNotNull<T>(string argName, [NotNull] T? argValue)
			where T : class
		{
			if (argValue == null)
				throw new ArgumentNullException(argName);

			return argValue;
		}

		/// <summary/>
		public static T ArgumentNotNullOrEmpty<T>(string argName, [NotNull] T? argValue)
			where T : class, IEnumerable
		{
			ArgumentNotNull(argName, argValue);

			if (!argValue.GetEnumerator().MoveNext())
				throw new ArgumentException("Argument was empty", argName);

			return argValue;
		}

		/// <summary/>
		public static void ArgumentValid(string argName, string message, bool test)
		{
			if (!test)
				throw new ArgumentException(message, argName);
		}

		/// <summary/>
		public static T ArgumentValidNotNull<T>(string argName, string message, [NotNull] T? testValue)
			where T : class
		{
			if (testValue == null)
				throw new ArgumentException(message, argName);

			return testValue;
		}

		/// <summary/>
		public static T ArgumentValidNotNullOrEmpty<T>(string argName, string message, [NotNull] T? testValue)
			where T : class, IEnumerable
		{
			if (testValue == null || !testValue.GetEnumerator().MoveNext())
				throw new ArgumentException(message, argName);

			return testValue;
		}

		/// <summary/>
		public static string FileExists(string argName, [NotNull] string? fileName)
		{
			ArgumentNotNullOrEmpty(argName, fileName);
			ArgumentValid(argName, $"File not found: {fileName}", File.Exists(fileName));

			return fileName;
		}

		/// <summary/>
		public static T NotNull<T>(string message, [NotNull] T? value)
			where T : class
		{
			if (value == null)
				throw new InvalidOperationException(message);

			return value;
		}
	}
}
