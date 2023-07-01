using Avalonia.Media.Imaging;
using Launcher.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Debug = System.Diagnostics.Debug;

namespace Launcher.Services;

public class ContentManager
{
    //private static readonly Lazy<ContentManager> LazyLoader = new Lazy<ContentManager>(() => new ContentManager());
    //public static ContentManager Instance => LazyLoader.Value;

    private const string LauncherDataPath = "Launcher";

    //public event EventHandler OnManifestLoaded;

    private readonly FileManager _fileManager;
    private readonly ContentDownloader _contentDownloader;
    private readonly ArchiveExtractor _archiveExtractor;
    private readonly HttpClient _httpClient;

    private Manifest _manifest = null!;
    private Settings _settings = null!;

    public ContentManager(HttpClient httpClient)
    {
        _fileManager = new FileManager(GetLocalDataPath("BrimWorld"));
        _contentDownloader = new ContentDownloader(httpClient, _fileManager);
        _archiveExtractor = new ArchiveExtractor(_contentDownloader, _fileManager);
        _httpClient = httpClient;
    }

    public async Task<bool> Initialize()
    {
        return await PrepareManifest() && await PrepareSettings();
    }

    private async Task<bool> PrepareManifest()
    {
        Manifest? manifest = await _contentDownloader.LoadManifest();

        Manifest? newManifest = await _contentDownloader.DownloadManifest();

        if (manifest == null && newManifest == null)
        {
            // TODO Плашка что сервер недоступен
            Debug.WriteLine("ПИЗДА ИНТЕРНЕТУ, ИГРАТЬ НЕВОЗМОЖНО!");
            return false;
        }

        if ((manifest == null && newManifest != null) ||
            (manifest != null && newManifest != null && manifest.ManifestVersion < newManifest.ManifestVersion))
        {
            await using Stream stream = _fileManager.CreateFile(ContentDownloader.ManifestPath);
            await JsonSerializer.SerializeAsync(stream, newManifest, ManifestContext.Default.Manifest);
            manifest = newManifest;
        }

        if (manifest != null && newManifest == null)
        {
            // TODO Продолжаем в offline режиме
        }

        _manifest = manifest!;

        for (var i = 0; i < _manifest.Servers.Count; i++)
        {
            await SaveBanner(i);
        }

        return true;
    }

    private async Task<bool> PrepareSettings()
    {
        Settings? settings = await _contentDownloader.LoadSettings();

        if (settings == null)
        {
            _settings = settings = new Settings();
            await SaveSettings();
        }

        _settings = settings;

        return true;
    }

    private async Task SaveSettings()
    {
        await using Stream stream = _fileManager.CreateFile(ContentDownloader.SettingsPath);
        await JsonSerializer.SerializeAsync(stream, _settings, SettingsContext.Default.Settings);
    }

    private async Task SaveBanner(int serverId)
    {
        if (GetServerManifest(serverId) is not { } serverManifest) return;
        string fileName = Path.GetFileName(serverManifest.BannerUrl);
        string localFilePath = Path.Join(LauncherDataPath, fileName);

        string url = serverManifest.BannerUrl;

        using HttpResponseMessage response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
        response.EnsureSuccessStatusCode();

        var etag = "";

        if (response.Headers.TryGetValues("etag", out var etagValues))
        {
            etag = etagValues.FirstOrDefault()?.Replace("\"", "").ToLowerInvariant();
        }

        var md5Sum = "none";

        if (_fileManager.FileExist(localFilePath))
        {
            using var md5 = MD5.Create();
            await using Stream md5Stream = _fileManager.ReadFile(localFilePath);
            byte[] hash = await md5.ComputeHashAsync(md5Stream);

            md5Sum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        if (etag == md5Sum) return;

        //Debug.WriteLine("etag != md5_sum:");
        //Debug.WriteLine(etag);
        //Debug.WriteLine(md5Sum);

        await _contentDownloader.DownloadFile(url, async contentStream =>
        {
            await using Stream stream = _fileManager.CreateFile(Path.Join(LauncherDataPath, fileName));
            await contentStream.CopyToAsync(stream);
        });
    }

    public async Task<Bitmap?> GetBanner(int serverId)
    {
        if (GetServerManifest(serverId) is not { } serverManifest) return null;
        string fileName = Path.GetFileName(serverManifest.BannerUrl);
        await using Stream stream = _fileManager.ReadFile(Path.Join(LauncherDataPath, fileName));
        return new Bitmap(stream);
    }

    public bool IsServerEnabled(int serverId)
    {
        return GetServerManifest(serverId) is { Enabled: true };
    }

    public static string GetLocalDataPath(params string[] relativePath)
    {
        var path = new string[relativePath.Length + 1];
        path[0] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Array.Copy(relativePath, 0, path, 1, relativePath.Length);
        return Path.Join(path);
    }

    private ServerManifest? GetServerManifest(int serverId)
    {
        if (serverId >= _manifest.Servers.Count || serverId < 0) return null;
        return _manifest.Servers[serverId];
    }

    public async void StartServer(int serverId, Action onComplete)
    {
        if (GetServerManifest(serverId) is not { } serverManifest) return;

        await InstallJava(serverManifest.JavaDistribution);



        onComplete?.Invoke();
    }

    private async Task InstallJava(string javaDist)
    {
        if (!_settings.DownloadedContent.Contains(javaDist))
        {
            JavaManifest javaManifest = _manifest.JavaDistributions[javaDist];
            await _contentDownloader.DownloadJava(javaDist, javaManifest, _archiveExtractor);
            _settings.DownloadedContent.Add(javaDist);
            await SaveSettings();
        }
    }
}