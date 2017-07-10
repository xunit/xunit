using Xunit;
using Xunit.Runner.VisualStudio.TestAdapter;

public class RunSettingsHelperTests
{
    void AssertDefaultValues()
    {
        Assert.False(RunSettingsHelper.DisableAppDomain);
        Assert.False(RunSettingsHelper.DisableParallelization);
        Assert.False(RunSettingsHelper.NoAutoReporters);
        Assert.True(RunSettingsHelper.DesignMode);
        Assert.True(RunSettingsHelper.CollectSourceInformation);
        Assert.Null(RunSettingsHelper.TargetFrameworkVersion);
    }

    [Fact]
    public void RunSettingsHelperShouldNotThrowExceptionOnBadXml()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <RunSettings";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        AssertDefaultValues();
    }

    [Fact]
    public void RunSettingsHelperShouldNotThrowExceptionOnInvalidValuesForElements()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        <DisableAppDomain>1234</DisableAppDomain>
                        <DisableParallelization>smfhekhgekr</DisableParallelization>
                        <DesignMode>3245sax</DesignMode>
                        <CollectSourceInformation>1234blah</CollectSourceInformation>
                        <NoAutoReporters>1x3_5f8g0</NoAutoReporters>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        AssertDefaultValues();
    }

    [Fact]
    public void RunSettingsHelperShouldUseDefaultValuesInCaseOfIncorrectSchemaAndIgnoreAttributes()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        <OuterElement>
                            <DisableParallelization>true</DisableParallelization>
                        </OuterElement>
                        <DisableAppDomain value=""false"">true</DisableAppDomain>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        // Use element value, not attribute value
        Assert.True(RunSettingsHelper.DisableAppDomain);
        // Ignore value that isn't at the right level
        Assert.False(RunSettingsHelper.DisableParallelization);
    }

    [Fact]
    public void RunSettingsHelperShouldUseDefaultValuesInCaseOfBadXml()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        Random Text
                        <DisableParallelization>true</DisableParallelization>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        // Allow value to be read even after unexpected element body
        Assert.True(RunSettingsHelper.DisableParallelization);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RunSettingsHelperShouldReadValuesCorrectly(bool testValue)
    {
        string settingsXml =
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        <CollectSourceInformation>{testValue.ToString().ToLowerInvariant()}</CollectSourceInformation>
                        <DesignMode>{testValue.ToString().ToLowerInvariant()}</DesignMode>
                        <DisableAppDomain>{testValue.ToString().ToLowerInvariant()}</DisableAppDomain>
                        <DisableParallelization>{testValue.ToString().ToLowerInvariant()}</DisableParallelization>
                        <NoAutoReporters>{testValue.ToString().ToLowerInvariant()}</NoAutoReporters>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        Assert.Equal(testValue, RunSettingsHelper.CollectSourceInformation);
        Assert.Equal(testValue, RunSettingsHelper.DesignMode);
        Assert.Equal(testValue, RunSettingsHelper.DisableAppDomain);
        Assert.Equal(testValue, RunSettingsHelper.DisableParallelization);
        Assert.Equal(testValue, RunSettingsHelper.NoAutoReporters);
    }

    [Fact]
    public void RunSettingsHelperShouldIgnoreEvenIfAdditionalElementsExist()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                        <RunConfiguration>
                            <TargetPlatform>x64</TargetPlatform>
                            <TargetFrameworkVersion>FrameworkCore10</TargetFrameworkVersion>
                            <SolutionDirectory>%temp%</SolutionDirectory>
                            <DisableAppDomain>true</DisableAppDomain>
                            <DisableParallelization>true</DisableParallelization>
                            <MaxCpuCount>4</MaxCpuCount>
                            <NoAutoReporters>true</NoAutoReporters>
                        </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        Assert.True(RunSettingsHelper.DisableAppDomain);
        Assert.True(RunSettingsHelper.DisableParallelization);
        Assert.True(RunSettingsHelper.NoAutoReporters);
        Assert.Equal("FrameworkCore10", RunSettingsHelper.TargetFrameworkVersion);
    }
}
