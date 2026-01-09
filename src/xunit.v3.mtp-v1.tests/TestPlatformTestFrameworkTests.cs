using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;
using NSubstitute;
using Xunit;
using Xunit.MicrosoftTestingPlatform;
using Xunit.Runner.Common;

public class TestPlatformTestFrameworkTests
{
	public class SessionManagement
	{
		[Fact]
		public async ValueTask CanCreateAndCloseSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var createResult = await framework.CreateTestSession(uid);

			Assert.True(createResult.IsSuccess);
			Assert.True(framework.RunnerReporter.HandlerCreated);
			Assert.False(framework.RunnerReporter.HandlerDisposed);

			var closeResult = await framework.CloseTestSession(uid);

			Assert.True(closeResult.IsSuccess);
			Assert.True(framework.RunnerReporter.HandlerDisposed);
		}

		[Fact]
		public async ValueTask CreateSession_WithLogo()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Project.Configuration.NoLogo = false;

			await framework.CreateTestSession(uid);

			var log = Assert.Single(framework.RunnerLogger.Messages);
			Assert.StartsWith("[Imp] xUnit.net v3 In-Process Runner", log);
		}

		[Fact]
		public async ValueTask CreateSession_WithoutLogo()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Project.Configuration.NoLogo = true;

			await framework.CreateTestSession(uid);

			Assert.Empty(framework.RunnerLogger.Messages);
		}

		[Fact]
		public async ValueTask CannotCreateSameSessionTwice()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var firstResult = await framework.CreateTestSession(uid);
			var secondResult = await framework.CreateTestSession(uid);

			Assert.True(firstResult.IsSuccess);
			Assert.False(secondResult.IsSuccess);
			Assert.Equal("Attempted to reuse session UID 'abc' already in progress", secondResult.ErrorMessage);
		}

		[Fact]
		public async ValueTask CannotCloseUnstartedSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var result = await framework.CloseTestSession(uid);

			Assert.False(result.IsSuccess);
			Assert.Equal("Attempted to close unknown session UID 'abc'", result.ErrorMessage);
		}

		[Fact]
		public async ValueTask CannotRecloseSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			await framework.CreateTestSession(uid);
			var firstResult = await framework.CloseTestSession(uid);
			var secondResult = await framework.CloseTestSession(uid);

			Assert.True(firstResult.IsSuccess);
			Assert.False(secondResult.IsSuccess);
			Assert.Equal("Attempted to close unknown session UID 'abc'", secondResult.ErrorMessage);
		}
	}

	[Collection(nameof(EnvironmentHelper.NullifyEnvironmentalReporters))]
	public sealed class DiscoveryAndExecution : IDisposable
	{
		readonly IDisposable environmentCleanup;

		public DiscoveryAndExecution() =>
			environmentCleanup = EnvironmentHelper.NullifyEnvironmentalReporters();

		public void Dispose() =>
			environmentCleanup.Dispose();

		[Fact]
		public async ValueTask SessionMustBeActiveForDiscovery()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();

			var ex = await Record.ExceptionAsync(async () => await framework.OnDiscover(uid, filter: null, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Attempted to run discovery request against unknown session UID 'abc'", ex.Message);
		}

		[Fact]
		public async ValueTask SessionMustBeActiveForExecution()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();

			var ex = await Record.ExceptionAsync(async () => await framework.OnExecute(uid, filter: null, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Attempted to run execution request against unknown session UID 'abc'", ex.Message);
		}

		[Fact]
		public async ValueTask UnsupportedDiscoveryFilter()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();
			await framework.CreateTestSession(uid);
			var filter = Substitute.For<ITestExecutionFilter, InterfaceProxy<ITestExecutionFilter>>();

			var ex = await Record.ExceptionAsync(async () => await framework.OnDiscover(uid, filter, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("filter", argEx.ParamName);
			Assert.StartsWith($"Unsupported discovery filter type '{filter.GetType().FullName}'", ex.Message);
		}

		[Fact]
		public async ValueTask UnsupportedExecutionFilter()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();
			await framework.CreateTestSession(uid);
			var filter = Substitute.For<ITestExecutionFilter, InterfaceProxy<ITestExecutionFilter>>();

			var ex = await Record.ExceptionAsync(async () => await framework.OnExecute(uid, filter, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			var argEx = Assert.IsType<ArgumentException>(ex);
			Assert.Equal("filter", argEx.ParamName);
			Assert.StartsWith($"Unsupported execution filter type '{filter.GetType().FullName}'", ex.Message);
		}

		[Fact]
		public async ValueTask CanDiscoverAndExecuteTests()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Configuration.Filters.AddIncludedClassFilter(typeof(SessionManagement).FullName!);
			await framework.CreateTestSession(uid);

			// Discover tests

			await framework.OnDiscover(uid, filter: null, messageBus, () => completionCalled = true, CancellationToken.None);

			Assert.True(completionCalled);
			var testNodeUpdates = messageBus.PublishedData.OfType<TestNodeUpdateMessage>().ToArray();
			Assert.Collection(
				testNodeUpdates.Select(tnu => tnu.TestNode.DisplayName).OrderBy(x => x),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CanCreateAndCloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCloseUnstartedSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCreateSameSessionTwice)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotRecloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithLogo)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithoutLogo)}", testCaseDisplayName)
			);

			// Reset observation

			messageBus.PublishedData.Clear();
			completionCalled = false;

			// Execute the discovered tests

			var testNodeUIDs = testNodeUpdates.Select(tnu => tnu.TestNode.Uid).ToArray();
			var filter = new TestNodeUidListFilter(testNodeUIDs);

			await framework.OnExecute(uid, filter, messageBus, () => completionCalled = true, CancellationToken.None);

			Assert.True(completionCalled);
			testNodeUpdates = messageBus.PublishedData.OfType<TestNodeUpdateMessage>().ToArray();
			var inProgressTestNodes = testNodeUpdates.Where(tnu => tnu.TestNode.Properties.Any<InProgressTestNodeStateProperty>()).ToArray();
			Assert.Collection(
				inProgressTestNodes.Select(tnu => tnu.TestNode.DisplayName).OrderBy(x => x),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CanCreateAndCloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCloseUnstartedSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCreateSameSessionTwice)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotRecloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithLogo)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithoutLogo)}", testCaseDisplayName)
			);
			var passingTestNodes = testNodeUpdates.Where(tnu => tnu.TestNode.Properties.Any<PassedTestNodeStateProperty>()).ToArray();
			Assert.Collection(
				passingTestNodes.Select(tnu => tnu.TestNode.DisplayName).OrderBy(x => x),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CanCreateAndCloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCloseUnstartedSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotCreateSameSessionTwice)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CannotRecloseSession)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithLogo)}", testCaseDisplayName),
				testCaseDisplayName => Assert.Equal($"{typeof(SessionManagement).FullName}.{nameof(SessionManagement.CreateSession_WithoutLogo)}", testCaseDisplayName)
			);
		}

		// TODO: Live output
		// TODO: Generated reports
	}

	class TestableTestPlatformTestFramework(
		SpyRunnerLogger runnerLogger,
		SpyRunnerReporter runnerReporter,
		SpyMessageSink diagnosticMessageSink,
		XunitProjectAssembly projectAssembly,
		Assembly testAssembly,
		XunitTrxCapability trxCapability,
		SpyTestPlatformOutputDevice outputDevice,
		bool serverMode) :
			TestPlatformTestFramework(runnerLogger, runnerReporter, diagnosticMessageSink, projectAssembly, testAssembly, trxCapability, outputDevice, serverMode, EmptyResultWriters)
	{
		static readonly Dictionary<string, IMicrosoftTestingPlatformResultWriter> EmptyResultWriters = [];

		public XunitProjectAssembly ProjectAssembly { get; } = projectAssembly;

		public SpyRunnerLogger RunnerLogger { get; } = runnerLogger;

		public SpyRunnerReporter RunnerReporter { get; } = runnerReporter;

		public static TestableTestPlatformTestFramework Create()
		{
			var testAssembly = typeof(TestPlatformTestFrameworkTests).Assembly;
			var assemblyMetadata = new AssemblyMetadata(3, "TargetFramework/1.0");
			var projectAssembly = new XunitProjectAssembly(new XunitProject(), testAssembly.Location, assemblyMetadata) { Assembly = testAssembly };
			var trxCapability = new XunitTrxCapability();

			return new TestableTestPlatformTestFramework(
				new SpyRunnerLogger(),
				new SpyRunnerReporter(),
				SpyMessageSink.Capture(),
				projectAssembly,
				testAssembly,
				trxCapability,
				new SpyTestPlatformOutputDevice(),
				serverMode: false
			);
		}
	}
}
