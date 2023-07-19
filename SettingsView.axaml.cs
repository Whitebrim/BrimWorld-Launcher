using Avalonia.Controls;
using Avalonia.Interactivity;
using Launcher.Services;
using System;
using System.Diagnostics;
using Launcher.Models;
using System.Reflection;

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
    public bool OpenConsole => openConsole.IsChecked ?? true;

    public SettingsView()
    {
        InitializeComponent();
        var ver = Assembly.GetExecutingAssembly().GetName().Version!;
        version.Text = "v" + ver.Major + "." + ver.Minor + "." + ver.Build;
    }

    public void UpdateView(Settings settings)
    {
        this.username.Text = settings.Username;
        this.memory.Text = settings.UseMemoryMB.ToString();
        this.closeOnLaunch.IsChecked = settings.CloseLauncher;
        this.openConsole.IsChecked = settings.OpenConsole;
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