using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk;

/// <summary>
/// Implementation of <see cref="IDataDiscoverer"/> for discovering <see cref="MemberDataAttribute"/>.
/// </summary>
public class MemberDataDiscoverer : DataDiscoverer
{
	/// <inheritdoc/>
	public override ValueTask<IReadOnlyCollection<ITheoryDataRow>?> GetData(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod,
		DisposalTracker disposalTracker)
	{
		try
		{
			return base.GetData(dataAttribute, testMethod, disposalTracker);
		}
		catch (ArgumentException)
		{
			// If we couldn't find the data on the base type, check if it is in current type.
			// This allows base classes to specify data that exists on a sub type, but not on the base type.
			if (testMethod is _IReflectionMethodInfo reflectionTestMethod &&
				dataAttribute is _IReflectionAttributeInfo reflectionDataAttribute &&
				reflectionDataAttribute.Attribute is MemberDataAttribute memberDataAttribute &&
				memberDataAttribute.MemberType is null &&
				reflectionTestMethod.Type is _IReflectionTypeInfo reflectionTestMethodType)
			{
				var newMemberDataAttribute = new MemberDataAttribute(memberDataAttribute.MemberName, memberDataAttribute.Arguments)
				{
					DisableDiscoveryEnumeration = memberDataAttribute.DisableDiscoveryEnumeration,
					Explicit = memberDataAttribute.Explicit,
					MemberType = reflectionTestMethodType.Type,
					Skip = memberDataAttribute.Skip,
					TestDisplayName = memberDataAttribute.TestDisplayName,
				};

				return newMemberDataAttribute.GetData(reflectionTestMethod.MethodInfo, disposalTracker);
			}

			throw;
		}
	}

	/// <inheritdoc/>
	public override bool SupportsDiscoveryEnumeration(
		_IAttributeInfo dataAttribute,
		_IMethodInfo testMethod)
	{
		Guard.ArgumentNotNull(dataAttribute);
		Guard.ArgumentNotNull(testMethod);

		return !dataAttribute.GetNamedArgument<bool>(nameof(MemberDataAttributeBase.DisableDiscoveryEnumeration));
	}
}
