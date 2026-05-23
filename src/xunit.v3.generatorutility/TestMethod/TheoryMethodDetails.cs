#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization
#pragma warning disable IDE0031 // Use null propagation

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators
{
	/// <summary>
	/// An override of <see cref="TestMethodDetails"/> specifically designed to handle test methods
	/// which are decorated with <c>[Theory]</c> or <c>[CulturedTheory]</c>.
	/// </summary>
	public class TheoryMethodDetails : TestMethodDetails
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TheoryMethodDetails"/> class.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="classSymbol">The test class symbol</param>
		/// <param name="methodDeclaration">The test method declaration</param>
		/// <param name="methodSymbol">The test method symbol</param>
		/// <param name="attribute">The <c>[Theory]</c> or <c>[CulturedTheory]</c> attribute</param>
		public TheoryMethodDetails(
			SemanticModel semanticModel,
			INamedTypeSymbol classSymbol,
			MethodDeclarationSyntax methodDeclaration,
			IMethodSymbol methodSymbol,
			AttributeData attribute) :
				base(semanticModel, classSymbol, methodDeclaration, methodSymbol, attribute)
		{
			var requiredParameterCount = methodSymbol.Parameters.Where(p => !p.IsOptional && !p.IsParams).Count();

			var invokerFactoryBuilder = new StringBuilder();
			invokerFactoryBuilder.Append(
@"async dataRow => {
	return async obj => {
		await using var disposalTracker = new global::Xunit.Sdk.DisposalTracker();
		var data = dataRow.GetData();
		disposalTracker.AddRange(data);
");

			if (requiredParameterCount > 0)
				invokerFactoryBuilder.Append(
$@"		if (data.Length < {requiredParameterCount})
			throw new global::Xunit.Sdk.TestPipelineException(
				string.Format(
					global::System.Globalization.CultureInfo.CurrentCulture,
					""The test method expected {requiredParameterCount} parameter value{(requiredParameterCount == 1 ? "" : "s")}, but {{0}} parameter value{{1}} {{2}} provided."",
					data.Length,
					data.Length == 1 ? """" : ""s"",
					data.Length == 1 ? ""was"" : ""were""
				)
			);
");

			var anyOptional = false;
			var anyRequired = false;

			foreach (var parameter in methodSymbol.Parameters)
			{
				if (parameter.IsOptional || parameter.IsParams)
					anyOptional = true;
				else
					anyRequired = true;

				if (anyOptional && anyRequired)
					break;
			}

			if (anyRequired)
				invokerFactoryBuilder.Append(
@"		var invalidArguments = new global::System.Collections.Generic.List<(string Type, string Name, object? Value)>();
");
			if (anyOptional)
				ParameterDefaultValues = new string?[methodSymbol.Parameters.Length];

			var parameterNamesInCode = new List<string>();

			for (var idx = 0; idx < MethodSymbol.Parameters.Length; ++idx)
			{
				var parameter = MethodSymbol.Parameters[idx];
				var parameterName = parameter.Name.ToCSharp();
				var parameterNameInCode = "param" + idx;

				ParameterNames.Add(parameter.Name);
				parameterNamesInCode.Add(parameterNameInCode);

				var conversion =
					parameter.NullableAnnotation == NullableAnnotation.NotAnnotated
						? "TryGet"
						: parameter.Type.IsReferenceType
							? "TryGetNullableClass"
							: "TryGetNullableStruct";

				invokerFactoryBuilder.Append(
$@"		var {parameterNameInCode} = data.{conversion}<{StripNullable(parameter.Type).ToCSharp()}>({idx});
		if (!{parameterNameInCode}.Success)
");

				if (parameter.IsOptional)
				{
					var defaultValue = parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null;
					var formattedDefaultValue =
						parameter.DeclaringSyntaxReferences.FirstOrDefault() is { } syntaxReference
							&& syntaxReference.GetSyntax() is ParameterSyntax parameterSyntax
							&& parameterSyntax.Default != null
								? parameterSyntax.Default.Value.ToFullString()
								: defaultValue.ToCSharp() ?? (parameter.Type.IsValueType ? $"default({parameter.Type.ToDisplayString()})" : "null");

					if (ParameterDefaultValues != null)
						ParameterDefaultValues[idx] = formattedDefaultValue;

					invokerFactoryBuilder.Append(
$@"			{parameterNameInCode}.Result = ({parameter.Type.ToCSharp()}){defaultValue.ToCSharp() ?? $"default({parameter.Type.ToCSharp()})!"};
");
				}
				else if (parameter.IsParams)
				{
					if (ParameterDefaultValues != null)
						ParameterDefaultValues[idx] = "[]";

					invokerFactoryBuilder.Append(
$@"			{parameterNameInCode}.Result = [];
");
				}
				else
					invokerFactoryBuilder.Append(
$@"			invalidArguments.Add(({parameter.Type.ToDisplayString().ToCSharp()}, {parameterName}, {parameterNameInCode}.RawValue));
");
			}

			if (anyRequired)
				invokerFactoryBuilder.Append(
$@"		if (invalidArguments.Count != 0)
			throw new global::Xunit.Sdk.TestPipelineException(
				string.Format(
					global::System.Globalization.CultureInfo.CurrentCulture,
					""Test method had one or more invalid theory data arguments: {{0}}"",
					string.Join("", "", global::System.Linq.Enumerable.Select(invalidArguments, a => $""{{a.Type}} {{a.Name}} ({{a.Value ?? ""null""}})""))
				)
			);
");

			var paramsText = string.Join(", ", parameterNamesInCode.Select(p => $"{p}.Result!"));

			invokerFactoryBuilder.Append((classSymbol.IsStatic || MethodSymbol.IsStatic, MethodSymbol.ReturnType.SpecialType == SpecialType.System_Void) switch
			{
				// Static, returning void
				(true, true) => $"		{classSymbol.ToCSharp()}.{MethodSymbol.Name}({paramsText});",
				// Static, returning non-void
				(true, false) => $"		await global::Xunit.Sdk.AsyncUtility.Await({classSymbol.ToCSharp()}.{MethodSymbol.Name}({paramsText}));",
				// Non-static, returning void
				(false, true) => $"		(({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}({paramsText});",
				// Non-static, returning non-void
				(false, false) => $"		await global::Xunit.Sdk.AsyncUtility.Await((({classSymbol.ToCSharp()})obj!).{MethodSymbol.Name}({paramsText}));",
			});

			invokerFactoryBuilder.Append(
@"
	};
}
");

			MethodInvokerFactory = invokerFactoryBuilder.ToString();
		}

		/// <summary>
		/// Gets the flag to indicate if discovery enumeration should be disabled, if provided
		/// </summary>
		public bool? DisableDiscoveryEnumeration { get; set; }

		/// <summary>
		/// Gets the flag to indicate if test case index names should be part of the test case display name
		/// </summary>
		public bool IncludeTestCaseIndex { get; set; }

		/// <summary>
		/// Gets the factory for method invokers, responsible for enumerating over data rows and invoking
		/// the test method for each row
		/// </summary>
		public string MethodInvokerFactory { get; set; }

		/// <summary>
		/// Gets a list of the default values of the test method parameters, if there are any
		/// </summary>
		public string?[]? ParameterDefaultValues { get; set; }

		/// <summary>
		/// Gets a list of the paremeter names
		/// </summary>
		public List<string> ParameterNames { get; } = new List<string>();

		/// <summary>
		/// Gets a flag which indicates if the test should skip rather than fail when no data is available
		/// </summary>
		public bool SkipTestWithoutData { get; set; }

		/// <summary>
		/// This method processes named arguments to the test method attribute.
		/// </summary>
		/// <param name="name">The argument name</param>
		/// <param name="value">The argument value</param>
		/// <remarks>
		/// This currently processes the following named arguments arguments:
		/// <list type="bullet">
		/// <item><c>DisableDiscoveryEnumeration</c></item>
		/// <item><c>DisplayName</c></item>
		/// <item><c>Explicit</c></item>
		/// <item><c>IncludeTestCaseIndex</c></item>
		/// <item><c>Skip</c></item>
		/// <item><c>SkipExceptions</c></item>
		/// <item><c>SkipTestWithoutData</c></item>
		/// <item><c>SkipType</c></item>
		/// <item><c>SkipUnless</c></item>
		/// <item><c>SkipWhen</c></item>
		/// <item><c>Timeout</c></item>
		/// </list>
		/// You can override this to provide support for additional named attribute arguments.
		/// </remarks>
		protected override void ProcessNamedArgument(
			string name,
			TypedConstant value)
		{
			switch (name)
			{
				case Names.TheoryAttribute.DisableDiscoveryEnumeration:
					DisableDiscoveryEnumeration = value.Value is true;
					break;

				case Names.TheoryAttribute.IncludeTestCaseIndex:
					IncludeTestCaseIndex = value.Value is true;
					break;

				case Names.TheoryAttribute.SkipTestWithoutData:
					SkipTestWithoutData = value.Value is true;
					break;

				default:
					base.ProcessNamedArgument(name, value);
					break;
			}
		}

		static ITypeSymbol StripNullable(ITypeSymbol type)
		{
			// We use the original definition and look for it to be "System.Nullable<T>", because the formatting engine has a
			// strong preference to return "T?"
			if (type is INamedTypeSymbol namedType
					&& namedType.IsGenericType
					&& namedType.OriginalDefinition.ToCSharp(includeGlobal: false) == "System.Nullable<T>")
				return namedType.TypeArguments[0];

			return type.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
		}
	}
}
