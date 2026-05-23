using Microsoft.CodeAnalysis;

namespace Xunit.Generators;

partial class Extensions
{
	static readonly HashSet<string> testMethodReturnTypes = ["void", Types.System.Threading.Tasks.Task, Types.System.Threading.Tasks.ValueTask];

	public static bool HasValidTestMethodReturnValue(this IMethodSymbol testMethod) =>
		testMethodReturnTypes.Contains(testMethod.ReturnType.ToCSharp(includeGlobal: false));
}
