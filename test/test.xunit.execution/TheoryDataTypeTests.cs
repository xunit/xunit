using System.Collections.Generic;
using Xunit;

namespace Xunit
{
    public class TheoryDataTypeTests
    {
        public class ClassWithTheoryDataTypeDiffrentToArgumentType
        {
            public class A
            {
            }

            public class B
            {
                public static explicit operator A(B value)
                {
                    return new A();
                }

                public static explicit operator B(A value)
                {
                    return new B();
                }
            }

            public static IEnumerable<object[]> Data = new[] { new object[] { new B() }, new object[] { new B() } };

            [Theory]
            [MemberData("Data")]
            public void Test(A x) { }
        }
    }
}