using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class DisposalTrackerTests
{
	public class AfterDisposed : IAsyncLifetime
	{
		readonly DisposalTracker classUnderTest = new();

		public ValueTask InitializeAsync() =>
			classUnderTest.DisposeAsync();

		public ValueTask DisposeAsync() => default;

		[Fact]
		public void AddThrows()
		{
			var ex = Record.Exception(() => classUnderTest.Add(new object()));

			Assert.NotNull(ex);
			Assert.IsType<ObjectDisposedException>(ex);
		}

		[Fact]
		public async ValueTask DisposeAsyncThrows()
		{
			var classUnderTest = new DisposalTracker();
			await classUnderTest.DisposeAsync();

			var ex = await Record.ExceptionAsync(() => classUnderTest.DisposeAsync());

			Assert.NotNull(ex);
			Assert.IsType<ObjectDisposedException>(ex);
		}
	}

	public class WithAction
	{
		readonly DisposalTracker classUnderTest = new();
		bool disposed = false;

		public WithAction() =>
			classUnderTest.AddAction(() => disposed = true);

		[Fact]
		public void GuardClause()
		{
			var ex = Record.Exception(() => classUnderTest.AddAction(null!));

			Assert.NotNull(ex);
			var argEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("cleanupAction", argEx.ParamName);
		}

		[Fact]
		public async ValueTask DisposeCallsAction()
		{
			await classUnderTest.DisposeAsync();

			Assert.True(disposed);
		}
	}

	public class WithAsyncAction
	{
		readonly DisposalTracker classUnderTest = new();
		bool disposed = false;

		public WithAsyncAction() =>
			classUnderTest.AddAsyncAction(() =>
			{
				disposed = true;
				return default;
			});

		[Fact]
		public void GuardClause()
		{
			var ex = Record.Exception(() => classUnderTest.AddAsyncAction(null!));

			Assert.NotNull(ex);
			var argEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("cleanupAction", argEx.ParamName);
		}

		[Fact]
		public async ValueTask DisposeCallsAction()
		{
			await classUnderTest.DisposeAsync();

			Assert.True(disposed);
		}
	}

	public class WithAsyncDisposable
	{
		readonly DisposalTracker classUnderTest = new();
		readonly IAsyncDisposable expected = Substitute.For<IAsyncDisposable, InterfaceProxy<IAsyncDisposable>>();

		public WithAsyncDisposable() =>
			classUnderTest.Add(expected);

		[Fact]
		public void AsyncDisposablesContainsObject()
		{
			var actuals = classUnderTest.AsyncDisposables;

			var actual = Assert.Single(actuals);
			Assert.Same(expected, actual);
		}

		[Fact]
		public void AsyncDisposablesEmptiesList()
		{
			_ = classUnderTest.AsyncDisposables;

			var actuals = classUnderTest.AsyncDisposables;

			Assert.Empty(actuals);
		}

		[Fact]
		public void DisposablesIsEmpty()
		{
			var actuals = classUnderTest.Disposables;

			Assert.Empty(actuals);
		}

		[Fact]
		public async ValueTask ObjectIsAsyncDisposed()
		{
			await classUnderTest.DisposeAsync();

			_ = expected.Received().DisposeAsync();
		}
	}

	public class WithDisposable
	{
		readonly DisposalTracker classUnderTest = new();
		readonly IDisposable expected = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();

		public WithDisposable() =>
			classUnderTest.Add(expected);

		[Fact]
		public void AsyncDisposablesIsEmpty()
		{
			var actuals = classUnderTest.AsyncDisposables;

			Assert.Empty(actuals);
		}

		[Fact]
		public void DisposablesContainsObject()
		{
			var actuals = classUnderTest.Disposables;

			var actual = Assert.Single(actuals);
			Assert.Same(expected, actual);
		}

		[Fact]
		public void DisposablesEmptiesList()
		{
			_ = classUnderTest.Disposables;

			var actuals = classUnderTest.Disposables;

			Assert.Empty(actuals);
		}

		[Fact]
		public async ValueTask ObjectIsDisposed()
		{
			await classUnderTest.DisposeAsync();

			expected.Received().Dispose();
		}
	}

	public class WithMixedDisposalObject
	{
		readonly DisposalTracker classUnderTest = new();
		readonly MixedDisposableObject expected = new();

		public WithMixedDisposalObject() =>
			classUnderTest.Add(expected);

		[Fact]
		public void AsyncDisposablesContainsObject()
		{
			var actuals = classUnderTest.AsyncDisposables;

			var actual = Assert.Single(actuals);
			Assert.Same(expected, actual);
		}

		[Fact]
		public void DisposablesContainsObject()
		{
			var actuals = classUnderTest.Disposables;

			var actual = Assert.Single(actuals);
			Assert.Same(expected, actual);
		}

		[Fact]
		public async ValueTask ObjectIsAsyncDisposedThenDisposed()
		{
			await classUnderTest.DisposeAsync();

			Assert.Collection(
				expected.Operations,
				op => Assert.Equal("DisposeAsync", op),
				op => Assert.Equal("Dispose", op)
			);
		}

		class MixedDisposableObject : IDisposable, IAsyncDisposable
		{
			public List<string> Operations = new List<string>();

			public void Dispose() =>
				Operations.Add("Dispose");

			public ValueTask DisposeAsync()
			{
				Operations.Add("DisposeAsync");
				return default(ValueTask);
			}
		}
	}

	public class WithNonDisposable
	{
		readonly DisposalTracker classUnderTest = new();
		readonly object expected = new();

		public WithNonDisposable() =>
			classUnderTest.Add(expected);

		[Fact]
		public void AsyncDisposablesIsEmpty()
		{
			var actuals = classUnderTest.AsyncDisposables;

			Assert.Empty(actuals);
		}

		[Fact]
		public void DisposablesIsEmpty()
		{
			var actuals = classUnderTest.Disposables;

			Assert.Empty(actuals);
		}
	}
}
