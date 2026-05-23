using System.ComponentModel;
using Xunit;
using Xunit.Sdk;

public static class PropertyAssertsTests
{
	public static class PropertyChanged
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("object", () => Assert.PropertyChanged(null!, "propertyName", delegate { }));
			Assert.Throws<ArgumentNullException>("testCode", () => Assert.PropertyChanged(new NotifiedClass(), "propertyName", (Action)null!));
		}

		[Fact]
		public static void ExceptionThrownWhenPropertyNotChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(() => Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => { }));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public static void ExceptionThrownWhenWrongPropertyChanged()
		{
			var obj = new NotifiedClass();

			var ex = Record.Exception(() => Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => obj.Property2 = 42));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public static void NoExceptionThrownWhenPropertyChanged()
		{
			var obj = new NotifiedClass();

			Assert.PropertyChanged(obj, nameof(NotifiedClass.Property1), () => obj.Property1 = "NewValue");
		}

		[Fact]
		public static void NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty()
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

	public static class PropertyChangedAsync
	{
		[Fact]
		public static async Task GuardClauses()
		{
			await Assert.ThrowsAsync<ArgumentNullException>("object", () => Assert.PropertyChangedAsync(null!, "propertyName", () => Task.FromResult(0)));
			await Assert.ThrowsAsync<ArgumentNullException>("testCode", () => Assert.PropertyChangedAsync(new NotifiedClass(), "propertyName", default!));
		}

		[Fact]
		public static async Task ExceptionThrownWhenPropertyNotChanged_Task()
		{
			var obj = new NotifiedClass();

			var ex = await Record.ExceptionAsync(() => Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), () => Task.FromResult(0)));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public static async Task ExceptionThrownWhenWrongPropertyChangedAsync_Task()
		{
			var obj = new NotifiedClass();
			async Task setter() => obj!.Property2 = 42;

			var ex = await Record.ExceptionAsync(() => Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), setter));

			Assert.IsType<PropertyChangedException>(ex);
			Assert.Equal("Assert.PropertyChanged() failure: Property 'Property1' was not set", ex.Message);
		}

		[Fact]
		public static async Task NoExceptionThrownWhenPropertyChangedAsync_Task()
		{
			var obj = new NotifiedClass();
			async Task setter() => obj!.Property1 = "NewValue";

			await Assert.PropertyChangedAsync(obj, nameof(NotifiedClass.Property1), setter);
		}

		[Fact]
		public static async Task NoExceptionThrownWhenMultiplePropertyChangesIncludesCorrectProperty_Task()
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
