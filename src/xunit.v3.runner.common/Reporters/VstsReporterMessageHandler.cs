using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.Common
{
	/// <summary>
	/// An implementation of <see cref="_IMessageSink" /> that supports <see cref="VstsReporter" />.
	/// </summary>
	public class VstsReporterMessageHandler : DefaultRunnerReporterMessageHandler
	{
		const int MaxLength = 4096;

		readonly string accessToken;
		int assembliesInFlight;
		readonly string baseUri;
		readonly int buildId;
		VstsClient? client;
		readonly object clientLock = new();

		/// <summary>
		/// Initializes a new instance of the <see cref="VstsReporterMessageHandler" /> class.
		/// </summary>
		/// <param name="logger">The logger used to report messages</param>
		/// <param name="baseUri">The base URI for talking to Azure DevOps/VSTS</param>
		/// <param name="accessToken">The access token required to talk to Azure DevOps/VSTS</param>
		/// <param name="buildId">The ID of build that's currently being run</param>
		public VstsReporterMessageHandler(
			IRunnerLogger logger,
			string baseUri,
			string accessToken,
			int buildId)
				: base(logger)
		{
			this.baseUri = baseUri;
			this.accessToken = accessToken;
			this.buildId = buildId;
		}

		VstsClient Client
		{
			get
			{
				lock (clientLock)
				{
					if (client == null)
						client = new VstsClient(Logger, baseUri, accessToken, buildId);
				}

				return client;
			}
		}

		/// <inheritdoc/>
		protected override void HandleTestAssemblyFinished(MessageHandlerArgs<_TestAssemblyFinished> args)
		{
			base.HandleTestAssemblyFinished(args);

			lock (clientLock)
			{
				assembliesInFlight--;

				if (assembliesInFlight == 0)
				{
					// Drain the queue
					client?.Dispose(CancellationToken.None);
					client = null;
				}
			}
		}

		/// <inheritdoc/>
		protected override void HandleTestAssemblyStarting(MessageHandlerArgs<_TestAssemblyStarting> args)
		{
			base.HandleTestAssemblyStarting(args);

			lock (clientLock)
			{
				assembliesInFlight++;

				// Use the TFM attrib to disambiguate
				var tfm = args.Message.TargetFramework;
				var assemblyFileName = Path.GetFileName(args.Message.AssemblyPath) ?? "<unknown filename>";
				if (!string.IsNullOrWhiteSpace(tfm))
					assemblyFileName = $"{assemblyFileName} ({tfm})";

				// TODO: What's going on here? ^ assemblyFileName is a throw-away.
			}
		}

		/// <inheritdoc/>
		protected override void HandleTestStarting(MessageHandlerArgs<_TestStarting> args)
		{
			base.HandleTestStarting(args);

			var testStarting = args.Message;
			var assemblyMetadata = MetadataCache.TryGetAssemblyMetadata(testStarting);
			var classMetadata = MetadataCache.TryGetClassMetadata(testStarting);
			var methodMetadata = MetadataCache.TryGetMethodMetadata(testStarting);

			if (assemblyMetadata != null && classMetadata != null && methodMetadata != null)
			{
				var testName = $"{classMetadata.TestClass}.{methodMetadata.TestMethod}";

				VstsAddTest(testName, testStarting.TestDisplayName, assemblyMetadata.SimpleAssemblyName(), testStarting.TestUniqueID);
			}
		}

		/// <inheritdoc/>
		protected override void HandleTestPassed(MessageHandlerArgs<_TestPassed> args)
		{
			var testPassed = args.Message;

			VstsUpdateTest(testPassed.TestUniqueID, "Passed", Convert.ToInt64(testPassed.ExecutionTime * 1000), stdOut: testPassed.Output);

			base.HandleTestPassed(args);
		}

		/// <inheritdoc/>
		protected override void HandleTestSkipped(MessageHandlerArgs<_TestSkipped> args)
		{
			var testSkipped = args.Message;

			VstsUpdateTest(testSkipped.TestUniqueID, "NotExecuted", Convert.ToInt64(testSkipped.ExecutionTime * 1000));

			base.HandleTestSkipped(args);
		}

		/// <inheritdoc/>
		protected override void HandleTestFailed(MessageHandlerArgs<_TestFailed> args)
		{
			var testFailed = args.Message;

			VstsUpdateTest(
				testFailed.TestUniqueID,
				"Failed",
				Convert.ToInt64(testFailed.ExecutionTime * 1000),
				ExceptionUtility.CombineMessages(testFailed),
				ExceptionUtility.CombineStackTraces(testFailed),
				testFailed.Output
			);

			base.HandleTestFailed(args);
		}

		void VstsAddTest(
			string testName,
			string displayName,
			string fileName,
			string testUniqueID)
		{
			var body = new Dictionary<string, object?>
			{
				{ "testCaseTitle", displayName },
				{ "automatedTestName", testName },
				{ "automatedTestType", "UnitTest" },
				{ "automatedTestTypeId", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b" }, // This is used in the sample response and also appears in web searches
				{ "automatedTestId", testUniqueID },
				{ "automatedTestStorage", fileName },
				{ "state", "InProgress" },
				{ "startedDate", DateTime.UtcNow }
			};

			Client.AddTest(body, testUniqueID);
		}

		void VstsUpdateTest(
			string testUniqueID,
			string outcome,
			long? durationMilliseconds = null,
			string? errorMessage = null,
			string? errorStackTrace = null,
			string? stdOut = null)
		{
			var body = new Dictionary<string, object?>
			{
				{ "outcome", outcome },
				{ "durationInMs", durationMilliseconds },
				{ "state", "Completed" }
			};

			var msg = $"{errorMessage}\n{errorStackTrace}\n{TrimStdOut(stdOut)}".Trim();
			if (!string.IsNullOrWhiteSpace(msg))
				body.Add("errorMessage", msg);

			Client.UpdateTest(body, testUniqueID);
		}

		static string? TrimStdOut(string? str) =>
			str?.Length > MaxLength ? str.Substring(0, MaxLength) : str;
	}
}
