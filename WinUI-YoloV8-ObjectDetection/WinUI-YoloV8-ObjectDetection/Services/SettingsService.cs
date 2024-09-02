using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using WinUI_YoloV8_ObjectDetection.Interface;

namespace WinUI_YoloV8_ObjectDetection.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly string yoloModelPath;

        private IPredictorService predictorService;
        public SettingsService()
        {
            predictorService = App.Current.ServiceProvider.GetService<IPredictorService>();
            yoloModelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/yolov8n.onnx");
        }
        public void ApplySettings(bool? useCuda = false)
        {
            predictorService.CreateYoloPredictor(yoloModelPath, useCuda);
        }
    }
}
