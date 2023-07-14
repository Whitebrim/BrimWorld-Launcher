using CommandLine;

namespace Launcher.Models;

public class CommandLineOptions
{
    [Option('p', "path", Required = false, HelpText = "Launcher data path to save minecraft to")]
    public string Path { get; set; }

    [Option('m', "multiple-launch", Required = false, Default = (bool)false, HelpText = "If true, launcher allows to start multiple instances of minecraft")]
    public bool MultipleLaunch { get; set; }

    public CommandLineOptions()
    {
        Path = "";
        MultipleLaunch = false;
    }

    public CommandLineOptions(string path, bool multipleLaunch)
    {
        Path = path;
        MultipleLaunch = multipleLaunch;
    }
}