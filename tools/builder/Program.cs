using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

public class Program
{
	public static Task<int> Main(string[] args)
		=> CommandLineApplication.ExecuteAsync<BuildContext>(args);
}
