namespace Xunit.Extensions
{
    /// <summary>
    /// A class which can be derived from for test classes, which bring an overridable version
    /// of Assert (using the <see cref="Assertions"/> class.
    /// </summary>
    public class TestClass
    {
        readonly Assertions assert = new Assertions();

        /// <summary>
        /// Gets a class which provides assertions.
        /// </summary>
        public virtual Assertions Assert
        {
            get { return assert; }
        }
    }
}