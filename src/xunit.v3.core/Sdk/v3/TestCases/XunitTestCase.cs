using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.v3
{
	/// <summary>
	/// Default implementation of <see cref="IXunitTestCase"/> for xUnit v3 that supports tests decorated with
	/// <see cref="FactAttribute"/>. Tests decorated with derived attributes may use this as a base class
	/// to build from.
	/// </summary>
	[Serializable]
	[DebuggerDisplay(@"\{ class = {TestMethod.TestClass.Class.Name}, method = {TestMethod.Method.Name}, display = {DisplayName}, skip = {SkipReason} \}")]
	public class XunitTestCase : TestMethodTestCase, IXunitTestCase
	{
		static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> assemblyTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);
		static readonly ConcurrentDictionary<string, IReadOnlyCollection<_IAttributeInfo>> typeTraitAttributeCache = new(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc/>
		protected XunitTestCase(
			SerializationInfo info,
			StreamingContext context) :
				base(info, context)
		{
			// No way for us to get access to the message sink on the execution deserialization path, but that should
			// be okay, because we assume all the issues were reported during discovery.
			DiagnosticMessageSink = new _NullMessageSink();
			Timeout = info.GetValue<int>("Timeout");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestCase"/> class.
		/// </summary>
		/// <remarks>
		/// This constructor is intended to be used by test methods which are decorated directly with <see cref="FactAttribute"/>
		/// (and not any derived attribute). Developers creating custom attributes derived from <see cref="FactAttribute"/>
		/// should create their own test case class (derived from this) and use the protected constructor instead.
		/// </remarks>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The test method this test case belongs to.</param>
		/// <param name="skipReason">The optional reason for skipping the test; if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="timeout">The optional timeout (in milliseconds); if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
		public XunitTestCase(
			_IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			string? skipReason = null,
			int? timeout = null,
			string? uniqueID = null)
				: this(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, null, skipReason, null, timeout, uniqueID)
		{ }

		/// <summary>
		/// Initializes a new instance of the <see cref="XunitTestCase"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="defaultMethodDisplay">Default method display to use (when not customized).</param>
		/// <param name="defaultMethodDisplayOptions">Default method display options to use (when not customized).</param>
		/// <param name="testMethod">The test method this test case belongs to.</param>
		/// <param name="testMethodArguments">The arguments for the test method.</param>
		/// <param name="skipReason">The optional reason for skipping the test; if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="traits">The optional traits list; if not provided, will be read from trait attributes.</param>
		/// <param name="timeout">The optional timeout (in milliseconds); if not provided, will be read from the <see cref="FactAttribute"/>.</param>
		/// <param name="uniqueID">The optional unique ID for the test case; if not provided, will be calculated.</param>
		protected XunitTestCase(
			_IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			object?[]? testMethodArguments,
			string? skipReason,
			Dictionary<string, List<string>>? traits,
			int? timeout,
			string? uniqueID)
				: base(defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments, skipReason, traits, uniqueID)
		{
			DiagnosticMessageSink = Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

			var factAttribute = TestMethod.Method.GetCustomAttributes(typeof(FactAttribute)).First();
			var baseDisplayName = factAttribute.GetNamedArgument<string>("DisplayName") ?? BaseDisplayName;

			DisplayName = TestMethod.Method.GetDisplayNameWithArguments(baseDisplayName, TestMethodArguments, MethodGenericTypes);
			SkipReason ??= factAttribute.GetNamedArgument<string>(nameof(FactAttribute.Skip));
			Timeout = timeout ?? factAttribute.GetNamedArgument<int>(nameof(FactAttribute.Timeout));

			if (traits == null)
			{
				foreach (var traitAttribute in GetTraitAttributesData(TestMethod))
				{
					var discovererAttribute = traitAttribute.GetCustomAttributes(typeof(TraitDiscovererAttribute)).FirstOrDefault();
					if (discovererAttribute != null)
					{
						var discoverer = ExtensibilityPointFactory.GetTraitDiscoverer(DiagnosticMessageSink, discovererAttribute);
						if (discoverer != null)
							foreach (var keyValuePair in discoverer.GetTraits(traitAttribute))
								Traits.Add(keyValuePair.Key, keyValuePair.Value);
					}
					else
						DiagnosticMessageSink.OnMessage(new _DiagnosticMessage { Message = $"Trait attribute on '{DisplayName}' did not have [TraitDiscoverer]" });
				}
			}
		}

		/// <summary>
		/// Gets the message sink used to report <see cref="_DiagnosticMessage"/> messages.
		/// </summary>
		protected _IMessageSink DiagnosticMessageSink { get; }

		/// <inheritdoc/>
		public int Timeout { get; protected set; }

		static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_IAssemblyInfo assembly)
		{
			Guard.ArgumentNotNull(nameof(assembly), assembly);

			return assemblyTraitAttributeCache.GetOrAdd(assembly.Name, () => assembly.GetCustomAttributes(typeof(ITraitAttribute)));
		}

		static IReadOnlyCollection<_IAttributeInfo> GetCachedTraitAttributes(_ITypeInfo type)
		{
			Guard.ArgumentNotNull(nameof(type), type);

			return typeTraitAttributeCache.GetOrAdd(type.Name, () => type.GetCustomAttributes(typeof(ITraitAttribute)));
		}

		static IReadOnlyCollection<_IAttributeInfo> GetTraitAttributesData(_ITestMethod testMethod)
		{
			Guard.ArgumentNotNull(nameof(testMethod), testMethod);

			return
				GetCachedTraitAttributes(testMethod.TestClass.Class.Assembly)
					.Concat(testMethod.Method.GetCustomAttributes(typeof(ITraitAttribute)))
					.Concat(GetCachedTraitAttributes(testMethod.TestClass.Class))
					.CastOrToReadOnlyCollection();
		}

		/// <inheritdoc/>
		public virtual Task<RunSummary> RunAsync(
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);
			Guard.ArgumentNotNull(nameof(messageBus), messageBus);
			Guard.ArgumentNotNull(nameof(constructorArguments), constructorArguments);
			Guard.ArgumentNotNull(nameof(aggregator), aggregator);
			Guard.ArgumentNotNull(nameof(cancellationTokenSource), cancellationTokenSource);

			return new XunitTestCaseRunner(
				this,
				DisplayName,
				SkipReason,
				constructorArguments,
				TestMethodArguments,
				messageBus,
				aggregator,
				cancellationTokenSource
			).RunAsync();
		}

		/// <inheritdoc/>
		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Timeout", Timeout);
		}
	}
}
