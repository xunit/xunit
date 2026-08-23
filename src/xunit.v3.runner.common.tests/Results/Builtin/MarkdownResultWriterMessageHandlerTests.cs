using Xunit;
using Xunit.Runner.Common;

public static class MarkdownResultWriterMessageHandlerTests
{
	[Fact]
	public static async ValueTask NoTests()
	{
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		var markdown = await handler.Markdown();

		Assert.Equal("""
			### Test Results

			No tests were run.

			""", markdown);
	}

	[Fact]
	public static async ValueTask Time_Milliseconds()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 0.123m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal("""
			### Test Results

			⌚ 123ms total run time
			🧪 1 test
			✅ 1 passed

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask Time_WithoutHours()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 02:0{{3.456}} total run time
			🧪 1 test
			✅ 1 passed

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask Time_WithHours()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 3723.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 1:02:0{{3.456}} total run time
			🧪 1 test
			✅ 1 passed

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask TestFailed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(executionTime: 123.456m);
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 02:0{{3.456}} total run time
			🧪 1 test
			❌ 1 failed

			#### Failed ❌

			* Test Display Name ⌚ {{123.456}}s
				_Exception:_
				```
				System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?
				/path/file.cs(42,0): at SomeInnerCall()
				/path/otherFile.cs(2112,0): at SomeOuterMethod
				```

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask TestSkipped()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testSkipped = TestData.TestSkipped(executionTime: 123.456m, reason: "I am not running today");
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 1, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 02:0{{3.456}} total run time
			🧪 1 test
			❔ 1 skipped

			#### Skipped ❔

			* Test Display Name: "I am not running today" ⌚ {{123.456}}s

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask TestNotRun()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testNotRun = TestData.TestNotRun();
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 1, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testNotRun);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 02:0{{3.456}} total run time
			🧪 1 test
			🚫 1 not run

			#### Not Run 🚫

			* Test Display Name ⌚0s

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask Error()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var error = TestData.ErrorMessage();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(error);
		handler.OnMessage(assemblyFinished);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 02:0{{3.456}} total run time
			🧪 1 test
			✅ 1 passed
			💣 1 error

			#### Errors 💣

			* Fatal Error ⌚ 0s
			  _Exception:_
			  ```
			  System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?
			  /path/file.cs(42,0): at SomeInnerCall()
			  /path/otherFile.cs(2112,0): at SomeOuterMethod
			  ```

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask MultipleAssemblies_UniqueNames()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(assemblyPath: "./assembly1.dll");
		var assemblyStarting2 = TestData.TestAssemblyStarting(assemblyPath: "./assembly2.dll", assemblyUniqueID: "assembly-id-2");
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(executionTime: 123.456m);
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished2 = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);
		handler.OnMessage(assemblyStarting2);
		handler.OnMessage(assemblyFinished2);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 04:0{{6.912}} total run time
			🧪 2 tests in 2 assemblies
			✅ 1 passed
			❌ 1 failed

			#### Failed ❌

			* Test Display Name ⌚ {{123.456}}s (`assembly1.dll`)
			  _Exception:_
			  ```
			  System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?
			  /path/file.cs(42,0): at SomeInnerCall()
			  /path/otherFile.cs(2112,0): at SomeOuterMethod
			  ```

			""", markdown, ignoreAllWhiteSpace: true);
	}

	[CulturedFact(["en-US", "fr-FR"])]
	public static async ValueTask MultipleAssemblies_NonUniqueNames()
	{
		var assemblyStarting = TestData.TestAssemblyStarting(assemblyPath: "./path1/assembly.dll");
		var assemblyStarting2 = TestData.TestAssemblyStarting(assemblyPath: "./path2/assembly.dll", assemblyUniqueID: "assembly-id-2");
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(executionTime: 123.456m);
		var testFinished = TestData.TestFinished();
		var assemblyFinished = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 1, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		var assemblyFinished2 = TestData.TestAssemblyFinished(executionTime: 123.456m, testsFailed: 0, testsNotRun: 0, testsSkipped: 0, testsTotal: 1);
		await using var handler = TestableMarkdownResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(testFinished);
		handler.OnMessage(assemblyFinished);
		handler.OnMessage(assemblyStarting2);
		handler.OnMessage(assemblyFinished2);

		var markdown = await handler.Markdown();

		Assert.Equal($$"""
			### Test Results

			⌚ 04:0{{6.912}} total run time
			🧪 2 tests in 2 assemblies
			✅ 1 passed
			❌ 1 failed

			#### Failed ❌

			* Test Display Name ⌚ {{123.456}}s (`./path1/assembly.dll`)
			  _Exception:_
			  ```
			  System.DivideByZeroException : Attempted to divide by zero. Did you really think that was going to work?
			  /path/file.cs(42,0): at SomeInnerCall()
			  /path/otherFile.cs(2112,0): at SomeOuterMethod
			  ```

			""", markdown, ignoreAllWhiteSpace: true);
	}

	class ClassUnderTest
	{
		[Fact]
		public async ValueTask TestMethod() { }
	}

	class TestableMarkdownResultWriterMessageHandler : MarkdownResultWriterMessageHandler
	{
		string? markdown;

		TestableMarkdownResultWriterMessageHandler() :
			base(new MemoryStream())
		{
			OnDisposed += text => markdown = text;
		}

		public async ValueTask<string> Markdown()
		{
			await DisposeAsync();

			return markdown?.ReplaceOrdinal(Environment.NewLine, "\n") ?? throw new InvalidOperationException("Markdown callback was never called");
		}

		public static TestableMarkdownResultWriterMessageHandler Create() => new();
	}
}
