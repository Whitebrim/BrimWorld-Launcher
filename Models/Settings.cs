using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Launcher.Models
{
    public class Settings
    {
        public string Username { get; set; } = "";
        public int WindowSizeX { get; set; } = 480;
        public int WindowSizeY { get; set; } = 854;
        public bool FullScreen { get; set; } = false;
        public bool CloseLauncher { get; set; } = false;
        public int UseMemoryMB { get; set; } = 4096;
        public string JavaArguments { get; set; } = "";
        public List<string> DownloadedContent { get; set; } = new List<string>();
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Settings))]
    public partial class SettingsContext : JsonSerializerContext { }
}