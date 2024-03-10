using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Capabilities.TestFramework;
using Microsoft.Testing.Platform.Extensions.TestFramework;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;
using Xunit.Runner.Common;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole.TestPlatform;

internal sealed class TestPlatformTestFramework : ITestFramework
{
	readonly object consoleLock;
	readonly _IMessageSink innerSink;
	readonly IEnumerable<string> parseWarnings;
	readonly XunitProject project;
	TestSessionContext? testSessionContext;

	public TestPlatformTestFramework(
		_IMessageSink innerSink,
		XunitProject project,
		IEnumerable<string> parseWarnings,
		object consoleLock)
	{
		this.innerSink = innerSink;
		this.project = project;
		this.parseWarnings = parseWarnings;
		this.consoleLock = consoleLock;
	}

	public string Description =>
		"Microsoft.Testing.Platform adapter for xUnit.net v3";

	public string DisplayName =>
		"xUnit.net";

	public string Uid =>
		"30ea7c6e-dd24-4152-a360-1387158cd41d";

	public string Version =>
		ThisAssembly.AssemblyVersion;

	public Task<CloseTestSessionResult> CloseTestSessionAsync(CloseTestSessionContext context)
	{
		testSessionContext = null;
		return Task.FromResult(new CloseTestSessionResult { IsSuccess = true });
	}

	public Task<CreateTestSessionResult> CreateTestSessionAsync(CreateTestSessionContext context)
	{
		testSessionContext = context;

		Console.WriteLine();
		ProjectRunner.PrintHeader();
		foreach (var parseWarning in parseWarnings)
			Console.WriteLine(parseWarning);

		return Task.FromResult(new CreateTestSessionResult { IsSuccess = true });
	}

	public async Task ExecuteRequestAsync(ExecuteRequestContext context)
	{
		if (testSessionContext is null)
			return;

		if (context.Request is RunTestExecutionRequest run)
		{
			var projectRunner = new ProjectRunner(consoleLock, () => context.CancellationToken.IsCancellationRequested);
			var messageHandler = new TestPlatformExecutionMessageSink(innerSink, testSessionContext, context, run);
			await projectRunner.RunProject(project, messageHandler);
		}
	}

	public Task<bool> IsEnabledAsync() =>
		Task.FromResult(true);

	public static void Register(
		_IMessageSink innerSink,
		ITestApplicationBuilder testApplicationBuilder,
		XunitProject project,
		IEnumerable<string> parseWarnings,
		object consoleLock)
	{
		var extension = new TestPlatformTestFramework(innerSink, project, parseWarnings, consoleLock);
		//testApplicationBuilder.AddRunSettingsService(extension);
		//testApplicationBuilder.AddTestCaseFilterService(extension);
		testApplicationBuilder.RegisterTestFramework(
			serviceProvider => new TestFrameworkCapabilities(),
			(capabilities, serviceProvider) => extension
		);
	}
}
