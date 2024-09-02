using System;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using WinUI_YoloV8_ObjectDetection.Model;

namespace WinUI_YoloV8_ObjectDetection.Interface
{
    public interface IPredictor : IDisposable
    {
        InferenceMetadata InferenceMetadata { get; init; }
        Task<Image> PerformDetectionAsync(Image img);

    }
}
