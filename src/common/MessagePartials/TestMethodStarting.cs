using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestMethodStarting"/>.
/// </summary>
[JsonTypeID("test-method-starting")]
sealed partial class TestMethodStarting : TestMethodMessage, ITestMethodStarting
{
	string ITestMethodMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestMethodUniqueID, nameof(TestMethodUniqueID));

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(MethodName), MethodName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} method={1}", base.ToString(), MethodName.Quoted());
}
