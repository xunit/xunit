using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Default implementation of <see cref="IXunitTestClass"/> for xUnit v3 tests based on reflection.
/// </summary>
public class XunitTestClass : IXunitTestClass, IXunitSerializable
{
	internal static BindingFlags MethodBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

	// Values that must be round-tripped in serialization
	Type? @class;
	IXunitTestCollection? testCollection;
	string? uniqueID;

	// Lazy accessors based on serialized values
	readonly Lazy<IReadOnlyCollection<IBeforeAfterTestAttribute>> beforeAfterTestAttributes;
	readonly Lazy<IReadOnlyCollection<Type>> classFixtureTypes;
	readonly Lazy<IReadOnlyCollection<ConstructorInfo>?> constructors;
	readonly Lazy<IReadOnlyCollection<MethodInfo>> methods;
	readonly Lazy<ITestCaseOrderer?> testCaseOrderer;
	readonly Lazy<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> traits;

	/// <summary>
	/// Called by the de-serializer; should only be called by deriving classes for de-serialization purposes
	/// </summary>
	[Obsolete("Called by the de-serializer; should only be called by deriving classes for de-serialization purposes")]
	public XunitTestClass()
	{
		beforeAfterTestAttributes = new(() => ExtensibilityPointFactory.GetClassBeforeAfterTestAttributes(Class, TestCollection.BeforeAfterTestAttributes));
		classFixtureTypes = new(() => ExtensibilityPointFactory.GetClassClassFixtureTypes(Class, TestCollection.ClassFixtureTypes));
		constructors = new(() => Class.IsAbstract && Class.IsSealed ? null : Class.GetConstructors().Where(ci => !ci.IsStatic && ci.IsPublic).CastOrToReadOnlyCollection());
		methods = new(() => Class.GetMethods(MethodBindingFlags).CastOrToReadOnlyCollection());
		testCaseOrderer = new(() => ExtensibilityPointFactory.GetClassTestCaseOrderer(Class));
		traits = new(() => ExtensibilityPointFactory.GetClassTraits(Class, TestCollection.Traits));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="XunitTestClass"/> class.
	/// </summary>
	/// <param name="class">The test class</param>
	/// <param name="testCollection">The test collection the class belongs to</param>
	/// <param name="uniqueID">The unique ID for the test class (only used to override default behavior in testing scenarios)</param>
	public XunitTestClass(
		Type @class,
		IXunitTestCollection testCollection,
		string? uniqueID = null)
#pragma warning disable CS0618
			: this()
#pragma warning restore CS0618
	{
		this.@class = @class;
		this.testCollection = testCollection;
		this.uniqueID = uniqueID ?? UniqueIDGenerator.ForTestClass(TestCollection.UniqueID, @class.SafeName());
	}

	/// <inheritdoc/>
	public IReadOnlyCollection<IBeforeAfterTestAttribute> BeforeAfterTestAttributes =>
		beforeAfterTestAttributes.Value;

	/// <inheritdoc/>
	public Type Class =>
		this.ValidateNullablePropertyValue(@class, nameof(Class));

	/// <inheritdoc/>
	public IReadOnlyCollection<Type> ClassFixtureTypes =>
		classFixtureTypes.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<ConstructorInfo>? Constructors =>
		constructors.Value;

	/// <inheritdoc/>
	public IReadOnlyCollection<MethodInfo> Methods =>
		methods.Value;

	/// <inheritdoc/>
	public ITestCaseOrderer? TestCaseOrderer =>
		testCaseOrderer.Value;

	/// <inheritdoc/>
	public string TestClassName =>
		Class.SafeName();

	/// <inheritdoc/>
	public string? TestClassNamespace =>
		Class.Namespace;

	/// <inheritdoc/>
	public string TestClassSimpleName =>
		Class.ToSimpleName();

	/// <inheritdoc/>
	public IXunitTestCollection TestCollection =>
		this.ValidateNullablePropertyValue(testCollection, nameof(TestCollection));

	/// <inheritdoc/>
	ITestCollection ITestClass.TestCollection => TestCollection;

	/// <inheritdoc/>
	public IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits =>
		traits.Value;

	/// <inheritdoc/>
	public string UniqueID =>
		this.ValidateNullablePropertyValue(uniqueID, nameof(UniqueID));

	/// <inheritdoc/>
	public void Deserialize(IXunitSerializationInfo info)
	{
		testCollection = Guard.NotNull("Could not retrieve TestCollection from serialization", info.GetValue<IXunitTestCollection>("tc"));
		uniqueID = Guard.NotNull("Could not retrieve UniqueID from serialization", info.GetValue<string>("id"));

		var assemblyName = Guard.NotNull("Could not retrieve ClassAssemblyName from serialization", info.GetValue<string>("ca"));
		var typeName = Guard.NotNull("Could not retrieve ClassTypeName from serialization", info.GetValue<string>("cn"));

		@class =
			TypeHelper.GetType(assemblyName, typeName)
				?? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Failed to deserialize type '{0}' in assembly '{1}'", typeName, assemblyName));
	}

	/// <inheritdoc/>
	public void Serialize(IXunitSerializationInfo info)
	{
		info.AddValue("tc", TestCollection);
		info.AddValue("ca", Class.Assembly.FullName);
		info.AddValue("cn", Class.SafeName());
		info.AddValue("id", UniqueID);
	}
}
