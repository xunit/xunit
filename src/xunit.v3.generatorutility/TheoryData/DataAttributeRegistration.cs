#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to generate a data attribute registration.
	/// </summary>
	/// <remarks>
	/// This collects information on an attribute that derives from <c>Xunit.v3.DataAttribute</c>, to create
	/// an instance of something that derives from <c>Xunit.v3.DataAttributeRegistration</c> to be used
	/// by theory data factories to produce theory data rows.
	/// </remarks>
	public class DataAttributeRegistration
	{
		/// <summary>
		/// Gets the value from the named attribute argument <c>Explicit</c>.
		/// </summary>
		protected bool? Explicit { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>Label</c>.
		/// </summary>
		protected string? Label { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>Skip</c>.
		/// </summary>
		protected string? Skip { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>SkipType</c>.
		/// </summary>
		protected ITypeSymbol? SkipType { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>SkipUnless</c>.
		/// </summary>
		protected string? SkipUnless { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>SkipWhen</c>.
		/// </summary>
		protected string? SkipWhen { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>TestDisplayName</c>.
		/// </summary>
		protected string? TestDisplayName { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>Timeout</c>.
		/// </summary>
		protected int? Timeout { get; set; }

		/// <summary>
		/// Gets the value from the named attribute argument <c>Traits</c>.
		/// </summary>
		protected Dictionary<string, HashSet<string>> Traits { get; } = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Generates the source code for the data attribute registration.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class</param>
		/// <param name="testMethod">The test method</param>
		/// <param name="attribute">The data attribute</param>
		/// <returns>
		/// By default, this validates the SkipUnless/SkipWhen rules, then calls <see cref="GetInitializers"/>
		/// to get the list of property initializers. If the initializers list is empty, the data attribute
		/// registration is just <c>Xunit.v3.DataAttributeRegistration.Empty</c>; otherwise, it generates source
		/// for a new instance of <c>Xunit.v3.DataAttributeRegistration</c> with the property initializers.<br />
		/// <br />
		/// Should return <see langword="null"/> if the attribute was invalid in some way.
		/// </returns>
		public virtual string? GenerateSource(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod,
			AttributeData attribute)
		{
			if (semanticModel is null)
				throw new ArgumentNullException(nameof(semanticModel));
			if (testClass is null)
				throw new ArgumentNullException(nameof(testClass));
			if (testMethod is null)
				throw new ArgumentNullException(nameof(testMethod));
			if (attribute is null)
				throw new ArgumentNullException(nameof(attribute));

			if (SkipUnless != null && SkipWhen != null)
				return null;
			if (!VerifySkipProperty(testClass, SkipUnless) || !VerifySkipProperty(testClass, SkipWhen))
				return null;

			var initializers = GetInitializers(semanticModel, testClass, testMethod, attribute);

			return
				initializers.Count == 0
					? "global::Xunit.v3.DataAttributeRegistration.Empty"
					: $"new global::Xunit.v3.DataAttributeRegistration() {{ {string.Join(", ", initializers)} }}";
		}

		/// <summary>
		/// Gets a list of property initializers for the creation of the data attribute registration
		/// in <see cref="GenerateSource"/> (typically <c>Xunit.v3.DataAttributeRegistration</c>, though
		/// it could be overridden).
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class</param>
		/// <param name="testMethod">The test method</param>
		/// <param name="attribute">The data attribute</param>
		/// <returns>The list of property initializers, appropriate to place in generated source</returns>
		protected virtual List<string> GetInitializers(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod,
			AttributeData attribute)
		{
			if (semanticModel is null)
				throw new ArgumentNullException(nameof(semanticModel));
			if (testClass is null)
				throw new ArgumentNullException(nameof(testClass));
			if (testMethod is null)
				throw new ArgumentNullException(nameof(testMethod));
			if (attribute is null)
				throw new ArgumentNullException(nameof(attribute));

			var result = new List<string>();

			if (Explicit.HasValue)
				result.Add($"Explicit = {Explicit.ToCSharp()}");
			if (Label != null)
				result.Add($"Label = {Label.ToCSharp()}");
			if (Skip != null)
				result.Add($"Skip = {Skip.ToCSharp()}");
			if (SkipUnless != null)
				result.Add($"SkipUnless = () => {(SkipType ?? testClass).ToCSharp()}.{SkipUnless}");
			if (SkipWhen != null)
				result.Add($"SkipWhen = () => {(SkipType ?? testClass).ToCSharp()}.{SkipWhen}");
			if (TestDisplayName != null)
				result.Add($"TestDisplayName = {TestDisplayName.ToCSharp()}");
			if (Timeout != null)
				result.Add($"Timeout = {Timeout}");
			if (Traits.Count != 0)
			{
				var initializer = new StringBuilder("Traits = new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.IReadOnlyCollection<string>>(global::System.StringComparer.OrdinalIgnoreCase) { ");

				foreach (var kvp in Traits)
					initializer.AppendFormat(CultureInfo.InvariantCulture, "[{0}] = new global::System.Collections.Generic.HashSet<string> {{ {1} }}", kvp.Key.ToCSharp(), string.Join(",", kvp.Value.Select(v => v.ToCSharp())));

				initializer.Append('}');
				result.Add(initializer.ToString());
			}

			return result;
		}

		/// <summary>
		/// Processes one named argument from the data attribute.
		/// </summary>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class</param>
		/// <param name="testMethod">The test method</param>
		/// <param name="attribute">The data attribute</param>
		/// <param name="argumentName">The argument name</param>
		/// <param name="argumentValue">The argument value</param>
		/// <remarks>
		/// By default, this handles the properties named:
		/// <list type="bullet">
		/// <item><c>Explicit</c></item>
		/// <item><c>Label</c></item>
		/// <item><c>Skip</c></item>
		/// <item><c>SkipType</c></item>
		/// <item><c>SkipUnless</c></item>
		/// <item><c>SkipWhen</c></item>
		/// <item><c>TestDisplayName</c></item>
		/// <item><c>Timeout</c></item>
		/// <item><c>Traits</c></item>
		/// </list>
		/// </remarks>
		public virtual void ProcessNamedArgument(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod,
			AttributeData attribute,
			string argumentName,
			TypedConstant argumentValue)
		{
			switch (argumentName)
			{
				case Names.DataAttribute.Explicit:
					if (argumentValue.Value is bool @explicit)
						Explicit = @explicit;
					break;

				case Names.DataAttribute.Label:
					Label = argumentValue.Value as string;
					break;

				case Names.DataAttribute.Skip:
					Skip = argumentValue.Value as string;
					break;

				case Names.DataAttribute.SkipType:
					SkipType = argumentValue.Value as ITypeSymbol;
					break;

				case Names.DataAttribute.SkipUnless:
					SkipUnless = argumentValue.Value as string;
					break;

				case Names.DataAttribute.SkipWhen:
					SkipWhen = argumentValue.Value as string;
					break;

				case Names.DataAttribute.TestDisplayName:
					TestDisplayName = argumentValue.Value as string;
					break;

				case Names.DataAttribute.Timeout:
					if (argumentValue.Value is int timeout)
						Timeout = timeout;
					break;

				case Names.DataAttribute.Traits:
					if (argumentValue.Kind == TypedConstantKind.Array)
					{
						var traitsArray = argumentValue.Values.Select(c => c.Value as string).WhereNotNull().ToArray();
						var idx = 0;

						while (idx < traitsArray.Length - 1)
						{
							if (!Traits.TryGetValue(traitsArray[idx], out var hash))
							{
								hash = new HashSet<string>();
								Traits[traitsArray[idx]] = hash;
							}

							hash.Add(traitsArray[idx + 1]);
							idx += 2;
						}
					}
					break;
			}
		}

		/// <summary>
		/// Tries to generate source for a data attribute registration from <see cref="DataAttributeRegistration"/>
		/// or any type which derives from it.
		/// </summary>
		/// <typeparam name="TRegistration">The registration type</typeparam>
		/// <param name="semanticModel">The semantic model</param>
		/// <param name="testClass">The test class</param>
		/// <param name="testMethod">The test method</param>
		/// <param name="attribute">The data attribute</param>
		/// <returns>The generated source, if the attribute is valid; <see langword="null"/>, otherwise</returns>
		public static string? TryGenerate<TRegistration>(
			SemanticModel semanticModel,
			INamedTypeSymbol testClass,
			IMethodSymbol testMethod,
			AttributeData attribute)
				where TRegistration : DataAttributeRegistration, new()
		{
			if (semanticModel is null)
				throw new ArgumentNullException(nameof(semanticModel));
			if (testClass is null)
				throw new ArgumentNullException(nameof(testClass));
			if (testMethod is null)
				throw new ArgumentNullException(nameof(testMethod));
			if (attribute is null)
				throw new ArgumentNullException(nameof(attribute));

			var result = new TRegistration();

			foreach (var namedArgument in attribute.NamedArguments)
				result.ProcessNamedArgument(semanticModel, testClass, testMethod, attribute, namedArgument.Key, namedArgument.Value);

			return result.GenerateSource(semanticModel, testClass, testMethod, attribute);
		}

		bool VerifySkipProperty(
			ITypeSymbol testClass,
			string? propertyName)
		{
			if (propertyName is null)
				return true;

			var currentSymbol = SkipType ?? testClass;

			while (currentSymbol != null)
			{
				var property =
					currentSymbol
						.GetMembers()
						.OfType<IPropertySymbol>()
						.FirstOrDefault(symbol => symbol.Name == propertyName);

				if (property != null)
					return property.IsStatic && property.DeclaredAccessibility == Accessibility.Public && property.Type.ToCSharp() == "bool";

				currentSymbol = currentSymbol.BaseType;
			}

			return false;
		}
	}
}
