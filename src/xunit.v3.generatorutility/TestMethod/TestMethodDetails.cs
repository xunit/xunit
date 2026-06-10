#nullable enable

#pragma warning disable IDE0028 // Simplify collection initialization

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Xunit.Generators
{
	/// <summary>
	/// This class encapsulates information about a test method that is decorated with an attribute that
	/// is shaped like <c>Xunit.FactAttribute</c>.
	/// </summary>
	/// <remarks>
	/// The attribute constructor is scanned for two parameters, decorated with <see cref="CallerFilePathAttribute"/>
	/// and <see cref="CallerLineNumberAttribute"/>, and uses any values passed to those parameters as
	/// the source location of the test method.<br />
	/// <br />
	/// It is customary to put these as the final two constructor arguments like this:<br />
	/// <br />
	/// <c>[CallerFilePath] string? sourceFilePath = null</c><br />
	/// <c>[CallerLineNumber] int sourceLineNumber = -1</c><br />
	/// <br />
	/// The compiler will fill in the values automatically, and this class will retrieve the compiler-provided
	/// values to help notate the source location of the test method.
	/// </remarks>
	public class TestMethodDetails
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TestMethodDetails"/> class.
		/// </summary>
		/// <param name="classSymbol">The test class symbol</param>
		/// <param name="methodDeclaration">The test method declaration</param>
		/// <param name="methodSymbol">The test method symbol</param>
		/// <param name="attribute">The attribute (expected to be shaped like <c>[FactAttribute]</c>)</param>
		public TestMethodDetails(
			INamedTypeSymbol classSymbol,
			MethodDeclarationSyntax methodDeclaration,
			IMethodSymbol methodSymbol,
			AttributeData attribute)
		{
			ClassSymbol = classSymbol ?? throw new ArgumentNullException(nameof(classSymbol));
			MethodDeclaration = methodDeclaration ?? throw new ArgumentNullException(nameof(methodDeclaration));
			MethodSymbol = methodSymbol ?? throw new ArgumentNullException(nameof(methodSymbol));
			Attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
			TypeIndex = classSymbol.ToTypeIndex();

			if (Attribute.AttributeConstructor != null)
				for (var idx = 0; idx < Attribute.ConstructorArguments.Length; ++idx)
				{
					var argument = Attribute.ConstructorArguments[idx];
					if (argument.Kind != TypedConstantKind.Primitive)
						continue;

					var attributes = Attribute.AttributeConstructor.Parameters[idx].GetAttributes();
					foreach (var attributeType in attributes.Select(a => a.AttributeClass?.ToCSharp(includeGlobal: false)))
					{
						if (attributeType == Types.System.Runtime.CompilerServices.CallerFilePathAttribute)
							SourceFilePath = argument.Value as string ?? SourceFilePath;
						else if (attributeType == Types.System.Runtime.CompilerServices.CallerLineNumberAttribute)
							SourceLineNumber = argument.Value is int intValue ? intValue : SourceLineNumber;
					}
				}

			var containingType = methodSymbol.ContainingType;
			if (containingType != null && !SymbolEqualityComparer.Default.Equals(containingType, classSymbol))
				DeclaredTypeIndex =
					containingType.IsGenericType
						? containingType.ConstructUnboundGenericType().ToCSharp()
						: containingType.ToCSharp();
		}

		/// <summary>
		/// Gets the arity of the test method
		/// </summary>
		public int Arity =>
			MethodDeclaration.Arity;

		/// <summary>
		/// Gets the attribute attached to the test method
		/// </summary>
		public AttributeData Attribute { get; }

		/// <summary>
		/// Gets a list of types that are the <c>BeforeAfterAttribute</c>s attached to the test method
		/// </summary>
		public List<string> BeforeAfterTestAttributes { get; } = new List<string>();

		/// <summary>
		/// Gets the test class symbol
		/// </summary>
		public INamedTypeSymbol ClassSymbol { get; }

		/// <summary>
		/// Gets the type index of the type where this test method was declared, if different
		/// from <see cref="ClassSymbol"/>. Note that this will be expressed in open generic
		/// form (e.g., <c>"global::Namespace.Type&lt;&gt;"</c>) for generic types.
		/// </summary>
		public string? DeclaredTypeIndex { get; }

		/// <summary>
		/// Gets the display name attached to the test method, if any
		/// </summary>
		public string? DisplayName { get; set; }

		/// <summary>
		/// Gets the explicit test flag for the test method
		/// </summary>
		public bool Explicit { get; set; }

		/// <summary>
		/// Gets the test method syntax
		/// </summary>
		public MethodDeclarationSyntax MethodDeclaration { get; }

		/// <summary>
		/// Gets a flag which indicates if the test method is <see langword="static"/>
		/// </summary>
		public bool MethodIsStatic =>
			MethodSymbol.IsStatic;

		/// <summary>
		/// Gets the name of the test method
		/// </summary>
		public string MethodName =>
			MethodSymbol.Name;

		/// <summary>
		/// Gets the test method symbol
		/// </summary>
		public IMethodSymbol MethodSymbol { get; }

		/// <summary>
		/// Gets a list of exception types that should be considered a skip rather than a
		/// fail if they're thrown
		/// </summary>
		public List<string> SkipExceptions { get; } = new List<string>();

		/// <summary>
		/// Gets the skip reason, if one was set
		/// </summary>
		public string? SkipReason { get; set; }

		/// <summary>
		/// Gets the skip type symbol, if one was set
		/// </summary>
		public INamedTypeSymbol? SkipType { get; set; }

		/// <summary>
		/// Gets the property name for SkipUnless, if one was set
		/// </summary>
		public string? SkipUnless { get; set; }

		/// <summary>
		/// Gets the property name for SkipWhen, if one was set
		/// </summary>
		public string? SkipWhen { get; set; }

		/// <summary>
		/// Gets the source file path of the test method
		/// </summary>
		public string? SourceFilePath { get; set; }

		/// <summary>
		/// Gets the source line number of the test method
		/// </summary>
		public int? SourceLineNumber { get; set; }

		/// <summary>
		/// Gets the factory for the test case orderer for the test method
		/// </summary>
		public string? TestCaseOrdererFactory { get; set; }

		/// <summary>
		/// Gets the timeout of the test method
		/// </summary>
		public int Timeout { get; set; }

		/// <summary>
		/// Gets the type index of the test class.
		/// </summary>
		public string TypeIndex { get; }

		/// <summary>
		/// Processes the test method.
		/// </summary>
		/// <returns>Return <see langword="true"/> if this is a valid test method; <see langword="false"/>, otherwise</returns>
		/// <remarks>
		/// This method does an initial set of validation (ensuring the class isn't abstract, isn't an open
		/// generic, and that the test method isn't generic), then processes all the named arguments on the
		/// attribute. Next, it will process the attributes attached to the test method itself, and then
		/// finally will validate the properties for <c>SkipUnless</c> and <c>SkipWhen</c>.
		/// </remarks>
		public virtual bool Process()
		{
			if (ClassSymbol.IsAbstract
					|| (ClassSymbol.IsGenericType && ClassSymbol.TypeArguments.Any(t => t.TypeKind == TypeKind.TypeParameter))
					|| MethodSymbol.IsGenericMethod)
				return false;

			foreach (var kvp in Attribute.NamedArguments)
				ProcessNamedArgument(kvp.Key, kvp.Value);

			foreach (var methodAttribute in MethodSymbol.GetAttributes())
			{
				var attributeTypeName =
					methodAttribute.AttributeClass?.IsGenericType == true
						? methodAttribute.AttributeClass.ConstructUnboundGenericType().ToString()
						: methodAttribute.AttributeClass?.ToString();

				if (attributeTypeName is null)
					continue;

				ProcessMethodAttributeSymbol(attributeTypeName, methodAttribute);
			}

			if (SkipUnless != null && SkipWhen != null)
				return false;

			return VerifySkipProperty(SkipUnless) && VerifySkipProperty(SkipWhen);
		}

		/// <summary>
		/// This method processes attributes attached to the test method symbolically (meaning, this
		/// list of attributes will include all attributes from all source files for partial methods).
		/// </summary>
		/// <param name="typeName">The attribute type name</param>
		/// <param name="attribute">The attribute</param>
		/// <remarks>
		/// This currently processes the following attributes:
		/// <list type="bullet">
		/// <item><c>BeforeAfterTestAttribute</c>-derived attributes</item>
		/// <item><c>TestCaseOrdererAttribute</c></item>
		/// </list>
		/// You can override this to provide support for additional attributes on the test method.
		/// </remarks>
		protected virtual void ProcessMethodAttributeSymbol(
			string typeName,
			AttributeData attribute)
		{
			if (typeName is null || attribute is null)
				return;

			switch (typeName)
			{
				case Types.Xunit.TestCaseOrdererAttribute:
				case Types.Xunit.TestCaseOrdererAttributeOfT:
					TestCaseOrdererFactory = attribute.ToOrdererFactory(Types.Xunit.v3.ITestCaseOrderer);
					break;

				default:
					if (attribute.AttributeClass.InheritsFrom(Types.Xunit.v3.BeforeAfterTestAttribute))
						if (attribute.AttributeClass.IsSafeToReference())
							BeforeAfterTestAttributes.Add(typeName);
					break;
			}
		}

		/// <summary>
		/// This method processes named arguments to the test method attribute.
		/// </summary>
		/// <param name="name">The argument name</param>
		/// <param name="value">The argument value</param>
		/// <remarks>
		/// This currently processes the following named arguments arguments:
		/// <list type="bullet">
		/// <item><c>DisplayName</c></item>
		/// <item><c>Explicit</c></item>
		/// <item><c>Skip</c></item>
		/// <item><c>SkipExceptions</c></item>
		/// <item><c>SkipType</c></item>
		/// <item><c>SkipUnless</c></item>
		/// <item><c>SkipWhen</c></item>
		/// <item><c>Timeout</c></item>
		/// </list>
		/// You can override this to provide support for additional named attribute arguments.
		/// </remarks>
		protected virtual void ProcessNamedArgument(
			string name,
			TypedConstant value)
		{
			switch (name)
			{
				case Names.FactAttribute.DisplayName:
					DisplayName = value.Value as string;
					break;

				case Names.FactAttribute.Explicit:
					Explicit = value.Value is true;
					break;

				case Names.FactAttribute.Skip:
					SkipReason = value.Value as string;
					break;

				case Names.FactAttribute.SkipExceptions:
					SkipExceptions.AddRange(value.Values.ToTypeArray());
					break;

				case Names.FactAttribute.SkipType:
					SkipType = value.Value as INamedTypeSymbol;
					break;

				case Names.FactAttribute.SkipUnless:
					SkipUnless = value.Value as string;
					break;

				case Names.FactAttribute.SkipWhen:
					SkipWhen = value.Value as string;
					break;

				case Names.FactAttribute.Timeout:
					if (value.Value is int timeoutValue)
						Timeout = timeoutValue;
					break;
			}
		}

		bool VerifySkipProperty(string? propertyName)
		{
			if (propertyName is null)
				return true;

			var currentSymbol = SkipType ?? ClassSymbol;

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
