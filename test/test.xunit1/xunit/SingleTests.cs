using System;
using System.Collections;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class SingleTests
    {
        public class NonGenericEnumerable
        {
            [Fact]
            public void NullCollectionThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Single(null));
            }

            [Fact]
            public void EmptyCollectionThrows()
            {
                ArrayList collection = new ArrayList();

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 0 elements instead of 1.", ex.Message);
            }

            [Fact]
            public void MultiItemCollectionThrows()
            {
                ArrayList collection = new ArrayList { "Hello", "World" };

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 2 elements instead of 1.", ex.Message);
            }

            [Fact]
            public void SingleItemCollectionDoesNotThrow()
            {
                ArrayList collection = new ArrayList { "Hello" };

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.Null(ex);
            }

            [Fact]
            public void SingleItemCollectionReturnsTheItem()
            {
                ArrayList collection = new ArrayList { "Hello" };

                object result = Assert.Single(collection);

                Assert.Equal("Hello", result);
            }
        }

        public class NonGenericEnumerableWithObject
        {
            [Fact]
            public void NullCollectionThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Single(null, null));
            }

            [Fact]
            public void ObjectSingleMatch()
            {
                IEnumerable collection = new[] { "Hello", "World!" };

                Assert.Single(collection, "Hello");
            }

            [Fact]
            public void NullSingleMatch()
            {
                IEnumerable collection = new[] { "Hello", "World!", null };

                Assert.Single(collection, null);
            }

            [Fact]
            public void ObjectNoMatch()
            {
                IEnumerable collection = new[] { "Hello", "World!" };

                Exception ex = Record.Exception(() => Assert.Single(collection, "foo"));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 0 instances of 'foo' instead of 1.", ex.Message);
            }

            [Fact]
            public void PredicateTooManyMatches()
            {
                string[] collection = new[] { "Hello", "World!", "Hello" };

                Exception ex = Record.Exception(() => Assert.Single(collection, "Hello"));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 2 instances of 'Hello' instead of 1.", ex.Message);
            }
        }

        public class GenericEnumerable
        {
            [Fact]
            public void NullCollectionThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null));
            }

            [Fact]
            public void EmptyCollectionThrows()
            {
                object[] collection = new object[0];

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 0 elements instead of 1.", ex.Message);
            }

            [Fact]
            public void MultiItemCollectionThrows()
            {
                string[] collection = new[] { "Hello", "World!" };

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 2 elements instead of 1.", ex.Message);
            }

            [Fact]
            public void SingleItemCollectionDoesNotThrow()
            {
                string[] collection = new[] { "Hello" };

                Exception ex = Record.Exception(() => Assert.Single(collection));

                Assert.Null(ex);
            }

            [Fact]
            public void SingleItemCollectionReturnsTheItem()
            {
                string[] collection = new[] { "Hello" };

                string result = Assert.Single(collection);

                Assert.Equal("Hello", result);
            }
        }

        public class GenericEnumerableWithPredicate
        {
            [Fact]
            public void NullCollectionThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null, _ => true));
            }

            [Fact]
            public void NullPredicateThrows()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(new object[0], null));
            }

            [Fact]
            public void PredicateSingleMatch()
            {
                string[] collection = new[] { "Hello", "World!" };

                string result = Assert.Single(collection, item => item.StartsWith("H"));

                Assert.Equal("Hello", result);
            }

            [Fact]
            public void PredicateNoMatch()
            {
                string[] collection = new[] { "Hello", "World!" };

                Exception ex = Record.Exception(() => Assert.Single(collection, item => false));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 0 elements instead of 1.", ex.Message);
            }

            [Fact]
            public void PredicateTooManyMatches()
            {
                string[] collection = new[] { "Hello", "World!" };

                Exception ex = Record.Exception(() => Assert.Single(collection, item => true));

                Assert.IsType<SingleException>(ex);
                Assert.Equal("The collection contained 2 elements instead of 1.", ex.Message);
            }
        }
    }
}
