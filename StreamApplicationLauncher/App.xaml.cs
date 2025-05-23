using System.Windows;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.ViewModels;
using StreamApplicationLauncher.Views;

namespace StreamApplicationLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    protected override void OnStartup(StartupEventArgs e) {
        base.OnStartup(e);

        LogManager logManager = new();

        MainWindow mainWindow = new() {
            DataContext = new MainViewModel(logManager, new RuntimeTimer())
        };
        mainWindow.Show();

        Launcher.Launcher launcher = new(logManager, e.Args[0]);
        Task.Run(() => launcher.Run());
    }
}