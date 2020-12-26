using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Runner.v2;
using Xunit.Sdk;

namespace Xunit.v3
{
	[Serializable]
	public class CulturedXunitTestCase : XunitTestCase
	{
		/// <inheritdoc/>
		protected CulturedXunitTestCase(
			SerializationInfo info,
			StreamingContext context) :
				base(info, context)
		{
			Culture = Guard.NotNull("Could not retrieve Culture from serialization", info.GetValue<string>("Culture"));
		}

		public CulturedXunitTestCase(
			_IMessageSink diagnosticMessageSink,
			TestMethodDisplay defaultMethodDisplay,
			TestMethodDisplayOptions defaultMethodDisplayOptions,
			_ITestMethod testMethod,
			string culture,
			object?[]? testMethodArguments = null)
				: base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod, testMethodArguments, null, null, null, null)
		{
			Culture = Guard.ArgumentNotNull(nameof(culture), culture);

			Traits.Add("Culture", Culture);

			var cultureDisplay = $"[{Culture}]";
			DisplayName += cultureDisplay;
			UniqueID += cultureDisplay;
		}

		public string Culture { get; }

		public override void GetObjectData(
			SerializationInfo info,
			StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue("Culture", Culture);
		}

		public override async Task<RunSummary> RunAsync(
			_IMessageSink diagnosticMessageSink,
			IMessageBus messageBus,
			object?[] constructorArguments,
			ExceptionAggregator aggregator,
			CancellationTokenSource cancellationTokenSource)
		{
			var originalCulture = CultureInfo.CurrentCulture;
			var originalUICulture = CultureInfo.CurrentUICulture;

			try
			{
				var cultureInfo = new CultureInfo(Culture);
				CultureInfo.CurrentCulture = cultureInfo;
				CultureInfo.CurrentUICulture = cultureInfo;

				return await base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
			}
			finally
			{
				CultureInfo.CurrentCulture = originalCulture;
				CultureInfo.CurrentUICulture = originalUICulture;
			}
		}
	}
}
