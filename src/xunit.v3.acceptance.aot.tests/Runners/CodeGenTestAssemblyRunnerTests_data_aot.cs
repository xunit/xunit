using Xunit;

public class CodeGenTestAssemblyRunnerTests
{
	public class Messages
	{
		public sealed class Passing : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public async Task TestMethod() => await Task.Yield();
		}

		public sealed class StaticPassing : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public static void TestMethod() { }
		}

		public sealed class Failed : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public void TestMethod() => Assert.True(false);
		}

		public sealed class SkippedViaAttribute : IDisposable
		{
			public void Dispose() { }

			[Fact(Skip = "Don't run me")]
			public void TestMethod() { }
		}

		public sealed class SkippedViaException : IDisposable
		{
			public void Dispose() { }

			[Fact]
			public void TestMethod() => Assert.Skip("This isn't a good time");
		}

		public sealed class SkippedViaRegisteredException : IDisposable
		{
			public void Dispose() { }

			[Fact(SkipExceptions = [typeof(DivideByZeroException)])]
			public void TestMethod() => throw new DivideByZeroException("Dividing by zero is really tough");
		}

		public sealed class NotRun : IDisposable
		{
			public void Dispose() { }

			[Fact(Explicit = true)]
			public void TestMethod() => Assert.Fail("Should not run");
		}
	}
}
