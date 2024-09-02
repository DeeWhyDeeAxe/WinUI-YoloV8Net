using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Pages;
using WinUI_YoloV8_ObjectDetection.Services;

namespace WinUI_YoloV8_ObjectDetection
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; }

        public new static App Current => (App)Application.Current;
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>

        public App()
        {
            this.InitializeComponent();
            ServiceProvider = ConfigureServices();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = ServiceProvider.GetRequiredService<MainWindow>();
            m_window.Activate();
        }

        private static IServiceProvider ConfigureServices()
        {
            ServiceCollection serviceDescriptors = new ServiceCollection();
            serviceDescriptors.AddSingleton<IServiceProvider, ServiceProvider>();
            serviceDescriptors.AddSingleton<IConfigProvider, ConfigProvider>();
            serviceDescriptors.AddSingleton<IPredictorService, PredictorService>();
            serviceDescriptors.AddSingleton<ISettingsService, SettingsService>();

            serviceDescriptors.AddTransient<MainWindow>();
            serviceDescriptors.AddTransient<MediaInferencePage>();

            return serviceDescriptors.BuildServiceProvider();
        }

        private Window m_window;
    }
}
