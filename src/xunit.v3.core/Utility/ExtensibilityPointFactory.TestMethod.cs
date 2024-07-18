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
		IReadOnlyCollection<IBeforeAfterTestAttribute> classBeforeAfterAttributes) =>
			Guard.ArgumentNotNull(testMethod)
				.GetMatchingCustomAttributes(typeof(IBeforeAfterTestAttribute))
				.Cast<IBeforeAfterTestAttribute>()
				.Concat(Guard.ArgumentNotNull(classBeforeAfterAttributes))
				.CastOrToReadOnlyCollection();

	/// <summary>
	/// Gets the <see cref="IDataAttribute"/>s attached to the given test method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	public static IReadOnlyCollection<IDataAttribute> GetMethodDataAttributes(MethodInfo testMethod)
	{
		var result =
			Guard.ArgumentNotNull(testMethod)
				.GetMatchingCustomAttributes(typeof(IDataAttribute))
				.Cast<IDataAttribute>()
				.CastOrToReadOnlyCollection();

		foreach (var typeAwareAttribute in result.OfType<ITypeAwareDataAttribute>())
			typeAwareAttribute.MemberType ??= testMethod.ReflectedType;

		return result;
	}

	/// <summary>
	/// Gets the <see cref="IFactAttribute"/>s attached to the given test method.
	/// </summary>
	/// <param name="testMethod">The test method</param>
	public static IReadOnlyCollection<IFactAttribute> GetMethodFactAttributes(MethodInfo testMethod) =>
		Guard.ArgumentNotNull(testMethod)
			.GetMatchingCustomAttributes(typeof(IFactAttribute))
			.Cast<IFactAttribute>()
			.CastOrToReadOnlyCollection();

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

		var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

		if (testClassTraits is not null)
			foreach (var trait in testClassTraits)
				result.AddOrGet(trait.Key).AddRange(trait.Value);

		foreach (var traitAttribute in testMethod.GetMatchingCustomAttributes(typeof(ITraitAttribute)).Cast<ITraitAttribute>())
			foreach (var kvp in traitAttribute.GetTraits())
				result.AddOrGet(kvp.Key).Add(kvp.Value);

		return result.ToReadOnly();
	}
}
