using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using LogViewerApp.Models;

namespace LogViewerApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    public ObservableCollection<LogMessage> LogMessages { get; } = [];

    public MainViewModel() {
        StartLogSimulation();
    }

    private async void StartLogSimulation() {
        Random rnd = new();
        LogLevel[] levels = Enum.GetValues<LogLevel>();

        while (true) {
            LogLevel level = levels[rnd.Next(levels.Length)];
            AddLog(level, $"Simulated {level} message");
            await Task.Delay(1000);
        }
    }

    public void AddLog(LogLevel level, string message) {
        Application.Current.Dispatcher.Invoke(() => {
            LogMessages.Add(new LogMessage() {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}