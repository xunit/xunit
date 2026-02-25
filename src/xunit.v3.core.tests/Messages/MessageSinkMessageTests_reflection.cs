using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class MessageSinkMessageTests
{
	[Fact]
	public void ValidatesAllDerivedTypesAreSupported()
	{
		var excludedTypes = new HashSet<Type> {
			typeof(MessageSinkMessage),
			typeof(DerivedMessageSinkMessage),
		};
		var derivedTypes =
			typeof(MessageSinkMessage)
				.Assembly
				.GetTypes()
				.Where(t => !t.IsAbstract && !excludedTypes.Contains(t) && typeof(IMessageSinkMessage).IsAssignableFrom(t))
				.ToList();
		var missingTypes =
			derivedTypes
				.Where(t => t.GetCustomAttribute<JsonTypeIDAttribute>() is null)
				.ToList();

		if (missingTypes.Count > 0)
			throw new XunitException($"The following message classes are missing [JsonTypeID]:{Environment.NewLine}{string.Join(Environment.NewLine, missingTypes.Select(t => $"  - {t.SafeName()}").OrderBy(t => t))}");
	}
}
