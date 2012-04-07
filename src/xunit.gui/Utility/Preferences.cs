using System;
using System.Drawing;
using Microsoft.Win32;

namespace Xunit.Gui
{
    public class Preferences
    {
        const string REGISTRY_SUBKEY_DISPLAY = "Display";
        const string REGISTRY_VALUE_HEIGHT = "Height";
        const string REGISTRY_VALUE_HORIZONTALSPLITTER = "Splitter";
        const string REGISTRY_VALUE_MAXIMIZED = "Maximized";
        const string REGISTRY_VALUE_WIDTH = "Width";
        const string REGISTRY_VALUE_X = "X";
        const string REGISTRY_VALUE_Y = "Y";
        const string REGISTRY_VALUE_VERTICALSPLITTER = "Splitter2";

        public int HorizontalSplitterDistance { get; set; }
        public bool IsMaximized { get; set; }
        public int VerticalSplitterDistance { get; set; }
        public Point WindowLocation { get; set; }
        public Size WindowSize { get; set; }

        public static void Clear()
        {
            try
            {
                using (var xunitKey = Registry.CurrentUser.OpenSubKey(Program.REGISTRY_KEY_XUNIT, true))
                    if (xunitKey != null)
                        xunitKey.DeleteSubKeyTree(REGISTRY_SUBKEY_DISPLAY);
            }
            catch (ArgumentException) { }
        }

        public static Preferences Load()
        {
            try
            {
                using (var xunitKey = Registry.CurrentUser.OpenSubKey(Program.REGISTRY_KEY_XUNIT, true))
                using (var displayKey = xunitKey.OpenSubKey(REGISTRY_SUBKEY_DISPLAY))
                    if (displayKey != null)
                    {
                        Preferences result = new Preferences();
                        result.WindowSize = new Size((int)displayKey.GetValue(REGISTRY_VALUE_WIDTH), (int)displayKey.GetValue(REGISTRY_VALUE_HEIGHT));
                        result.WindowLocation = new Point((int)displayKey.GetValue(REGISTRY_VALUE_X), (int)displayKey.GetValue(REGISTRY_VALUE_Y));
                        result.HorizontalSplitterDistance = (int)displayKey.GetValue(REGISTRY_VALUE_HORIZONTALSPLITTER);
                        result.VerticalSplitterDistance = (int)displayKey.GetValue(REGISTRY_VALUE_VERTICALSPLITTER);
                        result.IsMaximized = ((int)displayKey.GetValue(REGISTRY_VALUE_MAXIMIZED) == 1);

                        return result;
                    }
            }
            catch (Exception) { }

            return null;
        }

        public void Save()
        {
            Clear();

            using (var xunitKey = Registry.CurrentUser.CreateSubKey(Program.REGISTRY_KEY_XUNIT))
            using (var displayKey = xunitKey.CreateSubKey(REGISTRY_SUBKEY_DISPLAY))
            {
                displayKey.SetValue(REGISTRY_VALUE_MAXIMIZED, IsMaximized ? 1 : 0);
                displayKey.SetValue(REGISTRY_VALUE_X, WindowLocation.X);
                displayKey.SetValue(REGISTRY_VALUE_Y, WindowLocation.Y);
                displayKey.SetValue(REGISTRY_VALUE_WIDTH, WindowSize.Width);
                displayKey.SetValue(REGISTRY_VALUE_HEIGHT, WindowSize.Height);
                displayKey.SetValue(REGISTRY_VALUE_HORIZONTALSPLITTER, HorizontalSplitterDistance);
                displayKey.SetValue(REGISTRY_VALUE_VERTICALSPLITTER, VerticalSplitterDistance);
            }
        }
    }
}
