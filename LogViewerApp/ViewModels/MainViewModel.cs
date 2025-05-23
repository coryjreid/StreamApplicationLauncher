using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LogViewerApp.Models;
using LogViewerApp.Models.DesignTime;

namespace LogViewerApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    public ObservableCollection<LogMessage> LogMessages { get; }

    // Design-time constructor
    public MainViewModel() : this(new DesignTimeLogManager()) {
    }

    // Runtime constructor
    public MainViewModel(LogManager logManager) {
        LogMessages = logManager.LogMessages;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}