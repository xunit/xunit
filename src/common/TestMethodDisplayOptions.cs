#if XUNIT_FRAMEWORK
namespace Xunit.Sdk
#else
namespace Xunit
#endif
{
    using System;

    /// <summary>
    /// Indicates the method display options for test methods.
    /// </summary>
    [Flags]
    public enum TestMethodDisplayOptions
    {
        /// <summary>
        /// Indicates no additional method display options.
        /// </summary>
        /// <remarks>This is the default configuration option.</remarks>
        None = 0x00,

        /// <summary>
        /// Replace underscores in display names with a space.
        /// </summary>
        ReplaceUnderscoreWithSpace = 0x01,

        /// <summary>
        /// Replace well-known monikers with their equivalent operator.
        /// </summary>
        /// <list type="bullet">
        /// <item><description>lt : &lt;</description></item>
        /// <item><description>le : &lt;=</description></item>
        /// <item><description>eq : =</description></item>
        /// <item><description>ne : !=</description></item>
        /// <item><description>gt : &gt;</description></item>
        /// <item><description>ge : &gt;=</description></item>
        /// </list>
        UseOperatorMonikers = 0x02,

        /// <summary>
        /// Replace supported escape sequences with their equivalent character.
        /// <list type="table">
        /// <listheader>
        ///  <term>Encoding</term>
        ///  <description>Format</description>
        /// </listheader>
        /// <item><term>ASCII</term><description>X hex-digit hex-digit (ex: X2C)</description></item>
        /// <item><term>Unicode</term><description>U hex-digit hex-digit hex-digit hex-digit (ex: U00A9)</description></item>
        /// </list>
        /// </summary>
        UseEscapeSequences = 0x04,

        /// <summary>
        /// Replaces the period delimiter used in namespace and type references with a comma.
        /// </summary>
        /// <remarks>This option is only honored if the <see cref="TestMethodDisplay.ClassAndMethod"/> setting is also enabled.</remarks>
        ReplacePeriodWithComma = 0x08,

        /// <summary>
        /// Enables all method display options.
        /// </summary>
        All = ReplaceUnderscoreWithSpace | UseOperatorMonikers | UseEscapeSequences | ReplacePeriodWithComma
    }
}
