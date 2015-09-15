using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class PropertyAssertsTests
{
    [Fact]
    public void GuardClauses()
    {
        var ex1 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(null, "propertyName", delegate { }));
        Assert.Equal("object", ex1.ParamName);

        var ex2 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(Substitute.For<INotifyPropertyChanged>(), "propertyName", null));
        Assert.Equal("testCode", ex2.ParamName);
    }

    [Fact]
    public void ExceptionThrownWhenPropertyNotChanged()
    {
        ObservableClass obj = new ObservableClass();

        Exception ex = Record.Exception(
            () => Assert.PropertyChanged(obj, "Property1", () => { })
        );

        Assert.IsType<PropertyChangedException>(ex);
        Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
    }

    [Fact]
    public void ExceptionThrownWhenWrongPropertyChanged()
    {
        ObservableClass obj = new ObservableClass();

        Exception ex = Record.Exception(
            () => Assert.PropertyChanged(obj, "Property1", () => obj.Property2 = 42)
        );

        Assert.IsType<PropertyChangedException>(ex);
        Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
    }

    [Fact]
    public void NoExceptionThrownWhenPropertyChanged()
    {
        ObservableClass obj = new ObservableClass();

        Exception ex = Record.Exception(
            () => Assert.PropertyChanged(obj, "Property1", () => obj.Property1 = "NewValue")
        );

        Assert.Null(ex);
    }

    [Fact]
    public void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
    {
        ObservableClass obj = new ObservableClass();

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

    [Theory]
    [InlineData(42, 12)]
    [InlineData(12, 0)]
    public void ExceptionThrownWhenPropertyChangedPrematurely(int oldValue, int expectedValue)
    {
        var obj = new ObservableClass();
        obj.PropertyWithPrematureNotification = oldValue;

        var ex = Record.Exception(
            () =>
            {
                Assert.PropertyChanged(obj, nameof(obj.PropertyWithPrematureNotification), () =>
                {
                    obj.PropertyWithPrematureNotification = expectedValue;
                });
            }
        );

        Assert.IsType<PropertyChangedPrematurelyException>(ex);
        Assert.Equal("Assert.PropertyChanged failure: Property PropertyWithPrematureNotification was not set to a new value before PropertyChanged was raised", ex.Message);
    }

    [Fact]
    public void ExceptionThrownWhenPropertyIsInaccessible()
    {
        var obj = new ObservableClass();

        var ex = Record.Exception(
            () =>
            {
                Assert.PropertyChanged(obj, "PropertyWithNoGetter", () =>
                {
                    obj.PropertyWithNoGetter = "New Value";
                });
            }
        );
        Assert.IsType<InaccessiblePropertyException>(ex);
        Assert.Equal("Assert.PropertyChanged failure: Property PropertyWithNoGetter does not have a public getter", ex.Message);
    }

    class ObservableClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate {};

        private string _property1;
        public string Property1
        {
            set
            {
                _property1 = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Property1"));
            }
            get { return _property1; }
        }

        private int _property2;
        public int Property2
        {
            set
            {
                _property2 = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Property2"));
            }
            get { return _property2; }
        }

        private int _propertyWithPrematureNotification;
        public int PropertyWithPrematureNotification
        {
            set
            {
                PropertyChanged(this, new PropertyChangedEventArgs("PropertyWithPrematureNotification"));
                _propertyWithPrematureNotification = value;
            }
            get
            {
                return _propertyWithPrematureNotification;
            }
        }

        public Object PropertyWithNoGetter
        {
            set { PropertyChanged(this, new PropertyChangedEventArgs("PropertyWithNoGetter"));}
        }
    }
}