using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Default implementation of <see cref="IDataDiscoverer"/>. Uses reflection to find the
/// data associated with <see cref="DataAttribute"/>; may return <c>null</c> when called
/// without reflection-based abstraction implementations.
/// </summary>
public class DataDiscoverer : IDataDiscoverer
{
	/// <inheritdoc/>
	public virtual ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		Guard.ArgumentNotNull(dataAttribute);
		Guard.ArgumentNotNull(testMethod);
		Guard.ArgumentNotNull(disposalTracker);

		if (dataAttribute is _IReflectionAttributeInfo reflectionDataAttribute && testMethod is _IReflectionMethodInfo reflectionTestMethod)
			return ((DataAttribute)reflectionDataAttribute.Attribute).GetData(reflectionTestMethod.MethodInfo, disposalTracker);

		return new(default(IReadOnlyCollection<ITheoryDataRow>));
	}

	/// <inheritdoc/>
	public virtual bool SupportsDiscoveryEnumeration(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod)
	{
		Guard.ArgumentNotNull(dataAttribute);
		Guard.ArgumentNotNull(testMethod);

		return true;
	}
}
