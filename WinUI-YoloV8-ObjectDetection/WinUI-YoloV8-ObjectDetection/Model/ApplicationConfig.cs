namespace WinUI_YoloV8_ObjectDetection.Model
{
    public class ApplicationConfig
    {
        public ApplicationConfig() { }
        private string[] cocoDataset;

        public string[] CocoDataset { get { return cocoDataset; } set { cocoDataset = value; } }

    }
}
