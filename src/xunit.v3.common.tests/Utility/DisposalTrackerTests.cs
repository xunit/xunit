using Xunit;
using Xunit.Sdk;

public class DisposalTrackerTests
{
	public sealed class AfterDisposed : IAsyncLifetime
	{
		readonly DisposableClass classToDispose = new();
		readonly DisposalTracker classUnderTest = new();

		public ValueTask InitializeAsync()
		{
			classUnderTest.Add(classToDispose);

			return classUnderTest.DisposeAsync();
		}

		public ValueTask DisposeAsync() => default;

		[Fact]
		public void AddThrows()
		{
			var ex = Record.Exception(() => classUnderTest.Add(new object()));

			Assert.NotNull(ex);
			Assert.IsType<ObjectDisposedException>(ex);
		}

		[Fact]
		public async ValueTask DisposeAsyncDoesNotDoubleDispose()
		{
			Assert.Equal(1, classToDispose.DisposeCount);  // Already disposed in InitializeAsync

			await classUnderTest.DisposeAsync();

			Assert.Equal(1, classToDispose.DisposeCount);
		}

		class DisposableClass : IDisposable
		{
			public int DisposeCount;

			public void Dispose() =>
				DisposeCount++;
		}
	}

	public class ExceptionHandling
	{
		[Fact]
		public async ValueTask NoExceptions_DoesNotThrow()
		{
			var classUnderTest = new DisposalTracker();
			var obj = new SpyDisposable();
			classUnderTest.Add(obj);

			var ex = await Record.ExceptionAsync(async () => await classUnderTest.DisposeAsync());

			Assert.Null(ex);
			Assert.Equal(1, obj.DisposeCalled);
		}

		[Fact]
		public async ValueTask SingleException_CleansUpAllObjects_ThrowsTheSingleException()
		{
			var classUnderTest = new DisposalTracker();
			var obj1 = new SpyDisposable();
			classUnderTest.Add(obj1);
			var thrown = new DivideByZeroException();
			var obj2 = new SpyDisposable { DisposeException = thrown };
			classUnderTest.Add(obj2);
			var obj3 = new SpyDisposable();
			classUnderTest.Add(obj3);

			var ex = await Record.ExceptionAsync(async () => await classUnderTest.DisposeAsync());

			Assert.Same(thrown, ex);
			Assert.Equal(1, obj1.DisposeCalled);
			Assert.Equal(1, obj2.DisposeCalled);
			Assert.Equal(1, obj3.DisposeCalled);
		}

		[Fact]
		public async ValueTask MultipleExceptions_ThrowsAggregateException()
		{
			var classUnderTest = new DisposalTracker();
			var obj1 = new SpyDisposable { DisposeException = new DivideByZeroException() };
			classUnderTest.Add(obj1);
			var obj2 = new SpyDisposable { DisposeException = new InvalidOperationException() };
			classUnderTest.Add(obj2);

			var ex = await Record.ExceptionAsync(async () => await classUnderTest.DisposeAsync());

			var aggEx = Assert.IsType<AggregateException>(ex);
			Assert.Collection(
				aggEx.InnerExceptions,
				ex => Assert.IsType<InvalidOperationException>(ex),
				ex => Assert.IsType<DivideByZeroException>(ex)
			);
		}
	}

	public class MultiObject
	{
		readonly DisposalTracker classUnderTest = new();

		[Fact]
		public async ValueTask FirstInLastAsyncDisposed()
		{
			var messages = new List<string>();
			var obj1 = new TrackingAsyncDisposable("1", messages);
			classUnderTest.Add(obj1);
			var obj2 = new TrackingAsyncDisposable("2", messages);
			classUnderTest.Add(obj2);

			await classUnderTest.DisposeAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("2: DisposeAsync", msg),
				msg => Assert.Equal("1: DisposeAsync", msg)
			);
		}

