using System.Diagnostics;
using System.Reflection;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Maps fixture objects, including support for generic collection fixtures.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FixtureMappingManager"/> class.
/// </remarks>
/// <param name="fixtureCategory">The category of fixture (i.e., "Assembly"); used in exception messages</param>
/// <param name="parentMappingManager">The parent mapping manager (used to resolve constructor arguments)</param>
[DebuggerDisplay("category = {fixtureCategory}, cache count = {fixtureCache.Count}, known type count = {knownTypes.Count}")]
public partial class FixtureMappingManager(
	string fixtureCategory,
	FixtureMappingManager? parentMappingManager = null) :
		IAsyncDisposable
{
	readonly string fixtureCategory = fixtureCategory;
	readonly HashSet<Type> knownTypes = [];

	internal IReadOnlyDictionary<Type, object> FixtureCache => fixtureCache;

	/// <summary>
	/// Returns a list of all known fixture types at all category levels.
	/// </summary>
	/// <remarks>
	/// The type list returned here is based on fixtures that have already been created for use
	/// at the current level. It does not include open generic types, nor types that have never
	/// been requested at this level.
	/// </remarks>
	public IReadOnlyCollection<Type> GlobalFixtureTypes => fixtureCache.Keys;

	/// <summary>
	/// Returns a list of the known fixture types at this category level. This will not include fixture
	/// types known from parent categories and above.
	/// </summary>
	public IReadOnlyCollection<Type> LocalFixtureTypes => knownTypes;

	/// <summary>
	/// Initializes the known fixture types, always creating instances.
	/// </summary>
	/// <param name="fixtureTypes">The known fixture types</param>
	/// <remarks>
	/// This method is for testing purposes only. Production code should call <see cref="InitializeAsync(IReadOnlyCollection{Type}, bool)"/>.
	/// </remarks>
	public ValueTask InitializeAsync(params Type[] fixtureTypes) =>
		InitializeAsync(fixtureTypes, createInstances: true);

	/// <summary>
	/// Initializes the known fixture types, optionally creating the instances ahead of
	/// time.
	/// </summary>
	/// <param name="fixtureTypes">The known fixture types</param>
	/// <param name="createInstances">A flag indicating whether to create the instances</param>
	public async ValueTask InitializeAsync(
		IReadOnlyCollection<Type> fixtureTypes,
		bool createInstances)
	{
		Guard.ArgumentNotNull(fixtureTypes);

		foreach (var fixtureType in fixtureTypes)
		{
			var knownType = fixtureType;

			// If this looks like FixtureType<T> instead of FixtureType<int>, we want the known type to be
			// the open generic (i.e. FixtureType<>), which also means we can't directly create it now.
			// We may be asked to create it later as a dependency, at which point we'll know the concrete
			// type for the dependency.
			if (fixtureType.IsGenericType && fixtureType.GenericTypeArguments.Any(t => t.IsGenericParameter))
				knownType = fixtureType.GetGenericTypeDefinition();

			knownTypes.Add(knownType);

			// Pre-create the fixture type, because we want to make sure all concrete fixtures are
			// instantiated even if nobody comes along later to get the instance.
			if (createInstances && knownType == fixtureType)
				await GetFixture(knownType);
		}
	}

	/// <summary>
	/// Tries to get a value for the given fixture type. If the fixture type is unknown, will return
	/// <see langword="false"/> for <c>Success</c>.
	/// </summary>
	/// <param name="fixtureType">The type of the fixture</param>
	public async ValueTask<(bool Success, object? Result)> TryGetFixture(Type fixtureType)
	{
		Guard.ArgumentNotNull(fixtureType);

		// Pull from the cache if present
		if (fixtureCache.TryGetValue(fixtureType, out var result))
			return (true, result);

		// Ensure this is a type that's known to come from this fixture level; otherwise, ask the
		// parent mapping manager to generate the type (or return null if it's not)
		var isKnownType = knownTypes.Contains(fixtureType);
		if (!isKnownType && fixtureType.IsGenericType)
			isKnownType = knownTypes.Contains(fixtureType.GetGenericTypeDefinition());

		if (!isKnownType)
			return
				parentMappingManager is not null
					? await parentMappingManager.TryGetFixture(fixtureType)
					: (false, null);

		// Ensure there is a single public constructor
		var ctors =
			fixtureType
				.GetConstructors()
				.Where(ci => !ci.IsStatic && ci.IsPublic)
				.ToList();

		if (ctors.Count != 1)
			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"{0} fixture type '{1}' may only define a single public constructor.",
					fixtureCategory,
					fixtureType.SafeName()
				)
			);

		// Make sure we can accommodate all the constructor arguments from either known types or the parent
		var ctor = ctors[0];
		var parameters = ctor.GetParameters();
		var ctorArgs = new object[parameters.Length];
		var ctorIdx = 0;
		var missingParameters = new List<ParameterInfo>();

		foreach (var parameter in parameters)
		{
			object? arg = null;
			if (parameter.ParameterType == typeof(IMessageSink))
				arg = TestContext.CurrentInternal.DiagnosticMessageSink ?? NullMessageSink.Instance;
			else if (parameter.ParameterType == typeof(ITestContextAccessor))
				arg = TestContextAccessor.Instance;
			else if (parentMappingManager is not null)
				arg = await parentMappingManager.GetFixture(parameter.ParameterType);

			if (arg is null)
				missingParameters.Add(parameter);
			else
				ctorArgs[ctorIdx++] = arg;
		}

		if (missingParameters.Count > 0)
			throw new TestPipelineException(
				string.Format(
					CultureInfo.CurrentCulture,
					"{0} fixture type '{1}' had one or more unresolved constructor arguments: {2}",
					fixtureCategory,
					fixtureType.SafeName(),
					string.Join(", ", missingParameters.Select(p => string.Format(CultureInfo.CurrentCulture, "{0} {1}", p.ParameterType.Name, p.Name)))
				)
			);

		// Create the object
		try
		{
			result = ctor.Invoke(ctorArgs);
			fixtureCache[fixtureType] = result;
		}
		catch (Exception ex)
		{
			throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in its constructor", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
		}

		// Do async initialization
		if (result is IAsyncLifetime asyncLifetime)
			try
			{
				await asyncLifetime.InitializeAsync();
			}
			catch (Exception ex)
			{
				throw new TestPipelineException(string.Format(CultureInfo.CurrentCulture, "{0} fixture type '{1}' threw in InitializeAsync", fixtureCategory, fixtureType.SafeName()), ex.Unwrap());
			}

		return (true, result);
	}
}
