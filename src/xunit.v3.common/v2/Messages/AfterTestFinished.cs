using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="IAfterTestFinished"/>.
	/// </summary>
	public class AfterTestFinished : TestMessage, IAfterTestFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AfterTestFinished"/> class.
		/// </summary>
		public AfterTestFinished(
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
