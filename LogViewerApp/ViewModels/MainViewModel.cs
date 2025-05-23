using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using LogViewerApp.Models;
using LogViewerApp.Models.DesignTime;
using LogViewerApp.ViewModels.Commands;

namespace LogViewerApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    public ObservableCollection<LogMessage> LogMessages { get; }
    public ScrollState ScrollState { get; } = new ScrollState();
    public ICommand ScrollToBottomCommand { get; }
    private readonly LogManager _logManager;

    // Design-time constructor
    public MainViewModel() : this(new DesignTimeLogManager()) {
    }

    // Runtime constructor
    public MainViewModel(LogManager logManager) {
        _logManager = logManager;
        LogMessages = logManager.LogMessages;
        ScrollToBottomCommand = new RelayCommand(ExecuteScrollToBottom, CanScrollToBottom);
        ScrollState.PropertyChanged += (_, _) => { (ScrollToBottomCommand as RelayCommand)?.RaiseCanExecuteChanged(); };
    }

    private void ExecuteScrollToBottom(object? _) {
        ScrollState.IsScrollRequested = true;
    }


    private bool CanScrollToBottom(object? _) {
        return !ScrollState.IsAtBottom;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}