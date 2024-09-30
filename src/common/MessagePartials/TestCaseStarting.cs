using System.Globalization;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestCaseStarting"/>.
/// </summary>
[JsonTypeID("test-case-starting")]
sealed partial class TestCaseStarting : TestCaseMessage, ITestCaseStarting
{
	string ITestCaseMetadata.UniqueID =>
		TestCaseUniqueID;

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		serializer.Serialize(nameof(Explicit), Explicit);
		serializer.Serialize(nameof(SkipReason), SkipReason);
		serializer.Serialize(nameof(SourceFilePath), SourceFilePath);
		serializer.Serialize(nameof(SourceLineNumber), SourceLineNumber);
		serializer.Serialize(nameof(TestCaseDisplayName), TestCaseDisplayName);
		serializer.Serialize(nameof(TestClassMetadataToken), TestClassMetadataToken);
		serializer.Serialize(nameof(TestClassName), TestClassName);
		serializer.Serialize(nameof(TestClassNamespace), TestClassNamespace);
		serializer.Serialize(nameof(TestClassSimpleName), TestClassSimpleName);
		serializer.Serialize(nameof(TestMethodMetadataToken), TestMethodMetadataToken);
		serializer.Serialize(nameof(TestMethodName), TestMethodName);
		serializer.SerializeStringArray(nameof(TestMethodParameterTypesVSTest), TestMethodParameterTypesVSTest);
		serializer.Serialize(nameof(TestMethodReturnTypeVSTest), TestMethodReturnTypeVSTest);
		serializer.SerializeTraits(nameof(Traits), Traits);
	}

	/// <inheritdoc/>
	public override string ToString() =>
		string.Format(CultureInfo.CurrentCulture, "{0} name={1}", base.ToString(), TestCaseDisplayName.Quoted());
}
