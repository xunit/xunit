using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Xunit.BuildTools.Models;

namespace Xunit.BuildTools;

public class Program
{
    public static Task<int> Main(string[] args)
        => CommandLineApplication.ExecuteAsync<BuildContext>(args);
}
