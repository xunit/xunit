using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Xunit.Generators;

internal static class Extensions
{
	// List from https://learn.microsoft.com/dotnet/csharp/programming-guide/strings/#string-escape-sequences
	static readonly Dictionary<char, string> escapes = new()
	{
		['\\'] = "\\\\",
		['\''] = "\\\'",
		['"'] = "\\\"",
		['\0'] = "\\0",
		['\a'] = "\\a",
		['\b'] = "\\b",
		['\e'] = "\\e",
		['\f'] = "\\f",
		['\n'] = "\\n",
		['\r'] = "\\r",
		['\t'] = "\\t",
		['\v'] = "\\v",
	};
	static readonly HashSet<string> genericTaskTypes = [Types.System.Threading.Tasks.TaskOfT, Types.System.Threading.Tasks.ValueTaskOfT];
	static readonly Func<object, bool> notNullTest = x => x is not null;
	static readonly HashSet<string> theoryDataTypes = [
		"object", "object?",
		"object[]", "object?[]",
		Types.System.Runtime.CompilerServices.ITuple,
		Types.Xunit.ITheoryDataRow
	];

	// Based on SymbolDisplayFormat.FullyQualifiedFormat + nullable
	static readonly SymbolDisplayFormat CompilableDisplayFormat_WithGlobal = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Included,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
		miscellaneousOptions:
			SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
			SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
			SymbolDisplayMiscellaneousOptions.UseSpecialTypes
	);
	static readonly SymbolDisplayFormat CompilableDisplayFormat_WithoutGlobal = new(
		globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
		typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
		genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
		miscellaneousOptions:
			SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier |
			SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
			SymbolDisplayMiscellaneousOptions.UseSpecialTypes
	);

	public static string Escape(this char value) =>
		escapes.TryGetValue(value, out var escaped) ? escaped : value.ToString();

	public static string Escape(this string value)
	{
		var result = new StringBuilder(value.Length);

		foreach (var c in value)
		{
			if (escapes.TryGetValue(c, out var escaped))
				result.Append(escaped);
			else
				result.Append(c);
		}

		return result.ToString();
	}

	static string? FormatPrimitive(object? value) =>
		value switch
		{
			null => "null",
			// Let this fall through to whatever the default formatting will be, rather than being treated as integral
			Enum => null,
			// These constant values aren't emitted by Roslyn ToCSharpString() correctly, per https://github.com/xunit/xunit/issues/3524
			float.NaN => "float.NaN",
			float.PositiveInfinity => "float.PositiveInfinity",
			float.NegativeInfinity => "float.NegativeInfinity",
			double.NaN => "double.NaN",
			double.PositiveInfinity => "double.PositiveInfinity",
			double.NegativeInfinity => "double.NegativeInfinity",
			// Constant values don't preserve their data type, per https://github.com/xunit/xunit/issues/3548
			char c => "'" + Escape(c) + "'",
			bool b => b ? "true" : "false",
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
			_ => null,
		};

	public static ImmutableArray<ISymbol> GetAllMembers(
		this INamedTypeSymbol type,
		string name)
	{
		var result = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

		for (var current = type; current is not null; current = current.BaseType)
			foreach (var member in current.GetMembers(name))
				result.Add(member);

		foreach (var methodWithOverride in result.OfType<IMethodSymbol>().Where(m => m.IsOverride).ToArray())
			if (methodWithOverride.OverriddenMethod is not null)
				result.Remove(methodWithOverride.OverriddenMethod);

		return result.ToImmutableArray();
	}

	public static (bool IsAsyncEnumerable, ITypeSymbol EnumerableType)? GetEnumerable(
		this ITypeSymbol? type,
		INamedTypeSymbol objectType)
	{
		if (type is not INamedTypeSymbol namedType)
			return null;

		if (namedType.ToCSharp(includeGlobal: false) == Types.System.Collections.IEnumerable)
			return (false, objectType);

		if (!namedType.IsGenericType || namedType.TypeArguments.Length != 1)
			return null;

		return namedType.ConstructUnboundGenericType().ToCSharp(includeGlobal: false) switch
		{
			Types.System.Collections.Generic.IAsyncEnumerableOfT => (true, namedType.TypeArguments[0]),
			Types.System.Collections.Generic.IEnumerableOfT => (false, namedType.TypeArguments[0]),
			_ => null,
		};
	}

	public static bool HasConstructorParameters(
		this INamedTypeSymbol symbol,
		string[] parameterTypes)
	{
		if (symbol is null || parameterTypes is null)
			return false;

		var ctors =
			symbol
				.Constructors
				.Where(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.Length == parameterTypes.Length);

		return ctors.Any(ctor => CtorMatches(ctor, parameterTypes));

		static bool CtorMatches(
			IMethodSymbol ctor,
			string[] parameterTypes)
		{
			for (var idx = 0; idx < parameterTypes.Length; ++idx)
			{
				var ctorParameter = ctor.Parameters[idx];
				var expectedType = parameterTypes[idx];

				// TODO: Is it worth the time to look up the inheritance hierarchy of the target type?
				// This feels like, on balance, expensive, rather than just doing a strict requirement.
				if (ctorParameter.Type.ToString() != expectedType)
					return false;
			}

			return true;
		}
	}

	public static bool HasParameterlessPublicCtor(
		this INamedTypeSymbol symbol,
		[NotNullWhen(true)] out IMethodSymbol? ctor)
	{
		ctor =
			symbol
				?.Constructors
				.FirstOrDefault(c => !c.IsStatic && c.DeclaredAccessibility == Accessibility.Public && c.Parameters.All(p => p.IsOptional || p.IsParams));

		return ctor is not null;
	}

	public static bool ImplementsInterface(
		this ITypeSymbol symbol,
		string fullyQualifiedInterfaceName)
	{
		if (symbol is null || fullyQualifiedInterfaceName is null)
			return false;

		return symbol.Implements(fullyQualifiedInterfaceName);
	}

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

	public static (bool IsTask, bool IsAsyncEnumerable, ITypeSymbol EnumerableType)? GetTheoryDataInfo(
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
		if (enumerable is not null)
		{
			isAsyncEnumerable = enumerable.Value.IsAsyncEnumerable;
			enumerableType = enumerable.Value.EnumerableType;
		}
		else
		{
			foreach (var @interface in taskFreeType.AllInterfaces)
			{
				enumerable = GetEnumerable(@interface, objectType);
				if (enumerable is not null)
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
			return (isTask, isAsyncEnumerable, enumerableType);

		return null;
	}

	public static bool Implements(
		this ITypeSymbol symbol,
		string fullyQualifiedInterfaceName) =>
			symbol.AllInterfaces.Any(i => i.ToString() == fullyQualifiedInterfaceName);

	public static IReadOnlyList<string> ImplementsAll(
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

	public static bool InheritsFrom(
		[NotNullWhen(true)]
		this INamedTypeSymbol? symbol,
		string typeName)
	{
		if (symbol is null)
			return false;

		if (symbol.ToCSharp(includeGlobal: false) == typeName)
			return true;

		return InheritsFrom(symbol.BaseType, typeName);
	}

	// https://github.com/dotnet/roslyn/blob/58efd1373c4755c7402677c3249046886807d820/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Extensions/Symbols/ISymbolExtensions.cs#L673
	public static bool IsAwaitableNonDynamic(
		this ITypeSymbol type,
		SemanticModel semanticModel,
		int position)
	{
		var potentialGetAwaiters = semanticModel.LookupSymbols(
			position,
			container: type.OriginalDefinition,
			name: WellKnownMemberNames.GetAwaiter,
			includeReducedExtensionMethods: true
		);

		var getAwaiters = potentialGetAwaiters.OfType<IMethodSymbol>().Where(x => !x.Parameters.Any());
		return getAwaiters.Any(verifyGetAwaiter);

		// https://github.com/dotnet/roslyn/blob/58efd1373c4755c7402677c3249046886807d820/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Extensions/Symbols/ISymbolExtensions.cs#L707
		static bool verifyGetAwaiter(IMethodSymbol getAwaiter)
		{
			var returnType = getAwaiter.ReturnType;
			if (returnType == null)
				return false;

			// bool IsCompleted { get }
			if (!returnType
					.GetMembers()
					.OfType<IPropertySymbol>()
					.Any(p => p is { Name: WellKnownMemberNames.IsCompleted, Type.SpecialType: SpecialType.System_Boolean, GetMethod: not null }))
				return false;

			var methods = returnType.GetMembers().OfType<IMethodSymbol>().ToImmutableArray();

			// void OnCompleted(Action)
			if (!methods.Any(x => x is { Name: WellKnownMemberNames.OnCompleted, ReturnsVoid: true } && x.Parameters.Length == 1 && x.Parameters[0].Type.TypeKind == TypeKind.Delegate))
				return false;

			// void GetResult() || T GetResult()
			return methods.Any(m => m.Name == WellKnownMemberNames.GetResult && !m.Parameters.Any());
		}
	}

	public static bool IsGeneric(
		this INamedTypeSymbol? type,
		string genericTypeName) =>
			type is not null && type.IsGenericType && type.ConstructUnboundGenericType().ToCSharp(includeGlobal: false) == genericTypeName;

	public static bool IsPublicOrInternal(this ITypeSymbol type)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
			return true;

		if (type.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
			return false;

		if (type is not INamedTypeSymbol namedType)
			return true;

		return namedType.TypeArguments.All(IsPublicOrInternal);
	}

	public static string Quoted(this string? value) =>
		value is null ? "null" : "\"" + value.Escape() + "\"";

	public static string? QuotedIfString(this object? value) =>
		value is null
			? null
			: value is string s
				? s.Quoted()
				: (FormatPrimitive(value) ?? value.ToString());

	public static ITypeSymbol? RecursiveGetNonPublicNonInternalType(this ITypeSymbol type)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
			return null;

		if (type.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Internal)
			return type;

		if (type is not INamedTypeSymbol namedType)
			return null;

		return namedType.TypeArguments.Select(RecursiveGetNonPublicNonInternalType).FirstOrDefault(a => a is not null);
	}

	public static ITypeSymbol? RecursiveGetOpenGenericTypeParameter(this ITypeSymbol type)
	{
		if (type.TypeKind == TypeKind.TypeParameter)
			return type;

		if (type is not INamedTypeSymbol namedType)
			return null;

		return namedType.TypeArguments.Select(RecursiveGetOpenGenericTypeParameter).FirstOrDefault(a => a is not null);
	}

	public static ITypeSymbol StripNullable(this ITypeSymbol type)
	{
		if (type is INamedTypeSymbol namedType && namedType.IsGenericType && namedType.OriginalDefinition.ToCSharp(includeGlobal: false) == "System.Nullable<T>")
			return namedType.TypeArguments[0];

		return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
	}

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

	public static string ToCSharp(
		this ISymbol symbol,
		bool includeGlobal = true) =>
			symbol.ToDisplayString(includeGlobal ? CompilableDisplayFormat_WithGlobal : CompilableDisplayFormat_WithoutGlobal);

	public static string ToCSharp(this TypedConstant constant)
	{
		Guard.ArgumentNotNull(constant.Type);

		return constant.Kind switch
		{
			TypedConstantKind.Array => $"new {constant.Type?.ToCSharp()} {{ {string.Join(", ", constant.Values.Select(v => v.ToCSharp()))} }}",
			TypedConstantKind.Primitive => FormatPrimitive(constant.Value),
			_ => null,
		} ?? constant.ToCSharpString();
	}

	public static string ToCSharp(this ImmutableArray<TypedConstant> constants) =>
		string.Join(", ", constants.Select(ToCSharp));

	public static string ToCSharp(this bool value) =>
		value switch
		{
			true => "true",
			false => "false",
		};

	public static string ToCSharp(this bool? value) =>
		value switch
		{
			true => "true",
			false => "false",
			_ => "null",
		};

	public static ITypeSymbol UnwrapTask(this ITypeSymbol type)
	{
		if (type is not INamedTypeSymbol namedType)
			return type;

		if (!namedType.IsGenericType || namedType.TypeArguments.Length != 1)
			return type;

		var openGeneric = namedType.ConstructUnboundGenericType().ToCSharp(includeGlobal: false);
		if (!genericTaskTypes.Contains(openGeneric))
			return type;

		return namedType.TypeArguments[0];
	}

	public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
		where T : class =>
			source.Where((Func<T?, bool>)notNullTest)!;

	public static IncrementalValuesProvider<T> WhereNotNull<T>(this IncrementalValuesProvider<T?> source)
		where T : class =>
			source.Where((Func<T?, bool>)notNullTest)!;

	extension(SyntaxReference? syntaxReference)
	{
		public Location? Location =>
			syntaxReference is null ? null : Location.Create(syntaxReference.SyntaxTree, syntaxReference.Span);
	}
}
