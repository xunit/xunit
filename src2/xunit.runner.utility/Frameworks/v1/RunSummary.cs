using System;
using System.Linq;

namespace Xunit
{
    public class RunSummary
    {
        public bool Continue = true;
        public int Total = 0;
        public int Failed = 0;
        public int Skipped = 0;
        public decimal Time = 0M;

        public void Aggregate(RunSummary other)
        {
            Total += other.Total;
            Failed += other.Failed;
            Skipped += other.Skipped;
            Time += other.Time;
            Continue &= other.Continue;
        }

        public void Reset()
        {
            Total = 0;
            Failed = 0;
            Skipped = 0;
            Time = 0M;
        }
    }
}
