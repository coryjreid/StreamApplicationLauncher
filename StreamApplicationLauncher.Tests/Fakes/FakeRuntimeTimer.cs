using System.ComponentModel;
using System.Runtime.CompilerServices;
using StreamApplicationLauncher.Models;

namespace StreamApplicationLauncher.Tests.Fakes;

public class FakeRuntimeTimer : IRuntimeTimer {
    private int _seconds = 0;
    private string _durationText = "00:00:00";

    public string DurationText {
        get => _durationText;
        private set {
            if (_durationText != value) {
                _durationText = value;
                OnPropertyChanged();
            }
        }
    }

    public void Tick() {
        _seconds++;
        DurationText = TimeSpan.FromSeconds(_seconds).ToString(@"hh\:mm\:ss");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}