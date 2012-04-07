using System;
using System.Windows.Forms;

namespace Xunit.Gui
{
    public class WaitCursor : IDisposable
    {
        readonly Cursor oldCursor;

        public WaitCursor()
        {
            oldCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        public void Dispose()
        {
            Cursor.Current = oldCursor;
        }
    }
}