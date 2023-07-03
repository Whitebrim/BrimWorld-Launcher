using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Launcher.Models
{
    public class Manifest
    {
        public int ManifestVersion { get; init; } = 0;
        public string LauncherVersion { get; init; } = "1.0.0";
        public string UpdateLauncherUrl { get; init; } = "";

        public Dictionary<string, JavaManifest> JavaDistributions { get; init; } =
            new Dictionary<string, JavaManifest>()
            {
                {
                    "jre17", new JavaManifest
                    {
                        JavaVersion = 17,
                        DownloadUrls = new Dictionary<string, string>()
                        {
                            {
                                "windows-x86-64",
                                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.7%2B7/OpenJDK17U-jre_x64_windows_hotspot_17.0.7_7.zip"
                            },
                            {
                                "macos-aarch64",
                                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.7%2B7/OpenJDK17U-jre_aarch64_mac_hotspot_17.0.7_7.tar.gz"
                            },
                            {
                                "linux-x86-64",
                                "https://github.com/adoptium/temurin17-binaries/releases/download/jdk-17.0.7%2B7/OpenJDK17U-jre_x64_linux_hotspot_17.0.7_7.tar.gz"
                            },
                        },
                    }
                },
            };

        public List<ServerManifest> Servers { get; init; } = new List<ServerManifest>
        {
            new ServerManifest(),
            new ServerManifest(),
        };
    }

    public class JavaManifest
    {
        public int JavaVersion { get; init; } = 0;
        public Dictionary<string, string> DownloadUrls { get; init; } = new Dictionary<string, string>();
    }

    public class ServerManifest
    {
        public int Version { get; init; } = 0;
        public string Name { get; init; } = "";
        public string Alias { get; init; } = "";
        public bool Enabled { get; init; } = true;
        public string BannerUrl { get; init; } = "";
        public string ClientUrl { get; init; } = "";
        public string JavaDistribution { get; init; } = "";
        public string ServerIp { get; init; } = "";
        public string ServerPort { get; init; } = "";

        public string JavaArgs { get; init; } = "";

        public string MinecraftArgs { get; init; } = "";
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Manifest))]
    public partial class ManifestContext : JsonSerializerContext { }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(ServerManifest))]
    public partial class ServerManifestContext : JsonSerializerContext { }
}