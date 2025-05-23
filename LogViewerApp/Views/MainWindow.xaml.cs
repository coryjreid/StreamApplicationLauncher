using System.Windows;
using LogViewerApp.Models;
using LogViewerApp.ViewModels;

namespace LogViewerApp.Views {
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