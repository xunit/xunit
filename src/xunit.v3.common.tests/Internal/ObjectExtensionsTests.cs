using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;

public class ObjectExtensionsTests
{
	public class AsValueTask
	{
		[Fact]
		public void NullValue()
		{
			var result = ObjectExtensions.AsValueTask(null);

			Assert.Null(result);
		}

		[Fact]
		public void NonTaskValue()
		{
			var result = ObjectExtensions.AsValueTask(42);

			Assert.Null(result);
		}

		[Fact]
		public async ValueTask ValueTaskValue()
		{
			var task = new ValueTask<int>(42);

			var result = ObjectExtensions.AsValueTask(task);

			Assert.True(result.HasValue);
			Assert.Equal(42, await result.Value);
		}

		[Fact]
		public async ValueTask TaskValue()
		{
			var task = Task.FromResult(42);

			var result = ObjectExtensions.AsValueTask(task);

			Assert.True(result.HasValue);
			Assert.Equal(42, await result.Value);
		}
	}
}
