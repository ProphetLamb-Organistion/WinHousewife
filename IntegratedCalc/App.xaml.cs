using System;
using System.Threading;
using System.Windows;

namespace IntegratedCalc
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static Mutex mutex = new Mutex(true, "{a196e338-240e-4c04-b29e-c0c3f7330b08}");

        public App()
        {
            Init();
        }

        [STAThread]
        static void Init()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                new InitWindow().Show();
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("This is a single-instance application. Only one instance at a time is allowed.\nPress [Win]+[C] to show the window.", "Error: Single-Instance application", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }
    }
}
