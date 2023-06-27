using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Launcher.Models
{
    public class Settings
    {
        public int WindowSizeX { get; init; } = 480;
        public int WindowSizeY { get; init; } = 854;
        public bool FullScreen { get; init; } = false;
        public int UseMemoryMB { get; init; } = 6144;
        public string JavaArguments { get; init; } = "";
        public List<string> DownloadedContent { get; init; } = new List<string>();
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Settings))]
    public partial class SettingsContext : JsonSerializerContext { }
}