using System.Collections.Generic;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// Contains the information by <see cref="IFrontController.Run"/>.
	/// </summary>
	public class FrontControllerRunSettings
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FrontControllerFindSettings"/> class.
		/// </summary>
		/// <param name="options">The options used during execution</param>
		/// <param name="serializedTestCases">The test cases to be run</param>
		public FrontControllerRunSettings(
			_ITestFrameworkExecutionOptions options,
			IEnumerable<string> serializedTestCases)
		{
			Options = Guard.ArgumentNotNull(nameof(options), options);
			SerializedTestCases = Guard.ArgumentNotNull(nameof(serializedTestCases), serializedTestCases);
		}

		/// <summary>
		/// The options used during execution.
		/// </summary>
		public _ITestFrameworkExecutionOptions Options { get; }

		/// <summary>
		/// Get the list of test cases to be run.
		/// </summary>
		public IEnumerable<string> SerializedTestCases { get; }
	}
}
