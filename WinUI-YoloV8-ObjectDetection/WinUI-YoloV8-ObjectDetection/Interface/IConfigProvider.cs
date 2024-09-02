using WinUI_YoloV8_ObjectDetection.Model;

namespace WinUI_YoloV8_ObjectDetection.Interface
{
    public interface IConfigProvider
    {
        ApplicationConfig GetConfig();
        void LoadCocoDataset();
    }
}
