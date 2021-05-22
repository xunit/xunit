using System.Collections.Generic;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// This class is responsible for discovering the data available in an implementation
	/// of <see cref="DataAttribute"/>. The discovery process may not always have access
	/// to reflection (i.e., running in Resharper), so the discoverer must make a best
	/// effort to return data, but may return null when there is not enough information
	/// available (for example, if reflection is required to answer the question).
	/// </summary>
	public interface IDataDiscoverer
	{
		/// <summary>
		/// Returns the data to be used to test the theory.
		/// </summary>
		/// <remarks>
		/// This will be called during
		/// discovery, at which point the <paramref name="testMethod"/> may or may not
		/// be backed by reflection (i.e., implementing <see cref="_IReflectionMethodInfo"/>).
		/// If the data is not available because reflection is required, then you may return
		/// <c>null</c> to inform xUnit that the quantity of data is unknown at this point.
		/// When the tests are run, if you returned back <c>null</c> during discovery, then this
		/// method will be called again to retrieve the data, this time guaranteed to provide
		/// an implementation of <see cref="_IReflectionMethodInfo"/>. At this time, you
		/// must return the actual data, and returning <c>null</c> is not legal.
		/// </remarks>
		/// <param name="dataAttribute">The data attribute being discovered</param>
		/// <param name="testMethod">The method that is being tested/discovered</param>
		/// <returns>The theory data (or null during discovery, if not enough
		/// information is available to enumerate the data)</returns>
		IReadOnlyCollection<object?[]>? GetData(
			_IAttributeInfo dataAttribute,
			_IMethodInfo testMethod
		);

		/// <summary>
		/// Returns <c>true</c> if the data attribute supports enumeration during
		/// discovery; <c>false</c> otherwise. Data attributes with expensive computational
		/// costs and/or randomized data sets should return <c>false</c>.
		/// </summary>
		bool SupportsDiscoveryEnumeration(
			_IAttributeInfo dataAttribute,
			_IMethodInfo testMethod
		);
	}
}
