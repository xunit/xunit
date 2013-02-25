using System;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class ReflectorTests
{
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
            }

            class ChildAttribute : ParentAttribute { }

            [Child]
            class ClassWithChildAttribute { }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var childAttribute = CustomAttributeData.GetCustomAttributes(typeof(ClassWithChildAttribute)).Single();

                var results = Reflector.Wrap(childAttribute)
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
            }

            class Child : Parent
            {
                public override void TheMethod() { }
            }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Child).GetMethod("TheMethod"))
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
            }

            class Child : Parent { }

            [Fact]
            public void Child_ReturnsParentAndGrandparent()
            {
                var results = Reflector.Wrap(typeof(Child))
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Grandparent2", attributeUnderTest.Level);
                    });
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    });
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    });
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
                                       .GetCustomAttributes(typeof(AttributeUnderTest));

                Assert.Collection(results,
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent2", attributeUnderTest.Level);
                    },
                    attr =>
                    {
                        var reflectionResult = Assert.IsAssignableFrom<IReflectionAttributeInfo>(attr);
                        var attributeUnderTest = Assert.IsType<AttributeUnderTest>(reflectionResult.Attribute);
                        Assert.Equal("Parent1", attributeUnderTest.Level);
                    });
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
}