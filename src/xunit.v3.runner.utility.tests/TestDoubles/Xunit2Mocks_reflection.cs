using Xunit.Abstractions;

namespace Xunit.Runner.v2;

public static partial class Xunit2Mocks
{
	static readonly Dictionary<string, List<string>> EmptyTraits = [];
	static readonly Guid OneGuid = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
	static readonly ITypeInfo TypeOfVoid = TypeInfo(name: typeof(void).FullName);
}
