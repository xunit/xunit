using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.Requests;
using Microsoft.Testing.Platform.TestHost;
using Xunit;
using Xunit.Runner.Common;
using Xunit.Runner.InProc.SystemConsole.TestingPlatform;

public class TestPlatformTestFrameworkTests
{
	public class SessionManagement
	{
		[Fact]
		public void CanCreateAndCloseSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var createResult = framework.CreateTestSession(uid);

			Assert.True(createResult.IsSuccess);

			var closeResult = framework.CloseTestSession(uid);

			Assert.True(closeResult.IsSuccess);
		}

		[Fact]
		public void CreateSession_WithLogo()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Project.Configuration.NoLogo = false;

			framework.CreateTestSession(uid);

			var log = Assert.Single(framework.RunnerLogger.Messages);
			Assert.StartsWith("[Imp] xUnit.net v3 In-Process Runner", log);
		}

		[Fact]
		public void CreateSession_WithoutLogo()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Project.Configuration.NoLogo = true;

			framework.CreateTestSession(uid);

			Assert.Empty(framework.RunnerLogger.Messages);
		}

		[Fact]
		public void CannotCreateSameSessionTwice()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var firstResult = framework.CreateTestSession(uid);
			var secondResult = framework.CreateTestSession(uid);

			Assert.True(firstResult.IsSuccess);
			Assert.False(secondResult.IsSuccess);
			Assert.Equal("Attempted to reuse session UID 'abc' already in progress", secondResult.ErrorMessage);
		}

		[Fact]
		public void CannotCloseUnstartedSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			var result = framework.CloseTestSession(uid);

			Assert.False(result.IsSuccess);
			Assert.Equal("Attempted to close unknown session UID 'abc'", result.ErrorMessage);
		}

		[Fact]
		public void CannotRecloseSession()
		{
			var uid = new SessionUid("abc");
			var framework = TestableTestPlatformTestFramework.Create();

			framework.CreateTestSession(uid);
			var firstResult = framework.CloseTestSession(uid);
			var secondResult = framework.CloseTestSession(uid);

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

			var ex = await Record.ExceptionAsync(() => framework.OnDiscover(uid, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Attempted to execute request against unknown session UID 'abc'", ex.Message);
		}

		[Fact]
		public async ValueTask SessionMustBeActiveForExecution()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();

			var ex = await Record.ExceptionAsync(() => framework.OnExecute(uid, filter: null, messageBus, () => completionCalled = true, CancellationToken.None));

			Assert.False(completionCalled);
			Assert.IsType<ArgumentException>(ex);
			Assert.StartsWith("Attempted to execute request against unknown session UID 'abc'", ex.Message);
		}

		[Fact]
		public async ValueTask CanDiscoverAndExecuteTests()
		{
			var completionCalled = false;
			var uid = new SessionUid("abc");
			var messageBus = new SpyTestPlatformMessageBus();
			var framework = TestableTestPlatformTestFramework.Create();
			framework.ProjectAssembly.Configuration.Filters.AddIncludedClassFilter(typeof(SessionManagement).FullName!);
			framework.CreateTestSession(uid);

			// Discover tests

			await framework.OnDiscover(uid, messageBus, () => completionCalled = true, CancellationToken.None);

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
		SpyMessageSink innerSink,
		SpyMessageSink diagnosticMessageSink,
		XunitProjectAssembly projectAssembly,
		Assembly testAssembly,
		XunitTrxCapability trxCapability,
		SpyTestPlatformOutputDevice outputDevice) :
			TestPlatformTestFramework(runnerLogger, innerSink, diagnosticMessageSink, projectAssembly, testAssembly, trxCapability, outputDevice)
	{
		public XunitProjectAssembly ProjectAssembly { get; } = projectAssembly;

		public SpyRunnerLogger RunnerLogger { get; } = runnerLogger;

		public static TestableTestPlatformTestFramework Create()
		{
			var testAssembly = typeof(TestPlatformTestFrameworkTests).Assembly;
			var assemblyMetadata = new AssemblyMetadata(3, "TargetFramework/1.0");
			var projectAssembly = new XunitProjectAssembly(new XunitProject(), testAssembly.Location, assemblyMetadata) { Assembly = testAssembly };
			var trxCapability = new XunitTrxCapability();

			return new TestableTestPlatformTestFramework(
				new SpyRunnerLogger(),
				SpyMessageSink.Capture(),
				SpyMessageSink.Capture(),
				projectAssembly,
				testAssembly,
				trxCapability,
				new SpyTestPlatformOutputDevice()
			);
		}
	}
}
