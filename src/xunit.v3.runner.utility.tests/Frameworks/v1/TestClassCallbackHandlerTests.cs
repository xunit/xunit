#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NSubstitute;
using Xunit;
using Xunit.Runner.v1;
using Xunit.Sdk;
using Xunit.v3;

[UseCulture("pl-PL")]
public class TestClassCallbackHandlerTests
{
	[Fact]
	public static void WithClassNode_ParsesNumbersWithInvariantCulture()
	{
		var handler = new TestClassCallbackHandler([], Substitute.For<IMessageSink>());
		var xml = new XmlDocument();
		xml.LoadXml("<class time='1.234' total='4' failed='3' skipped='2' />");

		handler.OnXmlNode(xml.FirstChild);

		Assert.Equal(1.234M, handler.TestClassResults.Time);
		Assert.Equal(4, handler.TestClassResults.Total);
		Assert.Equal(3, handler.TestClassResults.Failed);
		Assert.Equal(2, handler.TestClassResults.Skipped);
	}

	[Fact]
	public static void WithTestNode_ParsesNumberWithInvariantCulture()
	{
		var messages = new List<IMessageSinkMessage>();
		var sink = SpyMessageSink.Create(messages: messages);
		var testCase = CreateTestCase("assembly", "config", "foo", "bar", "foo.bar");
		var handler = new TestClassCallbackHandler([testCase], sink);
		var startXml = new XmlDocument();
		startXml.LoadXml("<start type='foo' method='bar' name='foo.bar'></start>");
		var passXml = new XmlDocument();
		passXml.LoadXml("<test type='foo' method='bar' name='foo.bar' time='1.234' result='Pass' />");

		handler.OnXmlNode(startXml.FirstChild);
		handler.OnXmlNode(passXml.FirstChild);

		var message = Assert.Single(messages.OfType<ITestFinished>());
		Assert.Equal(1.234M, message.ExecutionTime);
	}

	[Fact]
	public static void WithTestNode_OutputResultsInOutputMessage()
	{
		var messages = new List<IMessageSinkMessage>();
		var sink = SpyMessageSink.Create(messages: messages);
		var testCase = CreateTestCase("assembly", "config", "foo", "bar", "foo.bar");
		var handler = new TestClassCallbackHandler([testCase], sink);
		var startXml = new XmlDocument();
		startXml.LoadXml("<start type='foo' method='bar' name='foo.bar'></start>");
		var passXml = new XmlDocument();
		passXml.LoadXml("<test type='foo' method='bar' name='foo.bar' time='1.234' result='Pass'><output>This is output text</output></test>");

		handler.OnXmlNode(startXml.FirstChild);
		handler.OnXmlNode(passXml.FirstChild);

		var message = Assert.Single(messages.OfType<ITestOutput>());
		Assert.Equal("This is output text", message.Output);
	}

	/// <summary>
	/// Apply this attribute to your test method to replace the
	/// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
	/// <see cref="CultureInfo.CurrentUICulture" /> with another culture.
	/// </summary>
	/// <remarks>
	/// Replaces the culture and UI culture of the current thread with
	/// <paramref name="culture" /> and <paramref name="uiCulture" />
	/// </remarks>
	/// <param name="culture">The name of the culture.</param>
	/// <param name="uiCulture">The name of the UI culture.</param>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	class UseCultureAttribute(
		string culture,
		string uiCulture) :
			BeforeAfterTestAttribute
	{
		readonly Lazy<CultureInfo> culture = new Lazy<CultureInfo>(() => new CultureInfo(culture, useUserOverride: false));
		readonly Lazy<CultureInfo> uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture, useUserOverride: false));

		CultureInfo? originalCulture;
		CultureInfo? originalUICulture;

		/// <summary>
		/// Replaces the culture and UI culture of the current thread with
		/// <paramref name="culture" />
		/// </summary>
		/// <param name="culture">The name of the culture.</param>
		/// <remarks>
		/// <para>
		/// This constructor overload uses <paramref name="culture" /> for both
		/// <see cref="Culture" /> and <see cref="UICulture" />.
		/// </para>
		/// </remarks>
		public UseCultureAttribute(string culture)
			: this(culture, culture)
		{ }

		/// <summary>
		/// Gets the culture.
		/// </summary>
		public CultureInfo Culture => culture.Value;

		/// <summary>
		/// Gets the UI culture.
		/// </summary>
		public CultureInfo UICulture => uiCulture.Value;

		/// <summary>
		/// Stores the current <see cref="Thread.CurrentPrincipal" />
		/// <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
		/// and replaces them with the new cultures defined in the constructor.
		/// </summary>
		/// <param name="methodUnderTest">The method under test</param>
		/// <param name="test">The current <see cref="IXunitTest"/></param>
		public override ValueTask Before(MethodInfo methodUnderTest, IXunitTest test)
		{
			originalCulture = CultureInfo.CurrentCulture;
			originalUICulture = CultureInfo.CurrentUICulture;

			CultureInfo.CurrentCulture = Culture;
			CultureInfo.CurrentUICulture = UICulture;

			return default;
		}

		/// <summary>
		/// Restores the original <see cref="CultureInfo.CurrentCulture" /> and
		/// <see cref="CultureInfo.CurrentUICulture" /> to <see cref="Thread.CurrentPrincipal" />
		/// </summary>
		/// <param name="methodUnderTest">The method under test</param>
		/// <param name="test">The current <see cref="IXunitTest"/></param>
		public override ValueTask After(MethodInfo methodUnderTest, IXunitTest test)
		{
			if (originalCulture is not null)
				CultureInfo.CurrentCulture = originalCulture;
			if (originalUICulture is not null)
				CultureInfo.CurrentUICulture = originalUICulture;

			return default;
		}
	}

	static Xunit1TestCase CreateTestCase(
		string assemblyPath,
		string configFileName,
		string typeName,
		string methodName,
		string testCaseDisplayName,
		string? skipReason = null,
		Dictionary<string, IReadOnlyCollection<string>>? traits = null)
	{
		return new Xunit1TestCase
		{
			AssemblyUniqueID = $"asm-id: {assemblyPath}:{configFileName}",
			SkipReason = skipReason,
			TestCaseDisplayName = testCaseDisplayName,
			TestCaseUniqueID = $"case-id: {typeName}:{methodName}:{assemblyPath}:{configFileName}",
			TestClass = typeName,
			TestClassUniqueID = $"class-id: {typeName}:{assemblyPath}:{configFileName}",
			TestCollectionUniqueID = $"collection-id: {assemblyPath}:{configFileName}",
			TestMethod = methodName,
			TestMethodUniqueID = $"method-id: {typeName}:{methodName}:{assemblyPath}:{configFileName}",
			Traits = traits ?? []
		};
	}

}

#endif
