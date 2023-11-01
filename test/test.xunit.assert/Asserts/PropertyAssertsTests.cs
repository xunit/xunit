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
			Assert.Throws<ArgumentNullException>("object", () => Assert.PropertyChanged(null!, "propertyName", delegate { }));
			Assert.Throws<ArgumentNullException>("testCode", () => Assert.PropertyChanged(Substitute.For<INotifyPropertyChanged>(), "propertyName", (Action)null!));
		}

		[Fact]
		public void ExceptionThrownWhenPropertyNotChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(() => Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => { }));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public void ExceptionThrownWhenWrongPropertyChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(() => Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => obj.Property2 = 42));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public void NoExceptionThrownWhenPropertyChanged()
		{
			var obj = new NotifiedClass();

			Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => obj.Property1 = "NewValue");
		}

		[Fact]
		public void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
		{
			var obj = new NotifiedClass();

			Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () =>
			{
				obj.Property2 = 12;
				obj.Property1 = "New Value";
				obj.Property2 = 42;
			});
		}
	}

	public class PropertyChangedAsync
	{
		[Fact]
		public async Task GuardClauses()
		{
			await Assert.ThrowsAsync<ArgumentNullException>("object", () => Assert.PropertyChangedAsync(null!, "propertyName", () => Task.FromResult(0)));
			await Assert.ThrowsAsync<ArgumentNullException>("testCode", () => Assert.PropertyChangedAsync(Substitute.For<INotifyPropertyChanged>(), "propertyName", default(Func<Task>)!));
		}

		[Fact]
		public async Task ExceptionThrownWhenPropertyNotChanged_Task()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(() => Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), () => Task.FromResult(0)));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

#pragma warning disable CS1998
		[Fact]
		public async Task ExceptionThrownWhenWrongPropertyChangedAsync_Task()
		{
			var obj = new NotifiedClass();
			async Task setter() => obj!.Property2 = 42;

			var ex = await Record.ExceptionAsync(() => Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), setter));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public async Task NoExceptionThrownWhenPropertyChangedAsync_Task()
		{
			var obj = new NotifiedClass();
			async Task setter() => obj!.Property1 = "NewValue";

			await Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), setter);
		}

		[Fact]
		public async Task NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty_Task()
		{
			var obj = new NotifiedClass();

			async Task setter()
			{
				obj.Property2 = 12;
				obj.Property1 = "New Value";
				obj.Property2 = 42;
			}

			await Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), setter);
		}
#pragma warning restore CS1998
	}

	class NotifiedClass : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		public string Property1
		{
			set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Property1))); }
		}

		public int Property2
		{
			set { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Property2))); }
		}
	}
}
