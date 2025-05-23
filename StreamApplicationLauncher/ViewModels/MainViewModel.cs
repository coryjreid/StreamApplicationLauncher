using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.Models.DesignTime;
using StreamApplicationLauncher.ViewModels.Commands;

namespace StreamApplicationLauncher.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    public ObservableCollection<LogMessage> LogMessages { get; }
    public ScrollState ScrollState { get; } = new();
    public ICommand ScrollToBottomCommand { get; }
    public string DurationText => _runtimeTimer.DurationText;
    private readonly IRuntimeTimer _runtimeTimer;

    // Design-time constructor
    public MainViewModel() : this(new DesignTimeLogManager(), new RuntimeTimer()) {
    }


    // Runtime constructor
    public MainViewModel(LogManager logManager, IRuntimeTimer runtimeTimer) {
        _runtimeTimer = runtimeTimer;

        LogMessages = logManager.LogMessages;
        ScrollToBottomCommand = new RelayCommand(ExecuteScrollToBottom, CanScrollToBottom);

        ScrollState.PropertyChanged += (_, _) => { (ScrollToBottomCommand as RelayCommand)?.RaiseCanExecuteChanged(); };

        _runtimeTimer.PropertyChanged += (_, args) => {
            if (args.PropertyName == nameof(DurationText)) {
                OnPropertyChanged(nameof(DurationText));
            }
        };
    }

    private void ExecuteScrollToBottom(object? _) => ScrollState.IsScrollRequested = true;
    private bool CanScrollToBottom(object? _) => !ScrollState.IsAtBottom;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}