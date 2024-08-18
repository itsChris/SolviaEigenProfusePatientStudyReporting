using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SolviaEigenProfusePatientStudyReporting.Services;
using SolviaEigenProfusePatientStudyReporting.ViewModels;
using SolviaEigenProfusePatientStudyReporting.Views;
using System.Windows;
using System.Windows.Threading;

namespace SolviaEigenProfusePatientStudyReporting
{
    public partial class App : Application
    {
        private readonly IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(context.Configuration, services);
                })
                .UseSerilog((context, services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(context.Configuration);
                })
                .Build();

            // Global exception handling
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<DatabaseService>(sp =>
                new DatabaseService(configuration.GetConnectionString("MariaDB")));
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _host.Start();
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unhandled UI exception occurred");
            MessageBox.Show("An unexpected error occurred. The application will be closed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Prevents the application from crashing
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log.Fatal((Exception)e.ExceptionObject, "A non-UI thread exception occurred");
            MessageBox.Show("A critical error occurred. The application will be closed.", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error(e.Exception, "An unobserved task exception occurred");
            e.SetObserved(); // Prevents the application from crashing
        }
    }
}
