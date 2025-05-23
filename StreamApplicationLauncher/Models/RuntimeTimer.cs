using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace StreamApplicationLauncher.Models;

public class RuntimeTimer : IRuntimeTimer {
    private readonly DispatcherTimer _timer;
    private readonly DateTime _startTime;
    private string _durationText = "00:00:00";

    public string DurationText {
        get => _durationText;
        private set {
            if (_durationText == value) {
                return;
            }

            _durationText = value;
            OnPropertyChanged();
        }
    }

    public RuntimeTimer() {
        _startTime = DateTime.Now;

        _timer = new DispatcherTimer {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (_, _) => Tick();
        _timer.Start();
    }

    public void Tick() {
        TimeSpan elapsed = DateTime.Now - _startTime;
        DurationText = elapsed.ToString(@"hh\:mm\:ss");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}