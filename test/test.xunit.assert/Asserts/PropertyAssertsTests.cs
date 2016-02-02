using System;
using System.ComponentModel;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class PropertyAssertsTests
{
    public class PropertyChanged
    {
        [Fact]
        public void GuardClauses()
        {
            var ex1 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(null, "propertyName", delegate { }));
            Assert.Equal("object", ex1.ParamName);

            var ex2 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(Substitute.For<INotifyPropertyChanged>(), "propertyName", (Action)null));
            Assert.Equal("testCode", ex2.ParamName);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("New Value")]
        public void ExceptionThrownWhenPropertyNotChanged(string expectedValue)
        {
            var obj = new NotifiedClass();

            var ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => { }, expectedValue)
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(42)]
        public void ExceptionThrownWhenWrongPropertyChanged(int? expectedValue)
        {
            var obj = new NotifiedClass();

            var ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => obj.Property2 = 42, expectedValue)
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("New Value")]
        public void NoExceptionThrownWhenPropertyChanged(string expectedValue)
        {
            var obj = new NotifiedClass();

            var ex = Record.Exception(
                () => Assert.PropertyChanged(obj, "Property1", () => obj.Property1 = "New Value", expectedValue)
            );

            Assert.Null(ex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("New Value")]
        public void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty(string expectedValue)
        {
            var obj = new NotifiedClass();

            var ex = Record.Exception(
                () =>
                {
                    Assert.PropertyChanged(obj, "Property1", () =>
                    {
                        obj.Property2 = 12;
                        obj.Property1 = "New Value";
                        obj.Property2 = 42;
                    }, expectedValue);
                }
            );

            Assert.Null(ex);
        }

        [Fact]
        public void ExceptionThrownWhenPropertyChangedToUnexpectedValue()
        {
            var obj = new NotifiedClass();

            var ex = Record.Exception(
                () =>
                    Assert.PropertyChanged(obj, "Property1", () => obj.Property1 = "Unexpected Value", "Expected Value"));

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set to expected value Expected Value", ex.Message);
        }
    }

    public class PropertyChangedAsync
    {
        [Fact]
        public async void GuardClauses()
        {
            var ex1 = await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.PropertyChangedAsync(null, "propertyName", async delegate { }));
            Assert.Equal("object", ex1.ParamName);

            var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.PropertyChangedAsync(Substitute.For<INotifyPropertyChanged>(), "propertyName", null));
            Assert.Equal("testCode", ex2.ParamName);
        }

        [Fact]
        public async void ExceptionThrownWhenPropertyNotChanged()
        {
            var obj = new NotifiedClass();

            var ex = await Record.ExceptionAsync(
                () => Assert.PropertyChangedAsync(obj, "Property1", async () => { })
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Fact]
        public async void ExceptionThrownWhenWrongPropertyChangedAsync()
        {
            var obj = new NotifiedClass();

            var ex = await Record.ExceptionAsync(
                () => Assert.PropertyChangedAsync(obj, "Property1", async () => obj.Property2 = 42)
            );

            Assert.IsType<PropertyChangedException>(ex);
            Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
        }

        [Fact]
        public async void NoExceptionThrownWhenPropertyChangedAsync()
        {
            var obj = new NotifiedClass();

            var ex = await Record.ExceptionAsync(
                () => Assert.PropertyChangedAsync(obj, "Property1", async () => obj.Property1 = "NewValue")
            );

            Assert.Null(ex);
        }

        [Fact]
        public async void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
        {
            var obj = new NotifiedClass();

            var ex = await Record.ExceptionAsync(
                () => Assert.PropertyChangedAsync(obj, "Property1", async () =>
                      {
                          obj.Property2 = 12;
                          obj.Property1 = "New Value";
                          obj.Property2 = 42;
                      })
            );

            Assert.Null(ex);
        }
    }

    class NotifiedClass : INotifyPropertyChanged
    {
        private string _property1;
        public event PropertyChangedEventHandler PropertyChanged;

        public string Property1
        {
            get { return _property1; }
            set
            {
                _property1 = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Property1"));
            }
        }

        public int Property2
        {
            set { PropertyChanged(this, new PropertyChangedEventArgs("Property2")); }
        }
    }
}