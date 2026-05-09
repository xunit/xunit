#nullable enable

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.Generators
{
	/// <summary>
	/// Extension methods used by xUnit.net source generators.
	/// </summary>
	/// <remarks>
	/// This class is marked as partial so that it can be extended by developers importing this source code.
	/// </remarks>
	internal static partial class Extensions
	{
		// Based on SymbolDisplayFormat.FullyQualifiedFormat + nullable
		static readonly SymbolDisplayFormat CompilableDisplayFormat_WithGlobal = new SymbolDisplayFormat(
			globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
			typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes
		);
		static readonly SymbolDisplayFormat CompilableDisplayFormat_WithoutGlobal = new SymbolDisplayFormat(
			globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
			typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
			genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
			miscellaneousOptions:
				SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
				SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
				SymbolDisplayMiscellaneousOptions.UseSpecialTypes
		);

		// List from https://learn.microsoft.com/dotnet/csharp/programming-guide/strings/#string-escape-sequences
		static readonly Dictionary<char, string> escapes = new Dictionary<char, string>()
		{
			['\''] = "\\\'",
			['"'] = "\\\"",
			['\\'] = "\\\\",
			['\u0000'] = "\\0",
			['\u0001'] = "\\u0001",
			['\u0002'] = "\\u0002",
			['\u0003'] = "\\u0003",
			['\u0004'] = "\\u0004",
			['\u0005'] = "\\u0005",
			['\u0006'] = "\\u0006",
			['\u0007'] = "\\a",
			['\u0008'] = "\\b",
			['\u0009'] = "\\t",
			['\u000A'] = "\\n",
			['\u000B'] = "\\v",
			['\u000C'] = "\\f",
			['\u000D'] = "\\r",
			['\u000E'] = "\\u000E",
			['\u000F'] = "\\u000F",
			['\u0010'] = "\\u0010",
			['\u0011'] = "\\u0011",
			['\u0012'] = "\\u0012",
			['\u0013'] = "\\u0013",
			['\u0014'] = "\\u0014",
			['\u0015'] = "\\u0015",
			['\u0016'] = "\\u0016",
			['\u0017'] = "\\u0017",
			['\u0018'] = "\\u0018",
			['\u0019'] = "\\u0019",
			['\u001A'] = "\\u001A",
			['\u001B'] = "\\e",
			['\u001C'] = "\\u001C",
			['\u001D'] = "\\u001D",
			['\u001E'] = "\\u001E",
			['\u001F'] = "\\u001F",
		};
		static readonly HashSet<string> genericTaskTypes = new HashSet<string> { "System.Threading.Tasks.Task<>", "System.Threading.Tasks.ValueTask<>" };
		static readonly Func<object?, bool> notNullTest = x => x != null;
		static readonly HashSet<string> theoryDataTypes = new HashSet<string> {
			"object", "object?",
			"object[]", "object?[]",
			"System.Runtime.CompilerServices.ITuple",
			"Xunit.ITheoryDataRow",
		};

		static string Escape(char value) =>
			escapes.TryGetValue(value, out var escaped) ? escaped : value.ToString();

		static string Escape(string value)
		{
			var result = new StringBuilder(value.Length);

			foreach (var c in value)
				if (escapes.TryGetValue(c, out var escaped))
					result.Append(escaped);
				else
					result.Append(c);

			return result.ToString();
		}

		static (bool IsAsyncEnumerable, ITypeSymbol EnumerableType)? GetEnumerable(
			this ITypeSymbol? type,
			INamedTypeSymbol objectType)
		{
			if (type is INamedTypeSymbol namedType)
			{
				if (namedType.ToCSharp(includeGlobal: false) == "System.Collections.IEnumerable")
					return (false, objectType);

				if (!namedType.IsGenericType || namedType.TypeArguments.Length != 1)
					return null;

				return namedType.ConstructUnboundGenericType().ToCSharp(includeGlobal: false) switch
				{
					"System.Collections.Generic.IAsyncEnumerable<>" => (true, namedType.TypeArguments[0]),
					"System.Collections.Generic.IEnumerable<>" => (false, namedType.TypeArguments[0]),
					_ => null,
				};
			}

			return null;
		}

		/// <summary>
		/// Gets information about the return type of a theory data source.
		/// </summary>
		/// <param name="type">The theory data return value type</param>
		/// <param name="objectType">The type that represents <see cref="object"/></param>
		/// <remarks>
		/// </remarks>
		public static TheoryDataInfo? GetTheoryDataInfo(
			this ITypeSymbol type,
			INamedTypeSymbol objectType)
		{
			var taskFreeType = UnwrapTask(type);
			if (taskFreeType.NullableAnnotation == NullableAnnotation.Annotated)
				return null;

			var isTask = !SymbolEqualityComparer.Default.Equals(taskFreeType, type);
			var isAsyncEnumerable = false;
			ITypeSymbol? enumerableType = null;

			var enumerable = GetEnumerable(taskFreeType, objectType);
			if (enumerable != null)
			{
				isAsyncEnumerable = enumerable.Value.IsAsyncEnumerable;
				enumerableType = enumerable.Value.EnumerableType;
			}
			else
			{
				foreach (var @interface in taskFreeType.AllInterfaces)
				{
					enumerable = GetEnumerable(@interface, objectType);
					if (enumerable != null)
					{
						isAsyncEnumerable = enumerable.Value.IsAsyncEnumerable;
						enumerableType = enumerable.Value.EnumerableType;
						break;
					}
				}
			}

			if (enumerableType is null)
				return null;

			if (theoryDataTypes.Contains(enumerableType.ToCSharp(includeGlobal: false))
					|| enumerableType.AllInterfaces.Any(i => theoryDataTypes.Contains(i.ToCSharp(includeGlobal: false))))
				return new TheoryDataInfo(enumerableType, isTask, isAsyncEnumerable);

			return null;
		}

		/// <summary>
		/// Determines if the given type has a constructor with parameters that match the requested types
		/// </summary>
		/// <param name="symbol">The type</param>
		/// <param name="parameterTypes">The parameter types to match</param>
		/// <remarks>
		/// This is a shallow check, for exact type matches.
		/// </remarks>
		public static bool HasConstructorParameters(
			this INamedTypeSymbol symbol,
			params string[] parameterTypes)
		{
			if (symbol is null || parameterTypes is null || !symbol.IsSafeToReference())
				return false;

			var ctors =
				symbol
					.Constructors
					.Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == parameterTypes.Length);

			return ctors.Any(ctor => ctorMatches(ctor, parameterTypes));

			static bool ctorMatches(
				IMethodSymbol ctor,
				string[] parameterTypes)
			{
				for (var idx = 0; idx < parameterTypes.Length; ++idx)
				{
					var ctorParameter = ctor.Parameters[idx];
					var expectedType = parameterTypes[idx];

					if (ctorParameter.Type.ToString() != expectedType)
						return false;
				}

				return true;
			}
		}

		static IReadOnlyList<string> ImplementsAll(
			this ITypeSymbol symbol,
			params string[] fullyQualifiedInterfaceNames)
		{
			var result = new HashSet<string>(fullyQualifiedInterfaceNames);

			if (result.Count != 0)
			{
				var interfaces = symbol.AllInterfaces;

				foreach (var @interface in interfaces.Select(symbol => symbol.ToString()).WhereNotNull())
				{
					result.Remove(@interface);
					if (result.Count == 0)
						break;
				}
			}

			return result.ToImmutableList();
		}

		/// <summary>
		/// Determines if a symbol implements the given interface.
		/// </summary>
		/// <param name="symbol"></param>
		/// <param name="fullyQualifiedInterfaceName"></param>
		/// <returns></returns>
		public static bool ImplementsInterface(
			this ITypeSymbol symbol,
			string fullyQualifiedInterfaceName)
		{
			if (symbol is null || fullyQualifiedInterfaceName is null)
				return false;

			return symbol.AllInterfaces.Any(i => i.ToString() == fullyQualifiedInterfaceName);
		}

		/// <summary>
		/// Determines if a symbol implements all of the given interfaces.
		/// </summary>
		/// <param name="symbol"></param>
		/// <param name="fullyQualifiedInterfaceNames"></param>
		/// <returns></returns>
		public static bool ImplementsInterfaces(
			this ITypeSymbol symbol,
			params string[] fullyQualifiedInterfaceNames)
		{
			if (symbol is null || fullyQualifiedInterfaceNames is null)
				return false;

			if (fullyQualifiedInterfaceNames.Length == 0)
				return true;

			var missingInterfaces = symbol.ImplementsAll(fullyQualifiedInterfaceNames);
			return missingInterfaces.Count == 0;
		}

		/// <summary>
		/// Determins if a symbol either is the given type, or inherits from it (directly or indirectly).
		/// </summary>
		/// <param name="symbol">The symbol to verify</param>
		/// <param name="typeName">The type that must be in the inheritance hierarchy</param>
		public static bool InheritsFrom(
			this INamedTypeSymbol? symbol,
			string typeName)
		{
			if (symbol is null)
				return false;

			if (symbol.ToCSharp(includeGlobal: false) == typeName)
				return true;

			return InheritsFrom(symbol.BaseType, typeName);
		}

		/// <summary>
		/// Determine if a type is safe to be referenced in generated source. This ensures that
		/// the type itself, as well as any type arguments, are public or internal.
		/// </summary>
		/// <param name="type">The type to be validated</param>
		public static bool IsSafeToReference(this ITypeSymbol? type)
		{
			if (type is null)
				return false;

			if (type.TypeKind == TypeKind.TypeParameter)
				return false;

			if (type.DeclaredAccessibility != Accessibility.Public && type.DeclaredAccessibility != Accessibility.Internal)
				return false;

			if (type is INamedTypeSymbol namedType)
				return namedType.TypeArguments.All(IsSafeToReference);

			return true;
		}

		/// <summary>
		/// Gets a fixture factory for a given assembly fixture type.
		/// </summary>
		/// <param name="type">The fixture type</param>
		/// <remarks>
		/// This looks for a single static public constructor on a non-static, non-abstract type, and returns
		/// code which will create an instance of the assembly fixture type. Assembly fixture factories accept
		/// no parameters and return <c>ValueTask&lt;object?&gt;</c>.
		/// </remarks>
		public static string? ToAssemblyFixtureFactory(this INamedTypeSymbol type)
		{
			if (type.IsStatic || type.IsAbstract || !type.IsSafeToReference())
				return null;

			var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
			if (publicCtors.Length != 1 || publicCtors[0].Parameters.Length != 0)
				return null;

			return $"async () => {ToObjectFactory(type, publicCtors[0])}";
		}

		/// <summary>
		/// Creates a compiler safe name from an arbitrary string value.
		/// </summary>
		/// <param name="value">The string value</param>
		/// <remarks>
		/// The purpose of this method is to create a hash of a string value that is high likelihood of uniqueness
		/// that is also a compiler-safe name, to be used in a namespace or type name.
		/// </remarks>
		public static string ToCompilerSafeName(this string value)
		{
			using var hasher = SHA256.Create();

			return
				Convert
					.ToBase64String(hasher.ComputeHash(Encoding.UTF8.GetBytes(value)))
					.Substring(0, 9)
					.Replace('+', 'à')
					.Replace('/', 'á')
					.Replace('=', 'â');
		}

		static string ToConstructorInvocation(
			StringBuilder factoryBuilder,
			IMethodSymbol ctor,
			INamedTypeSymbol type,
			string typeDescription,
			string argumentLookupFormat,
			string objectFactoryFormat)
		{
			var testClassTypeName = type.ToCSharp();
			var parameterNamesInCode = new List<string>();

			if (ctor.Parameters.Length != 0)
			{
				var anyRequired = ctor.Parameters.Any(p => !p.IsOptional && !p.IsParams);

				if (anyRequired)
					factoryBuilder.Append(
@"	var missingParameters = new global::System.Collections.Generic.List<(string Type, string Name)>();
");

				for (var idx = 0; idx < ctor.Parameters.Length; ++idx)
				{
					var parameter = ctor.Parameters[idx];
					var parameterName = parameter.Name.ToCSharp();
					var parameterNameInCode = $"param{idx}";
					parameterNamesInCode.Add(parameterNameInCode);

					factoryBuilder.Append(
$@"	var {parameterNameInCode} = await {string.Format(CultureInfo.InvariantCulture, argumentLookupFormat, parameter.Type.ToCSharp())};
	if (!{parameterNameInCode}.Success)
");

					if (parameter.IsOptional)
					{
						var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
						factoryBuilder.Append(
$@"		{parameterNameInCode}.Result = {defaultValue.ToCSharp() ?? $"default({parameter.Type.ToCSharp(includeGlobal: false)})"};
");
					}
					else if (parameter.IsParams)
						factoryBuilder.Append(
$@"		{parameterNameInCode}.Result = [];
");
					else
						factoryBuilder.Append(
$@"		missingParameters.Add(({parameter.Type.ToDisplayString().ToCSharp()}, {parameterName}));
");
				}

				if (anyRequired)
					factoryBuilder.Append(
$@"	if (missingParameters.Count != 0)
		throw new global::Xunit.Sdk.TestPipelineException(
			string.Format(
				global::System.Globalization.CultureInfo.CurrentCulture,
				""{typeDescription} '{type}' had one or more unresolved constructor arguments: {{0}}"",
				string.Join("", "", global::System.Linq.Enumerable.Select(missingParameters, p => $""{{p.Type}} {{p.Name}}""))
			)
		);
");
			}

			factoryBuilder.Append(
$@"	var instance = new {testClassTypeName}({string.Join(", ", parameterNamesInCode.Select(p => $"{p}.Result!"))});
");

			factoryBuilder.Append(
$@"	return {string.Format(CultureInfo.InvariantCulture, objectFactoryFormat, "instance")};
}}");

			return factoryBuilder.ToString();
		}

		/// <summary>
		/// Formats a non-<see langword="null"/> object, appropriate to place into generated source; a <see langword="null"/>
		/// object will return <see langword="null"/>.
		/// </summary>
		/// <param name="value">The value to be formatted</param>
		/// <remarks>
		/// This uses <see cref="ToFormattedPrimitive"/> to attempt to preserve type and fidelity of
		/// primitive values, but otherwise falls back <see cref="object.ToString()"/> for non-primitives.
		/// This may not be appropriate in all circumstances, so care should be exercised when trying
		/// to use this function with non-primitive values.
		/// </remarks>
		public static string? ToCSharp(this object? value) =>
			value is null ? null : (value.ToFormattedPrimitive() ?? value.ToString());

		/// <summary>
		/// Produces a quoted and escaped value of a character, appropriate for placement into generated source.
		/// </summary>
		/// <param name="value">The character value</param>
		public static string ToCSharp(this char value) =>
			"'" + Escape(value) + "'";

		/// <summary>
		/// Produces a quoted and escaped value of a character, appropriate for placement into generated source.
		/// </summary>
		/// <param name="value">The character value</param>
		public static string ToCSharp(this char? value) =>
			value is null ? "null" : "'" + Escape(value.Value) + "'";

		/// <summary>
		/// Produces a quoted and escaped value of a string, appropriate for placement into generated source.
		/// </summary>
		/// <param name="value">The string value</param>
		public static string ToCSharp(this string? value) =>
			value is null ? "null" : "\"" + Escape(value) + "\"";

		/// <summary>
		/// Gets a formatted name for a symbol, appropriate to place into generated source.
		/// </summary>
		/// <param name="symbol">The symbol to get the formatted name of</param>
		/// <param name="includeGlobal">A flag to indicate if the name should be prepended with <c>"global::"</c>
		/// (defaults to <see langword="true"/>)</param>
		public static string ToCSharp(
			this ISymbol symbol,
			bool includeGlobal = true) =>
				symbol.ToDisplayString(includeGlobal ? CompilableDisplayFormat_WithGlobal : CompilableDisplayFormat_WithoutGlobal);

		/// <summary>
		/// Gets a formatted value for a typed constant, appropriate to place into generated source.
		/// </summary>
		/// <param name="constant">The constant value to format</param>
		/// <remarks>
		/// This supports formatting arrays, and primitive values via <see cref="ToFormattedPrimitive"/>, and
		/// then falls back to <see cref="TypedConstantExtensions.ToCSharpString"/> for everything else.
		/// </remarks>
		public static string ToCSharp(this TypedConstant constant) =>
			constant.Kind switch
			{
				TypedConstantKind.Array => $"new {constant.Type?.ToCSharp()} {{ {constant.Values.ToCSharp()} }}",
				TypedConstantKind.Primitive => constant.Value.ToFormattedPrimitive(),
				_ => null,
			} ?? constant.ToCSharpString();

		/// <summary>
		/// Gets a comma-separated list of type constant values, via <see cref="ToCSharp(TypedConstant)"/>.
		/// </summary>
		/// <param name="constants">The array of constant values</param>
		/// <returns></returns>
		public static string ToCSharp(this ImmutableArray<TypedConstant> constants) =>
			string.Join(", ", constants.Select(ToCSharp));

		/// <summary>
		/// Gets a formatted value for a boolean, appropriate to place into generated source.
		/// </summary>
		/// <param name="value">The boolean value</param>
		public static string ToCSharp(this bool value) =>
			value switch
			{
				true => "true",
				false => "false",
			};

		/// <summary>
		/// Gets a formatted value for a nullable boolean, appropriate to place into generated source.
		/// </summary>
		/// <param name="value">The nullable boolean value</param>
		public static string ToCSharp(this bool? value) =>
			value switch
			{
				true => "true",
				false => "false",
				_ => "null",
			};

		/// <summary>
		/// Creates the source for a collection of fixture factories.
		/// </summary>
		/// <param name="factories">The collection of fixture factories</param>
		/// <remarks>
		/// In registration code, a collection of fixture factories is described as
		/// <c>Dictionary&lt;System.Type, Xunit.v3.FixtureFactory&gt;</c>.
		/// </remarks>
		public static string ToFixtureFactories(this IReadOnlyDictionary<string, string> factories) =>
$@"new global::System.Collections.Generic.Dictionary<global::System.Type, global::Xunit.v3.FixtureFactory> {{
	{string.Join(", ", factories.Select(f => $"[typeof({f.Key})] = {f.Value.Replace("\n", "\n\t")}"))}
}}";

		/// <summary>
		/// Gets a fixture factory for a given fixture type.
		/// </summary>
		/// <param name="type">The fixture type</param>
		/// <param name="fixtureCategory">The type of the fixture (e.g., <c>"Class"</c>)</param>
		/// <remarks>
		/// This looks for a single static public constructor on a non-static, non-abstract type, and returns
		/// code which will create an instance of the fixture type. Fixture factories accept two parameters
		/// (<c>FixtureMappingManager? parentMappingManager</c>, <c>bool forceCreation</c>) and return
		/// <c>ValueTask&lt;object?&gt;</c>.
		/// </remarks>
		public static string? ToFixtureFactory(
			this INamedTypeSymbol type,
			string fixtureCategory)
		{
			if (type.IsStatic || type.IsAbstract || !type.IsSafeToReference())
				return null;

			var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
			if (publicCtors.Length != 1)
				return null;

			var factoryBuilder = new StringBuilder();
			factoryBuilder.Append(
@"async (mappingManager, forceCreation) => {
");

			if (!type.ImplementsInterface("Xunit.v3.INotifyLifecycle"))
				factoryBuilder.Append(
@"	if (!forceCreation)
		return null;
");

			return ToConstructorInvocation(
				factoryBuilder,
				publicCtors[0],
				type,
				$"{fixtureCategory} fixture type",
				"global::Xunit.v3.FixtureMappingManager.TryGetFixtureArgument<{0}>(mappingManager)",
				"{0}"
			);
		}

		/// <summary>
		/// Creates a formatted primitive for source generation, from an arbitrary value. This function helps
		/// ensure values are formatted properly including preserving their original type.
		/// </summary>
		/// <param name="value">The value to be formatted</param>
		/// <returns>Returns the formatted value, if special handling is warranted; otherwise, returns
		/// <see langword="null"/></returns>
		/// <remarks>
		/// This includes special handling for the following values:
		/// <list type="bullet">
		/// <item><see langword="null"/></item>
		/// <item><see cref="char"/> values</item>
		/// <item><see cref="bool"/> values</item>
		/// <item><see cref="float"/> values (including <see cref="float.NaN"/>, <see cref="float.PositiveInfinity"/>, <see cref="float.NegativeInfinity"/>)</item>
		/// <item><see cref="double"/> values (including <see cref="double.NaN"/>, <see cref="double.PositiveInfinity"/>, <see cref="double.NegativeInfinity"/></item>
		/// <item><see cref="decimal"/> values</item>
		/// <item>Integral values (<see cref="byte"/>, <see cref="sbyte"/>, <see cref="short"/>, <see cref="ushort"/>, <see cref="int"/>, <see cref="uint"/>, <see cref="long"/>, <see cref="ulong"/>)</item>
		/// </list>
		/// </remarks>
		public static string? ToFormattedPrimitive(this object? value) =>
			// Let enums fall through to whatever the default formatting will be, rather than being treated as integral
			value is Enum ? null : value switch
			{
				null => "null",
				// These constant values aren't emitted by Roslyn ToCSharpString() correctly, per https://github.com/xunit/xunit/issues/3524
				float.NaN => "float.NaN",
				float.PositiveInfinity => "float.PositiveInfinity",
				float.NegativeInfinity => "float.NegativeInfinity",
				double.NaN => "double.NaN",
				double.PositiveInfinity => "double.PositiveInfinity",
				double.NegativeInfinity => "double.NegativeInfinity",
				// Constant values don't preserve their data type, per https://github.com/xunit/xunit/issues/3548
				char c => c.ToCSharp(),
				string s => s.ToCSharp(),
				bool b => b.ToCSharp(),
				byte b => "(byte)" + b.ToString(CultureInfo.InvariantCulture),
				sbyte sb => "(sbyte)" + sb.ToString(CultureInfo.InvariantCulture),
				short s => "(short)" + s.ToString(CultureInfo.InvariantCulture),
				ushort us => "(ushort)" + us.ToString(CultureInfo.InvariantCulture),
				int i => i.ToString(CultureInfo.InvariantCulture),
				uint ui => ui.ToString(CultureInfo.InvariantCulture) + "U",
				long l => l.ToString(CultureInfo.InvariantCulture) + "L",
				ulong ul => ul.ToString(CultureInfo.InvariantCulture) + "UL",
				float f => f.ToString("G9", CultureInfo.InvariantCulture) + "F",
				double d => d.ToString("G17", CultureInfo.InvariantCulture) + "D",
				decimal m => m.ToString("G29", CultureInfo.InvariantCulture) + "M",
				// Fall through and let default handling (object.ToString() or TypedConstant.ToCSharpString(), typically)
				_ => null,
			};

		/// <summary>
		/// Generate a factory to call a parameterless constructor.
		/// </summary>
		/// <param name="type">The type to create</param>
		/// <param name="ctor">The constructor to call</param>
		/// <returns>
		/// If the constructor is not decorated with <see cref="ObsoleteAttribute"/>, then this will return a string
		/// like <c>"new type()"</c>. It will fall back to a public static property named <c>Instance</c> if available
		/// (and returns the correct type), returning a string like <c>"type.Instance"</c>; otherwise, it will
		/// return <see langword="null"/>.
		/// </returns>
		public static string? ToObjectFactory(
			INamedTypeSymbol type,
			IMethodSymbol ctor)
		{
			if (!ctor.GetAttributes().Any(a => a.AttributeClass?.ToCSharp(includeGlobal: false) == "System.ObsoleteAttribute"))
				return $"new {type.ToCSharp()}()";

			// Support our implicit "Instance" static that we use to prevent over-creation
			if (type.GetMembers("Instance").FirstOrDefault() is IPropertySymbol propertySymbol
					&& propertySymbol.IsStatic
					&& SymbolEqualityComparer.Default.Equals(propertySymbol.Type, type))
				return $"{type.ToCSharp()}.Instance";

			return null;
		}

		/// <summary>
		/// Generates a factory for an orderer instance from <see cref="AttributeData"/>.
		/// </summary>
		/// <param name="ordererAttribute">The orderer attribute</param>
		/// <param name="requiredInterface">The interface that orderer type must implement</param>
		/// <remarks>
		/// Orderers are expected to provide a public parameterless constructor and implement one of
		/// the orderer interfaces. This method supports orderer attributes that are either non-generic
		/// (e.g., <c>[TestClassOrderer(typeof(MyOrderer))]</c>) or generic (e.g., <c>[TestClassOrderer&lt;MyOrderer&gt;]</c>).
		/// </remarks>
		public static string? ToOrdererFactory(
			this AttributeData ordererAttribute,
			string requiredInterface)
		{
			var ordererType = default(INamedTypeSymbol);

			if (ordererAttribute.AttributeClass?.TypeArguments.Length == 1)
				ordererType = ordererAttribute.AttributeClass.TypeArguments[0] as INamedTypeSymbol;
			else if (ordererAttribute.ConstructorArguments.Length == 1)
				ordererType = ordererAttribute.ConstructorArguments[0].Value as INamedTypeSymbol;

			return (ordererType != null) ? ToOrdererFactory(ordererType, requiredInterface) : null;
		}

		/// <summary>
		/// Generates a factory for an orderer instance from <see cref="INamedTypeSymbol"/>.
		/// </summary>
		/// <param name="ordererType">The orderer type</param>
		/// <param name="requiredInterface">The interface that orderer type must implement</param>
		/// <remarks>
		/// Orderers are expected to provide a public parameterless constructor and implement one of
		/// the orderer interfaces.
		/// </remarks>
		public static string? ToOrdererFactory(
			this INamedTypeSymbol ordererType,
			string requiredInterface)
		{
			if (ordererType is null)
				return null;

			if (!ordererType.ImplementsInterface(requiredInterface))
				return null;

			var ctor = ordererType.Constructors.FirstOrDefault(c => c.Parameters.Length == 0);
			if (ctor is null)
				return null;

			return ToObjectFactory(ordererType, ctor);
		}

		/// <summary>
		/// Creates a test class factory for the given type.
		/// </summary>
		/// <param name="type">The test class</param>
		public static string? ToTestClassFactory(this INamedTypeSymbol type)
		{
			if (type.IsStatic || type.IsAbstract)
				return null;

			var publicCtors = type.Constructors.Where(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsStatic).ToImmutableArray();
			if (publicCtors.Length != 1)
				return null;

			var factoryBuilder = new StringBuilder();
			factoryBuilder.Append(
@"async mappingManager => {
");

			return ToConstructorInvocation(
				factoryBuilder,
				publicCtors[0],
				type,
				"Test class",
				"mappingManager.TryGetFixtureArgument<{0}>()",
				"new global::Xunit.v3.CoreTestClassCreationResult({0})"
			);
		}

		/// <summary>
		/// Generates a list of type names from an array which is assumed to have type values.
		/// </summary>
		/// <param name="values">The array</param>
		/// <remarks>
		/// This method only returns values which are types. All other values are ignored.
		/// </remarks>
		public static IEnumerable<string> ToTypeArray(this ImmutableArray<TypedConstant> values)
		{
			foreach (var value in values.Where(v => v.Kind == TypedConstantKind.Type))
				if (value.Value is INamedTypeSymbol typeValue)
					if (typeValue.IsSafeToReference())
						yield return typeValue.ToCSharp();
		}

		/// <summary>
		/// Gets a type index used for registering a type.
		/// </summary>
		/// <param name="type">The type</param>
		public static string ToTypeIndex(this INamedTypeSymbol type) =>
			type.IsGenericType
				? type.ConstructUnboundGenericType().ToCSharp()
				: type.ToCSharp();

		/// <summary>
		/// Unwraps the task around a type.
		/// </summary>
		/// <param name="type">The type to unwrap</param>
		/// <returns>
		/// If <paramref name="type"/> is wrapped in <see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>,
		/// returns <c>TResult</c>; otherwise, returns the original type value.
		/// </returns>
		public static ITypeSymbol UnwrapTask(this ITypeSymbol type)
		{
			if (type is INamedTypeSymbol namedType)
			{
				if (!namedType.IsGenericType || namedType.TypeArguments.Length != 1)
					return type;

				var openGeneric = namedType.ConstructUnboundGenericType().ToCSharp(includeGlobal: false);
				if (!genericTaskTypes.Contains(openGeneric))
					return type;

				return namedType.TypeArguments[0];
			}

			return type;
		}

		/// <summary>
		/// Removes incremental value providers that produce null values.
		/// </summary>
		/// <typeparam name="T">The type inside the incremental value provider</typeparam>
		/// <param name="provider">The incremental value provider</param>
		public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> provider)
			where T : class =>
				provider.Where((Func<T?, bool>)notNullTest)!;

		/// <summary>
		/// Removes null items from a collection of <c><typeparamref name="T"/>?</c>, resulting
		/// in a collection of <c><typeparamref name="T"/></c>.
		/// </summary>
		/// <typeparam name="T">The type of the items in the collection</typeparam>
		/// <param name="source">The source collection</param>
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
			where T : class =>
				source.Where((Func<T?, bool>)notNullTest)!;
	}
}
