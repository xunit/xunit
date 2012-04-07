using System;
using System.IO;
using System.Timers;

namespace Xunit.Gui
{
    public class FileWatcher : IDisposable
    {
        readonly string filename;
        readonly object lockObject = new object();
        readonly Timer timer;
        FileSystemWatcher watcher;

        public FileWatcher(string filename)
        {
            this.filename = Path.GetFullPath(filename);

            timer = new Timer(2000);
            timer.AutoReset = false;
            timer.Enabled = false;
            timer.Elapsed += TimerHandler;

            watcher = new FileSystemWatcher(Path.GetDirectoryName(this.filename), Path.GetFileName(this.filename));
            watcher.Changed += WatcherHandler;
            watcher.Renamed += WatcherHandler;
            watcher.EnableRaisingEvents = true;
        }

        public event EventHandler Changed;

        public void Dispose()
        {
            if (watcher != null)
            {
                watcher.Dispose();
                watcher = null;
            }
        }

        void TimerHandler(object sender, EventArgs e)
        {
            lock (lockObject)
                if (Changed != null)
                    Changed(this, EventArgs.Empty);
        }

        void WatcherHandler(object sender, FileSystemEventArgs e)
        {
            bool signal = false;

            lock (lockObject)
            {
                switch (e.ChangeType)
                {
                    case WatcherChangeTypes.Changed:
                        signal = true;
                        break;

                    case WatcherChangeTypes.Renamed:
                        signal = (String.Compare(e.FullPath, filename, true) == 0);
                        break;
                }

                if (signal)
                {
                    if (!timer.Enabled)
                        timer.Enabled = true;

                    timer.Start();
                }
            }
        }
    }
}