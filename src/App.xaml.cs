using System;
using System.Diagnostics;
using System.Linq;
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
                goto LAUNCH;
            }
            else
            {
                if (MessageBoxResult.Yes == MessageBox.Show("This is a single-instance application. Only one instance at a time is allowed.\nPress [Win]+[C] to show the window.\n\nShould existing instances be terminated?", "Warning: Single-Instance application", MessageBoxButton.YesNo, MessageBoxImage.Warning))
                {
                    var processes = Process.GetProcesses().Where(x => x.ProcessName.StartsWith("IntegratedCalc", StringComparison.InvariantCultureIgnoreCase) || x.ProcessName.StartsWith("WinHousewife", StringComparison.InvariantCultureIgnoreCase));
                    var currentProcess = Process.GetCurrentProcess();
                    foreach (var process in processes.Where(p => p.Id != currentProcess.Id))
                        process.Kill();
                    goto LAUNCH;
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }
        LAUNCH:
            new InitWindow().Show();
            mutex.ReleaseMutex();
        }
    }
}
