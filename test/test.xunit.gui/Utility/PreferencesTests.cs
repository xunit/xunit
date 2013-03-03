using System.Drawing;
using System.Reflection;
using Microsoft.Win32;
using Xunit;
using Xunit.Gui;

public class PreferencesTests
{
    Preferences prefs;

    public PreferencesTests()
    {
        prefs = new Preferences
        {
            WindowSize = new Size(10, 20),
            WindowLocation = new Point(30, 40),
            HorizontalSplitterDistance = 50,
            VerticalSplitterDistance = 60,
            IsMaximized = true
        };
    }

    [Fact]
    [PreferencesRollback]
    public void CanLoadAndSavePreferences()
    {
        prefs.Save();

        Preferences loadedPrefs = Preferences.Load();
        Assert.NotNull(loadedPrefs);
        Assert.Equal(prefs.WindowSize, loadedPrefs.WindowSize);
        Assert.Equal(prefs.WindowLocation, loadedPrefs.WindowLocation);
        Assert.Equal(prefs.HorizontalSplitterDistance, loadedPrefs.HorizontalSplitterDistance);
        Assert.Equal(prefs.VerticalSplitterDistance, loadedPrefs.VerticalSplitterDistance);
        Assert.Equal(prefs.IsMaximized, loadedPrefs.IsMaximized);
    }

    [Fact]
    [PreferencesRollback]
    public void NoPreferenceDataReturnsNull()
    {
        Preferences loadedPrefs = Preferences.Load();

        Assert.Null(loadedPrefs);
    }

    [Fact]
    [PreferencesRollback]
    public void InvalidRegistryDataReturnsNull()
    {
        using (var xunitKey = Registry.CurrentUser.CreateSubKey(@"Software\Outercurve Foundation\xUnit.net"))
        using (var displayKey = xunitKey.CreateSubKey("Display"))
        {
            displayKey.SetValue("Maximized", "booyah");
            displayKey.SetValue("X", "yep");
            displayKey.SetValue("Y", "sure");
        }

        Preferences loadedPrefs = Preferences.Load();

        Assert.Null(loadedPrefs);
    }

    class PreferencesRollbackAttribute : BeforeAfterTestAttribute
    {
        Preferences preferences;

        public override void After(MethodInfo methodUnderTest)
        {
            if (preferences != null)
                preferences.Save();
            else
                Preferences.Clear();
        }

        public override void Before(MethodInfo methodUnderTest)
        {
            preferences = Preferences.Load();
            Preferences.Clear();
        }
    }
}
