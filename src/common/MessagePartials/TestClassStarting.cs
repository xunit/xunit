using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestClassStarting"/>.
/// </summary>
[JsonTypeID("test-class-starting")]
sealed partial class TestClassStarting : TestClassMessage, ITestClassStarting
{
	string ITestClassMetadata.UniqueID =>
		this.ValidateNullablePropertyValue(TestClassUniqueID, nameof(TestClassUniqueID));

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(TestClassName), TestClassName);
		serializer.Serialize(nameof(TestClassNamespace), TestClassNamespace);
		serializer.Serialize(nameof(TestClassSimpleName), TestClassSimpleName);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} class={1}", base.ToString(), TestClassName.Quoted());
}
