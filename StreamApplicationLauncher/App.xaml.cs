using System.IO;
using System.Windows;
using StreamApplicationLauncher.Data;
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

        Directory.CreateDirectory(Constants.ApplicationStoragePath);

        LogManager logManager = new();
        MainWindow mainWindow = new() {
            DataContext = new MainViewModel(logManager, new RuntimeTimer())
        };
        mainWindow.Show();

        PidSqliteDataService pidSqlite = new();
        pidSqlite.Initialize();

        Launcher.Launcher launcher = new(logManager, e.Args[0], pidSqlite);
        Task.Run(() => {
            launcher.Run();
            logManager.Info($"Application launch complete; Automatically shutting down");

            for (int seconds = Constants.ApplicationAutoShutdownDelaySeconds; seconds > 0; seconds--) {
                logManager.Info($"Shutdown in {seconds} second{(seconds != 1 ? "s" : "")}");
                Thread.Sleep(1000);
            }

            logManager.Info("Goodbye");
            Current.Dispatcher.Invoke(() => Current.Shutdown());
        });
    }
}