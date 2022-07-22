using System;
using System.ComponentModel;
using System.Threading.Tasks;
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
			var ex1 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(null!, "propertyName", delegate { }));
			Assert.Equal("object", ex1.ParamName);

			var ex2 = Assert.Throws<ArgumentNullException>(() => Assert.PropertyChanged(Substitute.For<INotifyPropertyChanged>(), "propertyName", (Action)null!));
			Assert.Equal("testCode", ex2.ParamName);
		}

		[Fact]
		public void ExceptionThrownWhenPropertyNotChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(
				() => Assert.PropertyChanged(obj, "Property1", () => { })
			);

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
		}

		[Fact]
		public void ExceptionThrownWhenWrongPropertyChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(
				() => Assert.PropertyChanged(obj, "Property1", () => obj.Property2 = 42)
			);

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
		}

		[Fact]
		public void NoExceptionThrownWhenPropertyChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(
				() => Assert.PropertyChanged(obj, "Property1", () => obj.Property1 = "NewValue")
			);

			Assert.Null(ex);
		}

		[Fact]
		public void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
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
					});
				}
			);

			Assert.Null(ex);
		}
	}

	public class PropertyChangedAsync
	{
#pragma warning disable CS1998
		[Fact]
		public async Task GuardClauses()
		{
			var ex1 = await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.PropertyChangedAsync(null!, "propertyName", async delegate { }));
			Assert.Equal("object", ex1.ParamName);

			var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.PropertyChangedAsync(Substitute.For<INotifyPropertyChanged>(), "propertyName", null!));
			Assert.Equal("testCode", ex2.ParamName);
		}

		[Fact]
		public async Task ExceptionThrownWhenPropertyNotChanged()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.PropertyChangedAsync(obj, "Property1", async () => { })
			);

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
		}

		[Fact]
		public async Task ExceptionThrownWhenWrongPropertyChangedAsync()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.PropertyChangedAsync(obj, "Property1", async () => obj.Property2 = 42)
			);

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged failure: Property Property1 was not set", ex.Message);
		}

		[Fact]
		public async Task NoExceptionThrownWhenPropertyChangedAsync()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.PropertyChangedAsync(obj, "Property1", async () => obj.Property1 = "NewValue")
			);

			Assert.Null(ex);
		}

		[Fact]
		public async Task NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(
				() => Assert.PropertyChangedAsync(
					obj,
					"Property1",
					async () =>
					{
						obj.Property2 = 12;
						obj.Property1 = "New Value";
						obj.Property2 = 42;
					}
				)
			);

			Assert.Null(ex);
		}
#pragma warning restore CS1998
	}

	class NotifiedClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Property1
		{
			set { PropertyChanged!(this, new PropertyChangedEventArgs("Property1")); }
		}

		public int Property2
		{
			set { PropertyChanged!(this, new PropertyChangedEventArgs("Property2")); }
		}
	}
}
