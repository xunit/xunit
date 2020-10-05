namespace Xunit.v3
{
	/// <summary>
	/// Represents source information about a test case.
	/// </summary>
	public interface _ISourceInformation
	{
		/// <summary>
		/// Gets the source file name. A <c>null</c> value indicates that the
		/// source file name is not known.
		/// </summary>
		string? FileName { get; }

		/// <summary>
		/// Gets the source file line number. A <c>null</c> value indicates that the
		/// source file line number is not known.
		/// </summary>
		int? LineNumber { get; set; }
	}
}
