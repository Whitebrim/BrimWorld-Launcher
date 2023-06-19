using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Debug = System.Diagnostics.Debug;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private const float windowSize = 0.8f;
        //private double _previousWidth;
        //private double _previousHeight;
        public MainWindow()
        {
            InitializeComponent();
            InitialResize();
            InitButtons();
            //_previousWidth = Width;
            //_previousHeight = Height;
        }

        private void InitialResize()
        {
            var screen = Screens.ScreenFromWindow(this).WorkingArea;
            int newHeight = (int)(Math.Round(screen.Height * windowSize));
            Height = newHeight;
            Width = newHeight * 11.0 / 19.0;
        }

        private void InitButtons()
        {
            
        }

        //private void OnResized(object? sender, WindowResizedEventArgs e)
        //{
        //    if (e.Reason == WindowResizeReason.User)
        //    {
        //        var screen = Screens.ScreenFromWindow(this).WorkingArea;
        //        var titlebarHeight = FrameSize.HasValue ? FrameSize.Value.Height - ClientSize.Height : 0;
        //        if ((int)_previousHeight != (int)e.ClientSize.Height) // Скейлим до 19:11 используя как базу высоту
        //        {
        //            Height = Math.Min(screen.Height - titlebarHeight, e.ClientSize.Height);
        //            Debug.WriteLine(screen.Height - titlebarHeight + " " + e.ClientSize.Height);
        //            Width = Height * 11 / 19;
        //        }
        //        else // Скейлим до 19:11 используя как базу ширину
        //        {
        //            Height = Math.Min(screen.Height - titlebarHeight, e.ClientSize.Width * 19 / 11);
        //            Width = Height * 11 / 19;
        //        }
        //    }
        //    _previousWidth = Width;
        //    _previousHeight = Height;
        //}

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
            var app = Application.Current;
            if (app?.ApplicationLifetime is ClassicDesktopStyleApplicationLifetime lifetime)
            {
                lifetime.Shutdown();
            }
        }

        private void OnSettingsClicked(object? sender, RoutedEventArgs e)
        {
            
        }

        private void OnVKClicked(object? sender, RoutedEventArgs e)
        {
            new Process {StartInfo = new ProcessStartInfo("https://vk.com/brimworld") {UseShellExecute = true}}.Start();
        }

        private void OnDiscordClicked(object? sender, RoutedEventArgs e)
        {
            new Process {StartInfo = new ProcessStartInfo("https://discord.gg/eD7dtgj94w") {UseShellExecute = true}}.Start();
        }

        private void OnFirstServerClicked(object? sender, RoutedEventArgs e)
        {
            
        }

        private void OnSecondServerClicked(object? sender, RoutedEventArgs e)
        {
            
        }
    }
}