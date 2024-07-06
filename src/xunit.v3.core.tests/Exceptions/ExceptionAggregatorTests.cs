using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.v3;

public class ExceptionAggregatorTests
{
	[Fact]
	public void EmptyByDefault()
	{
		var aggregator = new ExceptionAggregator();

		Assert.False(aggregator.HasExceptions);
	}

	[Fact]
	public void CanAddOneAggregatorToAnother()
	{
		var aggregator1 = new ExceptionAggregator();
		aggregator1.Add(new DivideByZeroException());
		var aggregator2 = new ExceptionAggregator();
		aggregator2.Add(new InvalidOperationException());

		aggregator2.Aggregate(aggregator1);

		var result = aggregator2.ToException();
		var aggEx = Assert.IsType<AggregateException>(result);
		Assert.Collection(
			aggEx.InnerExceptions,
			ex => Assert.IsType<InvalidOperationException>(ex),
			ex => Assert.IsType<DivideByZeroException>(ex)
		);
	}

	[Fact]
	public void CapturesException()
	{
		var aggregator = new ExceptionAggregator();

		aggregator.Run(() => throw new DivideByZeroException());

		var result = aggregator.ToException();
		Assert.IsType<DivideByZeroException>(result);
	}

	[Fact]
	public async ValueTask CapturesExceptionAsync()
	{
		var aggregator = new ExceptionAggregator();

		await aggregator.RunAsync(async () =>
		{
			await Task.Yield();
			throw new DivideByZeroException();
		});

		var result = aggregator.ToException();
		Assert.IsType<DivideByZeroException>(result);
	}
}
