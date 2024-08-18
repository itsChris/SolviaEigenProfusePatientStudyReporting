using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SolviaEigenProfusePatientStudyReporting.Services;
using SolviaEigenProfusePatientStudyReporting.ViewModels;
using SolviaEigenProfusePatientStudyReporting.Views;
using System.IO;
using System.Windows;


namespace SolviaEigenProfusePatientStudyReporting
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
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
    }
}
