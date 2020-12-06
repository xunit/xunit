using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// INTERNAL INTERFACE. DO NOT USE.
	/// </summary>
	public interface ITestCaseBulkDeserializer
	{
		/// <summary/>
		List<KeyValuePair<string?, ITestCase?>> BulkDeserialize(List<string> serializations);
	}
}
