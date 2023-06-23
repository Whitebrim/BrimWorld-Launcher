using System.Collections.Generic;
using Launcher.Extensions;
using Launcher.Interfaces;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Launcher.Services;

public class ZipExtractor : IArchiveExtractor
{
    private readonly FileManager _fileManager;
    private readonly bool _stripPrefix;

    public ZipExtractor(FileManager fileManager, bool stripPrefix = true)
    {
        _fileManager = fileManager;
        _stripPrefix = stripPrefix;
    }

    public async Task ExtractArchive(string extractPath, Stream contentStream)
    {
        using var archive = new ZipArchive(contentStream, ZipArchiveMode.Read);

        int prefixLength = _stripPrefix ? GetCommonPrefix(archive.Entries).Length : 0;

        foreach (ZipArchiveEntry entry in archive.Entries.Where((e) => !string.IsNullOrEmpty(e.Name)))
        {
            string fullName = entry.FullName[prefixLength..];
            string entryPath = Path.Join(extractPath, fullName);
            await using Stream entryStream = entry.Open();
            await using Stream outputFile = _fileManager.CreateFile(entryPath);
            await entryStream.CopyToAsync(outputFile);
        }
    }

    private string GetCommonPrefix(ReadOnlyCollection<ZipArchiveEntry> entries)
    {
        IEnumerable<string> samples = entries.Select(e => e.FullName);
        string commonPrefix = string.Join("/", samples.Select(s => s.Split('/').AsEnumerable())
            .Transpose()
            .TakeWhile(s => s.All(d => d == s.First()))
            .Select(s => s.First()));
        if (commonPrefix != "")
        {
            commonPrefix += "/";
        }

        return commonPrefix;
    }
}