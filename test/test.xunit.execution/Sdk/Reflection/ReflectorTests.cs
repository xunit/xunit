using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ReflectorTests
{
    public class Conversion
    {
        [Theory]
        [InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
        [InlineData("4EBCD32C-A2B8-4600-9E72-3873347E285C")]
        [InlineData("39A3B4C85FEF43A988EB4BB4AC4D4103")]
        [InlineData("{5b21e154-15eb-4b1e-bc30-127e8a41eca1}")]
        [InlineData("4ebcd32c-a2b8-4600-9e72-3873347e285c")]
        [InlineData("39a3b4c85fef43a988eb4bb4ac4d4103")]
        public void ConvertsStringToGuid(string text)
        {
            var guid = Guid.Parse(text);

            var args = Reflector.ConvertArguments(new object[] { text }, new[] { typeof(Guid) });

            Assert.Equal(guid, Assert.IsType<Guid>(args[0]));
        }

        [Theory]
        [InlineData("2017-11-3")]
        [InlineData("2017-11-3 16:48")]
        [InlineData("16:48")]
        public void ConvertsStringToDateTime(string text)
        {
            var dateTime = DateTime.Parse(text, CultureInfo.InvariantCulture);

            var args = Reflector.ConvertArguments(new object[] { text }, new[] { typeof(DateTime) });

            Assert.Equal(dateTime, Assert.IsType<DateTime>(args[0]));
        }

        [Theory]
        [InlineData("2017-11-3")]
        [InlineData("2017-11-3 16:48")]
        [InlineData("16:48")]
        public void ConvertsStringToDateTimeOffset(string text)
        {
            var dateTimeOffset = DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);

            var args = Reflector.ConvertArguments(new object[] { text }, new[] { typeof(DateTimeOffset) });

            Assert.Equal(dateTimeOffset, Assert.IsType<DateTimeOffset>(args[0]));
        }

        [Theory]
        [InlineData("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}")]
        public void GuidSmokeTest(Guid actual)
        {
            var expected = Guid.Parse("{5B21E154-15EB-4B1E-BC30-127E8A41ECA1}");

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2017-11-3 16:48")]
        public void DateTimeSmokeTest(DateTime actual)
        {
            var expected = DateTime.Parse("2017-11-3 16:48", CultureInfo.InvariantCulture);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("2017-11-3 16:48")]
        public void DateTimeOffsetSmokeTest(DateTimeOffset actual)
        {
            var expected = DateTimeOffset.Parse("2017-11-3 16:48", CultureInfo.InvariantCulture);

            Assert.Equal(expected, actual);
        }
    }

    public class Undecorated // Defaults, inherited = true, multiple = false
    {
        class AttributeUnderTest : Attribute
        {
            public AttributeUnderTest(string level)
            {
                Level = level;
            }

            public int Counter { get; set; }

            public string Level { get; private set; }
        }

        public class OnAttribute
        {
            [AttributeUnderTest("Grandparent")]
            class GrandparentAttribute : Attribute { }

            [AttributeUnderTest("Parent")]
            class ParentAttribute : GrandparentAttribute { }

            [Parent]
            class ClassWithParentAttribute { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var parentAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithParentAttribute)).Single();

                var results = Reflector.Wrap(parentAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class UndecoratedAttribute : Attribute { }

            [Undecorated]
            class ClassWithUndecoratedAttribute { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var undecoratedAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithUndecoratedAttribute)).Single();

                var results = Reflector.Wrap(undecoratedAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnMethod
        {
            class Grandparent
            {
                [AttributeUnderTest("Grandparent")]
                public virtual void TheMethod() { }
            }

            class Parent : Grandparent
            {
                [AttributeUnderTest("Parent")]
                public override void TheMethod() { }
            }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Undecorated
            {
                public void TheMethod() { }
            }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnType
        {
            [AttributeUnderTest("Grandparent")]
            class Grandparent { }

            [AttributeUnderTest("Parent")]
            class Parent : Grandparent { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Child))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }
    }

    public class Inherited_Single
    {
        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = false)]
        class AttributeUnderTest : Attribute
        {
            public AttributeUnderTest(string level)
            {
                Level = level;
            }

            public int Counter { get; set; }

            public string Level { get; private set; }
        }

        public class OnAttribute
        {
            [AttributeUnderTest("Grandparent")]
            class GrandparentAttribute : Attribute { }

            [AttributeUnderTest("Parent")]
            class ParentAttribute : GrandparentAttribute { }

            [Parent]
            class ClassWithParentAttribute { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var parentAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithParentAttribute)).Single();

                var results = Reflector.Wrap(parentAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class UndecoratedAttribute : Attribute { }

            [Undecorated]
            class ClassWithUndecoratedAttribute { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var undecoratedAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithUndecoratedAttribute)).Single();

                var results = Reflector.Wrap(undecoratedAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnMethod
        {
            class Grandparent
            {
                [AttributeUnderTest("Grandparent")]
                public virtual void TheMethod() { }
            }

            class Parent : Grandparent
            {
                [AttributeUnderTest("Parent")]
                public override void TheMethod() { }
            }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Undecorated
            {
                public void TheMethod() { }
            }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnType
        {
            [AttributeUnderTest("Grandparent")]
            class Grandparent { }

            [AttributeUnderTest("Parent")]
            class Parent : Grandparent { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Child))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }
    }

    public class Inherited_Multiple
    {
        [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
        class AttributeUnderTest : Attribute
        {
            public AttributeUnderTest(string level)
            {
                Level = level;
            }

            public int Counter { get; set; }

            public string Level { get; private set; }
        }

        public class OnAttribute
        {
            [AttributeUnderTest("Grandparent1")]
            [AttributeUnderTest("Grandparent2")]
            class GrandparentAttribute : Attribute { }

            [AttributeUnderTest("Parent1")]
            [AttributeUnderTest("Parent2")]
            class ParentAttribute : GrandparentAttribute { }

            [Parent]
            class ClassWithParentAttribute { }

            [Fact]
            public void Parent_ReturnsParentAndGrandparent()
            {
                var parentAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithParentAttribute)).Single();

                var results = Reflector.Wrap(parentAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class UndecoratedAttribute : Attribute { }

            [Undecorated]
            class ClassWithUndecoratedAttribute { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var undecoratedAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithUndecoratedAttribute)).Single();

                var results = Reflector.Wrap(undecoratedAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnMethod
        {
            class Grandparent
            {
                [AttributeUnderTest("Grandparent1")]
                [AttributeUnderTest("Grandparent2")]
                public virtual void TheMethod() { }
            }

            class Parent : Grandparent
            {
                [AttributeUnderTest("Parent1")]
                [AttributeUnderTest("Parent2")]
                public override void TheMethod() { }
            }

            [Fact]
            public void Parent_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Parent).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated)).GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnType
        {
            [AttributeUnderTest("Grandparent1")]
            [AttributeUnderTest("Grandparent2")]
            class Grandparent { }

            [AttributeUnderTest("Parent1")]
            [AttributeUnderTest("Parent2")]
            class Parent : Grandparent { }

            [Fact]
            public void Parent_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Parent))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Child))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Grandparent1", level),
                    level => Assert.Equal("Grandparent2", level),
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }
    }

    public class NonInherited_Single
    {
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
        class AttributeUnderTest : Attribute
        {
            public AttributeUnderTest(string level)
            {
                Level = level;
            }

            public int Counter { get; set; }

            public string Level { get; private set; }
        }

        public class OnAttribute
        {
            [AttributeUnderTest("Grandparent")]
            class GrandparentAttribute : Attribute { }

            [AttributeUnderTest("Parent")]
            class ParentAttribute : GrandparentAttribute { }

            [Parent]
            class ClassWithParentAttribute { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var parentAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithParentAttribute)).Single();

                var results = Reflector.Wrap(parentAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class UndecoratedAttribute : Attribute { }

            [Undecorated]
            class ClassWithUndecoratedAttribute { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var undecoratedAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithUndecoratedAttribute)).Single();

                var results = Reflector.Wrap(undecoratedAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnMethod
        {
            class Grandparent
            {
                [AttributeUnderTest("Grandparent")]
                public virtual void TheMethod() { }
            }

            class Parent : Grandparent
            {
                [AttributeUnderTest("Parent")]
                public override void TheMethod() { }
            }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class Undecorated
            {
                public void TheMethod() { }
            }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnType
        {
            [AttributeUnderTest("Grandparent")]
            class Grandparent { }

            [AttributeUnderTest("Parent")]
            class Parent : Grandparent { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                var result = Assert.Single(results);
                var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(result);
                var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                Assert.Equal("Parent", attributeUnderTest.Level);
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Child))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }
    }

    public class NonInherited_Multiple
    {
        [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
        class AttributeUnderTest : Attribute
        {
            public AttributeUnderTest(string level)
            {
                Level = level;
            }

            public int Counter { get; set; }

            public string Level { get; private set; }
        }

        public class OnAttribute
        {
            [AttributeUnderTest("Grandparent1")]
            [AttributeUnderTest("Grandparent2")]
            class GrandparentAttribute : Attribute { }

            [AttributeUnderTest("Parent1")]
            [AttributeUnderTest("Parent2")]
            class ParentAttribute : GrandparentAttribute { }

            [Parent]
            class ClassWithParentAttribute { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var parentAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithParentAttribute)).Single();

                var results = Reflector.Wrap(parentAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class UndecoratedAttribute : Attribute { }

            [Undecorated]
            class ClassWithUndecoratedAttribute { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var undecoratedAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithUndecoratedAttribute)).Single();

                var results = Reflector.Wrap(undecoratedAttribute)
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnMethod
        {
            class Grandparent
            {
                [AttributeUnderTest("Grandparent1")]
                [AttributeUnderTest("Grandparent2")]
                public virtual void TheMethod() { }
            }

            class Parent : Grandparent
            {
                [AttributeUnderTest("Parent1")]
                [AttributeUnderTest("Parent2")]
                public override void TheMethod() { }
            }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class Undecorated
            {
                public void TheMethod() { }
            }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated).GetMethod("TheMethod"))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }

        public class OnType
        {
            [AttributeUnderTest("Grandparent1")]
            [AttributeUnderTest("Grandparent2")]
            class Grandparent { }

            [AttributeUnderTest("Parent1")]
            [AttributeUnderTest("Parent2")]
            class Parent : Grandparent { }

            [Fact]
            public void Parent_ReturnsOnlyParent()
            {
                var results = Reflector.Wrap(typeof(Parent))
                                      .GetCustomAttributes(typeof(AttributeUnderTest))
                                      .Cast<IReflectionAttributeInfo>()
                                      .Select(attr => attr.Attribute)
                                      .Cast<AttributeUnderTest>()
                                      .Select(attr => attr.Level)
                                      .OrderBy(level => level);

                Assert.Collection(results,
                    level => Assert.Equal("Parent1", level),
                    level => Assert.Equal("Parent2", level)
                );
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Child))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }

            class Undecorated { }

            [Fact]
            public void Undecorated_ReturnsNothing()
            {
                var results = Reflector.Wrap(typeof(Undecorated))
                                      .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Empty(results);
            }
        }
    }

    public class AmbiguousMethodFromBaseClass_NonGenericToGeneric
    {
        class Parent
        {
            public virtual void TheMethod(int x) { }

            [Fact]
            public virtual void TheMethod<T>(T t, long y) { }
        }

        class Child : Parent
        {
            public override void TheMethod(int x) { }

            public override void TheMethod<T>(T t, long y) { }
        }

        [Fact]
        public void NonTest_ReturnsNothing()
        {
            var results = Reflector.Wrap(typeof(Child).GetMethods().Single(m => m.Name == "TheMethod" && !m.IsGenericMethod))
                                  .GetCustomAttributes(typeof(FactAttribute));

            Assert.Empty(results);
        }

        [Fact]
        public void Test_ReturnsMatch()
        {
            var results = Reflector.Wrap(typeof(Child).GetMethods().Single(m => m.Name == "TheMethod" && m.IsGenericMethod))
                                  .GetCustomAttributes(typeof(FactAttribute));

            Assert.Single(results);
        }
    }

    public class AmbiguousMethodFromBaseClass_GenericCountMismatch
    {
        class Parent
        {
            public virtual void TheMethod<T1>(T1 t1) { }

            [Fact]
            public virtual void TheMethod<T1, T2>(T1 t1, T2 t2) { }
        }

        class Child : Parent
        {
            public override void TheMethod<T1>(T1 t1) { }

            public override void TheMethod<T1, T2>(T1 t1, T2 t2) { }
        }

        [Fact]
        public void NonTest_ReturnsNothing()
        {
            var results = Reflector.Wrap(typeof(Child).GetMethods().Single(m => m.Name == "TheMethod" && m.GetGenericArguments().Length == 1))
                                  .GetCustomAttributes(typeof(FactAttribute));

            Assert.Empty(results);
        }

        [Fact]
        public void Test_ReturnsMatch()
        {
            var results = Reflector.Wrap(typeof(Child).GetMethods().Single(m => m.Name == "TheMethod" && m.GetGenericArguments().Length == 2))
                                  .GetCustomAttributes(typeof(FactAttribute));

            Assert.Single(results);
        }
    }

    public class ArrayTypesFromAttribute
    {
        enum FactState
        {
            Pause = 0,
            Start,
            Stop
        }

        class SuperFact : Attribute
        {
            public int[] Numbers { get; set; }
            public Type[] Types { get; set; }
            public FactState[] States { get; set; }
            public System.Collections.Generic.List<Type> ListTypes { get; set; }
        }

        [Fact]
        [SuperFact(Numbers = new[] { 1, 2, 3 })]
        public void ValueTypeArray()
        {
            var expected = new[] { 1, 2, 3 };
            var result = typeof(ArrayTypesFromAttribute).GetMethods().Single(m => m.Name == "ValueTypeArray")
                .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(SuperFact)))
                .Select(Reflector.Wrap)
                .Single();

            Assert.True(expected.SequenceEqual((result.Attribute as SuperFact).Numbers));
            Assert.True(expected.SequenceEqual(result.GetNamedArgument<int[]>("Numbers")));
        }

        [Fact]
        [SuperFact(Types = new[] { typeof(string), typeof(FactAttribute) })]
        public void ReferenceTypeArray()
        {
            var expected = new[] { typeof(string), typeof(FactAttribute) };
            var result = typeof(ArrayTypesFromAttribute).GetMethods().Single(m => m.Name == "ReferenceTypeArray")
                .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(SuperFact)))
                .Select(Reflector.Wrap)
                .Single();

            Assert.True(expected.SequenceEqual((result.Attribute as SuperFact).Types));
            Assert.True(expected.SequenceEqual(result.GetNamedArgument<Type[]>("Types")));
        }

        [Fact]
        [SuperFact(States = new[] { FactState.Pause, FactState.Start, FactState.Stop })]
        public void EnumTypeArray()
        {
            var expected = new[] { FactState.Pause, FactState.Start, FactState.Stop };
            var result = typeof(ArrayTypesFromAttribute).GetMethods().Single(m => m.Name == "EnumTypeArray")
                .CustomAttributes.Where(x => x.AttributeType.Equals(typeof(SuperFact)))
                .Select(Reflector.Wrap)
                .Single();

            Assert.True(expected.SequenceEqual((result.Attribute as SuperFact).States));
            Assert.True(expected.SequenceEqual(result.GetNamedArgument<FactState[]>("States")));
        }
    }
}
