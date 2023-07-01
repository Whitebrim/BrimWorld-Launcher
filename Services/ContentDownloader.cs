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
    private const string ApiUrl = "https://api.brimworld.online/";
    public const string ManifestPath = "launcher-manifest.json";
    public const string SettingsPath = "launcher-settings.json";
    public const string MinecraftPath = "Minecraft/";
    private readonly FileManager _fileManager;
    private readonly HttpClient _httpClient;

    public ContentDownloader(HttpClient httpClient, FileManager fileManager)
    {
        _httpClient = httpClient;
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
        Manifest? result = default;
        await DownloadFileFromApi(ManifestPath, async contentStream =>
        {
            result = await JsonSerializer.DeserializeAsync<Manifest>(contentStream, ManifestContext.Default.Manifest);
        });

        return result;
    }

    public async Task<Settings?> LoadSettings()
    {
        try
        {
            await using Stream stream = _fileManager.ReadFile(SettingsPath);
            return JsonSerializer.Deserialize<Settings>(stream, SettingsContext.Default.Settings);
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

    public async Task DownloadJava(string javaDist, JavaManifest javaManifest, ArchiveExtractor archiveExtractor)
    {
        string downloadUrl = javaManifest.DownloadUrls[PlatformDetector.GetSystemInfo()];
        await archiveExtractor.ExtractZip(Path.Join("Java", javaDist), downloadUrl);
    }

    private async Task DownloadFileFromApi(string relativePath, Func<Stream, Task> action)
    {
        await DownloadFile(Url.Combine(ApiUrl, relativePath), action);
    }


    public async Task DownloadFile(string url, Func<Stream, Task> action)
    {
        Debug.WriteLine("Downloading from " + url);

        using HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await action(await response.Content.ReadAsStreamAsync());
    }
}