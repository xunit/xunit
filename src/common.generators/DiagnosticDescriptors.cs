using Microsoft.CodeAnalysis;
using static Microsoft.CodeAnalysis.DiagnosticSeverity;

namespace Xunit.Generators;

static class DiagnosticDescriptors
{
	static DiagnosticDescriptor Diagnostic(
		string id,
		string title,
		DiagnosticSeverity defaultSeverity,
		string messageFormat) =>
			new(
				id,
				title,
				messageFormat,
				"Generators",
				defaultSeverity,
				isEnabledByDefault: true,
				helpLinkUri: $"https://xunit.net/xunit.analyzers/rules/{id}"
			);

	// === Overlap with existing analyzers ===

	// NOTE: Don't update messages here without updating the analyzers project as well (must have identical messages so we don't double up)

	public static DiagnosticDescriptor X1000_TestClassMustBePublic { get; } =
		Diagnostic(
			"xUnit1000",
			"Test classes must be public",
			Error,
			"Test classes must be public. Add or change the visibility modifier of the test class to public."
		);

	public static DiagnosticDescriptor X1001_FactMethodMustNotHaveParameters { get; } =
		Diagnostic(
			"xUnit1001",
			"Fact methods cannot have parameters",
			Error,
			"Fact methods cannot have parameters. Remove the parameters from the method or convert it into a Theory."
		);

	public static DiagnosticDescriptor X1002_TestMethodMustNotHaveMultipleFactAttributes { get; } =
		Diagnostic(
			"xUnit1002",
			"Test methods cannot have multiple Fact or Theory attributes",
			Error,
			"Test methods cannot have multiple Fact or Theory attributes. Remove all but one of the attributes."
		);

	public static DiagnosticDescriptor X1007_ClassDataAttributeMustPointAtValidClass { get; } =
		Diagnostic(
			"xUnit1007",
			"ClassData must point at a valid class",
			Error,
			"ClassData must point at a valid class. The class {0} must be public, not abstract, with an empty constructor, and implement IEnumerable<object[]>, IAsyncEnumerable<object[]>, IEnumerable<ITheoryDataRow>, or IAsyncEnumerable<ITheoryDataRow>."
		);

	public static DiagnosticDescriptor X1015_MemberDataMustReferenceExistingMember { get; } =
		Diagnostic(
			"xUnit1015",
			"MemberData must reference an existing member",
			Error,
			"MemberData must reference an existing member '{0}' on type '{1}'. Fix the member reference, or add the missing data member."
		);

	public static DiagnosticDescriptor X1016_MemberDataMustReferencePublicMember { get; } =
		Diagnostic(
			"xUnit1016",
			"MemberData must reference a public member",
			Error,
			"MemberData must reference a public member. Add or change the visibility of the data member to public."
		);

	public static DiagnosticDescriptor X1017_MemberDataMustReferenceStaticMember { get; } =
		Diagnostic(
			"xUnit1017",
			"MemberData must reference a static member",
			Error,
			"MemberData must reference a static member. Add the static modifier to the data member."
		);

	public static DiagnosticDescriptor X1018_MemberDataMustReferenceValidMemberKind { get; } =
		Diagnostic(
			"xUnit1018",
			"MemberData must reference a valid member kind",
			Error,
			"MemberData must reference a property, field, or method. Convert the data member to a compatible member type."
		);

	public static DiagnosticDescriptor X1019_MemberDataMustReferenceMemberOfValidType { get; } =
		Diagnostic(
			"xUnit1019",
			"MemberData must reference a member providing a valid data type",
			Error,
			"MemberData must reference a data type assignable to 'System.Collections.Generic.IEnumerable<object[]>', 'System.Collections.Generic.IAsyncEnumerable<object[]>', 'System.Collections.Generic.IEnumerable<Xunit.ITheoryDataRow>', 'System.Collections.Generic.IAsyncEnumerable<Xunit.ITheoryDataRow>', 'System.Collections.Generic.IEnumerable<System.Runtime.CompilerServices.ITuple>', or 'System.Collections.Generic.IAsyncEnumerable<System.Runtime.CompilerServices.ITuple>'. The referenced type '{0}' is not valid."
		);

	public static DiagnosticDescriptor X1020_MemberDataPropertyMustHaveGetter { get; } =
		Diagnostic(
			"xUnit1020",
			"MemberData must reference a property with a public getter",
			Error,
			"MemberData must reference a property with a public getter. Add a public getter to the data member, or change the visibility of the existing getter to public."
		);

	public static DiagnosticDescriptor X1024_TestMethodCannotHaveOverloads { get; } =
		Diagnostic(
			"xUnit1024",
			"Test methods cannot have overloads",
			Error,
			"Test method '{0}' on test class '{1}' has the same name as another method declared on class '{2}'. Rename method(s) so that there are no overloaded names."
		);

	public static DiagnosticDescriptor X1027_CollectionDefinitionClassMustBePublic { get; } =
		Diagnostic(
			"xUnit1027",
			"Collection definition classes must be public",
			Error,
			"Collection definition classes must be public. Add or change the visibility modifier of the collection definition class to public."
		);

	public static DiagnosticDescriptor X1028_TestMethodHasInvalidReturnType { get; } =
		Diagnostic(
			"xUnit1028",
			"Test method must have valid return type",
			Error,
			"Test methods must have a supported return type. Valid types are: void, Task, ValueTask. Change the return type to one of the compatible types."
		);

	public static DiagnosticDescriptor X1032_TestClassCannotBeNestedInGenericClass { get; } =
		Diagnostic(
			"xUnit1032",
			"Test classes cannot be nested within a generic class",
			Error,
			"Test classes cannot be nested within a generic class. Move the test class out of the class it is nested in."
		);

