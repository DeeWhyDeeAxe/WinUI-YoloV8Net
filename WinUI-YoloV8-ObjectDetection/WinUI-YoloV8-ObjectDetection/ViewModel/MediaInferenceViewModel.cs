using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml;

namespace WinUI_YoloV8_ObjectDetection.ViewModel
{
    public partial class MediaInferenceViewModel : ObservableObject
    {
        public MediaInferenceViewModel() { }

        [ObservableProperty]
        private Visibility inferenceLoading = Visibility.Collapsed;

        [ObservableProperty]
        private Visibility referenceGuideVisible = Visibility.Visible;
    }
}
