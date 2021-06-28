using System;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class _NullMessageSinkTests
{
	[Fact]
	public void OneInstanceAssertsEqual()
	{
		var sink = new _NullMessageSink();

		Assert.Equal(sink, sink);
	}

	[Fact]
	public void TwoInstancesAssertEqual()
	{
		var sink1 = new _NullMessageSink();
		var sink2 = new _NullMessageSink();

		Assert.Equal(sink1, sink2);
	}

	[Fact]
	public void TwoInstancesAreObjectEqual()
	{
		var sink1 = new _NullMessageSink();
		var sink2 = new _NullMessageSink();

		Assert.True(sink1.Equals(sink2));
	}

	[Fact]
	public void TwoInstancesAreOperatorEqual()
	{
		var sink1 = new _NullMessageSink();
		var sink2 = new _NullMessageSink();

		Assert.True(sink1 == sink2);
	}

	[Fact]
	public void DifferentTypeIsNotEqual()
	{
		var sink = new _NullMessageSink();
		var obj = new object();

		Assert.NotEqual(sink, obj);
		Assert.False(sink.Equals(obj));
	}
}
