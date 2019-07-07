using System.Threading.Tasks;

[Target(nameof(Restore))]
public static class Restore
{
    public static Task OnExecute(BuildContext context)
    {
        context.BuildStep("Restoring NuGet packages");

        return context.Exec("dotnet", "restore");
    }
}
