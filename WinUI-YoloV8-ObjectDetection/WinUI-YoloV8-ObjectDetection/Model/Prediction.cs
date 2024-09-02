using SixLabors.ImageSharp;

namespace WinUI_YoloV8_ObjectDetection.Model
{
    public class Prediction
    {
        public Label Label { get; init; }
        public RectangleF BoundingBox { get; init; }
        public float ConfidenceScore { get; init; }
    }
}
