using CommandLine;

namespace Launcher.Models;

public class CommandLineOptions
{
    private readonly string path;
    private readonly bool? multipleLaunch;

    public CommandLineOptions(string path, bool? multipleLaunch)
    {
        this.multipleLaunch = multipleLaunch;
        this.path = path;
    }

    [Option('p', "path", Required = false, HelpText = "Launcher data path to save minecraft to")]
    public string Path => path;

    [Option('m', "multiple-launch", Required = false, Default = (bool)false, HelpText = "If true, launcher allows to start multiple instances of minecraft")]
    public bool? MultipleLaunch => multipleLaunch;
}