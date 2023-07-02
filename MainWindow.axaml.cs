using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Launcher.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using AvaloniaProgressRing;
using Debug = System.Diagnostics.Debug;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private const float WindowScale = 0.8f;
        private readonly ContentManager _contentManager;
        private List<ProgressRing> _progressRings = new List<ProgressRing>();
        private bool _launchIsProcessing = false;
        private bool _settingsViewIsOpen = false;

        public MainWindow()
        {
            _contentManager = new ContentManager(new HttpClient());
            InitializeComponent();

            InitialResize();
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
            var settings = _contentManager.GetSettings();
            settingsView.UpdateView(settings.Username, settings.UseMemoryMB, settings.CloseLauncher);
        }

        private async void InitButtons()
        {
            _progressRings.Add(firstServerLoadingBar);
            _progressRings.Add(secondServerLoadingBar);
            firstServerButton.Click += (s, e) => OnServerClicked(s, e, 0);
            secondServerButton.Click += (s, e) => OnServerClicked(s, e, 1);
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
            var settings = _contentManager.GetSettings();
            settings.Username = settingsView.Username;
            settings.UseMemoryMB = settingsView.Memory;
            settings.CloseLauncher = settingsView.CloseOnLaunch;
            await _contentManager.SaveSettings(settings);
            ChangeSettingsViewVisibility(open: false);
        }

        private void ChangeSettingsViewVisibility(bool open)
        {
            _settingsViewIsOpen = open;
            settingsView.IsVisible = open;
            if (!open)
            {
                var settings = _contentManager.GetSettings();
                settingsView.UpdateView(settings.Username, settings.UseMemoryMB, settings.CloseLauncher);
            }
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

        private void OnServerClicked(object? sender, RoutedEventArgs e, int serverId)
        {
            if (_launchIsProcessing) return;
            _launchIsProcessing = true;
            _progressRings[serverId].IsActive = true;
            _contentManager.StartServer(serverId, () =>
            {
                _progressRings[serverId].IsActive = false;
                _launchIsProcessing = false;
            });
        }
    }
}