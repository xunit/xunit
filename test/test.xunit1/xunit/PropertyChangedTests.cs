using System;
using System.ComponentModel;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class PropertyChangedTests
    {
        [Fact]
        public void GuardClauses()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(null, "propertyName", delegate { }));
            Assert.Equal("object", ex1.ParamName);

            var ex2 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(new Mock<INotifyPropertyChanged>().Object, "propertyName", null));
            Assert.Equal("testCode", ex2.ParamName);
        }

        [Fact]
        public void ExceptionThrownWhenPropertyNotChanged()
        {
            NotifiedClass obj = new NotifiedClass();

            Exception ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => { })
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Fact]
        public void ExceptionThrownWhenWrongPropertyChanged()
        {
            NotifiedClass obj = new NotifiedClass();

            Exception ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => obj.Property2 = 42)
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Fact]
        public void NoExceptionThrownWhenPropertyChanged()
        {
            NotifiedClass obj = new NotifiedClass();

            Exception ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => obj.Property1 = "NewValue")
            );

            Assert.Null(ex);
        }

        [Fact]
        public void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
        {
            NotifiedClass obj = new NotifiedClass();

            Exception ex = Record.Exception(
                () =>
                {
                    Assert.PropertyChanged(obj, "Property1", () =>
                    {
                        obj.Property2 = 12;
                        obj.Property1 = "New Value";
                        obj.Property2 = 42;
                    });
                }
            );

            Assert.Null(ex);
        }

        class NotifiedClass : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public string Property1
            {
                set { PropertyChanged(this, new PropertyChangedEventArgs("Property1")); }
            }

            public int Property2
            {
                set { PropertyChanged(this, new PropertyChangedEventArgs("Property2")); }
            }
        }
    }
}
