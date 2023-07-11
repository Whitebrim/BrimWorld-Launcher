using System;

namespace Launcher.Services;

public class LaunchSettings
{
    private static readonly Lazy<LaunchSettings> LazyLoader = new Lazy<LaunchSettings>(() =>
    {
        return new LaunchSettings(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    });
    public static LaunchSettings Instance => LazyLoader.Value;

    public LaunchSettings(string dataPath)
    {
        DataPath = dataPath;
    }

    public string DataPath { get; set; }
}