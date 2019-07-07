using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

[Target(nameof(DownloadNuGet))]
public static class DownloadNuGet
{
    public static async Task OnExecute(BuildContext context)
    {
        if (File.Exists(context.NuGetExe))
            return;

        using (var httpClient = new HttpClient())
        using (var stream = File.OpenWrite(context.NuGetExe))
        {
            context.BuildStep($"Downloading {context.NuGetUrl} to {context.NuGetExe}");

            var response = await httpClient.GetAsync(context.NuGetUrl);
            response.EnsureSuccessStatusCode();

            await response.Content.CopyToAsync(stream);
        }
    }
}
