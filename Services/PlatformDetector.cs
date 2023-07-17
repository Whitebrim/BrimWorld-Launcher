using System.Runtime.InteropServices;

namespace Launcher.Services;

public static class PlatformDetector
{
    public static string GetSystemInfo()
    {
        string platform = GetPlatform();
        string architecture = GetArchitecture();
        var systemInfo = $"{platform}-{architecture}";

        return systemInfo;
    }

    public static string GetPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return "windows";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return "linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return "macos";
        return "unknown";
    }

    private static string GetArchitecture()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X86 => "x86",
            Architecture.X64 => "x86-64",
            Architecture.Arm64 => "aarch64",
            _ => "unknown"
        };
    }
}