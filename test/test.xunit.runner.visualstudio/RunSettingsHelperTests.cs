using Xunit;
using Xunit.Runner.VisualStudio.TestAdapter;

public class RunSettingsHelperTests
{
    [Fact]
    public void RunSettingsHelperShouldNotThrowExceptionOnBadXml()
    {
        string settingsXml =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
            <RunSettings";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        // Default values must be used
        Assert.False(RunSettingsHelper.DisableAppDomain);
        Assert.False(RunSettingsHelper.DisableParallelization);
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

        // Default values must be used
        Assert.False(RunSettingsHelper.DisableAppDomain);
        Assert.False(RunSettingsHelper.DisableParallelization);
        Assert.False(RunSettingsHelper.NoAutoReporters);
        Assert.True(RunSettingsHelper.DesignMode);
        Assert.True(RunSettingsHelper.CollectSourceInformation);
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

        // Attribute must be ignored
        Assert.True(RunSettingsHelper.DisableAppDomain);
        // Default value must be used for disableparallelization, designmode
        Assert.False(RunSettingsHelper.DisableParallelization);
        Assert.True(RunSettingsHelper.DesignMode);
        Assert.True(RunSettingsHelper.CollectSourceInformation);
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

        // Default value must be used for DisableAppDomain, DesignMode, NoAutoReporters
        Assert.False(RunSettingsHelper.DisableAppDomain);
        Assert.False(RunSettingsHelper.NoAutoReporters);
        Assert.True(RunSettingsHelper.DesignMode);
        Assert.True(RunSettingsHelper.CollectSourceInformation);
        // DisableParallelization can be set
        Assert.True(RunSettingsHelper.DisableParallelization);
    }

    [Theory]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, true, false, false)]
    public void RunSettingsHelperShouldReadValuesCorrectly(bool disableAppDomain, bool disableParallelization, bool noAutoReporters, bool collectSourceInformation)
    {
        string settingsXml =
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        <DisableAppDomain>{disableAppDomain.ToString().ToLowerInvariant()}</DisableAppDomain>
                        <DisableParallelization>{disableParallelization.ToString().ToLowerInvariant()}</DisableParallelization>
                        <NoAutoReporters>{noAutoReporters.ToString().ToLowerInvariant()}</NoAutoReporters>
                        <CollectSourceInformation>{collectSourceInformation.ToString().ToLowerInvariant()}</CollectSourceInformation>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        // Correct values must be set
        Assert.Equal(disableAppDomain, RunSettingsHelper.DisableAppDomain);
        Assert.Equal(disableParallelization, RunSettingsHelper.DisableParallelization);
        Assert.Equal(noAutoReporters, RunSettingsHelper.NoAutoReporters);
        Assert.Equal(collectSourceInformation, RunSettingsHelper.CollectSourceInformation);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RunSettingsHelperShouldReadDesignModeSettingCorrectly(bool designMode)
    {
        string settingsXml =
            $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <RunSettings>
                    <RunConfiguration>
                        <DisableAppDomain>true</DisableAppDomain>
                        <DisableParallelization>invalid</DisableParallelization>
                        <DesignMode>{designMode.ToString().ToLowerInvariant()}</DesignMode>
                    </RunConfiguration>
                </RunSettings>";

        RunSettingsHelper.ReadRunSettings(settingsXml);

        // Correct values must be set
        Assert.Equal(designMode, RunSettingsHelper.DesignMode);
        Assert.True(RunSettingsHelper.DisableAppDomain);
        // Default value should be set for DisableParallelization
        Assert.False(RunSettingsHelper.DisableParallelization);
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

        // Correct values must be used
        Assert.True(RunSettingsHelper.DisableAppDomain);
        Assert.True(RunSettingsHelper.DisableParallelization);
        Assert.True(RunSettingsHelper.NoAutoReporters);
    }
}
