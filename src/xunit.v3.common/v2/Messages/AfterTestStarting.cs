using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="IAfterTestStarting"/>.
	/// </summary>
	public class AfterTestStarting : TestMessage, IAfterTestStarting
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AfterTestStarting"/> class.
		/// </summary>
		public AfterTestStarting(
			ITest test,
			string attributeName)
				: base(test)
		{
			Guard.ArgumentNotNull(nameof(attributeName), attributeName);

			AttributeName = attributeName;
		}

		/// <inheritdoc/>
		public string AttributeName { get; }
	}
}
