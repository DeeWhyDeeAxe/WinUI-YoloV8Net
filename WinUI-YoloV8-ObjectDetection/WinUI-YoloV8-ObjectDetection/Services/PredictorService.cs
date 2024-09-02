using WinUI_YoloV8_ObjectDetection.Interface;

namespace WinUI_YoloV8_ObjectDetection.Services
{
    public class PredictorService : IPredictorService
    {
        private IPredictor yoloPredictor;
        public IPredictor YoloPredictor { get { return yoloPredictor; } set { yoloPredictor = value; } }

        public IPredictor CreateYoloPredictor(string modelPath, bool? useCuda)
        {
            YoloPredictor = YoloV8Predictor.Create(modelPath: modelPath, useCuda: useCuda);
            return YoloPredictor;
        }

        public void Dispose()
        {
            YoloPredictor.Dispose();
        }
    }
}
