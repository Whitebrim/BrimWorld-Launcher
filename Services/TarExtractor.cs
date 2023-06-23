using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Launcher.Interfaces;

namespace Launcher.Services;

public class TarExtractor : IArchiveExtractor
{
    private readonly FileManager _fileManager;

    public TarExtractor(FileManager fileManager)
    {
        _fileManager = fileManager;
    }

    public async Task ExtractArchive(string extractPath, Stream contentStream)
    {
        await using var gzipStream = new GZipStream(contentStream, CompressionMode.Decompress);
        await using var archive = new TarReader(gzipStream);
        for (;;)
        {
            TarEntry? entry = await archive.GetNextEntryAsync();
            if (entry == null) {
                break;
            }
            if (entry.EntryType is not TarEntryType.RegularFile || entry.DataStream == null)
            {
                continue;
            }
            await using Stream outputFile = _fileManager.CreateFile(Path.Join(extractPath, entry.Name));
            await entry.DataStream.CopyToAsync(outputFile);
        }
    }
}