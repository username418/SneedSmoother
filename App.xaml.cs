using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System;
using System.Windows.Threading;

namespace PoeFixer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error_log.txt");

        protected override void OnStartup(StartupEventArgs e)
        {
            // Set up global exception handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception, "UI Thread Exception");
            
            MessageBox.Show($"An error occurred: {e.Exception.Message}\n\nDetails logged to: {LogFilePath}", 
                           "SneedSmoother Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // Mark as handled to prevent crash
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                LogException(exception, "Application Domain Exception");
            }
            else
            {
                LogError($"Unknown exception type: {e.ExceptionObject}");
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {context}\n" +
                                $"Exception: {ex.GetType().Name}\n" +
                                $"Message: {ex.Message}\n" +
                                $"Stack Trace: {ex.StackTrace}\n" +
                                $"{"".PadLeft(50, '-')}\n";

                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Ignore logging errors to prevent infinite loops
            }
        }

        public static void LogError(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}\n";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }

}
