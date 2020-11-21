using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="ISourceInformation"/>.
	/// </summary>
	public class SourceInformation : LongLivedMarshalByRefObject, ISourceInformation
	{
		/// <inheritdoc/>
		public string? FileName { get; set; }

		/// <inheritdoc/>
		public int? LineNumber { get; set; }

		/// <inheritdoc/>
		public void Serialize(IXunitSerializationInfo info)
		{
			info.AddValue("FileName", FileName);
			info.AddValue("LineNumber", LineNumber);
		}

		/// <inheritdoc/>
		public void Deserialize(IXunitSerializationInfo info)
		{
			FileName = info.GetValue<string>("FileName");
			LineNumber = info.GetValue<int?>("LineNumber");
		}
	}
}
