using System;

namespace WinUI_YoloV8_ObjectDetection.Interface
{
    public interface IPredictorService : IDisposable
    {
        IPredictor YoloPredictor { get; set; }
        IPredictor CreateYoloPredictor(string modelPath, bool? useCuda);
    }
}
