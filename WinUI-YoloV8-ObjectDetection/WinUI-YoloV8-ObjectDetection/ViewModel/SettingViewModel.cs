using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WinUI_YoloV8_ObjectDetection.Model.Enums;

namespace WinUI_YoloV8_ObjectDetection.ViewModel
{
    public partial class SettingViewModel : ObservableObject
    {
        public SettingViewModel() { }

        [ObservableProperty]
        private ExecutionProvider executionProvider;

        [RelayCommand]
        private void UpdateExecutionProvider(ExecutionProvider executionProvider)
        {
            ExecutionProvider = executionProvider;
        }
    }
}
