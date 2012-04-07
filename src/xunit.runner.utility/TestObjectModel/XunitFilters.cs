
namespace Xunit
{
    /// <summary>
    /// Represents a set of filters for an <see cref="XunitProject"/>.
    /// </summary>
    public class XunitFilters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XunitFilters"/> class.
        /// </summary>
        public XunitFilters()
        {
            ExcludedTraits = new MultiValueDictionary<string, string>();
            IncludedTraits = new MultiValueDictionary<string, string>();
        }

        /// <summary>
        /// Gets the set of trait filters for tests to exclude.
        /// </summary>
        public MultiValueDictionary<string, string> ExcludedTraits { get; private set; }

        /// <summary>
        /// Gets the set of trait filters for tests to include.
        /// </summary>
        public MultiValueDictionary<string, string> IncludedTraits { get; private set; }

        /// <summary>
        /// Filters the given method using the defined filter values.
        /// </summary>
        /// <param name="method">The methods to filter.</param>
        /// <returns>Returns true if the method passed the filter; return false otherwise.</returns>
        public bool Filter(TestMethod method)
        {
            if (!FilterIncludedTraits(method))
                return false;
            if (!FilterExcludedTraits(method))
                return false;

            return true;
        }

        bool FilterExcludedTraits(TestMethod method)
        {
            // No traits in the filter == everything is okay
            if (ExcludedTraits.Count == 0)
                return true;

            // No traits in the method == it's always safe from exclusion
            if (method.Traits.Count == 0)
                return true;

            foreach (string key in ExcludedTraits.Keys)
                foreach (string value in ExcludedTraits[key])
                    if (method.Traits.Contains(key, value))
                        return false;

            return true;
        }

        bool FilterIncludedTraits(TestMethod method)
        {
            // No traits in the filter == everything is okay
            if (IncludedTraits.Count == 0)
                return true;

            // No traits in the method == it'll never match anything, don't try
            if (method.Traits.Count == 0)
                return false;

            foreach (string key in IncludedTraits.Keys)
                foreach (string value in IncludedTraits[key])
                    if (method.Traits.Contains(key, value))
                        return true;

            return false;
        }
    }
}