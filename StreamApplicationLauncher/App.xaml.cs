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

        if (e.Args.Length > 0) {
            string configPath = e.Args[0];
            Launcher.Launcher launcher = new(logManager, configPath);
            launcher.Run();
        }

        MainWindow mainWindow = new() {
            DataContext = new MainViewModel(logManager, new RuntimeTimer())
        };
        mainWindow.Show();
    }
}