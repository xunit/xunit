namespace Xunit.v3
{
	/// <summary>
	/// Default implementation of <see cref="_ISourceInformation"/>.
	/// </summary>
	public class _SourceInformation : _ISourceInformation
	{
		/// <inheritdoc/>
		public string? FileName { get; set; }

		/// <inheritdoc/>
		public int? LineNumber { get; set; }
	}
}
