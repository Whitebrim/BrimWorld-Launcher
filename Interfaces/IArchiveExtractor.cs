using System.IO;
using System.Threading.Tasks;

namespace Launcher.Interfaces;

public interface IArchiveExtractor
{
    public async Task ExtractArchive(string extractPath, Stream contentStream){}
}