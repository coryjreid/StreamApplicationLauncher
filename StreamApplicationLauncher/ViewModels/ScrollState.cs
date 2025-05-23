using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StreamApplicationLauncher.ViewModels;

public class ScrollState : INotifyPropertyChanged {
    private bool _isAtBottom = true;
    private bool _isScrollRequested = false;

    public bool IsAtBottom {
        get => _isAtBottom;
        set {
            if (_isAtBottom != value) {
                _isAtBottom = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsScrollRequested {
        get => _isScrollRequested;
        set {
            if (_isScrollRequested != value) {
                _isScrollRequested = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}