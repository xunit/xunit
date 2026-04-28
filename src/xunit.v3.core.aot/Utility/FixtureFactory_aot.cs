namespace Xunit.v3;

/// <summary>
/// Describes a fixture factory function.
/// </summary>
/// <param name="parentMappingManager">The parent mapping manager, if there is one</param>
/// <param name="forceCreation">A flag to indicate creation should be forced</param>
/// <returns>The fixture if it was created, or else returns <see langword="null"/>.</returns>
/// <remarks>
/// The <paramref name="forceCreation"/> flag is used to indicate that the fixture should always be
/// created; this will be <see langword="true"/> during initialization if fixture pre-creation is
/// requested, and during any call to <see cref="FixtureMappingManager"/> to get the actual fixture
/// value. The factory should use a <see langword="false"/> value to determine whether to create the
/// fixture based on whether it's decorated with <see cref="INotifyLifecycle"/>, in which case we need
/// to ensure the fixture gets created even for cases where it wouldn't normally get created (for example,
/// when used in a static test class) so that the fixture can subscribe to test lifecycle events.<br />
/// <br />
/// This logic is left to the fixture factory, since the source generator should be able to make this
/// decision without resorting to reflection.<br />
/// </remarks>
public delegate ValueTask<object?> FixtureFactory(FixtureMappingManager? parentMappingManager, bool forceCreation);
