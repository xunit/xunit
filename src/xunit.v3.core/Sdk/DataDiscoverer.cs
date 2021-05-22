using System;
using System.Collections.Generic;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Default implementation of <see cref="IDataDiscoverer"/>. Uses reflection to find the
	/// data associated with <see cref="DataAttribute"/>; may return <c>null</c> when called
	/// without reflection-based abstraction implementations.
	/// </summary>
	public class DataDiscoverer : IDataDiscoverer
	{
		/// <inheritdoc/>
		public virtual IReadOnlyCollection<object?[]>? GetData(
			_IAttributeInfo dataAttribute,
			_IMethodInfo testMethod)
		{
			Guard.ArgumentNotNull(nameof(dataAttribute), dataAttribute);
			Guard.ArgumentNotNull(nameof(testMethod), testMethod);

			if (dataAttribute is _IReflectionAttributeInfo reflectionDataAttribute &&
				testMethod is _IReflectionMethodInfo reflectionTestMethod)
			{
				var attribute = (DataAttribute)reflectionDataAttribute.Attribute;

				try
				{
					return attribute.GetData(reflectionTestMethod.MethodInfo);
				}
				catch (ArgumentException)
				{
					// If we couldn't find the data on the base type, check if it is in current type.
					// This allows base classes to specify data that exists on a sub type, but not on the base type.
					var reflectionTestMethodType = (_IReflectionTypeInfo)reflectionTestMethod.Type;
					if (attribute is MemberDataAttribute memberDataAttribute && memberDataAttribute.MemberType == null)
						memberDataAttribute.MemberType = reflectionTestMethodType.Type;

					return attribute.GetData(reflectionTestMethod.MethodInfo);
				}
			}

			return null;
		}

		/// <inheritdoc/>
		public virtual bool SupportsDiscoveryEnumeration(
			_IAttributeInfo dataAttribute,
			_IMethodInfo testMethod)
		{
			Guard.ArgumentNotNull(nameof(dataAttribute), dataAttribute);
			Guard.ArgumentNotNull(nameof(testMethod), testMethod);

			return true;
		}
	}
}