		[Fact]
		public async ValueTask FirstInLastDisposed()
		{
			var messages = new List<string>();
			var obj1 = new TrackingDisposable("1", messages);
			classUnderTest.Add(obj1);
			var obj2 = new TrackingDisposable("2", messages);
			classUnderTest.Add(obj2);

			await classUnderTest.DisposeAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("2: Dispose", msg),
				msg => Assert.Equal("1: Dispose", msg)
			);
		}

		[Fact]
		public async ValueTask MixedObjectTypes()
		{
			var messages = new List<string>();
			var obj1 = new TrackingDisposable("1", messages);
			classUnderTest.Add(obj1);
			var obj2 = new TrackingAsyncDisposable("2", messages);
			classUnderTest.Add(obj2);
			var obj3 = new TrackingDisposable("3", messages);
			classUnderTest.Add(obj3);
			var obj4 = new TrackingAsyncDisposable("4", messages);
			classUnderTest.Add(obj4);

			await classUnderTest.DisposeAsync();

			Assert.Collection(
				messages,
				msg => Assert.Equal("4: DisposeAsync", msg),
				msg => Assert.Equal("3: Dispose", msg),
				msg => Assert.Equal("2: DisposeAsync", msg),
				msg => Assert.Equal("1: Dispose", msg)
			);
		}

		[Fact]
		public void TrackedObjectsReturnsReverseOrder()
		{
			var obj1 = new SpyDisposable();
			classUnderTest.Add(obj1);
			var obj2 = new SpyAsyncDisposable();
			classUnderTest.Add(obj2);
			var obj3 = new SpyDisposable();
			classUnderTest.Add(obj3);
			var obj4 = new SpyAsyncDisposable();
			classUnderTest.Add(obj4);

			var trackedObjects = classUnderTest.TrackedObjects;

			Assert.Collection(
				trackedObjects,
				obj => Assert.Same(obj4, obj),
				obj => Assert.Same(obj3, obj),
				obj => Assert.Same(obj2, obj),
				obj => Assert.Same(obj1, obj)
			);
		}

		class TrackingDisposable(
			string id,
			List<string> messages) :
				IDisposable
		{
			public void Dispose() =>
				messages.Add($"{id}: Dispose");
		}

		class TrackingAsyncDisposable(
			string id,
			List<string> messages) :
				IAsyncDisposable
		{
			public ValueTask DisposeAsync()
			{
				messages.Add($"{id}: DisposeAsync");
				return default;
			}
		}

		class TrackingMixedDisposable(
			string id,
			List<string> messages) :
				IDisposable, IAsyncDisposable
		{
			public void Dispose() =>
				messages.Add($"{id}: Dispose");

			public ValueTask DisposeAsync()
			{
				messages.Add($"{id}: DisposeAsync");
				return default;
			}
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
		readonly SpyAsyncDisposable expected = new();

		public WithAsyncDisposable() =>
			classUnderTest.Add(expected);

		[Fact]
		public void ClearRemovesAllObjects()
		{
			Assert.NotEmpty(classUnderTest.TrackedObjects);

			classUnderTest.Clear();

			Assert.Empty(classUnderTest.TrackedObjects);
		}

		[Fact]
		public async ValueTask ObjectIsAsyncDisposed()
		{
			await classUnderTest.DisposeAsync();

			Assert.Equal(1, expected.DisposeAsyncCalled);
		}
	}

	public class WithDisposable
	{
		readonly DisposalTracker classUnderTest = new();
		readonly SpyDisposable expected = new();

		public WithDisposable() =>
			classUnderTest.Add(expected);

		[Fact]
		public void ClearRemovesAllObjects()
		{
			Assert.NotEmpty(classUnderTest.TrackedObjects);

			classUnderTest.Clear();

			Assert.Empty(classUnderTest.TrackedObjects);
		}

		[Fact]
		public async ValueTask ObjectIsDisposed()
		{
			await classUnderTest.DisposeAsync();

			Assert.Equal(1, expected.DisposeCalled);
		}
	}

	public class WithMixedDisposalObject
	{
		readonly DisposalTracker classUnderTest = new();
		readonly MixedDisposableObject expected = new();

		public WithMixedDisposalObject() =>
			classUnderTest.Add(expected);

		[Fact]
		public void ClearRemovesAllObjects()
		{
			Assert.NotEmpty(classUnderTest.TrackedObjects);

			classUnderTest.Clear();

			Assert.Empty(classUnderTest.TrackedObjects);
		}

		[Fact]
		public async ValueTask OnlyDisposeAsyncIsCalled()
		{
			await classUnderTest.DisposeAsync();

			var op = Assert.Single(expected.Operations);
			Assert.Equal("DisposeAsync", op);
		}

		class MixedDisposableObject : IDisposable, IAsyncDisposable
		{
			public List<string> Operations = [];

			public void Dispose() =>
				Operations.Add("Dispose");

			public ValueTask DisposeAsync()
			{
				Operations.Add("DisposeAsync");
				return default;
			}
		}
	}
}
