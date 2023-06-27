using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Launcher.Interfaces;
using Launcher.Models;

namespace Launcher.Services;

public class ArchiveExtractor
{
    private readonly IArchiveExtractor _zipExtractor;
    private readonly IArchiveExtractor _tarExtractor;
    private readonly ContentDownloader _contentDownloader;

    public ArchiveExtractor(ContentDownloader contentDownloader, FileManager fileManager)
    {
        _contentDownloader = contentDownloader;
        _zipExtractor = new ZipExtractor(fileManager);
        _tarExtractor = new TarExtractor(fileManager);
    }

    public async Task ExtractZip(string extractPath, string url)
    {
        var uri = new Uri(url);
        string ext = Path.GetExtension(uri.AbsolutePath);

        await _contentDownloader.DownloadFile(url, async contentStream =>
        {
            switch (ext)
            {
                case ".zip":
                    await _zipExtractor.ExtractArchive(extractPath, contentStream);
                    break;
                case ".tar.gz":
                    await _tarExtractor.ExtractArchive(extractPath, contentStream);
                    break;
                default:
                    throw new Exception($"Unknown archive extension: {ext}");
            }
        });
    }
}