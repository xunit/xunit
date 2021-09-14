#if !NET35

using System.Threading.Tasks;

static class CommonTasks
{
    internal static readonly Task Completed = Task.FromResult(0);
}

#endif
