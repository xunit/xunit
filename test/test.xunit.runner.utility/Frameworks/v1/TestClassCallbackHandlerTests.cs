#if NETFRAMEWORK

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Xml;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[UseCulture("pl-PL")]
public class TestClassCallbackHandlerTests
{
    [Fact]
    public static void WithClassNode_ParsesNumbersWithInvariantCulture()
    {
        var handler = new TestClassCallbackHandler(new Xunit1TestCase[0], Substitute.For<IMessageSink>());
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
        var sink = Substitute.For<IMessageSink>();
        var testCase = new Xunit1TestCase("assembly", "config", "foo", "bar", "foo.bar");
        var handler = new TestClassCallbackHandler(new [] { testCase }, sink);
        var xml = new XmlDocument();
        xml.LoadXml("<test type='foo' method='bar' name='foo.bar' time='1.234' result='Pass' />");

        handler.OnXmlNode(xml.FirstChild);

        var args = sink.Captured(1, x => x.OnMessage(null));
        var message = args.Arg<ITestFinished>();
        Assert.Equal(1.234M, message.ExecutionTime);
    }

    [Fact]
    public static void WithTestNode_OutputResultsInOutputMessage()
    {
        var sink = Substitute.For<IMessageSink>();
        var testCase = new Xunit1TestCase("assembly", "config", "foo", "bar", "foo.bar");
        var handler = new TestClassCallbackHandler(new[] { testCase }, sink);
        var xml = new XmlDocument();
        xml.LoadXml("<test type='foo' method='bar' name='foo.bar' time='1.234' result='Pass'><output>This is output text</output></test>");

        handler.OnXmlNode(xml.FirstChild);

        var args = sink.Captured(0, x => x.OnMessage(null));
        var message = args.Arg<ITestOutput>();
        Assert.Same(testCase, message.TestCase);
        Assert.Equal("This is output text", message.Output);
    }

    /// <summary>
    /// Apply this attribute to your test method to replace the
    /// <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
    /// <see cref="CultureInfo.CurrentUICulture" /> with another culture.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    class UseCultureAttribute : BeforeAfterTestAttribute
    {
        readonly Lazy<CultureInfo> culture;
        readonly Lazy<CultureInfo> uiCulture;

        CultureInfo originalCulture;
        CultureInfo originalUICulture;

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
            : this(culture, culture) { }

        /// <summary>
        /// Replaces the culture and UI culture of the current thread with
        /// <paramref name="culture" /> and <paramref name="uiCulture" />
        /// </summary>
        /// <param name="culture">The name of the culture.</param>
        /// <param name="uiCulture">The name of the UI culture.</param>
        public UseCultureAttribute(string culture, string uiCulture)
        {
            this.culture = new Lazy<CultureInfo>(() => new CultureInfo(culture));
            this.uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture));
        }

        /// <summary>
        /// Gets the culture.
        /// </summary>
        public CultureInfo Culture { get { return culture.Value; } }

        /// <summary>
        /// Gets the UI culture.
        /// </summary>
        public CultureInfo UICulture { get { return uiCulture.Value; } }

        /// <summary>
        /// Stores the current <see cref="Thread.CurrentPrincipal" />
        /// <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
        /// and replaces them with the new cultures defined in the constructor.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            originalCulture = Thread.CurrentThread.CurrentCulture;
            originalUICulture = Thread.CurrentThread.CurrentUICulture;

            Thread.CurrentThread.CurrentCulture = Culture;
            Thread.CurrentThread.CurrentUICulture = UICulture;
        }

        /// <summary>
        /// Restores the original <see cref="CultureInfo.CurrentCulture" /> and
        /// <see cref="CultureInfo.CurrentUICulture" /> to <see cref="Thread.CurrentPrincipal" />
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            Thread.CurrentThread.CurrentCulture = originalCulture;
            Thread.CurrentThread.CurrentUICulture = originalUICulture;
        }
    }
}

#endif
