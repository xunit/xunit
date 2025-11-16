using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

// Extensibility points related to test methods

public static partial class ExtensibilityPointFactory
{
	/// <summary>
	/// Gets the <see cref="IBeforeAfterTestAttribute"/>s attached to the given method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="classBeforeAfterAttributes">The before after attributes from the test class,
	/// to be merged into the result.</param>
	public static IReadOnlyCollection<IBeforeAfterTestAttribute> GetMethodBeforeAfterTestAttributes(
		MethodInfo testMethod,
		IReadOnlyCollection<IBeforeAfterTestAttribute> classBeforeAfterAttributes)
	{
		var warnings = new List<string>();

		try
		{
			return
				Guard.ArgumentNotNull(classBeforeAfterAttributes)
					.Concat(Guard.ArgumentNotNull(testMethod).GetMatchingCustomAttributes<IBeforeAfterTestAttribute>(warnings))
					.CastOrToReadOnlyCollection();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the <see cref="IDataAttribute"/>s attached to the given test method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	public static IReadOnlyCollection<IDataAttribute> GetMethodDataAttributes(MethodInfo testMethod)
	{
		var warnings = new List<string>();

		try
		{
			var result = Guard.ArgumentNotNull(testMethod).GetMatchingCustomAttributes<IDataAttribute>(warnings);

			foreach (var typeAwareAttribute in result.OfType<ITypeAwareDataAttribute>())
				typeAwareAttribute.MemberType ??= testMethod.ReflectedType;

			return result;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the <see cref="IFactAttribute"/>s attached to the given test method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	public static IReadOnlyCollection<IFactAttribute> GetMethodFactAttributes(MethodInfo testMethod)
	{
		var warnings = new List<string>();

		try
		{
			return Guard.ArgumentNotNull(testMethod).GetMatchingCustomAttributes<IFactAttribute>(warnings);
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the test case orderer that's attached to a test method. Returns <see langword="null"/> if there
	/// isn't one attached.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	public static ITestCaseOrderer? GetMethodTestCaseOrderer(MethodInfo testMethod) =>
		GetMethodTestOrderer<ITestCaseOrderer, ITestCaseOrdererAttribute>(testMethod, "case");

	static TTestOrderer? GetMethodTestOrderer<TTestOrderer, TTestOrdererAttribute>(
		MethodInfo testMethod,
		string ordererType)
			where TTestOrderer : class
			where TTestOrdererAttribute : ITestOrdererAttribute
	{
		Guard.ArgumentNotNull(testMethod);
		var methodType = Guard.NotNull("Test methods must come from a type", testMethod.ReflectedType ?? testMethod.DeclaringType);

		var warnings = new List<string>();

		try
		{
			var ordererAttributes = testMethod.GetMatchingCustomAttributes<TTestOrdererAttribute>(warnings);
			if (ordererAttributes.Count > 1)
				TestContext.Current.SendDiagnosticMessage(
					"Found more than one method-level test {0} orderer for test method '{1}.{2}': {3}",
					ordererType,
					methodType.SafeName(),
					methodType.Name,
					string.Join(", ", ordererAttributes.Select(a => a.GetType()).ToCommaSeparatedList())
				);

			if (ordererAttributes.FirstOrDefault() is TTestOrdererAttribute ordererAttribute)
				try
				{
					return Get<TTestOrderer>(ordererAttribute.OrdererType);
				}
				catch (Exception ex)
				{
					var innerEx = ex.Unwrap();

					TestContext.Current.SendDiagnosticMessage(
						"Method-level test {0} orderer '{1}' for test method '{2}.{3}' threw '{4}' during construction: {5}{6}{7}",
						ordererType,
						ordererAttribute.OrdererType,
						methodType.SafeName(),
						methodType.Name,
						innerEx.GetType().SafeName(),
						innerEx.Message,
						Environment.NewLine,
						innerEx.StackTrace
					);
				}

			return null;
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}

	/// <summary>
	/// Gets the traits that are attached to the test method via <see cref="ITraitAttribute"/>s.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	/// <param name="testClassTraits">The traits inherited from the test class</param>
	public static IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetMethodTraits(
		MethodInfo testMethod,
		IReadOnlyDictionary<string, IReadOnlyCollection<string>>? testClassTraits)
	{
		Guard.ArgumentNotNull(testMethod);

		var warnings = new List<string>();

		try
		{
			var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

			if (testClassTraits is not null)
				foreach (var trait in testClassTraits)
					result.AddOrGet(trait.Key).AddRange(trait.Value);

			foreach (var traitAttribute in testMethod.GetMatchingCustomAttributes<ITraitAttribute>(warnings))
				foreach (var kvp in traitAttribute.GetTraits())
					result.AddOrGet(kvp.Key).Add(kvp.Value);

			return result.ToReadOnly();
		}
		finally
		{
			foreach (var warning in warnings)
				TestContext.Current.SendDiagnosticMessage(warning);
		}
	}
}
