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

    private Manifest _manifest = null!;

    public ContentManager()
    {
        _fileManager = new FileManager(GetLocalDataPath("BrimWorld"));
        _contentDownloader = new ContentDownloader(_fileManager);
        _archiveExtractor = new ArchiveExtractor(_fileManager);
    }

    public async Task<bool> Initialize()
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

        //OnManifestLoaded?.Invoke(this, EventArgs.Empty);

        return true;
    }

    private async Task SaveBanner(int serverIndex)
    {
        if (serverIndex >= _manifest.Servers.Count) return;
        string fileName = Path.GetFileName(_manifest.Servers[serverIndex].BannerUrl);
        string localFilePath = Path.Join(LauncherDataPath, fileName);

        string url = _manifest.Servers[serverIndex].BannerUrl;
        using var httpClient = new HttpClient();
        using HttpResponseMessage response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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
        //Debug.WriteLine(md5_sum);

        await using Stream contentStream = await response.Content.ReadAsStreamAsync();

        await using Stream stream = _fileManager.CreateFile(Path.Join(LauncherDataPath, fileName));
        await contentStream.CopyToAsync(stream);
    }

    public async Task<Bitmap?> GetBanner(int serverIndex)
    {
        if (serverIndex >= _manifest.Servers.Count) return null;
        string fileName = Path.GetFileName(_manifest.Servers[serverIndex].BannerUrl);
        await using Stream stream = _fileManager.ReadFile(Path.Join(LauncherDataPath, fileName));
        return new Bitmap(stream);
    }

    public bool IsServerEnabled(int serverIndex)
    {
        return serverIndex < _manifest.Servers.Count && _manifest.Servers[serverIndex].Enabled;
    }

    public static string GetLocalDataPath(params string[] relativePath)
    {
        var path = new string[relativePath.Length + 1];
        path[0] = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Array.Copy(relativePath, 0, path, 1, relativePath.Length);
        return Path.Join(path);
    }
}