using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Xunit;
using Xunit.Extensions;

namespace Xunit1
{
    public class ExceptionRequirementTests
    {
        [Theory]
        [ExceptionTypes]
        public void ExceptionMustBeSerializable(Type type)
        {
            Assert.Single(type.GetCustomAttributes(typeof(SerializableAttribute), false));
            Assert.NotNull(type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null));
        }

        class ExceptionTypes : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                return typeof(Assert).Assembly
                                     .GetExportedTypes()
                                     .Where(t => typeof(Exception).IsAssignableFrom(t))
                                     .Select(t => new object[] { t });
            }
        }
    }
}
