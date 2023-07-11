using CommandLine;

namespace Launcher.Models;

public class CommandLineOptions
{
    [Option('p', "path", Required = false, HelpText = "Launcher data path to save minecraft to")]
    public string Path { get; set; }
}