using System.ComponentModel;

namespace StreamApplicationLauncher.Models;

public interface IRuntimeTimer : INotifyPropertyChanged {
    string DurationText { get; }
    void Tick(); // manually triggerable for test
}