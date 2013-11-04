using Xunit;

public class XunitFiltersTests
{
    public class ExcludedTraits
    {
        [Fact]
        public void EmptyFiltersListAlwaysPasses()
        {
            var filters = new XunitFilters();
            var method = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");

            bool result = filters.Filter(method);

            Assert.True(result);
        }

        [Fact]
        public void CanFilterItemsByTrait()
        {
            var filters = new XunitFilters();
            var methodWithFooBar = Mocks.TestCase<ClassUnderTest>("MethodWithFooBar");
            var methodWithBazBiff = Mocks.TestCase<ClassUnderTest>("MethodWithBazBiff");
            var methodWithNoTraits = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");
            filters.ExcludedTraits.Add("foo", "bar");

            Assert.False(filters.Filter(methodWithFooBar));
            Assert.True(filters.Filter(methodWithBazBiff));
            Assert.True(filters.Filter(methodWithNoTraits));
        }

        [Fact]
        public void MultipleTraitFiltersAreAnAndOperation()
        {
            var filters = new XunitFilters();
            var methodWithFooBar = Mocks.TestCase<ClassUnderTest>("MethodWithFooBar");
            var methodWithBazBiff = Mocks.TestCase<ClassUnderTest>("MethodWithBazBiff");
            var methodWithNoTraits = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");
            filters.ExcludedTraits.Add("foo", "bar");
            filters.ExcludedTraits.Add("baz", "biff");

            Assert.False(filters.Filter(methodWithFooBar));
            Assert.False(filters.Filter(methodWithBazBiff));
            Assert.True(filters.Filter(methodWithNoTraits));
        }

        class ClassUnderTest
        {
            [Fact]
            public void MethodWithNoTraits() { }

            [Fact]
            [Trait("foo", "bar")]
            public void MethodWithFooBar() { }

            [Fact]
            [Trait("baz", "biff")]
            public void MethodWithBazBiff() { }
        }
    }

    public class IncludedTraitsTests
    {
        [Fact]
        public void EmptyFiltersListAlwaysPasses()
        {
            var filters = new XunitFilters();
            var method = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");

            bool result = filters.Filter(method);

            Assert.True(result);
        }

        [Fact]
        public void CanFilterItemsByTrait()
        {
            var filters = new XunitFilters();
            var methodWithFooBar = Mocks.TestCase<ClassUnderTest>("MethodWithFooBar");
            var methodWithBazBiff = Mocks.TestCase<ClassUnderTest>("MethodWithBazBiff");
            var methodWithNoTraits = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");
            filters.IncludedTraits.Add("foo", "bar");

            Assert.True(filters.Filter(methodWithFooBar));
            Assert.False(filters.Filter(methodWithBazBiff));
            Assert.False(filters.Filter(methodWithNoTraits));
        }

        [Fact]
        public void MultipleTraitFiltersAreAnAndOperation()
        {
            var filters = new XunitFilters();
            var methodWithFooBar = Mocks.TestCase<ClassUnderTest>("MethodWithFooBar");
            var methodWithBazBiff = Mocks.TestCase<ClassUnderTest>("MethodWithBazBiff");
            var methodWithNoTraits = Mocks.TestCase<ClassUnderTest>("MethodWithNoTraits");
            filters.IncludedTraits.Add("foo", "bar");
            filters.IncludedTraits.Add("baz", "biff");

            Assert.True(filters.Filter(methodWithFooBar));
            Assert.True(filters.Filter(methodWithBazBiff));
            Assert.False(filters.Filter(methodWithNoTraits));
        }

        class ClassUnderTest
        {
            [Fact]
            public void MethodWithNoTraits() { }

            [Fact]
            [Trait("foo", "bar")]
            public void MethodWithFooBar() { }

            [Fact]
            [Trait("baz", "biff")]
            public void MethodWithBazBiff() { }
        }
    }
}