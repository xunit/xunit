namespace Xunit.Extensions
{
    public class PrioritizedFixtureAttribute : RunWithAttribute
    {
        public PrioritizedFixtureAttribute() : base(typeof(PrioritizedFixtureClassCommand)) { }
    }
}