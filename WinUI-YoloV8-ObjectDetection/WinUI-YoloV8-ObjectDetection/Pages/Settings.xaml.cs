using System;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using WinUI_YoloV8_ObjectDetection.Interface;
using WinUI_YoloV8_ObjectDetection.Model.Enums;
using WinUI_YoloV8_ObjectDetection.ViewModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI_YoloV8_ObjectDetection.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        private SettingViewModel settingViewModel;
        private ISettingsService settingsService;
        public Settings()
        {
            this.InitializeComponent();

            settingViewModel = new SettingViewModel();
            DataContext = settingViewModel;
            PopulateDropdown();
            settingViewModel.PropertyChanged += SettingViewModel_PropertyChanged;
            settingsService = App.Current.ServiceProvider.GetService<ISettingsService>();
        }

        private void PopulateDropdown()
        {
            foreach (var executionProviders in Enum.GetValues<ExecutionProvider>())
            {
                ExecutionProviderMenuFlyout.Items.Add(new MenuFlyoutItem { Text = executionProviders.ToString(), Command = settingViewModel.UpdateExecutionProviderCommand, CommandParameter = executionProviders });
            }
        }

        private void SettingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            bool useCuda = settingViewModel.ExecutionProvider == ExecutionProvider.CUDA ? true : false;
            settingsService.ApplySettings(useCuda);
        }
    }
}
