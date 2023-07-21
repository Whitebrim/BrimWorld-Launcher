using Avalonia;
using System;
using Debug = System.Diagnostics.Debug;
using Launcher.Services;
using System.Diagnostics;
using System.IO;

namespace Launcher
{
    internal class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception e)
            {
                switch (PlatformDetector.GetPlatform())
                {
                    case "windows":
                        string path = ContentManager.CrashReportPath;
                        if (string.IsNullOrEmpty(path))
                            path = Path.Join(Path.GetTempPath(), Path.GetTempFileName());
                        var n = Environment.NewLine;
                        File.WriteAllText(path, $"{n}╔════════════════════════════════════════════" +
                                                $"{n}║ ▶️ Этот краш-репорт был сохранен по пути:" +
                                                $"{n}║ ▶️ {path}" +
                                                $"{n}║ ▶️ Покажи его Бриму чтобы получить помощь." +
                                                $"{n}╚════════════════════════════════════════════" +
                                                $"{n}{n}" +
                                                e);
                        ShowErrorWindows(path);
                        break;
                }
            }
        }

        static void ShowErrorWindows(string errorLogPath)
        {
            Process.Start("notepad.exe", errorLogPath);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}