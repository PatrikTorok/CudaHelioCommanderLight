using CudaHelioCommanderLight.Config;
using CudaHelioCommanderLight.Helpers;
using CudaHelioCommanderLight.Interfaces;
using CudaHelioCommanderLight.MainWindowServices;
using CudaHelioCommanderLight.Operations;
using CudaHelioCommanderLight.Services;
using CudaHelioCommanderLight.ViewModels;
using CudaHelioCommanderLight.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace CudaHelioCommanderLight
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMainHelper, MainHelper>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IFileWriter, FileWriter>();
            services.AddSingleton<IMetricsConfig>(provider => MetricsConfig.GetInstance(provider.GetRequiredService<IMainHelper>()));
            services.AddTransient<CompareLibraryOperation>();
            services.AddTransient<ComputeErrorOperation>();
            services.AddTransient<ButtonService>();
            services.AddTransient<IRenderingService, RenderingService>();
            services.AddTransient<HeatMapService>();
            services.AddTransient<IHeatMapGraphFactory, HeatMapGraphFactory>();
            services.AddTransient<ICompareService, CompareService>();
            services.AddTransient<IOpenConfigurationWindowOperation, OpenConfigurationWindowOperation>();
            services.AddTransient<IButtonService, ButtonService>();

            services.AddTransient<MainWindow>(provider => new MainWindow(
                provider.GetRequiredService<IMainHelper>(),
                provider.GetRequiredService<IDialogService>(),
                provider.GetRequiredService<IButtonService>(),
                provider.GetRequiredService<IRenderingService>(),
                provider.GetRequiredService<HeatMapService>(),
                provider.GetRequiredService<ICompareService>(),
                provider.GetRequiredService<IFileWriter>(),
                provider.GetRequiredService<CompareLibraryOperation>(),
                provider.GetRequiredService<IMetricsConfig>(),
                provider.GetRequiredService<IOpenConfigurationWindowOperation>()
            ));
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}