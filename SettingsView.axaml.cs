using Avalonia.Controls;
using Avalonia.Interactivity;
using Launcher.Services;
using System;
using System.Diagnostics;

namespace Launcher;

public partial class SettingsView : UserControl
{
    public Action onApplyClicked = null!;
    public Action onCancelClicked = null!;
    public string Username => username.Text ?? "";
    public int Memory
    {
        get
        {
            if (int.TryParse(memory.Text, out var result))
            {
                return result;
            }
            return 4096;
        }
    }
    public bool CloseOnLaunch => closeOnLaunch.IsChecked ?? true;

    public SettingsView()
    {
        InitializeComponent();
    }

    public void UpdateView(string username, int memory, bool closeOnLaunch)
    {
        this.username.Text = username;
        this.memory.Text = memory.ToString();
        this.closeOnLaunch.IsChecked = closeOnLaunch;
    }

    private void OnApplyClicked(object? sender, RoutedEventArgs e)
    {
        onApplyClicked?.Invoke();
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        onCancelClicked?.Invoke();
    }

    private void OnFolderClicked(object? sender, RoutedEventArgs e)
    {
        new Process
        {
            StartInfo = new ProcessStartInfo("file://" + ContentManager.GetLocalDataPath("BrimWorld"))
            { UseShellExecute = true }
        }.Start();
    }
}