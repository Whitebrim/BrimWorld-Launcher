using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Launcher.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using AvaloniaProgressRing;
using Debug = System.Diagnostics.Debug;
using Launcher.Extensions;
using CommandLine;
using Launcher.Models;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private const float WindowScale = 0.8f;
        private ContentManager _contentManager;
        private readonly List<ProgressRing> _progressRings = new List<ProgressRing>();
        private bool _launchIsProcessing = false;
        private bool _settingsViewIsOpen = false;

        public MainWindow()
        {
            Application? app = Application.Current;
            if (app?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime!.Startup += (sender, args) =>
                {
                    Init(args.Args);
                };
            }
        }

        private void Init(IEnumerable<string> args)
        {
            ParseLaunchArgs(args);
            _contentManager = new ContentManager(new HttpClient(), ChangeProgressBar);
            InitializeComponent();

            InitialResize();
        }

        private void ParseLaunchArgs(IEnumerable<string> args)
        {
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o =>
                {
                    if (!string.IsNullOrEmpty(o.Path))
                    {
                        DirectoryInfo dir = Directory.CreateDirectory(o.Path);
                        if (!dir.Exists)
                        {
                            throw new LaunchArgumentException($"Invalid launch argument: Path {o.Path} cant be created.");
                        }
                        LaunchSettings.Instance.DataPath = o.Path;
                    }
                    if (o.MultipleLaunch.HasValue)
                    {
                        LaunchSettings.Instance.MultipleLaunch = o.MultipleLaunch.Value;
                    }

                });
        }

        private async void OnLoaded(object? sender, RoutedEventArgs e)
        {
            bool success = await _contentManager.Initialize();
            if (success)
            {
                InitButtons();
                InitSettingsView();
            }
            else
            {
                Debug.WriteLine("OnLoaded can't access internet, show window and close");
            }
        }

        private void InitSettingsView()
        {
            settingsView.onApplyClicked += OnSettingsApplyClicked;
            settingsView.onCancelClicked += OnSettingsCancelClicked;
            Settings settings = _contentManager.GetSettings();
            settingsView.UpdateView(settings);
        }

        private async void InitButtons()
        {
            _progressRings.Add(firstServerLoadingBar);
            _progressRings.Add(secondServerLoadingBar);
            firstServerButton.Click += (s, e) => OnServerClicked(0);
            secondServerButton.Click += (s, e) => OnServerClicked(1);
            firstServerButton.IsEnabled = _contentManager.IsServerEnabled(0);
            secondServerButton.IsEnabled = _contentManager.IsServerEnabled(1);
            settingsButton.IsEnabled = true;
            foreach (ProgressRing ring in _progressRings)
            {
                ring.IsActive = false;
            }
            (firstServerButton.Content as Image)!.Source = await _contentManager.GetBanner(0);
            (secondServerButton.Content as Image)!.Source = await _contentManager.GetBanner(1);
        }

        private void InitialResize()
        {
            PixelRect screen = Screens.ScreenFromWindow(this)!.WorkingArea;
            var newHeight = (int)(Math.Round(screen.Height * (WindowScale / DesktopScaling)));
            Height = newHeight;
            Width = (int)(Math.Round(newHeight * 11.0 / 19.0));
        }

        private void OnResized(object? sender, WindowResizedEventArgs e)
        {
            if (e.Reason != WindowResizeReason.Application)
            {
                InitialResize();
            }
        }

        private void OnScalingChanged(object? sender, EventArgs e)
        {
            InitialResize();
        }

        private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            BeginMoveDrag(e);
        }

        private void MinimizeApplication(object? sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ExitApplication(object? sender, RoutedEventArgs e)
        {
            Application? app = Application.Current;
            if (app?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }

        private void OnSettingsClicked(object? sender, RoutedEventArgs e)
        {
            if (_settingsViewIsOpen) return;
            ChangeSettingsViewVisibility(open: true);
        }

        private void OnSettingsCancelClicked()
        {
            ChangeSettingsViewVisibility(open: false);
        }

        private async void OnSettingsApplyClicked()
        {
            Settings settings = _contentManager.GetSettings();
            settings.Username = settingsView.Username;
            settings.UseMemoryMB = Math.Max(2048, settingsView.Memory);
            settings.CloseLauncher = settingsView.CloseOnLaunch;
            settings.OpenConsole = settingsView.OpenConsole;
            await _contentManager.SaveSettings(settings);
            ChangeSettingsViewVisibility(open: false);
        }

        private void ChangeSettingsViewVisibility(bool open)
        {
            _settingsViewIsOpen = open;
            settingsView.IsVisible = open;

            if (open) return;

            Settings settings = _contentManager.GetSettings();
            settingsView.UpdateView(settings);
        }

        private void OnVKClicked(object? sender, RoutedEventArgs e)
        {
            new Process { StartInfo = new ProcessStartInfo("https://vk.com/brimworld") { UseShellExecute = true } }
                .Start();
        }

        private void OnDiscordClicked(object? sender, RoutedEventArgs e)
        {
            new Process { StartInfo = new ProcessStartInfo("discord:///invite/eD7dtgj94w") { UseShellExecute = true } }
                .Start();
        }

        private void OnServerClicked(int serverId)
        {
            if (_launchIsProcessing) return;
            EnableProgressRings(serverId, true);
            _launchIsProcessing = true;

            var task = new Task(async () =>
            {
                try
                {
                    await _contentManager.StartServer(serverId, () => _launchIsProcessing = false);
                }
                catch (InvalidUsernameException)
                {
                    Dispatcher.UIThread.Invoke(() => ChangeSettingsViewVisibility(open: true));
                    _launchIsProcessing = false;
                }
                finally
                {
                    Dispatcher.UIThread.Invoke(() => EnableProgressRings(serverId, false));
                    if (LaunchSettings.Instance.MultipleLaunch) _launchIsProcessing = false;
                }
            });

            task.Start();
        }

        private void EnableProgressRings(int serverId, bool enable)
        {
            _progressRings[serverId].IsActive = enable;
        }

        private void ChangeProgressBar(float value)
        {
            if (!progressBar.IsVisible && value < 1)
            {
                progressBar.IsVisible = true;
            }
            if (progressBar.IsVisible && Math.Abs(value - 1) < 0.0001f)
            {
                progressBar.IsVisible = false;
            }
            progressBar.Value = value;
        }
    }
}