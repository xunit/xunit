using System.Threading.Tasks;

static class TaskHelpers
{
	public static Task CompletedTask { get; } = Task.FromResult(0);
}
