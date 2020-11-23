using System;
using Xunit.Abstractions;

namespace Xunit.Runner.Common
{
	public partial struct StackFrameInfo
	{
		/// <summary>
		/// Creates a stack frame info from failure information.
		/// </summary>
		/// <param name="failureInfo">The failure information to inspect</param>
		/// <returns>The stack frame info</returns>
		public static StackFrameInfo FromFailure(IFailureInformation? failureInfo)
		{
			if (failureInfo == null)
				return None;

			var stackTraces = ExceptionUtility.CombineStackTraces(failureInfo);
			if (stackTraces != null)
			{
				foreach (var frame in stackTraces.Split(new[] { Environment.NewLine }, 2, StringSplitOptions.RemoveEmptyEntries))
				{
					var match = stackFrameRegex.Match(frame);
					if (match.Success)
						return new StackFrameInfo(match.Groups["file"].Value, int.Parse(match.Groups["line"].Value));
				}
			}

			return None;
		}

		/// <summary>
		/// Creates a tack frame from source information. This can be useful when simulating a
		/// stack frame in a non-exceptional situation (f.e., for a skipped test).
		/// </summary>
		/// <param name="sourceInfo">The source information to inspect</param>
		/// <returns>The stack frame info</returns>
		public static StackFrameInfo FromSourceInformation(ISourceInformation? sourceInfo)
		{
			if (sourceInfo == null)
				return None;

			return new StackFrameInfo(sourceInfo.FileName, sourceInfo.LineNumber ?? 0);
		}
	}
}
