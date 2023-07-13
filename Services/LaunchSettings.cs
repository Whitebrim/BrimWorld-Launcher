using System;

namespace Launcher.Services;

public class LaunchSettings
{
    private static readonly Lazy<LaunchSettings> LazyLoader = new Lazy<LaunchSettings>(() =>
    {
        return new LaunchSettings(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), false);
    });
    public static LaunchSettings Instance => LazyLoader.Value;

    public LaunchSettings(string dataPath, bool multipleLaunch)
    {
        DataPath = dataPath;
        MultipleLaunch = multipleLaunch;
    }

    public string DataPath { get; set; }
    public bool MultipleLaunch {get; set; }
}