	public static DiagnosticDescriptor X1036_MemberDataArgumentsMustMatchMethodParameters_ExtraValue { get; } =
		Diagnostic(
			"xUnit1036",
			"There is no matching method parameter",
			Error,
			"There is no matching method parameter for value: {0}. Remove unused value(s), or add more parameter(s)."
		);

	public static DiagnosticDescriptor X1049_DoNotUseAsyncVoidForTestMethods_V3 { get; } =
		Diagnostic(
			"xUnit1049",
			"Do not use 'async void' for test methods as it is no longer supported",
			Error,
			"Support for 'async void' unit tests has been removed from xUnit.net v3. Convert the test to 'async Task' or 'async ValueTask' instead."
		);

	// === Unique to Native AOT ===

	public static DiagnosticDescriptor X9000_TypeMustHaveCorrectPublicConstructor { get; } =
		Diagnostic(
			"xUnit9000",
			"Type must have non-obsolete public constructor",
			Error,
			"Type '{0}' must have a non-obsolete public constructor: public {0}({1})"
		);

	public static DiagnosticDescriptor X9001_TypeMustImplementInterface { get; } =
		Diagnostic(
			"xUnit9001",
			"Type must implement appropriate interface",
			Error,
			"Type '{0}' must implement interface '{1}'"
		);

	public static DiagnosticDescriptor X9002_TypeMustHaveStaticPublicProperty { get; } =
		Diagnostic(
			"xUnit9002",
			"Type must have public static property",
			Error,
			"Type '{0}' must include a public static property named '{1}' returning {2}"
		);

	public static DiagnosticDescriptor X9003_TypeMustHaveSinglePublicConstructor { get; } =
		Diagnostic(
			"xUnit9003",
			"Type must have a single public non-static constructor",
			Error,
			"{0} '{1}' must have a single public non-static constructor"
		);

	public static DiagnosticDescriptor X9004_TypeMustBePublicOrInternal { get; } =
		Diagnostic(
			"xUnit9004",
			"Type must be public or internal",
			Error,
			"{0} type '{1}' must be declared public or internal"
		);

	public static DiagnosticDescriptor X9005_GenericCollectionDefinitionNotSupported { get; } =
		Diagnostic(
			"xUnit9005",
			"Generic collection definitions are not supported",
			Error,
			"Generic collection definitions require reflection features unavailable in Native AOT"
		);

	public static DiagnosticDescriptor X9006_CannotSetBothSkipUnlessAndSkipWhen { get; } =
		Diagnostic(
			"xUnit9006",
			"Tests cannot set both SkipUnless and SkipWhen",
			Error,
			"Tests cannot set both SkipUnless and SkipWhen"
		);

	public static DiagnosticDescriptor X9007_TestClassCannotImplementICollectionFixture { get; } =
		Diagnostic(
			"xUnit9007",
			"Test classes may not be decorated with ICollectionFixture<>",
			Error,
			"Test class '{0}' may not be decorated with ICollectionFixture<> (decorate the test collection class instead)"
		);

	public static DiagnosticDescriptor X9008_CulturedTestMustHaveAtLeastOneCulture { get; } =
		Diagnostic(
			"xUnit9008",
			"Cultured test methods must have at least one culture",
			Error,
			"Cultured test methods must have at least one culture"
		);

	public static DiagnosticDescriptor X9009_FactCannotBeGeneric { get; } =
		Diagnostic(
			"xUnit9009",
			"Fact methods cannot be generic",
			Error,
			"Fact methods cannot be generic"
		);

	public static DiagnosticDescriptor X9010_TheoryMethodCannotBeGeneric { get; } =
		Diagnostic(
			"xUnit9010",
			"Theory methods cannot be generic",
			Error,
			"Generic theory methods require reflection features unavailable in Native AOT. Convert the method to be non-generic."
		);

	public static DiagnosticDescriptor X9011_TheoryParameterCannotBeParams { get; } =
		Diagnostic(
			"xUnit9011",
			"Theory parameter cannot use params modifier",
			Error,
			"The params modifier cannot be used with a theory parameter in Native AOT. Remove the modifier and create the arrays in the data sources instead."
		);

	public static DiagnosticDescriptor X9012_MemberDataMemberCannotBeOverloaded { get; } =
		Diagnostic(
			"xUnit9012",
			"MemberData member may not be overloaded",
			Error,
			"Member data '{0}.{1}' is ambiguous. MemberData members may not be overloaded."
		);

	public static DiagnosticDescriptor X9013_MemberDataTypeMustBePublicOrInternal { get; } =
		Diagnostic(
			"xUnit9013",
			"MemberData type must be either public or internal",
			Error,
			"Member data '{0}.{1}' must be in a class that is declared either public or internal"
		);

	public static DiagnosticDescriptor X9014_MemberDataParameterCannotBeParams { get; } =
		Diagnostic(
			"xUnit9014",
			"MemberData parameter cannot use params modifier",
			Error,
			"The params modifier on parameter '{0}' of {1}.{2} cannot be used for [MemberData] in Native AOT. Remove the modifier and create the arrays the [MemberData] arguments yourself."
		);

	public static DiagnosticDescriptor X9015_MemberDataParameterCannotBeParams { get; } =
		Diagnostic(
			"xUnit9015",
			"There is no matching MemberData method argument",
			Error,
			"There is no matching argument for {0}.{1} parameter: {2} {3}. Remove unused parameter(s), or add more argument(s)."
		);
}
