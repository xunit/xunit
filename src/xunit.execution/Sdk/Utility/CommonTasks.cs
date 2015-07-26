using System.Threading.Tasks;

namespace Xunit.Sdk
{
    static class CommonTasks
    {
        internal static readonly Task Completed = Task.FromResult(0);
    }
}
