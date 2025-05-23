using System.Windows;
using LogViewerApp.Models;
using LogViewerApp.ViewModels;

namespace LogViewerApp.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            LogManager logManager = new();
            MainViewModel viewModel = new(logManager);
            DataContext = viewModel;

            logManager.StartSimulatedLogging();
        }
    }
}