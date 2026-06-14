#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Xunit.Generators
{
	/// <summary>
	/// A helper class designed to perform test-method-level registration.
	/// </summary>
	public class CodeGenTestMethodRegistration : IEquatable<CodeGenTestMethodRegistration>
	{
		readonly int arity;
		readonly IReadOnlyCollection<string>? beforeAfterTestAttributes;
		readonly string? declaredTypeIndex;
		readonly bool disableParallelization;
		readonly bool isStatic;
		readonly string methodName;
		readonly string? sourceFilePath;
		readonly int? sourceLineNumber;
		readonly IReadOnlyCollection<string> testCaseFactories;
		readonly string? testCaseOrdererFactory;
		readonly string typeIndex;

		/// <summary>
		/// Initializes a new instance of the <see cref="CodeGenTestMethodRegistration"/> class.
		/// </summary>
		/// <param name="arity">The test method arity</param>
		/// <param name="beforeAfterTestAttributes">The before after test attribute type names, in global format (e.g., <c>"global::Namespace.Type"</c>)</param>
		/// <param name="declaredTypeIndex">The type index of the declared type, if it's different from <paramref name="typeIndex"/></param>
		/// <param name="disableParallelization">A flag to indicate whether the test method wishes to opt out of test parallelization</param>
		/// <param name="isStatic">A flag to indicate if the test method is static</param>
		/// <param name="methodName">The test method name</param>
		/// <param name="sourceFilePath">The source file path, if known</param>
		/// <param name="sourceLineNumber">The source line number, if known</param>
		/// <param name="testCaseFactories">The test case factories (must not be empty)</param>
		/// <param name="testCaseOrdererFactory">The optional test case orderer factory</param>
		/// <param name="typeIndex">The type index of the test class</param>
		public CodeGenTestMethodRegistration(
			int arity,
			IReadOnlyCollection<string>? beforeAfterTestAttributes,
			string? declaredTypeIndex,
			bool disableParallelization,
			bool isStatic,
			string methodName,
			string? sourceFilePath,
			int? sourceLineNumber,
			IReadOnlyCollection<string> testCaseFactories,
			string? testCaseOrdererFactory,
			string typeIndex)
		{
			this.arity = arity;
			this.beforeAfterTestAttributes = beforeAfterTestAttributes;
			this.declaredTypeIndex = declaredTypeIndex;
			this.disableParallelization = disableParallelization;
			this.isStatic = isStatic;
			this.methodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
			this.sourceFilePath = sourceFilePath;
			this.sourceLineNumber = sourceLineNumber;
			this.testCaseFactories = testCaseFactories ?? throw new ArgumentNullException(nameof(testCaseFactories));
			this.testCaseOrdererFactory = testCaseOrdererFactory;
			this.typeIndex = typeIndex ?? throw new ArgumentNullException(nameof(typeIndex));

			if (testCaseFactories.Count == 0)
				throw new ArgumentException("testCaseFactories must contain at least one factory", nameof(testCaseFactories));
		}

		/// <inheritdoc/>
		public override bool Equals(object? obj) =>
			Equals(obj as CodeGenTestMethodRegistration);

		/// <inheritdoc/>
		public bool Equals(CodeGenTestMethodRegistration? other) =>
			other != null &&
			ComparerHelper.Equal(arity, other.arity) &&
			ComparerHelper.Equal(beforeAfterTestAttributes, other.beforeAfterTestAttributes) &&
			ComparerHelper.Equal(declaredTypeIndex, other.declaredTypeIndex) &&
			ComparerHelper.Equal(disableParallelization, other.disableParallelization) &&
			ComparerHelper.Equal(isStatic, other.isStatic) &&
			ComparerHelper.Equal(methodName, other.methodName) &&
			ComparerHelper.Equal(sourceFilePath, other.sourceFilePath) &&
			ComparerHelper.Equal(sourceLineNumber, other.sourceLineNumber) &&
			ComparerHelper.Equal(testCaseFactories, other.testCaseFactories) &&
			ComparerHelper.Equal(testCaseOrdererFactory, other.testCaseOrdererFactory) &&
			ComparerHelper.Equal(typeIndex, other.typeIndex);

		/// <summary>
		/// Creates an instance of <see cref="CodeGenTestMethodRegistration"/> using the data found
		/// in <paramref name="testMethod"/>.
		/// </summary>
		/// <param name="testMethod">The test method details</param>
		/// <param name="testCaseFactories">The test case factories</param>
		/// <remarks>
		/// It is required that there will be one or more test case factories in <paramref name="testCaseFactories"/>.
		/// </remarks>
		public static CodeGenTestMethodRegistration FromTestMethodDetails(
			TestMethodDetails testMethod,
			params string[] testCaseFactories)
		{
			if (testMethod is null)
				throw new ArgumentNullException(nameof(testMethod));
			if (testCaseFactories is null)
				throw new ArgumentNullException(nameof(testCaseFactories));

			return new(
				testMethod.Arity,
				testMethod.BeforeAfterTestAttributes,
				testMethod.DeclaredTypeIndex,
				testMethod.DisableParallelization,
				testMethod.MethodIsStatic,
				testMethod.MethodName,
				testMethod.SourceFilePath,
				testMethod.SourceLineNumber,
				testCaseFactories,
				testMethod.TestCaseOrdererFactory,
				testMethod.TypeIndex
			);
		}

		/// <summary>
		/// Generates the source for the test method registration.
		/// </summary>
		/// <param name="builder">The <see cref="StringBuilder"/> to generate the source into</param>
		/// <remarks>
		/// This will generate a call to <c>RegisteredEngineConfig.RegisterCodeGenTestMethod</c>, zero or
		/// more calls to <c>RegisteredEngineConfig.RegisterCodeGenTestMethodTrait</c> for the traits
		/// attached to the test method, and one or more calls to <c>RegisteredEngineConfig.RegisterCodeGenTestCaseFactory</c>
		/// that generate the test case instances.
		/// </remarks>
		public void GenerateSource(StringBuilder builder)
		{
			if (builder is null || testCaseFactories.Count == 0)
				return;

			builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestMethod({typeIndex.ToCSharp()}, {methodName.ToCSharp()}, {ToMethodRegistration()});
");

			foreach (var testCaseFactory in testCaseFactories)
				builder.Append(
$@"global::Xunit.v3.RegisteredEngineConfig.RegisterCodeGenTestCaseFactory({typeIndex.ToCSharp()}, {methodName.ToCSharp()}, {testCaseFactory});
");
		}

		/// <inheritdoc/>
		public override int GetHashCode() =>
			HashCodeHelper.Start()
				.With(arity)
				.With(beforeAfterTestAttributes)
				.With(declaredTypeIndex)
				.With(disableParallelization)
				.With(isStatic)
				.With(methodName)
				.With(sourceFilePath)
				.With(sourceLineNumber)
				.With(testCaseFactories)
				.With(testCaseOrdererFactory)
				.With(typeIndex);

		string ToMethodRegistration()
		{
			var initValues = new List<string>();

			if (arity != 0)
				initValues.Add($"Arity = {arity}");
			if (beforeAfterTestAttributes is not null && beforeAfterTestAttributes.Count != 0)
				initValues.Add($"BeforeAfterAttributesFactory = () => new global::Xunit.v3.BeforeAfterTestAttribute[] {{ {string.Join(", ", beforeAfterTestAttributes.Select(t => $"new {t}()"))} }}");
			if (declaredTypeIndex != null)
				initValues.Add($"DeclaredTypeIndex = {declaredTypeIndex.ToCSharp()}");
			if (disableParallelization)
				initValues.Add("DisableParallelization = true");
			if (isStatic)
				initValues.Add("IsStatic = true");
			if (sourceFilePath != null)
				initValues.Add($"SourceFilePath = {sourceFilePath.ToCSharp()}");
			if (sourceLineNumber != null)
				initValues.Add($"SourceLineNumber = {sourceLineNumber}");
			if (testCaseOrdererFactory != null)
				initValues.Add($"TestCaseOrdererFactory = () => {testCaseOrdererFactory}");

			if (initValues.Count == 0)
				return "global::Xunit.v3.CodeGenTestMethodRegistration.Empty";

			return $"new global::Xunit.v3.CodeGenTestMethodRegistration() {{ {string.Join(", ", initValues)} }}";
		}
	}
}
