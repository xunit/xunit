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
			var obj = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			classUnderTest.Add(obj);

			var ex = await Record.ExceptionAsync(() => classUnderTest.DisposeAsync());

			Assert.Null(ex);
			obj.Received().Dispose();
		}

		[Fact]
		public async ValueTask SingleException_CleansUpAllObjects_ThrowsTheSingleException()
		{
			var classUnderTest = new DisposalTracker();
			var obj1 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			classUnderTest.Add(obj1);
			var thrown = new DivideByZeroException();
			var obj2 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			obj2.When(x => x.Dispose()).Throw(thrown);
			classUnderTest.Add(obj2);
			var obj3 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			classUnderTest.Add(obj3);

			var ex = await Record.ExceptionAsync(() => classUnderTest.DisposeAsync());

			Assert.Same(thrown, ex);
			obj1.Received().Dispose();
			obj2.Received().Dispose();
			obj3.Received().Dispose();
		}

		[Fact]
		public async ValueTask MultipleExceptions_ThrowsAggregateException()
		{
			var classUnderTest = new DisposalTracker();
			var obj1 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			obj1.When(x => x.Dispose()).Throw<DivideByZeroException>();
			classUnderTest.Add(obj1);
			var obj2 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			obj2.When(x => x.Dispose()).Throw<InvalidOperationException>();
			classUnderTest.Add(obj2);

			var ex = await Record.ExceptionAsync(() => classUnderTest.DisposeAsync());

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
			var obj1 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			classUnderTest.Add(obj1);
			var obj2 = Substitute.For<IAsyncDisposable, InterfaceProxy<IAsyncDisposable>>();
			classUnderTest.Add(obj2);
			var obj3 = Substitute.For<IDisposable, InterfaceProxy<IDisposable>>();
			classUnderTest.Add(obj3);
			var obj4 = Substitute.For<IAsyncDisposable, InterfaceProxy<IAsyncDisposable>>();
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

		class TrackingDisposable : IDisposable
		{
			readonly string id;
			readonly List<string> messages;

			public TrackingDisposable(
				string id,
				List<string> messages)
			{
				this.id = id;
				this.messages = messages;
			}

			public void Dispose() =>
				messages.Add($"{id}: Dispose");
		}

		class TrackingAsyncDisposable : IAsyncDisposable
		{
			readonly string id;
			readonly List<string> messages;

			public TrackingAsyncDisposable(
				string id,
				List<string> messages)
			{
				this.id = id;
				this.messages = messages;
			}

			public ValueTask DisposeAsync()
			{
				messages.Add($"{id}: DisposeAsync");
				return default;
			}
		}

		class TrackingMixedDisposable : IDisposable, IAsyncDisposable
		{
			readonly string id;
			readonly List<string> messages;

			public TrackingMixedDisposable(
				string id,
				List<string> messages)
			{
				this.id = id;
				this.messages = messages;
			}

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
		readonly IAsyncDisposable expected = Substitute.For<IAsyncDisposable, InterfaceProxy<IAsyncDisposable>>();

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
			public List<string> Operations = new();

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
