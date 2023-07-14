using CommandLine;

namespace Launcher.Models;

public class CommandLineOptions
{
    public CommandLineOptions(string path, bool? multipleLaunch)
    {
        MultipleLaunch = multipleLaunch;
        Path = path;
    }

    [Option('p', "path", Required = false, HelpText = "Launcher data path to save minecraft to")]
    public string Path { get; }

    [Option('m', "multiple-launch", Required = false, Default = (bool)false, HelpText = "If true, launcher allows to start multiple instances of minecraft")]
    public bool? MultipleLaunch { get; }
}