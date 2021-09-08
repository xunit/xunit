using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// A base implementation of <see cref="_ITestFrameworkDiscoverer"/> that supports test filtering
	/// and runs the discovery process on a thread pool thread.
	/// </summary>
	/// <typeparam name="TTestCase">The type of the test case used by the test framework. Must
	/// derive from <see cref="_ITestCase"/>.</typeparam>
	public abstract class TestFrameworkDiscoverer<TTestCase> : _ITestFrameworkDiscoverer, IAsyncDisposable
		where TTestCase : _ITestCase
	{
		_IAssemblyInfo assemblyInfo;
		_IMessageSink diagnosticMessageSink;
		bool disposed;
		readonly Lazy<string> targetFramework;

		/// <summary>
		/// Initializes a new instance of the <see cref="TestFrameworkDiscoverer{TTestCase}"/> class.
		/// </summary>
		/// <param name="assemblyInfo">The test assembly.</param>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		protected TestFrameworkDiscoverer(
			_IAssemblyInfo assemblyInfo,
			_IMessageSink diagnosticMessageSink)
		{
			this.assemblyInfo = Guard.ArgumentNotNull(nameof(assemblyInfo), assemblyInfo);
			this.diagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

			targetFramework = new Lazy<string>(() => AssemblyInfo.GetTargetFramework());
		}

		/// <summary>
		/// Gets the assembly that's being discovered.
		/// </summary>
		protected internal _IAssemblyInfo AssemblyInfo
		{
			get => assemblyInfo;
			set => assemblyInfo = Guard.ArgumentNotNull(nameof(AssemblyInfo), value);
		}

		/// <summary>
		/// Gets the message sink used to report diagnostic messages.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink
		{
			get => diagnosticMessageSink;
			set => diagnosticMessageSink = Guard.ArgumentNotNull(nameof(DiagnosticMessageSink), value);
		}

		/// <summary>
		/// Gets the disposal tracker for the test framework discoverer.
		/// </summary>
		protected DisposalTracker DisposalTracker { get; } = new();

		/// <summary>
		/// Gets the unique ID for the test assembly under test.
		/// </summary>
		public abstract string TestAssemblyUniqueID { get; }

		/// <inheritdoc/>
		public string TargetFramework => targetFramework.Value;

		/// <inheritdoc/>
		public abstract string TestFrameworkDisplayName { get; }

		/// <summary>
		/// Implement this method to create a test class for the given CLR type.
		/// </summary>
		/// <param name="class">The CLR type.</param>
		/// <returns>The test class.</returns>
		protected internal abstract ValueTask<_ITestClass> CreateTestClass(_ITypeInfo @class);

		/// <inheritdoc/>
		public virtual ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			return DisposalTracker.DisposeAsync();
		}

		/// <inheritdoc/>
		public ValueTask Find(
			Func<_ITestCase, ValueTask<bool>> callback,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			Type[]? types = null)
		{
			Guard.ArgumentNotNull("callback", callback);
			Guard.ArgumentNotNull("discoveryOptions", discoveryOptions);

			var tcs = new TaskCompletionSource<object?>();

			ThreadPool.QueueUserWorkItem(async _ =>
			{
				using (new PreserveWorkingFolder(AssemblyInfo))
				using (new CultureOverride(discoveryOptions.Culture()))
				{
					var typeInfos =
						types == null
							? AssemblyInfo.GetTypes(includePrivateTypes: false)
							: types.Select(Reflector.Wrap).WhereNotNull().CastOrToReadOnlyList();

					foreach (var typeInfo in typeInfos.Where(IsValidTestClass))
					{
						var testClass = await CreateTestClass(typeInfo);

						try
						{
							if (!await FindTestsForType(testClass, discoveryOptions, testCase => callback(testCase)))
								break;
						}
						catch (Exception ex)
						{
							DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Exception during discovery:{Environment.NewLine}{ex}" });
						}
					}
				}

				tcs.SetResult(null);
			});

			return new(tcs.Task);
		}

		/// <summary>
		/// Core implementation to discover unit tests in a given test class.
		/// </summary>
		/// <param name="testClass">The test class.</param>
		/// <param name="discoveryOptions">The options used by the test framework during discovery.</param>
		/// <param name="discoveryCallback">The callback that is called for each discovered test case.
		/// The return value of the callback indicates the same thing as the return value of this function:
		/// return <c>true</c> to continue discovery, or <c>false</c> to halt it.</param>
		/// <returns>Returns <c>true</c> if discovery should continue; <c>false</c> otherwise.</returns>
		protected abstract ValueTask<bool> FindTestsForType(
			_ITestClass testClass,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			Func<TTestCase, ValueTask<bool>> discoveryCallback
		);

		/// <summary>
		/// Determines if a type should be used for discovery. Can be used to filter out types that
		/// are not desirable. The default implementation filters out abstract (non-static) classes.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>Returns <c>true</c> if the type can contain tests; <c>false</c>, otherwise.</returns>
		protected virtual bool IsValidTestClass(_ITypeInfo type)
		{
			Guard.ArgumentNotNull(nameof(type), type);

			return !type.IsAbstract || type.IsSealed;
		}
	}
}
