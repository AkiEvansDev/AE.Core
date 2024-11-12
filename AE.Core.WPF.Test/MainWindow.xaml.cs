using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace AE.Core.WPF.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new TestVM();
        }
    }

    public partial class TestVM : ObservableObject
    {
        [ObservableProperty]
        private byte a = 200;

        [ObservableProperty]
        private byte r = 200;

        [ObservableProperty]
        private byte g = 200;

        [ObservableProperty]
        private byte b = 200;
    }
}