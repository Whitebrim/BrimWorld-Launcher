using System.IO;
using Debug = System.Diagnostics.Debug;

namespace Launcher.Services;

public class FileManager
{
    private readonly string _baseDirectory;

    public FileManager(string baseDirectory)
    {
        _baseDirectory = baseDirectory;
    }

    public Stream ReadFile(string filePath)
    {
        filePath = Path.Join(_baseDirectory, filePath);
        return File.OpenRead(filePath);
    }

    public Stream CreateFile(string filePath)
    {
        filePath = Path.Join(_baseDirectory, filePath);
        Debug.WriteLine("Created file " + filePath);
        EnsureDirectoryExists(filePath);
        return File.Create(filePath);
    }

    public bool FileExist(string filePath)
    {
        return File.Exists(Path.Join(_baseDirectory, filePath));
    }

    private static void EnsureDirectoryExists(string path)
    {
        string? dirName = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dirName))
        {
            Directory.CreateDirectory(dirName);
        }
    }
}