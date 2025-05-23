using System.ComponentModel;

namespace LogViewerApp.Models;

public interface IRuntimeTimer : INotifyPropertyChanged {
    string DurationText { get; }
    void Tick(); // manually triggerable for test
}