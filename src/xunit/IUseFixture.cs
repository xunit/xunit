namespace Xunit
{
    /// <summary>
    /// Used to decorate xUnit.net test classes that utilize fixture classes.
    /// An instance of the fixture data is initialized just before the first
    /// test in the class is run, and if it implements IDisposable, is disposed
    /// after the last test in the class is run.
    /// </summary>
    /// <typeparam name="T">The type of the fixture</typeparam>
    public interface IUseFixture<T> where T : new()
    {
        /// <summary>
        /// Called on the test class just before each test method is run,
        /// passing the fixture data so that it can be used for the test.
        /// All test runs share the same instance of fixture data.
        /// </summary>
        /// <param name="data">The fixture data</param>
        void SetFixture(T data);
    }
}