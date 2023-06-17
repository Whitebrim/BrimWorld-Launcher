using System;
using Avalonia.Controls;
using Debug = System.Diagnostics.Debug;

namespace Launcher
{
    public partial class MainWindow : Window
    {
        private double _previousWidth;
        private double _previousHeight;
        public MainWindow()
        {
            InitializeComponent();
            _previousWidth = Width;
            _previousHeight = Height;
        }

        private void OnResized(object? sender, WindowResizedEventArgs e)
        {
            if (e.Reason == WindowResizeReason.User)
            {
                var screen = Screens.ScreenFromWindow(this).WorkingArea;
                var titlebarHeight = FrameSize.HasValue ? FrameSize.Value.Height - ClientSize.Height : 0;
                if ((int)_previousHeight != (int)e.ClientSize.Height) // Скейлим до 19:11 используя как базу высоту
                {
                    Height = Math.Min(screen.Height - titlebarHeight, e.ClientSize.Height);
                    Debug.WriteLine(screen.Height - titlebarHeight + " " + e.ClientSize.Height);
                    Width = Height * 11 / 19;
                }
                else // Скейлим до 19:11 используя как базу ширину
                {
                    Height = Math.Min(screen.Height - titlebarHeight, e.ClientSize.Width * 19 / 11);
                    Width = Height * 11 / 19;
                }
            }
            _previousWidth = Width;
            _previousHeight = Height;
        }
    }
}