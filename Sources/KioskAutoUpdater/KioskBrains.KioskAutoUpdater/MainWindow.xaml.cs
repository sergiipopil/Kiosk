using System.Windows;

namespace KioskBrains.KioskAutoUpdater
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Run();
        }

        private async void Run()
        {
            try
            {
                await KioskAutoUpdater.Current.RunAsync();
            }
            catch (System.Exception ex)
            {
                Log.Text = ex.Message;
            }
        }
    }
}