using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Launcher.Interfaces;

namespace Launcher.Services;

public class ArchiveExtractor
{
    private readonly IArchiveExtractor _zipExtractor;
    private readonly IArchiveExtractor _tarExtractor;

    public ArchiveExtractor(FileManager fileManager)
    {
        _zipExtractor = new ZipExtractor(fileManager);
        _tarExtractor = new TarExtractor(fileManager);
    }

    public async Task ExtractZip(string extractPath, string url)
    {
        string postfix = url[^4..];

        using var httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using Stream contentStream = await response.Content.ReadAsStreamAsync();

        if (postfix == ".zip")
        {
            await _zipExtractor.ExtractArchive(extractPath, contentStream);
        }
        else
        {
            await _tarExtractor.ExtractArchive(extractPath, contentStream);
        }
    }
}