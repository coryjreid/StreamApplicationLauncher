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
    public string DurationText => _runtimeTimer.DurationText;
    private readonly LogManager _logManager;
    private readonly RuntimeTimer _runtimeTimer;

    // Design-time constructor
    public MainViewModel() : this(new DesignTimeLogManager()) {
    }

    // Runtime constructor
    public MainViewModel(LogManager logManager) {
        _logManager = logManager;
        _runtimeTimer = new RuntimeTimer();
        LogMessages = logManager.LogMessages;
        ScrollToBottomCommand = new RelayCommand(ExecuteScrollToBottom, CanScrollToBottom);
        ScrollState.PropertyChanged += (_, _) => { (ScrollToBottomCommand as RelayCommand)?.RaiseCanExecuteChanged(); };
        _runtimeTimer.PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(RuntimeTimer.DurationText)) {
                OnPropertyChanged(nameof(DurationText));
            }
        };
    }

    private void ExecuteScrollToBottom(object? _) => ScrollState.IsScrollRequested = true;
    private bool CanScrollToBottom(object? _) => !ScrollState.IsAtBottom;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}