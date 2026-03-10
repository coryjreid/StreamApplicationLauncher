using System.IO;
using System.Windows;
using StreamApplicationLauncher.Cleaner;
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

        bool isCleanupMode = e.Args.Contains("--cleanup", StringComparer.OrdinalIgnoreCase);
        bool hasConfigArg = e.Args.Length > 0 && !isCleanupMode;

        if (!isCleanupMode && !hasConfigArg) {
            MessageBox.Show(
                "Usage:\n  StreamApplicationLauncher.exe <config.json>\n  StreamApplicationLauncher.exe --cleanup",
                "Invalid Arguments",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            Shutdown(1);
            return;
        }

        Directory.CreateDirectory(Constants.ApplicationStoragePath);

        LogManager logManager = new();
        MainWindow mainWindow = new() {
            DataContext = new MainViewModel(logManager, new RuntimeTimer())
        };
        mainWindow.Show();

        PidSqliteDataService pidSqlite = new();
        pidSqlite.Initialize();

        if (isCleanupMode) {
            Cleaner.Cleaner cleaner = new(logManager, pidSqlite);
            Task.Run(() => {
                cleaner.Run();
                RunShutdownCountdown(logManager, "Cleanup complete");
            });
        } else {
            pidSqlite.ClearAllPids();
            Launcher.Launcher launcher = new(logManager, e.Args[0], pidSqlite);
            Task.Run(() => {
                launcher.Run();
                RunShutdownCountdown(logManager, "Application launch complete");
            });
        }
    }

    private void RunShutdownCountdown(LogManager logManager, string completionMessage) {
        logManager.Info($"{completionMessage}; Automatically shutting down");

        for (int seconds = Constants.ApplicationAutoShutdownDelaySeconds; seconds > 0; seconds--) {
            logManager.Info($"Shutdown in {seconds} second{(seconds != 1 ? "s" : "")}");
            Thread.Sleep(1000);
        }

        logManager.Info("Goodbye");
        Current.Dispatcher.Invoke(() => Current.Shutdown());
    }
}