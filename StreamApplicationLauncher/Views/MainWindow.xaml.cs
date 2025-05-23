using System.Windows;
using StreamApplicationLauncher.Models;
using StreamApplicationLauncher.ViewModels;

namespace StreamApplicationLauncher.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            LogManager logManager = new();
            RuntimeTimer runtimeTimer = new();
            MainViewModel viewModel = new(logManager, runtimeTimer);
            DataContext = viewModel;

            logManager.StartSimulatedLogging();
        }
    }
}