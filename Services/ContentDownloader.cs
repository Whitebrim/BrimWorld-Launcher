using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Launcher.Extensions;
using Launcher.Models;
using Debug = System.Diagnostics.Debug;

namespace Launcher.Services;

public class ContentDownloader
{
    private const string ApiUrl = "https://api.brimworld.ru/";
    public const string ManifestPath = "launcher-manifest.json";
    private readonly FileManager _fileManager;
    public ContentDownloader(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public async Task<Manifest?> LoadManifest()
    {
        try
        {
            await using Stream stream = _fileManager.ReadFile(ManifestPath);
            return JsonSerializer.Deserialize<Manifest>(stream, ManifestContext.Default.Manifest);
        }
        catch (Exception e)
        {
            if (e is FileNotFoundException or DirectoryNotFoundException)
            {
                return null;
            }
            throw;
        }
    }

    public async Task<Manifest?> DownloadManifest()
    {
        string url = Url.Combine(ApiUrl, ManifestPath);
        using var httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using Stream contentStream = await response.Content.ReadAsStreamAsync();

        return await JsonSerializer.DeserializeAsync<Manifest>(contentStream, ManifestContext.Default.Manifest);
    }

    //private static Task<Stream> DownloadFileFromApi(string relativePath)
    //{
    //    return DownloadFile(Url.Combine(ApiUrl, relativePath));
    //}

    //// Doesn't work because of using block ending in this method
    //public static async Task<Stream> DownloadFile(string url, Action<Stream> action)
    //{
    //    Debug.WriteLine("Downloading from " + url);

    //    using var httpClient = new HttpClient();
    //    using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
    //    response.EnsureSuccessStatusCode();

    //    return await response.Content.ReadAsStreamAsync();
    //}
}