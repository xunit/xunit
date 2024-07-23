using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xunit.Internal;
using Xunit.Sdk;

#if XUNIT_RUNNER_COMMON
namespace Xunit.Runner.Common;
#else
namespace Xunit.v3;
#endif

/// <summary>
/// Default implementation of <see cref="ITestFinished"/>.
/// </summary>
[JsonTypeID("test-finished")]
sealed partial class TestFinished : TestResultMessage, ITestFinished
{
	/// <summary>
	/// An empty set of attachments that can be used when none are provided.
	/// </summary>
	internal static readonly IReadOnlyDictionary<string, TestAttachment> EmptyAttachments = new ReadOnlyDictionary<string, TestAttachment>(new Dictionary<string, TestAttachment>());

	/// <inheritdoc/>
	protected override void Serialize(JsonObjectSerializer serializer)
	{
		Guard.ArgumentNotNull(serializer);

		base.Serialize(serializer);

		if (Attachments.Count > 0)
			using (var attachmentsObj = serializer.SerializeObject(nameof(Attachments)))
				foreach (var attachment in Attachments)
					attachmentsObj.Serialize(attachment.Key, attachment.Value.ToString());
	}
}
