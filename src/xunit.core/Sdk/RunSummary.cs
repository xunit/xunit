namespace Xunit.Sdk
{
    public class RunSummary
    {
        public int Total;
        public int Failed;
        public int Skipped;
        public decimal Time;

        public void Aggregate(RunSummary other)
        {
            Total += other.Total;
            Failed += other.Failed;
            Skipped += other.Skipped;
            Time += other.Time;
        }
    }
}
