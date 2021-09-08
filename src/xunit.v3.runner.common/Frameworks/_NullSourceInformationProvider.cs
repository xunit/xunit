﻿namespace Xunit.Runner.Common
{
	/// <summary>
	/// A null implementation of <see cref="_ISourceInformationProvider"/> which always returns empty
	/// source information. Get the singleton via <see cref="Instance"/>.
	/// </summary>
	public class _NullSourceInformationProvider : _ISourceInformationProvider
	{
		_NullSourceInformationProvider() { }

		/// <summary>
		/// Gets the singleton instance of the <see cref="_NullSourceInformationProvider"/>.
		/// </summary>
		public static _NullSourceInformationProvider Instance { get; } = new _NullSourceInformationProvider();

		/// <inheritdoc/>
		(string? sourceFile, int? sourceLine) _ISourceInformationProvider.GetSourceInformation(
			string? testClassName,
			string? testMethodName) =>
				(null, null);
	}
}
