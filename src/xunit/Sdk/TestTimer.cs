using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Sdk
{
    /// <summary>
    /// A timer class used to figure out how long tests take to run. On most .NET implementations
    /// this will use the <see cref="System.Diagnostics.Stopwatch"/> class because it's a high
    /// resolution timer; however, on Silverlight/CoreCLR, it will use <see cref="DateTime"/>
    /// (which will provide lower resolution results).
    /// </summary>
    public class TestTimer
    {
        readonly ITestTimer timer;

        /// <summary>
        /// Creates a new instance of the <see cref="TestTimer"/> class.
        /// </summary>
        public TestTimer()
        {
            if (StopwatchTimer.Supported)
                timer = new StopwatchTimer();
            else
                timer = new DateTimeTimer();
        }

        /// <summary>
        /// Gets how long the timer ran, in milliseconds. In order for this to be valid,
        /// both <see cref="Start()"/> and <see cref="Stop()"/> must have been called.
        /// </summary>
        public long ElapsedMilliseconds
        {
            get { return timer.ElapsedMilliseconds; }
        }

        /// <summary>
        /// Starts timing.
        /// </summary>
        public void Start()
        {
            timer.Start();
        }

        /// <summary>
        /// Stops timing.
        /// </summary>
        public void Stop()
        {
            timer.Stop();
        }

        class DateTimeTimer : ITestTimer
        {
            DateTime startTime;
            DateTime stopTime;

            public long ElapsedMilliseconds
            {
                get { return (stopTime - startTime).Milliseconds; }
            }

            public void Start()
            {
                startTime = DateTime.Now;
            }

            public void Stop()
            {
                stopTime = DateTime.Now;
            }
        }

        interface ITestTimer
        {
            long ElapsedMilliseconds { get; }

            void Start();

            void Stop();
        }

        class StopwatchTimer : ITestTimer
        {
            const string typeName = "System.Diagnostics.Stopwatch, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";

            static readonly PropertyInfo elapsed;
            static readonly object[] emptyObjects = new object[0];
            static readonly MethodInfo start;
            static readonly MethodInfo stop;
            static readonly Type type = Type.GetType(typeName);

            readonly object stopwatch;

            [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "No can do.")]
            static StopwatchTimer()
            {
                if (type != null)
                {
                    elapsed = type.GetProperty("ElapsedMilliseconds");
                    start = type.GetMethod("Start");
                    stop = type.GetMethod("Stop");
                }
            }

            public StopwatchTimer()
            {
                stopwatch = Activator.CreateInstance(type);
            }

            public long ElapsedMilliseconds
            {
                get { return (long)elapsed.GetValue(stopwatch, emptyObjects); }
            }

            public static bool Supported
            {
                get { return type != null; }
            }

            public void Start()
            {
                start.Invoke(stopwatch, emptyObjects);
            }

            public void Stop()
            {
                stop.Invoke(stopwatch, emptyObjects);
            }
        }
    }
}