using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public class DefaultTestCaseBulkDeserializer : ITestCaseBulkDeserializer
	{
		readonly _ITestFrameworkExecutor executor;

		/// <summary/>
		public DefaultTestCaseBulkDeserializer(_ITestFrameworkExecutor executor)
		{
			Guard.ArgumentNotNull(nameof(executor), executor);

			this.executor = executor;
		}

		/// <inheritdoc/>
		public List<KeyValuePair<string?, ITestCase?>> BulkDeserialize(List<string> serializations) =>
			serializations
				.Select(serialization => executor.Deserialize(serialization))
				.Select(testCase => new KeyValuePair<string?, ITestCase?>(testCase?.UniqueID, testCase))
				.ToList();
	}
}
