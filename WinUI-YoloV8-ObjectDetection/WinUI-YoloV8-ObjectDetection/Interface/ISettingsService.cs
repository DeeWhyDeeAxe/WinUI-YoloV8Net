namespace WinUI_YoloV8_ObjectDetection.Interface
{
    public interface ISettingsService
    {
        void ApplySettings(bool? useCuda = false);
    }
}
