using Xunit;

public class XunitFiltersTests
{
    public class ExcludedTraitsTests
    {
        [Fact]
        public void EmptyFiltersListAlwaysPasses()
        {
            var filters = new XunitFilters();
            var method = new TestMethod("method1", "Display Name 1", traits: null);

            bool result = filters.Filter(method);

            Assert.True(result);
        }

        [Fact]
        public void CanFilterItemsByTrait()
        {
            var filters = new XunitFilters();
            var method1 = new TestMethod("method1", "Display Name 1", MakeTraits("foo", "bar"));
            var method2 = new TestMethod("method2", "Display Name 2", MakeTraits("baz", "biff"));
            var method3 = new TestMethod("method3", "Display Name 3", traits: null);
            filters.ExcludedTraits.AddValue("foo", "bar");

            Assert.False(filters.Filter(method1));
            Assert.True(filters.Filter(method2));
            Assert.True(filters.Filter(method3));
        }

        [Fact]
        public void MultipleTraitFiltersAreAnAndOperation()
        {
            var filters = new XunitFilters();
            var method1 = new TestMethod("method1", "Display Name 1", MakeTraits("foo", "bar"));
            var method2 = new TestMethod("method2", "Display Name 2", MakeTraits("baz", "biff"));
            var method3 = new TestMethod("method3", "Display Name 3", traits: null);
            filters.ExcludedTraits.AddValue("foo", "bar");
            filters.ExcludedTraits.AddValue("baz", "biff");

            Assert.False(filters.Filter(method1));
            Assert.False(filters.Filter(method2));
            Assert.True(filters.Filter(method3));
        }
    }

    public class IncludedTraitsTests
    {
        [Fact]
        public void EmptyFiltersListAlwaysPasses()
        {
            var filters = new XunitFilters();
            var method = new TestMethod("method1", "Display Name 1", traits: null);

            bool result = filters.Filter(method);

            Assert.True(result);
        }

        [Fact]
        public void CanFilterItemsByTrait()
        {
            var filters = new XunitFilters();
            var method1 = new TestMethod("method1", "Display Name 1", MakeTraits("foo", "bar"));
            var method2 = new TestMethod("method2", "Display Name 2", MakeTraits("baz", "biff"));
            var method3 = new TestMethod("method3", "Display Name 3", traits: null);
            filters.IncludedTraits.AddValue("foo", "bar");

            Assert.True(filters.Filter(method1));
            Assert.False(filters.Filter(method2));
            Assert.False(filters.Filter(method3));
        }

        [Fact]
        public void MultipleTraitFiltersAreAnOrOperation()
        {
            var filters = new XunitFilters();
            var method1 = new TestMethod("method1", "Display Name 1", MakeTraits("foo", "bar"));
            var method2 = new TestMethod("method2", "Display Name 2", MakeTraits("baz", "biff"));
            var method3 = new TestMethod("method3", "Display Name 3", traits: null);
            filters.IncludedTraits.AddValue("foo", "bar");
            filters.IncludedTraits.AddValue("baz", "biff");

            Assert.True(filters.Filter(method1));
            Assert.True(filters.Filter(method2));
            Assert.False(filters.Filter(method3));
        }

    }

    private static MultiValueDictionary<string, string> MakeTraits(string name, string value)
    {
        var result = new MultiValueDictionary<string, string>();
        result.AddValue(name, value);
        return result;
    }
}