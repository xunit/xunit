using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestCollectionStarting"/>.
/// </summary>
[JsonTypeID("test-collection-starting")]
sealed partial class TestCollectionStarting : TestCollectionMessage, ITestCollectionStarting
{
	string ITestCollectionMetadata.UniqueID =>
		TestCollectionUniqueID;

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestCollectionClassName), TestCollectionClassName);
		serializer.Serialize(nameof(TestCollectionDisplayName), TestCollectionDisplayName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1} class={2}", base.ToString(), TestCollectionDisplayName.Quoted(), TestCollectionClassName.Quoted());
}
