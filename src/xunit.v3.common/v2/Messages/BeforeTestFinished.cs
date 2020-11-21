using Xunit.Abstractions;
using Xunit.Internal;

namespace Xunit.Runner.v2
{
	/// <summary>
	/// Default implementation of <see cref="IBeforeTestFinished"/>.
	/// </summary>
	public class BeforeTestFinished : TestMessage, IBeforeTestFinished
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BeforeTestFinished"/> class.
		/// </summary>
		public BeforeTestFinished(
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
