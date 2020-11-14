using System;
using System.Windows;

namespace IntegratedCalc
{
    public static class ExceptionExtentions
    {
        public static void Try(Action action)
        {
            for (int i = 1; i <= 10; i++)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    if (MessageBoxResult.OK == MessageBox.Show(ex.Message + "\nSource:\n" + ex.Source + "\nin\n" + ex.TargetSite + "\n\nStack trace:\n" + ex.StackTrace, "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error))
                        continue;
                    else
                        throw ex;
                }
                break;
            }
        }
    }
}
